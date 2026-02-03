using System.Collections.Generic;
using App.Core.Domain.Customers;
using App.Core.Domain.Security;

namespace App.Services.Security
{
    /// <summary>
    /// Standard permission provider
    /// </summary>
    public partial class StandardPermissionProvider : IPermissionProvider
    {
        //admin area permissions
        public static readonly PermissionRecord AccessAdminPanel = new() { Name = "Access admin area", SystemName = "AccessAdminPanel", Category = "Standard" };

        public static readonly PermissionRecord ManageTeamPerformanceMeasurement = new() { Name = "Admin area. Manage TeamPerformanceMeasurement", SystemName = "ManageTeamPerformanceMeasurement", Category = "Standard" };
        public static readonly PermissionRecord ManageAnnouncements = new() { Name = "Admin area. Manage Announcements", SystemName = "ManageAnnouncements", Category = "Standard" };
        public static readonly PermissionRecord ManageAnnouncementAcknowledgments = new() { Name = "Admin area. Manage Announcement Acknowledgments", SystemName = "ManageAnnouncementAcknowledgments", Category = "Standard" };
        public static readonly PermissionRecord AllowCustomerImpersonation = new() { Name = "Admin area. Allow Customer Impersonation", SystemName = "AllowCustomerImpersonation", Category = "Customers" };
        public static readonly PermissionRecord ManageCustomers = new() { Name = "Admin area. Manage Customers", SystemName = "ManageCustomers", Category = "Customers" };
        public static readonly PermissionRecord ManageAffiliates = new() { Name = "Admin area. Manage Affiliates", SystemName = "ManageAffiliates", Category = "Promo" };
        public static readonly PermissionRecord ManageCampaigns = new() { Name = "Admin area. Manage Campaigns", SystemName = "ManageCampaigns", Category = "Promo" };
        public static readonly PermissionRecord ManageNewsletterSubscribers = new() { Name = "Admin area. Manage Newsletter Subscribers", SystemName = "ManageNewsletterSubscribers", Category = "Promo" };
        public static readonly PermissionRecord ManagePolls = new() { Name = "Admin area. Manage Polls", SystemName = "ManagePolls", Category = "Content Management" };
        public static readonly PermissionRecord ManageNews = new() { Name = "Admin area. Manage News", SystemName = "ManageNews", Category = "Content Management" };
        public static readonly PermissionRecord ManageBlog = new() { Name = "Admin area. Manage Blog", SystemName = "ManageBlog", Category = "Content Management" };
        public static readonly PermissionRecord ManageWidgets = new() { Name = "Admin area. Manage Widgets", SystemName = "ManageWidgets", Category = "Content Management" };
        public static readonly PermissionRecord ManageTopics = new() { Name = "Admin area. Manage Topics", SystemName = "ManageTopics", Category = "Content Management" };
        public static readonly PermissionRecord ManageForums = new() { Name = "Admin area. Manage Forums", SystemName = "ManageForums", Category = "Content Management" };
        public static readonly PermissionRecord ManageMessageTemplates = new() { Name = "Admin area. Manage Message Templates", SystemName = "ManageMessageTemplates", Category = "Content Management" };
        public static readonly PermissionRecord ManageCountries = new() { Name = "Admin area. Manage Countries", SystemName = "ManageCountries", Category = "Configuration" };
        public static readonly PermissionRecord ManageLanguages = new() { Name = "Admin area. Manage Languages", SystemName = "ManageLanguages", Category = "Configuration" };
        public static readonly PermissionRecord ManageSettings = new() { Name = "Admin area. Manage Settings", SystemName = "ManageSettings", Category = "Configuration" };
        public static readonly PermissionRecord ManageExternalAuthenticationMethods = new() { Name = "Admin area. Manage External Authentication Methods", SystemName = "ManageExternalAuthenticationMethods", Category = "Configuration" };
        public static readonly PermissionRecord ManageMultifactorAuthenticationMethods = new() { Name = "Admin area. Manage Multi-factor Authentication Methods", SystemName = "ManageMultifactorAuthenticationMethods", Category = "Configuration" };
        public static readonly PermissionRecord ManageCurrencies = new() { Name = "Admin area. Manage Currencies", SystemName = "ManageCurrencies", Category = "Configuration" };
        public static readonly PermissionRecord ManageActivityLog = new() { Name = "Admin area. Manage Activity Log", SystemName = "ManageActivityLog", Category = "Configuration" };
        public static readonly PermissionRecord ManageAcl = new() { Name = "Admin area. Manage ACL", SystemName = "ManageACL", Category = "Configuration" };
        public static readonly PermissionRecord ManageEmailAccounts = new() { Name = "Admin area. Manage Email Accounts", SystemName = "ManageEmailAccounts", Category = "Configuration" };
        public static readonly PermissionRecord ManageStores = new() { Name = "Admin area. Manage Stores", SystemName = "ManageStores", Category = "Configuration" };
        public static readonly PermissionRecord ManagePlugins = new() { Name = "Admin area. Manage Plugins", SystemName = "ManagePlugins", Category = "Configuration" };
        public static readonly PermissionRecord ManageSystemLog = new() { Name = "Admin area. Manage System Log", SystemName = "ManageSystemLog", Category = "Configuration" };
        public static readonly PermissionRecord ManageMessageQueue = new() { Name = "Admin area. Manage Message Queue", SystemName = "ManageMessageQueue", Category = "Configuration" };
        public static readonly PermissionRecord ManageMaintenance = new() { Name = "Admin area. Manage Maintenance", SystemName = "ManageMaintenance", Category = "Configuration" };
        public static readonly PermissionRecord HtmlEditorManagePictures = new() { Name = "Admin area. HTML Editor. Manage pictures", SystemName = "HtmlEditor.ManagePictures", Category = "Configuration" };
        public static readonly PermissionRecord ManageScheduleTasks = new() { Name = "Admin area. Manage Schedule Tasks", SystemName = "ManageScheduleTasks", Category = "Configuration" };
        public static readonly PermissionRecord ManageAppSettings = new() { Name = "Admin area. Manage App Settings", SystemName = "ManageAppSettings", Category = "Configuration" };

