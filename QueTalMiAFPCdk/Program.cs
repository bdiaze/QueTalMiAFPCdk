using Amazon.S3;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using QueTalMiAFPCdk.Repositories;
using QueTalMiAFPCdk.Services;
using System;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Se obtienen parámetros para configurar cognito...
ParameterStoreHelper parameterStoreHelper = new(builder.Configuration);
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

builder.Services.AddAuthentication(options => {
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
.AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options => {
    options.Authority = $"https://cognito-idp.{cognitoRegion}.amazonaws.com/{userPoolId}";
    options.ClientId = userPoolClientId;
    options.ResponseType = "code";
    options.SaveTokens = true;
    options.Scope.Add("openid");
    options.Scope.Add("email");
    options.Scope.Add("profile");
    options.CallbackPath = callbackPath;
    options.TokenValidationParameters = new TokenValidationParameters {
        ValidateIssuer = true,
        ValidIssuer = $"https://cognito-idp.{cognitoRegion}.amazonaws.com/{userPoolId}",
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
        }
    };
});

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();

builder.Services.AddSingleton<ParameterStoreHelper, ParameterStoreHelper>();
builder.Services.AddSingleton<SecretManagerHelper, SecretManagerHelper>();
builder.Services.AddSingleton<S3BucketHelper, S3BucketHelper>();
builder.Services.AddSingleton<MercadoPagoHelper, MercadoPagoHelper>();
builder.Services.AddSingleton<ICuotaUfComisionDAO, CuotaUfComisionDAO>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
    app.UseDeveloperExceptionPage();
} else {
    app.UseExceptionHandler("/Error/Index");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Resumen}/{action=Index}/{id?}");

app.Run();
