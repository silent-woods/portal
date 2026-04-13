using App.Core.Infrastructure;
using App.Services.ScheduleTasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Satyanam.Nop.Plugin.Misc.SatyanamCRM.Services;
using Satyanam.Nop.Plugin.Misc.SatyanamCRM.Services.ScheduleTasks;
using Satyanam.Nop.Core.Services;
using Satyanam.Nop.Plugin.Misc.SatyanamCRM.Services.Zoho;

namespace Satyanam.Nop.Plugin.Misc.SatyanamCRM.Infrastructure
{
    public class NopStartup : INopStartup
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<ILeadImportService, LeadImportService>();
            services.AddScoped<ILeadExportService, LeadExportService>();
            services.AddScoped<IReplyIoService, ReplyIoService>();
            services.AddScoped<ILinkedInFollowupsExportService, LinkedInFollowupsExportService>();
            services.AddScoped<ILinkedInFollowupsImportService, LinkedInFollowupsImportService>();
            services.AddScoped<IConnectionRequestExportService, ConnectionRequestExportService>();
            services.AddScoped<IConnectionRequestImportService, ConnectionRequestImportService>();
            services.AddScoped<IWorkflowMessageCRMService, WorkflowMessageCRMService>();
            services.AddScoped<IScheduleTask, SyncLeadsToReplyTask>();
            services.AddHttpClient<ZohoCampaignsHttpClient>();
            services.AddScoped<IZohoCampaignService, ZohoCampaignService>();
            services.AddScoped<IScheduleTask, SyncZohoCampaignsTask>();
        }
        public void Configure(IApplicationBuilder application)
        {
        }
        public int Order => 3000;
    }
}
