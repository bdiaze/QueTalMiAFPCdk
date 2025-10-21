using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using QueTalMiAFPCdk.Repositories;
using QueTalMiAFPCdk.Services;
using System.Globalization;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Se obtienen parámetros para configurar cognito...
ParameterStoreHelper parameterStoreHelper = new(builder.Configuration);
string cognitoBaseUrl = await parameterStoreHelper.ObtenerParametro("/QueTalMiAFP/Cognito/BaseUrl");
string cognitoRegion = parameterStoreHelper.ObtenerParametro("/QueTalMiAFP/Cognito/Region").Result;
string userPoolId = parameterStoreHelper.ObtenerParametro("/QueTalMiAFP/Cognito/UserPoolId").Result;
string userPoolClientId = parameterStoreHelper.ObtenerParametro("/QueTalMiAFP/Cognito/UserPoolClientId").Result;
string[] cognitoCallbacks = parameterStoreHelper.ObtenerParametro("/QueTalMiAFP/Cognito/Callbacks").Result.Split(",");

string callbackUrl = cognitoCallbacks.First(l => !l.Contains("localhost"));
if (builder.Environment.IsDevelopment()) {
    callbackUrl = cognitoCallbacks.First(l => l.Contains("localhost"));
}

Uri uri = new(callbackUrl);
string callbackPath = uri.AbsolutePath;

using HttpClient client = new();
string response = await client.GetStringAsync($"https://cognito-idp.{cognitoRegion}.amazonaws.com/{userPoolId}/.well-known/openid-configuration");
JsonElement openidConfiguration = JsonDocument.Parse(response).RootElement;
string? tokenEndpoint = openidConfiguration.GetProperty("token_endpoint").GetString();

builder.Services.AddAuthentication(options => {
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options => {
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;

    options.ExpireTimeSpan = TimeSpan.FromDays(30);

    options.Events = new CookieAuthenticationEvents {
        OnValidatePrincipal = async context => {
            string? strExpiresAt = context.Properties.GetTokenValue("expires_at");
            if (strExpiresAt != null && DateTimeOffset.TryParse(strExpiresAt, out DateTimeOffset expiresAt) && expiresAt < DateTimeOffset.UtcNow.AddMinutes(5)) {
                string? refreshToken = context.Properties.GetTokenValue("refresh_token");

                if (tokenEndpoint != null && refreshToken != null) {
                    HttpResponseMessage tokenResponse = await client.PostAsync(tokenEndpoint, new FormUrlEncodedContent(new Dictionary<string, string> {
                        { "grant_type", "refresh_token" },
                        { "client_id", userPoolClientId },
                        { "refresh_token", refreshToken },
                    }));

                    if (tokenResponse.IsSuccessStatusCode) {
                        string payload = await tokenResponse.Content.ReadAsStringAsync();
                        JsonElement tokenData = JsonDocument.Parse(payload).RootElement;

                        string? newIdToken = tokenData.TryGetProperty("id_token", out JsonElement jIdToken) ? jIdToken.GetString() : context.Properties.GetTokenValue(OpenIdConnectParameterNames.IdToken);
                        string? newAccessToken = tokenData.GetProperty("access_token").GetString();
                        string? newRefreshToken = tokenData.TryGetProperty("refresh_token", out JsonElement jRefreshToken) ? jRefreshToken.GetString() : refreshToken;
                        int? newExpiresIn = tokenData.GetProperty("expires_in").GetInt32();

                        if (newIdToken != null && newAccessToken != null && newRefreshToken != null && newExpiresIn != null) {
                            context.Properties.StoreTokens([
                                new AuthenticationToken { Name = OpenIdConnectParameterNames.IdToken, Value = newIdToken },
                                new AuthenticationToken { Name = OpenIdConnectParameterNames.AccessToken, Value = newAccessToken },
                                new AuthenticationToken { Name = OpenIdConnectParameterNames.RefreshToken, Value = newRefreshToken },
                                new AuthenticationToken { Name = "expires_at", Value = DateTime.UtcNow.AddSeconds(newExpiresIn.Value).ToString("o", CultureInfo.InvariantCulture) },
                            ]);
                            context.Properties.IsPersistent = true;
                            context.Properties.ExpiresUtc = DateTimeOffset.UtcNow.AddDays(30);
                            context.ShouldRenew = true;
                        }
                    }
                }
            }
        }
    };
})
.AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options => {
    options.Authority = cognitoBaseUrl;
    options.MetadataAddress = $"https://cognito-idp.{cognitoRegion}.amazonaws.com/{userPoolId}/.well-known/openid-configuration";
    options.ClientId = userPoolClientId;
    options.ResponseType = OpenIdConnectResponseType.Code;
    options.SaveTokens = true;
    options.UseTokenLifetime = false;
    options.Scope.Add("openid");
    options.Scope.Add("email");
    options.Scope.Add("profile");
    options.CallbackPath = callbackPath;

    options.TokenValidationParameters = new TokenValidationParameters {
        ValidateIssuer = true,
        ValidIssuer = cognitoBaseUrl,
        ValidateAudience = true,
        ValidAudience = userPoolClientId,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
    };

    options.Events = new OpenIdConnectEvents { 
        OnRedirectToIdentityProvider = context => {
            context.ProtocolMessage.SetParameter("lang", "es");
            context.ProtocolMessage.RedirectUri = callbackUrl;
            return Task.CompletedTask;
        },
        OnTicketReceived = context => {
            context.Properties!.IsPersistent = true;
            context.Properties.ExpiresUtc = DateTimeOffset.UtcNow.AddDays(30);
            return Task.CompletedTask;
        }
    };
});

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
if (builder.Environment.IsDevelopment()) {
    builder.Logging.AddDebug();
}
builder.Logging.SetMinimumLevel(LogLevel.Information);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();

builder.Services.AddSingleton<ParameterStoreHelper>();
builder.Services.AddSingleton<SecretManagerHelper>();
builder.Services.AddSingleton<ApiKeyHelper>();
builder.Services.AddSingleton<S3BucketHelper>();
builder.Services.AddSingleton<MercadoPagoHelper>();
builder.Services.AddSingleton<CuotaUfComisionDAO>();
builder.Services.AddSingleton<NotificacionDAO>();
builder.Services.AddSingleton<ApiKeyDAO>();
builder.Services.AddSingleton<EnvioCorreo>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
    app.UseDeveloperExceptionPage();
} else {
    app.UseExceptionHandler("/Error/Index");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseStaticFiles(new StaticFileOptions { 
    OnPrepareResponse = context => {
        context.Context.Response.Headers.CacheControl = $"public,max-age={30 * 24 * 60 * 60},immutable";
    }
});

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Resumen}/{action=Index}/{id?}");

app.Run();
