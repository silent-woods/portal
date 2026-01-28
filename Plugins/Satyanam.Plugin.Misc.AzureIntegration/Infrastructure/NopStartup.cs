using App.Core.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Satyanam.Plugin.Misc.AzureIntegration.Areas.Admin.Factories;
using Satyanam.Plugin.Misc.AzureIntegration.Services;
using Satyanam.Plugin.Misc.AzureIntegration.ViewEngine;

namespace Satyanam.Plugin.Misc.AzureIntegration.Infrastructure;

public partial class NopStartup : INopStartup
{
    #region Methods

    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IAzureIntegrationModelFactory, AzureIntegrationModelFactory>();

        services.AddScoped<IAzureIntegrationService, AzureIntegrationService>();

        services.Configure<RazorViewEngineOptions>(options =>
        {
            options.ViewLocationExpanders.Add(new AzureIntegrationViewEngine());
        });
    }

    public void Configure(IApplicationBuilder application)
    {
    }

    public int Order => int.MaxValue;

    #endregion
}
