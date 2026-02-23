using App.Core.Configuration;
using App.Core.Infrastructure;
using App.Web.Framework.Infrastructure.Extensions;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);


builder.Configuration.AddJsonFile(NopConfigurationDefaults.AppSettingsFilePath, true, true);
if (!string.IsNullOrEmpty(builder.Environment?.EnvironmentName))
{
    var path = string.Format(NopConfigurationDefaults.AppSettingsEnvironmentFilePath, builder.Environment.EnvironmentName);
    builder.Configuration.AddJsonFile(path, true, true);
}
builder.Configuration.AddEnvironmentVariables();

//load application settings
builder.Services.ConfigureApplicationSettings(builder);

var appSettings = Singleton<AppSettings>.Instance;
var useAutofac = appSettings.Get<CommonConfig>().UseAutofac;
builder.Services.Configure<MvcOptions>(options =>
{
    options.MaxModelBindingCollectionSize = 5000; // ✅ Increase limit from default 1024
});
if (useAutofac)
    builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
else
    builder.Host.UseDefaultServiceProvider(options =>
    {
        //we don't validate the scopes, since at the app start and the initial configuration we need 
        //to resolve some services (registered as "scoped") through the root container
        options.ValidateScopes = false;
        options.ValidateOnBuild = true;
    });

//add services to the application and configure service provider
builder.Services.ConfigureApplicationServices(builder);

builder.Services.Configure<FormOptions>(options =>
{
    options.ValueCountLimit = 50000;
});

var app = builder.Build();

//configure the application HTTP request pipeline
app.ConfigureRequestPipeline();
app.StartEngine();

app.Run();