        //public store permissions
        public static readonly PermissionRecord DisplayPrices = new() { Name = "Public store. Display Prices", SystemName = "DisplayPrices", Category = "PublicStore" };
        public static readonly PermissionRecord PublicStoreAllowNavigation = new() { Name = "Public store. Allow navigation", SystemName = "PublicStoreAllowNavigation", Category = "PublicStore" };
        public static readonly PermissionRecord AccessClosedStore = new() { Name = "Public store. Access a closed store", SystemName = "AccessClosedStore", Category = "PublicStore" };
        public static readonly PermissionRecord AccessProfiling = new() { Name = "Public store. Access MiniProfiler results", SystemName = "AccessProfiling", Category = "PublicStore" };

        //Security
        public static readonly PermissionRecord EnableMultiFactorAuthentication = new() { Name = "Security. Enable Multi-factor authentication", SystemName = "EnableMultiFactorAuthentication", Category = "Security" };
        public static readonly PermissionRecord ManageAlertConfiguration = new() { Name = "Admin area. Task Alert - Manage Alert Configuration", SystemName = "ManageAlertConfiguration", Category = "Configuration" };
        public static readonly PermissionRecord ManageAlertReason = new() { Name = "Admin area. Task Alert - Manage Alert Reason", SystemName = "ManageAlertReason", Category = "Configuration" };
        public static readonly PermissionRecord ManageAlertReport = new() { Name = "Admin area. Task Alert - Manage Alert Report", SystemName = "ManageAlertReport", Category = "Configuration" };

