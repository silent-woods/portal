using App.Core.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Satyanam.Plugin.Misc.TrackerAPI.Areas.Admin.Factories;
using Satyanam.Plugin.Misc.TrackerAPI.Services;
using Satyanam.Plugin.Misc.TrackerAPI.ViewEngine;

namespace Satyanam.Plugin.Misc.TrackerAPI.Infrastructure;

public partial class NopStartup : INopStartup
{
    #region Methods

    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<ITrackerAPIModelFactory, TrackerAPIModelFactory>();

        services.AddScoped<ITrackerAPIService, TrackerAPIService>();

        services.Configure<RazorViewEngineOptions>(options =>
        {
            options.ViewLocationExpanders.Add(new TrackerAPIViewEngine());
        });
    }

    public void Configure(IApplicationBuilder application)
    {
    }

    public int Order => int.MaxValue;

    #endregion
}
