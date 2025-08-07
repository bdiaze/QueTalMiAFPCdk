using Amazon.S3;
using QueTalMiAFPCdk.Repositories;
using QueTalMiAFPCdk.Services;

var builder = WebApplication.CreateBuilder(args);

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

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Resumen}/{action=Index}/{id?}");

app.Run();
