using App.Core;
using App.Core.Caching;
using App.Core.Configuration;
using App.Core.Events;
using App.Core.Infrastructure;
using App.Data;
using App.Services.Activities;
using App.Services.ActivityEvents;
using App.Services.Affiliates;
using App.Services.Authentication;
using App.Services.Authentication.External;
using App.Services.Authentication.MultiFactor;
using App.Services.Blogs;
using App.Services.Caching;
using App.Services.Cms;
using App.Services.Common;
using App.Services.Configuration;
using App.Services.Customers;
using App.Services.Departments;
using App.Services.Designations;
using App.Services.Directory;
using App.Services.EmployeeAttendances;
using App.Services.Employees;
using App.Services.Events;
using App.Services.ExportImport;
using App.Services.Forums;
using App.Services.Gdpr;
using App.Services.Helpers;
using App.Services.Holidays;
using App.Services.Html;
using App.Services.Installation;
using App.Services.JobPostings;
using App.Services.Leaves;
using App.Services.Localization;
using App.Services.Logging;
using App.Services.ManageResumes;
using App.Services.Media;
using App.Services.Media.RoxyFileman;
using App.Services.Messages;
using App.Services.News;
using App.Services.PerformanceMeasurements;
using App.Services.Plugins;
using App.Services.Plugins.Marketplace;
using App.Services.Polls;
using App.Services.ProjectEmployeeMappings;
using App.Services.ProjectIntegrations;
using App.Services.Projects;
using App.Services.ProjectTasks;
using App.Services.ScheduleTasks;
using App.Services.Security;
using App.Services.Seo;
using App.Services.Stores;
using App.Services.TaskAlerts;
using App.Services.Themes;
using App.Services.TimeSheets;
using App.Services.Topics;
using App.Services.WeeklyQuestion;
using App.Web.Framework.Menu;
using App.Web.Framework.Mvc.Routing;
using App.Web.Framework.Themes;
using App.Web.Framework.UI;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Services.Recruitements;
using System;
using System.Linq;

