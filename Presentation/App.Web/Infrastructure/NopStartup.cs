using App.Core.Infrastructure;
using App.Web.Areas.Admin.Factories;
using App.Web.Areas.Admin.Factories.Extension;
using App.Web.Areas.Admin.Helpers;
using App.Web.Areas.Admin.InterviewQeations;
using App.Web.Areas.Admin.JobPostings;
using App.Web.Areas.Admin.Manageresumes;
using App.Web.Areas.Admin.WeeklyQuestion;
using App.Web.Framework.Factories;
using App.Web.Infrastructure.Installation;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace App.Web.Infrastructure
{
    /// <summary>
    /// Represents the registering services on application startup
    /// </summary>
    public partial class NopStartup : INopStartup
    {
        /// <summary>
        /// Add and configure any of the middleware
        /// </summary>
        /// <param name="services">Collection of service descriptors</param>
        /// <param name="configuration">Configuration of the application</param>
        public virtual void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            //installation localization service
            services.AddScoped<IInstallationLocalizationService, InstallationLocalizationService>();

            //common factories
            services.AddScoped<IAclSupportedModelFactory, AclSupportedModelFactory>();
            services.AddScoped<ILocalizedModelFactory, LocalizedModelFactory>();
            services.AddScoped<IStoreMappingSupportedModelFactory, StoreMappingSupportedModelFactory>();

            //admin factories
            services.AddScoped<IBaseAdminModelFactory, BaseAdminModelFactory>();
            services.AddScoped<IActivityLogModelFactory, ActivityLogModelFactory>();
            services.AddScoped<IAddressModelFactory, AddressModelFactory>();
            services.AddScoped<IAddressAttributeModelFactory, AddressAttributeModelFactory>();
            services.AddScoped<IAffiliateModelFactory, AffiliateModelFactory>();
            services.AddScoped<IBlogModelFactory, BlogModelFactory>();
            services.AddScoped<ICampaignModelFactory, CampaignModelFactory>();
            services.AddScoped<ICommonModelFactory, CommonModelFactory>();
            services.AddScoped<ICountryModelFactory, CountryModelFactory>();
            services.AddScoped<ICurrencyModelFactory, CurrencyModelFactory>();
            services.AddScoped<ICustomerAttributeModelFactory, CustomerAttributeModelFactory>();
            services.AddScoped<ICustomerModelFactory, CustomerModelFactory>();
            services.AddScoped<ICustomerRoleModelFactory, CustomerRoleModelFactory>();
            services.AddScoped<IEmailAccountModelFactory, EmailAccountModelFactory>();
            services.AddScoped<IExternalAuthenticationMethodModelFactory, ExternalAuthenticationMethodModelFactory>();
            services.AddScoped<IForumModelFactory, ForumModelFactory>();
            services.AddScoped<IHomeModelFactory, HomeModelFactory>();
            services.AddScoped<ILanguageModelFactory, LanguageModelFactory>();
            services.AddScoped<ILogModelFactory, LogModelFactory>();
            services.AddScoped<IMeasureModelFactory, MeasureModelFactory>();
            services.AddScoped<IMessageTemplateModelFactory, MessageTemplateModelFactory>();
            services.AddScoped<IMultiFactorAuthenticationMethodModelFactory, MultiFactorAuthenticationMethodModelFactory>();
            services.AddScoped<INewsletterSubscriptionModelFactory, NewsletterSubscriptionModelFactory>();
            services.AddScoped<INewsModelFactory, NewsModelFactory>();
            services.AddScoped<IPluginModelFactory, PluginModelFactory>();
            services.AddScoped<IPollModelFactory, PollModelFactory>();
            services.AddScoped<IQueuedEmailModelFactory, QueuedEmailModelFactory>();
            services.AddScoped<IScheduleTaskModelFactory, ScheduleTaskModelFactory>();
            services.AddScoped<ISecurityModelFactory, SecurityModelFactory>();
            services.AddScoped<ISettingModelFactory, SettingModelFactory>();
            services.AddScoped<IStoreModelFactory, StoreModelFactory>();
            services.AddScoped<ITemplateModelFactory, TemplateModelFactory>();
            services.AddScoped<ITopicModelFactory, TopicModelFactory>();
            services.AddScoped<IWidgetModelFactory, WidgetModelFactory>();
            services.AddScoped<IHolidayModelFactory, HolidayModelFactory>();
            services.AddScoped<IDesignationModelFactory, DesignationModelFactory>();
            services.AddScoped<ILeaveTypeModelFactory, LeaveTypeModelFactory>();
            services.AddScoped<IEmployeeModelFactory, EmployeeModelFactory>();
            services.AddScoped<ILeaveManagementModelFactory, LeaveManagementModelFactory>();
            services.AddScoped<ILeaveTransactionLogModelFactory, LeaveTransactionLogModelFactory>();

            services.AddScoped<IDepartmentModelFactory, DepartmentModelFactory>();
            services.AddScoped<IProjectModelFactory, ProjectModelFactory>();
            services.AddScoped<IProjectEmployeeMappingModelFactory, ProjectEmployeeMappingModelFactory>();
            services.AddScoped<ITimeSheetModelFactory, TimeSheetModelFactory>();
            services.AddScoped<IMonthlyPerformanceReportModelFactory, MonthlyPerformanceReportModelFactory>();

            services.AddScoped<IKPIMasterModelFactory, KPIMasterModelFactory>();
            services.AddScoped<IKPIWeightageModelFactory, KPIWeightageModelFactory>();
            services.AddScoped<ITeamPerformanceMeasurementModelFactory, TeamPerformanceMeasurementModelFactory>();
            services.AddScoped<IEmployeeAttendanceModelFactory, EmployeeAttendanceModelFactory>();
            services.AddScoped<IEducationModelFactory, EducationModelFactory>();
            services.AddScoped<IExperienceModelFactory, ExperienceModelFactory>();
            services.AddScoped<IAssetsModelFactory, AssetsModelFactory>();
            services.AddScoped<IEmpAddressModelFactory, EmpAddressModelFactory>();
            services.AddScoped<IJobPostingsModelFactory, JobPostingsModelFactory>();
            services.AddScoped<ICandiatesResumesModelFactory, CandiatesResumesModelFactory>();
            services.AddScoped<IQuestionsModelFactory, QuestionsModelFactory>();
            services.AddScoped<IWeeklyQuestionsModelFactory, WeeklyQuestionsModelFactory>();
            services.AddScoped<IProjectTaskModelFactory, ProjectTaskModelFactory>();
            services.AddScoped<IActivityModelFactory, ActivityModelFactory>();
            services.AddScoped<IProcessWorkflowModelFactory, ProcessWorkflowModelFactory>();
            services.AddScoped<IWorkflowStatusModelFactory, WorkflowStatusModelFactory>();
            services.AddScoped<IProcessRulesModelFactory, ProcessRulesModelFactory>();
            services.AddScoped<IAnnouncementModelFactory, AnnouncementModelFactory>();




            //factories
            services.AddScoped<Factories.IAddressModelFactory, Factories.AddressModelFactory>();
            services.AddScoped<Factories.IBlogModelFactory, Factories.BlogModelFactory>();
            services.AddScoped<Factories.ICommonModelFactory, Factories.CommonModelFactory>();
            services.AddScoped<Factories.ICountryModelFactory, Factories.CountryModelFactory>();
            services.AddScoped<Factories.ICustomerModelFactory, Factories.CustomerModelFactory>();
            services.AddScoped<Factories.IForumModelFactory, Factories.ForumModelFactory>();
            services.AddScoped<Factories.IExternalAuthenticationModelFactory, Factories.ExternalAuthenticationModelFactory>();
            services.AddScoped<Factories.INewsModelFactory, Factories.NewsModelFactory>();
            services.AddScoped<Factories.INewsletterModelFactory, Factories.NewsletterModelFactory>();
            services.AddScoped<Factories.IPollModelFactory, Factories.PollModelFactory>();
            services.AddScoped<Factories.IPrivateMessagesModelFactory, Factories.PrivateMessagesModelFactory>();
            services.AddScoped<Factories.IProfileModelFactory, Factories.ProfileModelFactory>();
            services.AddScoped<Factories.ISitemapModelFactory, Factories.SitemapModelFactory>();
            services.AddScoped<Factories.ITopicModelFactory, Factories.TopicModelFactory>();
            services.AddScoped<Factories.IWidgetModelFactory, Factories.WidgetModelFactory>();

            services.AddScoped<Factories.IEmployeeModelFactory, Factories.EmployeeModelFactory>();
            services.AddScoped<Factories.IEmployeeAddressModelFactory, Factories.EmployeeAddressModelFactory>();
            services.AddScoped<Factories.IEmployeeEducationModelFactory, Factories.EmployeeEducationModelFactory>();
            services.AddScoped<Factories.IEmployeeAssetsModelFactory, Factories.EmployeeAssetsModelFactory>();
            services.AddScoped<Factories.IEmployeeExperienceModelFactory, Factories.EmployeeExperienceModelFactory>();
            services.AddScoped<Factories.IPerformanceModelFactory, Factories.PerformanceModelFactory>();
            services.AddScoped<Factories.Extensions.ILeaveManagementModelFactory, Factories.Extensions.LeaveManagementModelFactory>();

            services.AddScoped<Factories.Extensions.ITimeSheetModelFactory, Factories.Extensions.TimeSheetModelFactory>();
            services.AddScoped<Factories.Extensions.IProjectTaskModelFactory, Factories.Extensions.ProjectTaskModelFactory>();
            services.AddScoped<Factories.Extensions.IReportsModelFactory, Factories.Extensions.ReportsModelFactory>();
            services.AddScoped<Factories.Extensions.IDashboardModelFactory, Factories.Extensions.DashboardModelFactory>();
            services.AddScoped<ITaskCategoryModelFactory, TaskCategoryModelFactory>();
            services.AddScoped<IProjectTaskCategoryMappingModelFactory, ProjectTaskCategoryMappingModelFactory>();
            services.AddScoped<ICheckListMasterModelFactory, CheckListMasterModelFactory>();
            services.AddScoped<ICheckListMappingModelFactory, CheckListMappingModelFactory>();
            services.AddScoped<IReportsModelFactory,ReportsModelFactory>();
            services.AddScoped<ITaskCommentsModelFactory, TaskCommentsModelFactory>();
            services.AddScoped<ITaskChangeLogModelFactory, TaskChangeLogModelFactory>();
            services.AddScoped<IUpdateTemplateModelFactory, UpdateTemplateModelFactory>();
            services.AddScoped<IUpdateTemplateQuestionModelFactory, UpdateTemplateQuestionModelFactory>();

            //Project Integrations
            services.AddScoped<IProjectIntegrationModelFactory, ProjectIntegrationModelFactory>();

            //Task Alerts
            services.AddScoped<ITaskAlertModelFactory, TaskAlertModelFactory>();

            //helpers classes
            services.AddScoped<ITinyMceHelper, TinyMceHelper>();
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
        public int Order => 2002;
    }
}