        // Project Management
        public static readonly PermissionRecord ManageProject = new() { Name = "Admin area. Project Management - Manage Project", SystemName = "ManageProject", Category = "Standard" };
        public static readonly PermissionRecord ManageProjectEmployeeMapping = new() { Name = "Admin area. Project Management - Manage ProjectEmployeeMapping", SystemName = "ManageProjectEmployeeMapping", Category = "Standard" };
        public static readonly PermissionRecord ManageProjectTaskCategoryMapping = new() { Name = "Admin area. Project Management - Manage Project Task Category Mapping", SystemName = "ManageProjectTaskCategoryMapping", Category = "Standard" };
        public static readonly PermissionRecord ManageProjectIntegration = new() { Name = "Admin area. Project Management - Manage Project Integration", SystemName = "ManageProjectIntegration", Category = "Standard" };
        public static readonly PermissionRecord ManageProjectIntegrationMappings = new() { Name = "Admin area. Project Management - Manage Project Integration Mappings", SystemName = "ManageProjectIntegrationMappings", Category = "Standard" };
        public static readonly PermissionRecord ManageProjectIntegrationSettings = new() { Name = "Admin area. Project Management - Manage Project Integration Settings", SystemName = "ManageProjectIntegrationSettings", Category = "Standard" };
        public static readonly PermissionRecord ManageProjectTask = new() { Name = "Admin area. Project Management - Manage Project Task", SystemName = "ManageProjectTask", Category = "Standard" };
        public static readonly PermissionRecord ManageProjectTaskChangeLog = new() { Name = "Admin area. Project Management - Manage Project Task Change Logs", SystemName = "ManageProjectTaskChangeLog", Category = "Standard" };
        public static readonly PermissionRecord ManageProjectTaskComments = new() { Name = "Admin area. Project Management - Manage Project Task Comments", SystemName = "ManageProjectTaskComments", Category = "Standard" };
        public static readonly PermissionRecord ManageTaskCategory = new() { Name = "Admin area. Project Management - Manage Task Category", SystemName = "ManageTaskCategory", Category = "Standard" };
        public static readonly PermissionRecord ManageTaskCategoryChecklists = new() { Name = "Admin area. Project Management - Manage Task Category Checklists", SystemName = "ManageTaskCategoryChecklists", Category = "Standard" };
        public static readonly PermissionRecord ManageChecklist = new() { Name = "Admin area. Project Management - Manage Checklist", SystemName = "ManageChecklist", Category = "Standard" };
        public static readonly PermissionRecord ManageActivity = new() { Name = "Admin area. Project Management - Manage Activity", SystemName = "ManageActivity", Category = "Standard" };
        public static readonly PermissionRecord ManageProcessWorkflow = new() { Name = "Admin area. Project Management - Manage Process Workflow", SystemName = "ManageProcessWorkflow", Category = "Standard" };
        public static readonly PermissionRecord ManageWorkflowStatus = new() { Name = "Admin area. Project Management - Manage Workflow Status", SystemName = "ManageWorkflowStatus", Category = "Standard" };
        public static readonly PermissionRecord ManageProcessRules = new() { Name = "Admin area. Project Management - Manage Process Rules", SystemName = "ManageProcessRules", Category = "Standard" };
        public static readonly PermissionRecord ManageEmployeeReport = new() { Name = "Admin area. Project Management - Manage Employee Report", SystemName = "ManageEmployeeReport", Category = "Standard" };
        public static readonly PermissionRecord ManageProjectReport = new() { Name = "Admin area. Project Management - Manage Project Report", SystemName = "ManageProjectReport", Category = "Standard" };
        public static readonly PermissionRecord ManageTaskReport = new() { Name = "Admin area. Project Management - Manage Task Report", SystemName = "ManageTaskReport", Category = "Standard" };
        public static readonly PermissionRecord ManageTimeSummmaryReport = new() { Name = "Admin area. Project Management - Manage Time Summary Report", SystemName = "ManageTimeSummmaryReport", Category = "Standard" };
        public static readonly PermissionRecord ManageTimeSheet = new() { Name = "Admin area. Project Management - Manage TimeSheet", SystemName = "ManageTimeSheet", Category = "Standard" };

