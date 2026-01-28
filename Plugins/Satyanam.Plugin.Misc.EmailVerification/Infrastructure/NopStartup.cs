using App.Core.Infrastructure;
using App.Web.Areas.Admin.Controllers;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Satyanam.Plugin.Misc.EmailVerification.Services;

namespace Satyanam.Plugin.Misc.Emailverification.Infrastructure
{
    /// <summary>
    /// Represents object for the configuring services on application startup
    /// </summary>
    public class NopStartup : INopStartup
    {
        /// <summary>
        /// Add and configure any of the middleware
        /// </summary>
        /// <param name="services">Collection of service descriptors</param>
        /// <param name="configuration">Configuration of the application</param>
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {

            //services.AddHttpClient<MarketingAutomationHttpClient>().WithProxy();

            ////register custom services
            // services.AddScoped<EmailVerificationService>();
            //services.AddScoped<MarketingAutomationManager>();

            //override services
            //services.AddScoped<CommonController, EmailCommonController>();
            //services.AddScoped<CustomerController, EmailRegisterController>();
            services.AddScoped<IEmailverificationService, EmailverificationService>();
            //services.AddScoped< DiscussProjectFormModelFactory>();
            //services.AddScoped<CommonModelFactory>();
        }

        /// <summary>
        /// Configure the using of added middleware
        /// </summary>
        /// <param name="application">Builder for configuring an application's request pipeline</param>
        public void Configure(IApplicationBuilder application)
        {
        }

        /// <summary>
        /// Gets order of this startup configuration implementation
        /// </summary>
        public int Order => 3000;
    }
}