using AplikasiWebMethodSOAP.Contracts;
using AplikasiWebMethodSOAP.Services;
using CoreWCF;
using CoreWCF.Configuration;
using CoreWCF.Description;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddServiceModelServices();
builder.Services.AddServiceModelMetadata();
builder.Services.AddSingleton<IServiceBehavior, ServiceMetadataBehavior>();
builder.Services.AddSingleton<IBankingService, BankingService>();

builder.Services.AddControllersWithViews();

var app = builder.Build();

var serviceMetadataBehavior = app.Services.GetRequiredService<ServiceMetadataBehavior>();
serviceMetadataBehavior.HttpGetEnabled = true;

app.UseServiceModel(serviceBuilder =>
{
    serviceBuilder.AddService<BankingService>();
    serviceBuilder.AddServiceEndpoint<BankingService, IBankingService>(new BasicHttpBinding(), "/BankingService.svc");
});

app.UseStaticFiles();
app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Banking}/{action=Index}/{id?}");

app.MapGet("/", () => Results.Redirect("/Banking"));

app.Run();