        //HRM Management
        public static readonly PermissionRecord ManageEmployee = new() { Name = "Admin area. HRM - Manage Employee", SystemName = "ManageEmployee", Category = "Standard" };
        public static readonly PermissionRecord ManageEmployeeAddress = new() { Name = "Admin area. HRM - Manage EmployeeAddress", SystemName = "ManageEmployeeAddress", Category = "Standard" };
        public static readonly PermissionRecord ManageEducation = new() { Name = "Admin area. HRM - Manage Education", SystemName = "ManageEducation", Category = "Standard" };
        public static readonly PermissionRecord ManageExperience = new() { Name = "Admin area. HRM - Manage Experience", SystemName = "ManageExperience", Category = "Standard" };
        public static readonly PermissionRecord ManageAssets = new() { Name = "Admin area. HRM - Manage Assets", SystemName = "ManageAssets", Category = "Standard" };
        public static readonly PermissionRecord ManageDesignation = new() { Name = "Admin area. HRM - Manage Designation", SystemName = "ManageDesignation", Category = "Standard" };
        public static readonly PermissionRecord ManageHoliday = new() { Name = "Admin area. HRM - Manage Holiday", SystemName = "ManageHoliday", Category = "Standard" };
        public static readonly PermissionRecord ManageDepartment = new() { Name = "Admin area. HRM - Manage Department", SystemName = "ManageDepartment", Category = "Standard" };
        public static readonly PermissionRecord ManageEmployeeCurrentActivity = new() { Name = "Admin area. HRM - Manage EmployeeCurrentActivity", SystemName = "ManageEmployeeCurrentActivity", Category = "Standard" };
        public static readonly PermissionRecord ManageEmployeeActivities = new() { Name = "Admin area. HRM - Manage EmployeeActivities", SystemName = "ManageEmployeeActivities", Category = "Standard" };
        public static readonly PermissionRecord ManageEmployeeAttendance = new() { Name = "Admin area. HRM - Manage EmployeeAttendance", SystemName = "ManageEmployeeAttendance", Category = "Standard" };
        public static readonly PermissionRecord ManageAttendanceReport = new() { Name = "Admin area. HRM - Manage AttendanceReport", SystemName = "ManageAttendanceReport", Category = "Standard" };
        public static readonly PermissionRecord ManageLeaveManagement = new() { Name = "Admin area. HRM - Manage LeaveManagement", SystemName = "ManageLeaveManagement", Category = "Standard" };
        public static readonly PermissionRecord ManageLeaveTransaction = new() { Name = "Admin area. HRM - Manage LeaveTransaction", SystemName = "ManageLeaveTransaction", Category = "Standard" };
        public static readonly PermissionRecord ManageLeaveType = new() { Name = "Admin area. HRM - Manage LeaveType", SystemName = "ManageLeaveType", Category = "Standard" };
        public static readonly PermissionRecord ManagePerformanceSummary = new() { Name = "Admin area. HRM - Manage Performance Summary", SystemName = "ManagePerformanceSummary", Category = "Standard" };
        public static readonly PermissionRecord ManageEmployeePerfomance = new() { Name = "Admin area. HRM - Manage Employee Perfomance", SystemName = "ManageEmployeePerfomance", Category = "Standard" };
        public static readonly PermissionRecord ManageAddRating = new() { Name = "Admin area. HRM - Manage Add Rating", SystemName = "ManageAddRating", Category = "Standard" };
        public static readonly PermissionRecord ManageMonthlyReview = new() { Name = "Admin area. HRM - Manage Monthly Review", SystemName = "ManageMonthlyReview", Category = "Standard" };
        public static readonly PermissionRecord ManageYearlyReview = new() { Name = "Admin area. HRM - Manage Yearly Review", SystemName = "ManageYearlyReview", Category = "Standard" };
        public static readonly PermissionRecord ManageProjectLeaderReview = new() { Name = "Admin area. HRM - Manage Project Leader Review", SystemName = "ManageProjectLeaderReview", Category = "Standard" };
        public static readonly PermissionRecord ManageKPIMaster = new() { Name = "Admin area. HRM - Manage KPIMaster", SystemName = "ManageKPIMaster", Category = "Standard" };
        public static readonly PermissionRecord ManageKPIWeightage = new() { Name = "Admin area. HRM - Manage KPIWeightage", SystemName = "ManageKPIWeightage", Category = "Standard" };
        public static readonly PermissionRecord ManageJobPosting = new() { Name = "Admin area. HRM - Manage JobPosting", SystemName = "ManageJobPosting", Category = "Standard" };
        public static readonly PermissionRecord ManageQuestion = new() { Name = "Admin area. HRM - Manage Question", SystemName = "ManageQuestion", Category = "Standard" };
        public static readonly PermissionRecord ManageCandiatesResumes = new() { Name = "Admin area. HRM - Manage CandiatesResumes", SystemName = "ManageCandiatesResumes", Category = "Standard" };
        public static readonly PermissionRecord ManageWeeklyQuestions = new() { Name = "Admin area. HRM - Manage WeeklyQuestions", SystemName = "ManageWeeklyQuestions", Category = "Standard" };
        public static readonly PermissionRecord ManageWeeklyReports = new() { Name = "Admin area. HRM - Manage WeeklyReports", SystemName = "ManageWeeklyReports", Category = "Standard" };

