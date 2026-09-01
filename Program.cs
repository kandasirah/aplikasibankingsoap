using AplikasiWebMethodSOAP.Contracts;
using AplikasiWebMethodSOAP.Services;
using CoreWCF;
using CoreWCF.Configuration;
using CoreWCF.Description;
using Microsoft.OpenApi.Models;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddServiceModelServices();
builder.Services.AddServiceModelMetadata();
builder.Services.AddSingleton<IServiceBehavior, ServiceMetadataBehavior>();
builder.Services.AddSingleton<IBankingService, BankingService>();

builder.Services.AddControllersWithViews();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Banking SOAP Service API",
        Version = "v1",
        Description = "API Documentation untuk layanan Banking SOAP menggunakan CoreWCF",
        Contact = new OpenApiContact
        {
            Name = "Banking Service Support",
            Email = "support@banking.com"
        },
        License = new OpenApiLicense
        {
            Name = "MIT License",
            Url = new Uri("https://opensource.org/licenses/MIT")
        }
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

var app = builder.Build();

var serviceMetadataBehavior = app.Services.GetRequiredService<ServiceMetadataBehavior>();
serviceMetadataBehavior.HttpGetEnabled = true;

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Banking SOAP Service API v1");
        c.RoutePrefix = "swagger";
        c.DocumentTitle = "Banking SOAP Service - API Documentation";
    });
}

app.UseStaticFiles();
app.UseRouting();

app.UseServiceModel(serviceBuilder =>
{
    serviceBuilder.AddService<BankingService>();
    serviceBuilder.AddServiceEndpoint<BankingService, IBankingService>(new BasicHttpBinding(), "/BankingService.svc");
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Banking}/{action=Index}/{id?}");

// app.MapGet("/", () => Results.Redirect("/Banking"));

app.MapGet("/api/info", () => Results.Json(new
{
    name = "Banking SOAP Service API",
    version = "v1",
    description = "API Documentation untuk layanan Banking SOAP menggunakan CoreWCF",
    swagger = "/swagger",
    endpoints = new
    {
        web = "/Banking",
        api = "/api/BankingApi",
        soap = "/BankingService.svc"
    }
}));

app.Run();
