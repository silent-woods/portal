using App.Core.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Satyanam.Plugin.Misc.LeadAPI.Areas.Admin.Factories;
using Satyanam.Plugin.Misc.LeadAPI.ViewEngine;

namespace Satyanam.Plugin.Misc.LeadAPI.Infrastructure;

public partial class NopStartup : INopStartup
{
    #region Methods

    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<ILeadAPIModelFactory, LeadAPIModelFactory>();


        services.Configure<RazorViewEngineOptions>(options =>
        {
            options.ViewLocationExpanders.Add(new LeadAPIViewEngine());
        });
    }

    public void Configure(IApplicationBuilder application)
    {
    }

    public int Order => int.MaxValue;

    #endregion
}