        //Public Screens
        public static readonly PermissionRecord PublicStoreEmployeeInfo = new() { Name = "Public store. Employee Info", SystemName = "EmployeeInfo", Category = "PublicStore" };
        public static readonly PermissionRecord PublicStoreEmployeeAddresses = new() { Name = "Public store. Employee Addresses", SystemName = "EmployeeAddresses", Category = "PublicStore" };
        public static readonly PermissionRecord PublicStoreEmployeeEducations = new() { Name = "Public store. Employee Educations", SystemName = "EmployeeEducations", Category = "PublicStore" };
        public static readonly PermissionRecord PublicStoreEmployeeExperiences = new() { Name = "Public store. Employee Experiences", SystemName = "EmployeeExperiences", Category = "PublicStore" };
        public static readonly PermissionRecord PublicStoreEmployeeAssets = new() { Name = "Public store. Employee Assets", SystemName = "EmployeeAssets", Category = "PublicStore" };


        /// <summary>
        /// Get permissions
        /// </summary>
        /// <returns>Permissions</returns>
        public virtual IEnumerable<PermissionRecord> GetPermissions()
        {
            return new[]
            {
                AccessAdminPanel,
                AllowCustomerImpersonation,
                ManageCustomers,
                ManageAffiliates,
                ManageCampaigns,
                ManageNewsletterSubscribers,
                ManagePolls,
                ManageNews,
                ManageBlog,
                ManageWidgets,
                ManageTopics,
                ManageForums,
                ManageMessageTemplates,
                ManageCountries,
                ManageLanguages,
                ManageSettings,
                ManageExternalAuthenticationMethods,
                ManageMultifactorAuthenticationMethods,
                ManageCurrencies,
                ManageActivityLog,
                ManageAcl,
                ManageEmailAccounts,
                ManageStores,
                ManagePlugins,
                ManageSystemLog,
                ManageMessageQueue,
                ManageMaintenance,
                HtmlEditorManagePictures,
                ManageScheduleTasks,
                ManageAppSettings,
                DisplayPrices,
                PublicStoreAllowNavigation,
                AccessClosedStore,
                AccessProfiling,
                PublicStoreEmployeeInfo,
                PublicStoreEmployeeAddresses,
                PublicStoreEmployeeEducations,
                PublicStoreEmployeeExperiences,
                PublicStoreEmployeeAssets,
                EnableMultiFactorAuthentication,
                ManageDesignation,
                ManageLeaveType,
                ManageLeaveManagement,
                ManageLeaveTransaction,
                ManageLeaveType,
                ManageProject,
                ManageProjectEmployeeMapping,
                ManageProjectTaskCategoryMapping,
                ManageProjectTask,
                ManageProjectTaskChangeLog,
                ManageProjectTaskComments,
                ManageKPIMaster,
                ManageKPIWeightage,
                ManageTeamPerformanceMeasurement,
                ManageTimeSheet,
                ManageEmployeeAttendance,
                ManageAttendanceReport,
                ManageHoliday,
                ManageEmployee,
                ManageEducation,
                ManageExperience,
                ManageAssets,
                ManageDepartment,
                ManageEmployeeAddress,
                ManageEmployeeCurrentActivity,
                ManageEmployeeActivities,
                ManageJobPosting,
                ManageCandiatesResumes,
                ManageQuestion,
                ManageWeeklyQuestions,
                ManageWeeklyReports,
                ManageProjectIntegration,
                ManageProjectIntegrationMappings,
                ManageProjectIntegrationSettings,
                ManageAlertConfiguration,
                ManageAlertReason,
                ManageAlertReport,
                ManageTaskCategory,
                ManageTaskCategoryChecklists,
                ManageChecklist,
                ManageActivity,
                ManageProcessWorkflow,
                ManageWorkflowStatus,
                ManageProcessRules,
                ManageEmployeeReport,
                ManageProjectReport,
                ManageTaskReport,
                ManageTimeSummmaryReport,
                ManagePerformanceSummary,
                ManageEmployeePerfomance,
                ManageAddRating,
                ManageMonthlyReview,
                ManageYearlyReview,
                ManageProjectLeaderReview
            };
        }

