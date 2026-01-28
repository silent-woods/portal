using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using App.Core.Infrastructure;
using Satyanam.Nop.Core.Services;
using App.Services.Security;
using App.Services.ScheduleTasks;

namespace Satyanam.Nop.Core.Infrastructure
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
            services.AddScoped<ITitleService,TitleService>();
            services.AddScoped<IIndustryService, IndustryService>();
            services.AddScoped<ILeadSourceService, LeadSourceService>();
            services.AddScoped<ILeadStatusService, LeadStatusService>();
            services.AddScoped<ICategorysService, CategorysService>();
            services.AddScoped<ITagsService, TagsService>();
            services.AddScoped<ILeadService, LeadService>();
            services.AddScoped<ICampaingsService, CampaingsService>();
            services.AddScoped<IContactsService, ContactsService>();
            services.AddScoped<IAccountTypeService, AccountTypeService>();
            services.AddScoped<ICompanyService, CompanyService>();
            services.AddScoped<IDealsService, DealsService>();
            services.AddScoped<IPermissionProvider, SatyanamPermissionProvider>();
            services.AddScoped<ILeadAPIService, LeadAPIService>();
            services.AddScoped<ICampaingsEmailLogsService, CampaingsEmialLogsService>();
            services.AddScoped<IUpdateTemplateService, UpdateTemplateService>();
            services.AddScoped<IUpdateTemplateQuestionService, UpdateTemplateQuestionService>();
            services.AddScoped<IUpdateQuestionOptionService, UpdateQuestionOptionService>();
            services.AddScoped<IUpdateSubmissionService, UpdateSubmissionService>();
            services.AddScoped<IScheduleTask, GenerateUpdateTemplatePeriodsTask>();
            services.AddScoped<IUpdateSubmissionCommentService, UpdateSubmissionCommentService>();
            services.AddScoped<ILinkedInFollowupsService, LinkedInFollowupsService>();
            services.AddScoped<IConnectionRequestService, ConnectionRequestService>();

            services.AddScoped<ITaskCommentsService, TaskCommentsService>();
            services.AddScoped<ITaskChangeLogService, TaskChangeLogService>();
            services.AddScoped<IProcessWorkflowService, ProcessWorkflowService>();
            services.AddScoped<IWorkflowStatusService, WorkflowStatusService>();
            services.AddScoped<IProcessRulesService, ProcessRulesService>();
            services.AddScoped<IActivityTrackingService, ActivityTrackingService>();
            services.AddScoped<IWorkflowMessagePluginService, WorkflowMessagePluginService>();
            services.AddScoped<ICommonPluginService, CommonPluginService>();
            services.AddScoped<IAnnouncementService, AnnouncementService>();
            services.AddScoped<ITaskCategoryService, TaskCategoryService>();
            services.AddScoped<ICheckListMasterService, CheckListMasterService>();
            services.AddScoped<ICheckListMappingService, CheckListMappingService>();
            services.AddScoped<ITaskCheckListEntryService, TaskCheckListEntryService>();
            services.AddScoped<IInquiryService, InquiryService>();
            services.AddScoped<IProjectTaskCategoryMappingService, ProjectTaskCategoryMappingService>();
            services.AddScoped<IFollowUpTaskService, FollowUpTaskService>();


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