namespace App.Web.Framework.Infrastructure
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

            //file provider
            services.AddScoped<INopFileProvider, NopFileProvider>();

            //web helper
            services.AddScoped<IWebHelper, WebHelper>();

            //user agent helper
            services.AddScoped<IUserAgentHelper, UserAgentHelper>();

            //plugins
            services.AddScoped<IPluginService, PluginService>();
            services.AddScoped<OfficialFeedManager>();

            //static cache manager
            var appSettings = Singleton<AppSettings>.Instance;
            var distributedCacheConfig = appSettings.Get<DistributedCacheConfig>();
            if (distributedCacheConfig.Enabled)
            {
                switch (distributedCacheConfig.DistributedCacheType)
                {
                    case DistributedCacheType.Memory:
                        services.AddScoped<ILocker, MemoryDistributedCacheManager>();
                        services.AddScoped<IStaticCacheManager, MemoryDistributedCacheManager>();
                        break;
                    case DistributedCacheType.SqlServer:
                        services.AddScoped<ILocker, MsSqlServerCacheManager>();
                        services.AddScoped<IStaticCacheManager, MsSqlServerCacheManager>();
                        break;
                    case DistributedCacheType.Redis:
                        services.AddScoped<ILocker, RedisCacheManager>();
                        services.AddScoped<IStaticCacheManager, RedisCacheManager>();
                        break;
                }
            }
            else
            {
                services.AddSingleton<ILocker, MemoryCacheManager>();
                services.AddSingleton<IStaticCacheManager, MemoryCacheManager>();
            }

            //work context
            services.AddScoped<IWorkContext, WebWorkContext>();

            //store context
            services.AddScoped<IStoreContext, WebStoreContext>();
            services.AddScoped<ITopicTemplateService, TopicTemplateService>();
            services.AddScoped<IAddressAttributeFormatter, AddressAttributeFormatter>();
            services.AddScoped<IAddressAttributeParser, AddressAttributeParser>();
            services.AddScoped<IAddressAttributeService, AddressAttributeService>();
            services.AddScoped<IAddressService, AddressService>();
            services.AddScoped<IAffiliateService, AffiliateService>();
            services.AddScoped<ISearchTermService, SearchTermService>();
            services.AddScoped<IGenericAttributeService, GenericAttributeService>();
            services.AddScoped<IMaintenanceService, MaintenanceService>();
            services.AddScoped<ICustomerAttributeFormatter, CustomerAttributeFormatter>();
            services.AddScoped<ICustomerAttributeParser, CustomerAttributeParser>();
            services.AddScoped<ICustomerAttributeService, CustomerAttributeService>();
            services.AddScoped<ICustomerService, CustomerService>();
            services.AddScoped<ICustomerRegistrationService, CustomerRegistrationService>();
            services.AddScoped<ICustomerReportService, CustomerReportService>();
            services.AddScoped<IPermissionService, PermissionService>();
            services.AddScoped<IAclService, AclService>();
            services.AddScoped<IGeoLookupService, GeoLookupService>();
            services.AddScoped<ICountryService, CountryService>();
            services.AddScoped<ICurrencyService, CurrencyService>();
            services.AddScoped<IMeasureService, MeasureService>();
            services.AddScoped<IStateProvinceService, StateProvinceService>();
            services.AddScoped<IStoreService, StoreService>();
            services.AddScoped<IStoreMappingService, StoreMappingService>();
            services.AddScoped<ILocalizationService, LocalizationService>();
            services.AddScoped<ILocalizedEntityService, LocalizedEntityService>();
            services.AddScoped<ILanguageService, LanguageService>();
            services.AddScoped<IDownloadService, DownloadService>();
            services.AddScoped<IMessageTemplateService, MessageTemplateService>();
            services.AddScoped<IQueuedEmailService, QueuedEmailService>();
            services.AddScoped<INewsLetterSubscriptionService, NewsLetterSubscriptionService>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<ICampaignService, CampaignService>();
            services.AddScoped<IEmailAccountService, EmailAccountService>();
            services.AddScoped<IWorkflowMessageService, WorkflowMessageService>();
            services.AddScoped<IMessageTokenProvider, MessageTokenProvider>();
            services.AddScoped<ITokenizer, Tokenizer>();
            services.AddScoped<ISmtpBuilder, SmtpBuilder>();
            services.AddScoped<IEmailSender, EmailSender>();
            services.AddScoped<IEncryptionService, EncryptionService>();
            services.AddScoped<IAuthenticationService, CookieAuthenticationService>();
            services.AddScoped<IUrlRecordService, UrlRecordService>();
            services.AddScoped<ILogger, DefaultLogger>();
            services.AddScoped<ICustomerActivityService, CustomerActivityService>();
            services.AddScoped<IForumService, ForumService>();
            services.AddScoped<IGdprService, GdprService>();
            services.AddScoped<IPollService, PollService>();
            services.AddScoped<IBlogService, BlogService>();
            services.AddScoped<ITopicService, TopicService>();
            services.AddScoped<INewsService, NewsService>();
            services.AddScoped<IDateTimeHelper, DateTimeHelper>();
            services.AddScoped<INopHtmlHelper, NopHtmlHelper>();
            services.AddScoped<IScheduleTaskService, ScheduleTaskService>();
            services.AddScoped<IExportManager, ExportManager>();
            services.AddScoped<IImportManager, ImportManager>();
            services.AddScoped<IPdfService, PdfService>();
            services.AddScoped<IUploadService, UploadService>();
            services.AddScoped<IThemeProvider, ThemeProvider>();
            services.AddScoped<IThemeContext, ThemeContext>();
            services.AddScoped<IExternalAuthenticationService, ExternalAuthenticationService>();
            services.AddSingleton<IRoutePublisher, RoutePublisher>();
            services.AddSingleton<IEventPublisher, EventPublisher>();
            services.AddScoped<ISettingService, SettingService>();
            services.AddScoped<IBBCodeHelper, BBCodeHelper>();
            services.AddScoped<IHtmlFormatter, HtmlFormatter>();
            services.AddScoped<IVideoService, VideoService>();
            services.AddScoped<INopUrlHelper, NopUrlHelper>();
            services.AddScoped<IHolidayService, HolidayService>();
            services.AddScoped<IDesignationService, DesignationService>();
            services.AddScoped<ILeaveTypeService, LeaveTypeService>();
            services.AddScoped<ILeaveTransactionLogService, LeaveTransactionLogService>();

            services.AddScoped<IEmployeeService, EmployeeService>();
            services.AddScoped<IDepartmentService, DepartmentService>();
            services.AddScoped<ILeaveManagementService, LeaveManagementService>();
            services.AddScoped<IProjectsService, ProjectsService>();
            services.AddScoped<IProjectEmployeeMappingService, ProjectEmployeeMappingService>();
            services.AddScoped<IWeeklyUpdateService, WeeklyUpdateService>();
            services.AddScoped<IKPIMasterService, KPIMasterService>();
            services.AddScoped<IKPIWeightageService, KPIWeightageService>();
            services.AddScoped<ITeamPerformanceMeasurementService, TeamPerformanceMeasurementService>();
            services.AddScoped<IEmployeeAttendanceService, EmployeeAttendanceService>();
            services.AddScoped<IEducationService, EducationService>();
            services.AddScoped<IExperienceService, ExperienceService>();
            services.AddScoped<IAssetsService, AssetsService>();
            services.AddScoped<IEmpAddressService, EmpAddressService>();
            services.AddScoped<IJobPostingService, JobPostingService>();
            services.AddScoped<ICandiatesResumesService, CandiatesResumesService>();
            services.AddScoped<IWeeklyQuestionService, WeeklyQuestionService>();
            services.AddScoped<ITimeSheetsService, TimeSheetService>();
            services.AddScoped<IRecruitementService, RecruitementService>();
            services.AddScoped<IProjectTaskService, ProjectTaskService>();
            services.AddScoped<IActivityService, ActivityService>();
            services.AddScoped<IActivityEventService, ActivityEventService>();

            //Project Integration Services
            services.AddScoped<IProjectIntegrationService, ProjectIntegrationService>();

            //Task Alerts
            services.AddScoped<ITaskAlertService, TaskAlertService>();

            //plugin managers
            services.AddScoped(typeof(IPluginManager<>), typeof(PluginManager<>));
            services.AddScoped<IAuthenticationPluginManager, AuthenticationPluginManager>();
            services.AddScoped<IMultiFactorAuthenticationPluginManager, MultiFactorAuthenticationPluginManager>();
            services.AddScoped<IWidgetPluginManager, WidgetPluginManager>();
            services.AddScoped<IExchangeRatePluginManager, ExchangeRatePluginManager>();

            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();

            //register all settings
            var typeFinder = Singleton<ITypeFinder>.Instance;

            var settings = typeFinder.FindClassesOfType(typeof(ISettings), false).ToList();
            foreach (var setting in settings)
            {
                services.AddScoped(setting, serviceProvider =>
                {
                    var storeId = DataSettingsManager.IsDatabaseInstalled()
                        ? serviceProvider.GetRequiredService<IStoreContext>().GetCurrentStore()?.Id ?? 0
                        : 0;

                    return serviceProvider.GetRequiredService<ISettingService>().LoadSettingAsync(setting, storeId).Result;
                });
            }

            //picture service
            if (appSettings.Get<AzureBlobConfig>().Enabled)
                services.AddScoped<IPictureService, AzurePictureService>();
            else
                services.AddScoped<IPictureService, PictureService>();

            //roxy file manager
            services.AddScoped<IRoxyFilemanService, RoxyFilemanService>();
            services.AddScoped<IRoxyFilemanFileProvider, RoxyFilemanFileProvider>();

            //installation service
            services.AddScoped<IInstallationService, InstallationService>();

            //slug route transformer
            if (DataSettingsManager.IsDatabaseInstalled())
                services.AddScoped<SlugRouteTransformer>();

            //schedule tasks
            services.AddSingleton<ITaskScheduler, TaskScheduler>();
            services.AddTransient<IScheduleTaskRunner, ScheduleTaskRunner>();

            //event consumers
            var consumers = typeFinder.FindClassesOfType(typeof(IConsumer<>)).ToList();
            foreach (var consumer in consumers)
                foreach (var findInterface in consumer.FindInterfaces((type, criteria) =>
                {
                    var isMatch = type.IsGenericType && ((Type)criteria).IsAssignableFrom(type.GetGenericTypeDefinition());
                    return isMatch;
                }, typeof(IConsumer<>)))
                    services.AddScoped(findInterface, consumer);

            //XML sitemap
            services.AddScoped<IXmlSiteMap, XmlSiteMap>();

            //register the Lazy resolver for .Net IoC
            var useAutofac = appSettings.Get<CommonConfig>().UseAutofac;
            if (!useAutofac)
                services.AddScoped(typeof(Lazy<>), typeof(LazyInstance<>));
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
        public int Order => 2000;
    }
}