        /// <summary>
        /// Get default permissions
        /// </summary>
        /// <returns>Permissions</returns>
        public virtual HashSet<(string systemRoleName, PermissionRecord[] permissions)> GetDefaultPermissions()
        {
            return new HashSet<(string, PermissionRecord[])>
            {
                (
                    NopCustomerDefaults.AdministratorsRoleName,
                    new[]
                    {
                        AccessAdminPanel,
                        AllowCustomerImpersonation,
                        ManageCustomers,
                        ManageAffiliates,
                        ManageCampaigns,
                        ManageNewsletterSubscribers,
                        ManagePolls,
                        ManageNews,
                        ManageBlog,
                        ManageWidgets,
                        ManageTopics,
                        ManageForums,
                        ManageMessageTemplates,
                        ManageCountries,
                        ManageLanguages,
                        ManageSettings,
                        ManageExternalAuthenticationMethods,
                        ManageMultifactorAuthenticationMethods,
                        ManageCurrencies,
                        ManageActivityLog,
                        ManageAcl,
                        ManageEmailAccounts,
                        ManageStores,
                        ManagePlugins,
                        ManageSystemLog,
                        ManageMessageQueue,
                        ManageMaintenance,
                        HtmlEditorManagePictures,
                        ManageScheduleTasks,
                        ManageAppSettings,
                        DisplayPrices,
                        PublicStoreAllowNavigation,
                        PublicStoreEmployeeInfo,
                        PublicStoreEmployeeAddresses,
                        PublicStoreEmployeeEducations,
                        PublicStoreEmployeeExperiences,
                        PublicStoreEmployeeAssets,
                        AccessClosedStore,
                        AccessProfiling,
                        EnableMultiFactorAuthentication,
                        ManageTaskCategory,
                        ManageTaskCategoryChecklists,
                        ManageChecklist,
                        ManageActivity,
                        ManageProcessWorkflow,
                        ManageWorkflowStatus,
                        ManageProcessRules,
                        ManageEmployeeReport,
                        ManageProjectReport,
                        ManageTaskReport,
                        ManageTimeSummmaryReport,
                        ManagePerformanceSummary,
                        ManageEmployeePerfomance,
                        ManageAddRating,
                        ManageMonthlyReview,
                        ManageYearlyReview,
                        ManageProjectLeaderReview
                    }
                ),
                (
                    NopCustomerDefaults.ForumModeratorsRoleName,
                    new[]
                    {
                        DisplayPrices,
                        PublicStoreAllowNavigation
                    }
                ),
                (
                    NopCustomerDefaults.GuestsRoleName,
                    new[]
                    {
                        DisplayPrices,
                        PublicStoreAllowNavigation
                    }
                ),
                (
                    NopCustomerDefaults.RegisteredRoleName,
                    new[]
                    {
                        DisplayPrices,
                        PublicStoreAllowNavigation,
                        EnableMultiFactorAuthentication
                    }
                ),
                (
                    NopCustomerDefaults.VendorsRoleName,
                    new[]
                    {
                        AccessAdminPanel
                    }
                )
            };
        }
    }
}