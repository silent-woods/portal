using App.Core.Infrastructure;
using App.Services.ScheduleTasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Satyanam.Nop.Plugin.Misc.SatyanamCRM.Services;
using Satyanam.Nop.Plugin.Misc.SatyanamCRM.Services.ScheduleTasks;

namespace Satyanam.Nop.Plugin.Misc.SatyanamCRM.Infrastructure
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
            //register custom services
            services.AddScoped<ILeadImportService, LeadImportService>();
            services.AddScoped<ILeadExportService, LeadExportService>();
            services.AddScoped<IReplyIoService, ReplyIoService>();
            services.AddScoped<ILinkedInFollowupsExportService, LinkedInFollowupsExportService>();
            services.AddScoped<ILinkedInFollowupsImportService, LinkedInFollowupsImportService>();
            services.AddScoped<IConnectionRequestExportService, ConnectionRequestExportService>();
            services.AddScoped<IConnectionRequestImportService, ConnectionRequestImportService>();

            services.AddScoped<IWorkflowMessageCRMService, WorkflowMessageCRMService>();
            services.AddScoped<IScheduleTask, SyncLeadsToReplyTask>();
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
