using App.Core.Configuration;
using App.Core.Domain.Activities;
using App.Core.Domain.Affiliates;
using App.Core.Domain.Blogs;
using App.Core.Domain.Catalog;
using App.Core.Domain.Common;
using App.Core.Domain.Configuration;
using App.Core.Domain.Customers;
using App.Core.Domain.Directory;
using App.Core.Domain.EmployeeAttendances;
using App.Core.Domain.Employees;
using App.Core.Domain.Extension.Alerts;
using App.Core.Domain.Extension.EmployeeAttendanceSetting;
using App.Core.Domain.Extension.Employees;
using App.Core.Domain.Extension.Leaves;
using App.Core.Domain.Extension.PerformanceMeasurements;
using App.Core.Domain.Extension.ProjectTasks;
using App.Core.Domain.Extension.TimeSheets;
using App.Core.Domain.Extension.WeeklyQuestions;
using App.Core.Domain.Forums;
using App.Core.Domain.Gdpr;
using App.Core.Domain.Holidays;
using App.Core.Domain.JobPostings;
using App.Core.Domain.Leaves;
using App.Core.Domain.Localization;
using App.Core.Domain.Logging;
using App.Core.Domain.ManageResumes;
using App.Core.Domain.Media;
using App.Core.Domain.Messages;
using App.Core.Domain.News;
using App.Core.Domain.PerformanceMeasurements;
using App.Core.Domain.Polls;
using App.Core.Domain.ProjectEmployeeMappings;
using App.Core.Domain.Projects;
using App.Core.Domain.ProjectTasks;
using App.Core.Domain.result;
using App.Core.Domain.ScheduleTasks;
using App.Core.Domain.Security;
using App.Core.Domain.Seo;
using App.Core.Domain.Stores;
using App.Core.Domain.TimeSheets;
using App.Core.Domain.Topics;
using App.Core.Domain.WeeklyQuestion;
using App.Core.Infrastructure.Mapper;
using App.Data.Configuration;
using App.Services.Authentication.External;
using App.Services.Authentication.MultiFactor;
using App.Services.Cms;
using App.Services.Plugins;
using App.Web.Areas.Admin.InterviewQeations.Models;
using App.Web.Areas.Admin.Models.Affiliates;
using App.Web.Areas.Admin.Models.Blogs;
using App.Web.Areas.Admin.Models.CheckListMappings;
using App.Web.Areas.Admin.Models.Cms;
using App.Web.Areas.Admin.Models.Common;
using App.Web.Areas.Admin.Models.Customers;
using App.Web.Areas.Admin.Models.Directory;
using App.Web.Areas.Admin.Models.EmployeeAttendances;
using App.Web.Areas.Admin.Models.Employees;
using App.Web.Areas.Admin.Models.Extension.Activities;
using App.Web.Areas.Admin.Models.Extension.Announcements;
using App.Web.Areas.Admin.Models.Extension.CheckLists;
using App.Web.Areas.Admin.Models.Extension.MonthlyPerformanceReports;
using App.Web.Areas.Admin.Models.Extension.ProcessRules;
using App.Web.Areas.Admin.Models.Extension.ProcessWorkflows;
using App.Web.Areas.Admin.Models.Extension.ProjectTasks;
using App.Web.Areas.Admin.Models.Extension.TaskCategories;
using App.Web.Areas.Admin.Models.Extension.TaskChangeLogs;
using App.Web.Areas.Admin.Models.Extension.TaskComments;
using App.Web.Areas.Admin.Models.Extension.TimesheetReports;
using App.Web.Areas.Admin.Models.Extension.WorkflowStatus;
using App.Web.Areas.Admin.Models.ExternalAuthentication;
using App.Web.Areas.Admin.Models.Forums;
using App.Web.Areas.Admin.Models.Holidays;
using App.Web.Areas.Admin.Models.JobPostings;
using App.Web.Areas.Admin.Models.LeaveManagement;
using App.Web.Areas.Admin.Models.Leavetypes;
using App.Web.Areas.Admin.Models.Localization;
using App.Web.Areas.Admin.Models.Logging;
using App.Web.Areas.Admin.Models.ManageResumes;
using App.Web.Areas.Admin.Models.Messages;
using App.Web.Areas.Admin.Models.MultiFactorAuthentication;
using App.Web.Areas.Admin.Models.News;
using App.Web.Areas.Admin.Models.PerformanceMeasurements;
using App.Web.Areas.Admin.Models.Plugins;
using App.Web.Areas.Admin.Models.Polls;
using App.Web.Areas.Admin.Models.ProjectEmployeeMappings;
using App.Web.Areas.Admin.Models.Projects;
using App.Web.Areas.Admin.Models.ProjectTaskCategoryMappings;
using App.Web.Areas.Admin.Models.Settings;
using App.Web.Areas.Admin.Models.Stores;
using App.Web.Areas.Admin.Models.Tasks;
using App.Web.Areas.Admin.Models.Templates;
using App.Web.Areas.Admin.Models.TimeSheets;
using App.Web.Areas.Admin.Models.Topics;
using App.Web.Areas.Admin.Models.WeeklyQuestions;
using App.Web.Areas.Admin.Models.WeeklyReports;
using App.Web.Framework.Models;
using App.Web.Framework.WebOptimizer;
using AutoMapper;
using AutoMapper.Internal;
using Nop.Core.Domain.Catalog;
using Satyanam.Nop.Core.Domains;

namespace App.Web.Areas.Admin.Infrastructure.Mapper
{
    /// <summary>
    /// AutoMapper configuration for admin area models
    /// </summary>
    public partial class AdminMapperConfiguration : Profile, IOrderedMapperProfile
    {
        #region Ctor

        public AdminMapperConfiguration()
        {
            //create specific maps
            CreateConfigMaps();
            CreateAffiliatesMaps();
            CreateAuthenticationMaps();
            CreateMultiFactorAuthenticationMaps();
            CreateBlogsMaps();
            CreateCmsMaps();
            CreateCommonMaps();
            CreateCustomersMaps();
            CreateDirectoryMaps();
            CreateForumsMaps();
            CreateGdprMaps();
            CreateLocalizationMaps();
            CreateLoggingMaps();
            CreateMediaMaps();
            CreateMessagesMaps();
            CreateNewsMaps();
            CreatePluginsMaps();
            CreatePollsMaps();
            CreateSecurityMaps();
            CreateSeoMaps();
            CreateStoresMaps();
            CreateTasksMaps();
            CreateTopicsMaps();
            CreateLeaveTypeMaps();
            CreateLeaveTransactionLogMaps();
            CreateLeaveManagementMaps();
            CreateHolidayMaps();
            CreateProjectMaps();
            CreateProjectEmpMappingMaps();
            CreateTimeSheetMaps();
            CreateMonthlyPerformanceReportMaps();
            CreateKPIMasterMaps();
            CreateKPIWeightageMaps();
            CreateTeamPerformanceMeasurementMaps();
            CreateEmployeeAttendanceMaps();
            CreateEmployeeMaps();
            CreateEducationMaps();
            CreateExperienceMaps();
            CreateAssetsMaps();
            CreateAddressMaps();
            CreateJobPostingMaps();
            CreateCandiatesResumesMaps();
            CreateInterviewQeations();
            CreateIresult();
            CreateIWeeklyQuestions();
            CreateWeeklyreport();
            CreateProjectTaskMaps();
            CreateActivityMaps();
            CreateAnnouncementMaps();
            CreateTaskCategoryMaps();
            CreateChecklistMasterMaps();
            CreateProjectTaskCategoryMappingMaps();
            CreateCheckListMappingMaps();
            CreateTaskChangeLogMaps();
            CreateTimeSheetReportMaps();
            CreateTaskCommentsMaps();
            CreateProcessWorkflowMaps();
            CreateProcessRulesMaps();

            CreateWorkflowStatusMaps();

            //add some generic mapping rules
            this.Internal().ForAllMaps((mapConfiguration, map) =>
            {
                //exclude Form and CustomProperties from mapping BaseNopModel
                if (typeof(BaseNopModel).IsAssignableFrom(mapConfiguration.DestinationType))
                {
                    //map.ForMember(nameof(BaseNopModel.Form), options => options.Ignore());
                    map.ForMember(nameof(BaseNopModel.CustomProperties), options => options.Ignore());
                }

                //exclude ActiveStoreScopeConfiguration from mapping ISettingsModel
                if (typeof(ISettingsModel).IsAssignableFrom(mapConfiguration.DestinationType))
                    map.ForMember(nameof(ISettingsModel.ActiveStoreScopeConfiguration), options => options.Ignore());

                //exclude some properties from mapping configuration and models
                if (typeof(IConfig).IsAssignableFrom(mapConfiguration.DestinationType))
                    map.ForMember(nameof(IConfig.Name), options => options.Ignore());

                //exclude Locales from mapping ILocalizedModel
                if (typeof(ILocalizedModel).IsAssignableFrom(mapConfiguration.DestinationType))
                    map.ForMember(nameof(ILocalizedModel<ILocalizedModel>.Locales), options => options.Ignore());

                //exclude some properties from mapping store mapping supported entities and models
                if (typeof(IStoreMappingSupported).IsAssignableFrom(mapConfiguration.DestinationType))
                    map.ForMember(nameof(IStoreMappingSupported.LimitedToStores), options => options.Ignore());
                if (typeof(IStoreMappingSupportedModel).IsAssignableFrom(mapConfiguration.DestinationType))
                {
                    map.ForMember(nameof(IStoreMappingSupportedModel.AvailableStores), options => options.Ignore());
                    map.ForMember(nameof(IStoreMappingSupportedModel.SelectedStoreIds), options => options.Ignore());
                }

                //exclude some properties from mapping ACL supported entities and models
                if (typeof(IAclSupported).IsAssignableFrom(mapConfiguration.DestinationType))
                    map.ForMember(nameof(IAclSupported.SubjectToAcl), options => options.Ignore());
                if (typeof(IAclSupportedModel).IsAssignableFrom(mapConfiguration.DestinationType))
                {
                    map.ForMember(nameof(IAclSupportedModel.AvailableCustomerRoles), options => options.Ignore());
                    map.ForMember(nameof(IAclSupportedModel.SelectedCustomerRoleIds), options => options.Ignore());
                }

                //exclude some properties from mapping discount supported entities and models
                if (typeof(IDiscountSupportedModel).IsAssignableFrom(mapConfiguration.DestinationType))
                {
                    map.ForMember(nameof(IDiscountSupportedModel.AvailableDiscounts), options => options.Ignore());
                    map.ForMember(nameof(IDiscountSupportedModel.SelectedDiscountIds), options => options.Ignore());
                }

                if (typeof(IPluginModel).IsAssignableFrom(mapConfiguration.DestinationType))
                {
                    //exclude some properties from mapping plugin models
                    map.ForMember(nameof(IPluginModel.ConfigurationUrl), options => options.Ignore());
                    map.ForMember(nameof(IPluginModel.IsActive), options => options.Ignore());
                    map.ForMember(nameof(IPluginModel.LogoUrl), options => options.Ignore());

                    //define specific rules for mapping plugin models
                    if (typeof(IPlugin).IsAssignableFrom(mapConfiguration.SourceType))
                    {
                        map.ForMember(nameof(IPluginModel.DisplayOrder), options => options.MapFrom(plugin => ((IPlugin)plugin).PluginDescriptor.DisplayOrder));
                        map.ForMember(nameof(IPluginModel.FriendlyName), options => options.MapFrom(plugin => ((IPlugin)plugin).PluginDescriptor.FriendlyName));
                        map.ForMember(nameof(IPluginModel.SystemName), options => options.MapFrom(plugin => ((IPlugin)plugin).PluginDescriptor.SystemName));
                    }
                }
            });
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Create configuration maps 
        /// </summary>
        protected virtual void CreateConfigMaps()
        {
            CreateMap<CacheConfig, CacheConfigModel>();
            CreateMap<CacheConfigModel, CacheConfig>();

            CreateMap<HostingConfig, HostingConfigModel>();
            CreateMap<HostingConfigModel, HostingConfig>();

            CreateMap<DistributedCacheConfig, DistributedCacheConfigModel>()
                .ForMember(model => model.DistributedCacheTypeValues, options => options.Ignore());
            CreateMap<DistributedCacheConfigModel, DistributedCacheConfig>();

            CreateMap<AzureBlobConfig, AzureBlobConfigModel>();
            CreateMap<AzureBlobConfigModel, AzureBlobConfig>()
                .ForMember(entity => entity.Enabled, options => options.Ignore())
                .ForMember(entity => entity.DataProtectionKeysEncryptWithVault, options => options.Ignore());

            CreateMap<InstallationConfig, InstallationConfigModel>();
            CreateMap<InstallationConfigModel, InstallationConfig>();

            CreateMap<PluginConfig, PluginConfigModel>();
            CreateMap<PluginConfigModel, PluginConfig>();

            CreateMap<CommonConfig, CommonConfigModel>();
            CreateMap<CommonConfigModel, CommonConfig>();

            CreateMap<DataConfig, DataConfigModel>()
                .ForMember(model => model.DataProviderTypeValues, options => options.Ignore());
            CreateMap<DataConfigModel, DataConfig>();

            CreateMap<WebOptimizerConfig, WebOptimizerConfigModel>();
            CreateMap<WebOptimizerConfigModel, WebOptimizerConfig>()
                .ForMember(entity => entity.CdnUrl, options => options.Ignore())
                .ForMember(entity => entity.AllowEmptyBundle, options => options.Ignore())
                .ForMember(entity => entity.HttpsCompression, options => options.Ignore())
                .ForMember(entity => entity.EnableTagHelperBundling, options => options.Ignore())
                .ForMember(entity => entity.EnableCaching, options => options.Ignore())
                .ForMember(entity => entity.EnableMemoryCache, options => options.Ignore());
        }

        /// <summary>
        /// Create affiliates maps 
        /// </summary>
        protected virtual void CreateAffiliatesMaps()
        {
            CreateMap<Affiliate, AffiliateModel>()
                .ForMember(model => model.Address, options => options.Ignore())
                .ForMember(model => model.AffiliatedCustomerSearchModel, options => options.Ignore())
                .ForMember(model => model.Url, options => options.Ignore());

            CreateMap<AffiliateModel, Affiliate>()
                .ForMember(entity => entity.Deleted, options => options.Ignore());

            CreateMap<Customer, AffiliatedCustomerModel>()
                .ForMember(model => model.Name, options => options.Ignore());

        }

        /// <summary>
        /// Create authentication maps 
        /// </summary>
        protected virtual void CreateAuthenticationMaps()
        {
            CreateMap<IExternalAuthenticationMethod, ExternalAuthenticationMethodModel>();
        }

        /// <summary>
        /// Create multi-factor authentication maps 
        /// </summary>
        protected virtual void CreateMultiFactorAuthenticationMaps()
        {
            CreateMap<IMultiFactorAuthenticationMethod, MultiFactorAuthenticationMethodModel>();
        }

        /// <summary>
        /// Create blogs maps 
        /// </summary>new
        protected virtual void CreateBlogsMaps()
        {
            CreateMap<BlogComment, BlogCommentModel>()
                .ForMember(model => model.BlogPostTitle, options => options.Ignore())
                .ForMember(model => model.Comment, options => options.Ignore())
                .ForMember(model => model.CreatedOn, options => options.Ignore())
                .ForMember(model => model.CustomerInfo, options => options.Ignore())
                .ForMember(model => model.StoreName, options => options.Ignore());

            CreateMap<BlogCommentModel, BlogComment>()
                .ForMember(entity => entity.CommentText, options => options.Ignore())
                .ForMember(entity => entity.CreatedOnUtc, options => options.Ignore())
                .ForMember(entity => entity.BlogPostId, options => options.Ignore())
                .ForMember(entity => entity.CustomerId, options => options.Ignore())
                .ForMember(entity => entity.StoreId, options => options.Ignore());

            CreateMap<BlogPost, BlogPostModel>()
                .ForMember(model => model.ApprovedComments, options => options.Ignore())
                .ForMember(model => model.AvailableLanguages, options => options.Ignore())
                .ForMember(model => model.CreatedOn, options => options.Ignore())
                .ForMember(model => model.LanguageName, options => options.Ignore())
                .ForMember(model => model.NotApprovedComments, options => options.Ignore())
                .ForMember(model => model.SeName, options => options.Ignore())
                .ForMember(model => model.InitialBlogTags, options => options.Ignore());
            CreateMap<BlogPostModel, BlogPost>()
                .ForMember(entity => entity.CreatedOnUtc, options => options.Ignore());

            CreateMap<BlogSettings, BlogSettingsModel>()
                .ForMember(model => model.AllowNotRegisteredUsersToLeaveComments_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.BlogCommentsMustBeApproved_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.Enabled_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.NotifyAboutNewBlogComments_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.NumberOfTags_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.PostsPageSize_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.ShowHeaderRssUrl_OverrideForStore, options => options.Ignore());
            CreateMap<BlogSettingsModel, BlogSettings>();
        }

        /// <summary>
        /// Create catalog maps 
        /// </summary>
        protected virtual void CreateCatalogMaps()
        {
            CreateMap<CatalogSettings, CatalogSettingsModel>()
                .ForMember(model => model.AllowAnonymousUsersToEmailAFriend_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.AllowAnonymousUsersToReviewProduct_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.AllowProductSorting_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.AllowProductViewModeChanging_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.AllowViewUnpublishedProductPage_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.AvailableViewModes, options => options.Ignore())
                .ForMember(model => model.CategoryBreadcrumbEnabled_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.CompareProductsEnabled_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.DefaultViewMode_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.DisplayDiscontinuedMessageForUnpublishedProducts_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.DisplayTaxShippingInfoFooter_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.DisplayTaxShippingInfoOrderDetailsPage_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.DisplayTaxShippingInfoProductBoxes_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.DisplayTaxShippingInfoProductDetailsPage_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.DisplayTaxShippingInfoShoppingCart_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.DisplayTaxShippingInfoWishlist_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.EmailAFriendEnabled_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.ExportImportAllowDownloadImages_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.ExportImportCategoriesUsingCategoryName_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.ExportImportProductAttributes_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.ExportImportProductCategoryBreadcrumb_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.ExportImportProductSpecificationAttributes_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.ExportImportRelatedEntitiesByName_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.ExportImportProductUseLimitedToStores_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.ExportImportSplitProductsFile_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.IncludeFullDescriptionInCompareProducts_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.IncludeShortDescriptionInCompareProducts_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.ManufacturersBlockItemsToDisplay_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.NewProductsEnabled_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.NewProductsPageSize_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.NewProductsAllowCustomersToSelectPageSize_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.NewProductsPageSizeOptions_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.NotifyCustomerAboutProductReviewReply_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.NotifyStoreOwnerAboutNewProductReviews_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.NumberOfBestsellersOnHomepage_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.NumberOfProductTags_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.PageShareCode_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.ProductReviewPossibleOnlyAfterPurchasing_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.ProductReviewsMustBeApproved_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.OneReviewPerProductFromCustomer_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.ProductReviewsPageSizeOnAccountPage_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.ProductReviewsSortByCreatedDateAscending_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.ProductsAlsoPurchasedEnabled_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.ProductsAlsoPurchasedNumber_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.ProductsByTagAllowCustomersToSelectPageSize_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.ProductsByTagPageSizeOptions_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.ProductsByTagPageSize_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.ProductSearchAutoCompleteEnabled_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.ProductSearchEnabled_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.ProductSearchAutoCompleteNumberOfProducts_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.ProductSearchTermMinimumLength_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.RecentlyViewedProductsEnabled_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.RecentlyViewedProductsNumber_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.RemoveRequiredProducts_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.SearchPageAllowCustomersToSelectPageSize_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.SearchPagePageSizeOptions_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.SearchPageProductsPerPage_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.ShowBestsellersOnHomepage_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.ShowCategoryProductNumberIncludingSubcategories_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.ShowCategoryProductNumber_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.ShowFreeShippingNotification_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.ShowShortDescriptionOnCatalogPages_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.ShowGtin_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.ShowLinkToAllResultInSearchAutoComplete_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.ShowManufacturerPartNumber_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.ShowProductImagesInSearchAutoComplete_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.ShowProductReviewsOnAccountPage_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.ShowProductReviewsPerStore_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.ShowProductsFromSubcategories_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.ShowShareButton_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.ShowSkuOnCatalogPages_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.ShowSkuOnProductDetailsPage_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.DisplayDatePreOrderAvailability_OverrideForStore, mo => mo.Ignore())
                .ForMember(model => model.UseAjaxCatalogProductsLoading_OverrideForStore, mo => mo.Ignore())
                .ForMember(model => model.SearchPagePriceRangeFiltering_OverrideForStore, mo => mo.Ignore())
                .ForMember(model => model.SearchPagePriceFrom_OverrideForStore, mo => mo.Ignore())
                .ForMember(model => model.SearchPagePriceTo_OverrideForStore, mo => mo.Ignore())
                .ForMember(model => model.SearchPageManuallyPriceRange_OverrideForStore, mo => mo.Ignore())
                .ForMember(model => model.ProductsByTagPriceRangeFiltering_OverrideForStore, mo => mo.Ignore())
                .ForMember(model => model.ProductsByTagPriceFrom_OverrideForStore, mo => mo.Ignore())
                .ForMember(model => model.ProductsByTagPriceTo_OverrideForStore, mo => mo.Ignore())
                .ForMember(model => model.ProductsByTagManuallyPriceRange_OverrideForStore, mo => mo.Ignore())
                .ForMember(model => model.EnableManufacturerFiltering_OverrideForStore, mo => mo.Ignore())
                .ForMember(model => model.EnablePriceRangeFiltering_OverrideForStore, mo => mo.Ignore())
                .ForMember(model => model.EnableSpecificationAttributeFiltering_OverrideForStore, mo => mo.Ignore())
                .ForMember(model => model.DisplayFromPrices_OverrideForStore, mo => mo.Ignore())
                .ForMember(model => model.AttributeValueOutOfStockDisplayTypes, mo => mo.Ignore())
                .ForMember(model => model.AttributeValueOutOfStockDisplayType_OverrideForStore, mo => mo.Ignore())
                .ForMember(model => model.SortOptionSearchModel, options => options.Ignore())
                .ForMember(model => model.PrimaryStoreCurrencyCode, options => options.Ignore())
                .ForMember(model => model.AllowCustomersToSearchWithManufacturerName_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.AllowCustomersToSearchWithCategoryName_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.DisplayAllPicturesOnCatalogPages_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.ProductUrlStructureTypeId_OverrideForStore, mo => mo.Ignore())
                .ForMember(model => model.ProductUrlStructureTypes, mo => mo.Ignore());
            CreateMap<CatalogSettingsModel, CatalogSettings>()
                .ForMember(settings => settings.AjaxProcessAttributeChange, options => options.Ignore())
                .ForMember(settings => settings.CompareProductsNumber, options => options.Ignore())
                .ForMember(settings => settings.CountDisplayedYearsDatePicker, options => options.Ignore())
                .ForMember(settings => settings.DefaultCategoryPageSize, options => options.Ignore())
                .ForMember(settings => settings.DefaultCategoryPageSizeOptions, options => options.Ignore())
                .ForMember(settings => settings.DefaultManufacturerPageSize, options => options.Ignore())
                .ForMember(settings => settings.DefaultManufacturerPageSizeOptions, options => options.Ignore())
                .ForMember(settings => settings.DefaultProductRatingValue, options => options.Ignore())
                .ForMember(settings => settings.DisplayTierPricesWithDiscounts, options => options.Ignore())
                .ForMember(settings => settings.ExportImportProductsCountInOneFile, options => options.Ignore())
                .ForMember(settings => settings.ExportImportUseDropdownlistsForAssociatedEntities, options => options.Ignore())
                .ForMember(settings => settings.IncludeFeaturedProductsInNormalLists, options => options.Ignore())
                .ForMember(settings => settings.MaximumBackInStockSubscriptions, options => options.Ignore())
                .ForMember(settings => settings.ProductSortingEnumDisabled, options => options.Ignore())
                .ForMember(settings => settings.ProductSortingEnumDisplayOrder, options => options.Ignore())
                .ForMember(settings => settings.PublishBackProductWhenCancellingOrders, options => options.Ignore())
                .ForMember(settings => settings.UseAjaxLoadMenu, options => options.Ignore())
                .ForMember(settings => settings.UseLinksInRequiredProductWarnings, options => options.Ignore())
                .ForMember(settings => settings.ActiveSearchProviderSystemName, options => options.Ignore());
              }

        /// <summary>
        /// Create CMS maps 
        /// </summary>
        protected virtual void CreateCmsMaps()
        {
            CreateMap<IWidgetPlugin, WidgetModel>()
                .ForMember(model => model.WidgetViewComponentArguments, options => options.Ignore())
                .ForMember(model => model.WidgetViewComponentName, options => options.Ignore());
        }

        /// <summary>
        /// Create common maps 
        /// </summary>
        protected virtual void CreateCommonMaps()
        {
            CreateMap<Core.Domain.Common.Address, Models.Common.AddressModel>()
                .ForMember(model => model.AddressHtml, options => options.Ignore())
                .ForMember(model => model.AvailableCountries, options => options.Ignore())
                .ForMember(model => model.AvailableStates, options => options.Ignore())
                .ForMember(model => model.CountryName, options => options.Ignore())
                .ForMember(model => model.CustomAddressAttributes, options => options.Ignore())
                .ForMember(model => model.FormattedCustomAddressAttributes, options => options.Ignore())
                .ForMember(model => model.StateProvinceName, options => options.Ignore())
                .ForMember(model => model.CityRequired, options => options.Ignore())
                .ForMember(model => model.CompanyRequired, options => options.Ignore())
                .ForMember(model => model.CountryRequired, options => options.Ignore())
                .ForMember(model => model.CountyRequired, options => options.Ignore())
                .ForMember(model => model.EmailRequired, options => options.Ignore())
                .ForMember(model => model.FaxRequired, options => options.Ignore())
                .ForMember(model => model.FirstNameRequired, options => options.Ignore())
                .ForMember(model => model.LastNameRequired, options => options.Ignore())
                .ForMember(model => model.PhoneRequired, options => options.Ignore())
                .ForMember(model => model.StateProvinceName, options => options.Ignore())
                .ForMember(model => model.StreetAddress2Required, options => options.Ignore())
                .ForMember(model => model.StreetAddressRequired, options => options.Ignore())
                .ForMember(model => model.ZipPostalCodeRequired, options => options.Ignore());
            CreateMap<Models.Common.AddressModel, Core.Domain.Common.Address>()
                .ForMember(entity => entity.CreatedOnUtc, options => options.Ignore())
                .ForMember(entity => entity.CustomAttributes, options => options.Ignore());

            CreateMap<AddressAttribute, AddressAttributeModel>()
                .ForMember(model => model.AddressAttributeValueSearchModel, options => options.Ignore())
                .ForMember(model => model.AttributeControlTypeName, options => options.Ignore());
            CreateMap<AddressAttributeModel, AddressAttribute>()
                .ForMember(entity => entity.AttributeControlType, options => options.Ignore());

            CreateMap<AddressAttributeValue, AddressAttributeValueModel>();
            CreateMap<AddressAttributeValueModel, AddressAttributeValue>();

            CreateMap<AddressSettings, AddressSettingsModel>();
            CreateMap<AddressSettingsModel, AddressSettings>()
                .ForMember(settings => settings.PreselectCountryIfOnlyOne, options => options.Ignore());

            CreateMap<Setting, SettingModel>()
                .ForMember(setting => setting.AvailableStores, options => options.Ignore())
                .ForMember(setting => setting.Store, options => options.Ignore());
        }

        /// <summary>
        /// Create customers maps 
        /// </summary>
        protected virtual void CreateCustomersMaps()
        {
            CreateMap<CustomerAttribute, CustomerAttributeModel>()
                .ForMember(model => model.AttributeControlTypeName, options => options.Ignore())
                .ForMember(model => model.CustomerAttributeValueSearchModel, options => options.Ignore());
            CreateMap<CustomerAttributeModel, CustomerAttribute>()
                .ForMember(entity => entity.AttributeControlType, options => options.Ignore());

            CreateMap<CustomerAttributeValue, CustomerAttributeValueModel>();
            CreateMap<CustomerAttributeValueModel, CustomerAttributeValue>();

            CreateMap<CustomerRole, CustomerRoleModel>()
                .ForMember(model => model.TaxDisplayTypeValues, options => options.Ignore());
            CreateMap<CustomerRoleModel, CustomerRole>();

            CreateMap<CustomerSettings, CustomerSettingsModel>();
            CreateMap<CustomerSettingsModel, CustomerSettings>()
                .ForMember(settings => settings.AvatarMaximumSizeBytes, options => options.Ignore())
                .ForMember(settings => settings.DeleteGuestTaskOlderThanMinutes, options => options.Ignore())
                .ForMember(settings => settings.DownloadableProductsValidateUser, options => options.Ignore())
                .ForMember(settings => settings.HashedPasswordFormat, options => options.Ignore())
                .ForMember(settings => settings.OnlineCustomerMinutes, options => options.Ignore())
                .ForMember(settings => settings.SuffixDeletedCustomers, options => options.Ignore())
                .ForMember(settings => settings.LastActivityMinutes, options => options.Ignore());

            CreateMap<MultiFactorAuthenticationSettings, MultiFactorAuthenticationSettingsModel>();
            CreateMap<MultiFactorAuthenticationSettingsModel, MultiFactorAuthenticationSettings>()
                .ForMember(settings => settings.ActiveAuthenticationMethodSystemNames, option => option.Ignore());

            CreateMap<RewardPointsSettings, RewardPointsSettingsModel>()
                .ForMember(model => model.ActivatePointsImmediately, options => options.Ignore())
                .ForMember(model => model.ActivationDelay_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.DisplayHowMuchWillBeEarned_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.Enabled_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.ExchangeRate_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.MaximumRewardPointsToUsePerOrder_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.MinimumRewardPointsToUse_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.MinOrderTotalToAwardPoints_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.PageSize_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.PointsForPurchases_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.MaximumRedeemedRate_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.PointsForRegistration_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.PrimaryStoreCurrencyCode, options => options.Ignore())
                .ForMember(model => model.PurchasesPointsValidity_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.RegistrationPointsValidity_OverrideForStore, options => options.Ignore());
            CreateMap<RewardPointsSettingsModel, RewardPointsSettings>();

            CreateMap<ActivityLog, CustomerActivityLogModel>()
               .ForMember(model => model.CreatedOn, options => options.Ignore())
               .ForMember(model => model.ActivityLogTypeName, options => options.Ignore());

            CreateMap<Customer, CustomerModel>()
                .ForMember(model => model.Email, options => options.Ignore())
                .ForMember(model => model.FullName, options => options.Ignore())
                .ForMember(model => model.Company, options => options.Ignore())
                .ForMember(model => model.Phone, options => options.Ignore())
                .ForMember(model => model.ZipPostalCode, options => options.Ignore())
                .ForMember(model => model.CreatedOn, options => options.Ignore())
                .ForMember(model => model.LastActivityDate, options => options.Ignore())
                .ForMember(model => model.CustomerRoleNames, options => options.Ignore())
                .ForMember(model => model.AvatarUrl, options => options.Ignore())
                .ForMember(model => model.UsernamesEnabled, options => options.Ignore())
                .ForMember(model => model.Password, options => options.Ignore())
                .ForMember(model => model.GenderEnabled, options => options.Ignore())
                .ForMember(model => model.Gender, options => options.Ignore())
                .ForMember(model => model.FirstNameEnabled, options => options.Ignore())
                .ForMember(model => model.FirstName, options => options.Ignore())
                .ForMember(model => model.LastNameEnabled, options => options.Ignore())
                .ForMember(model => model.LastName, options => options.Ignore())
                .ForMember(model => model.DateOfBirthEnabled, options => options.Ignore())
                .ForMember(model => model.DateOfBirth, options => options.Ignore())
                .ForMember(model => model.CompanyEnabled, options => options.Ignore())
                .ForMember(model => model.StreetAddressEnabled, options => options.Ignore())
                .ForMember(model => model.StreetAddress, options => options.Ignore())
                .ForMember(model => model.StreetAddress2Enabled, options => options.Ignore())
                .ForMember(model => model.StreetAddress2, options => options.Ignore())
                .ForMember(model => model.ZipPostalCodeEnabled, options => options.Ignore())
                .ForMember(model => model.CityEnabled, options => options.Ignore())
                .ForMember(model => model.City, options => options.Ignore())
                .ForMember(model => model.CountyEnabled, options => options.Ignore())
                .ForMember(model => model.County, options => options.Ignore())
                .ForMember(model => model.CountryEnabled, options => options.Ignore())
                .ForMember(model => model.CountryId, options => options.Ignore())
                .ForMember(model => model.AvailableCountries, options => options.Ignore())
                .ForMember(model => model.StateProvinceEnabled, options => options.Ignore())
                .ForMember(model => model.StateProvinceId, options => options.Ignore())
                .ForMember(model => model.AvailableStates, options => options.Ignore())
                .ForMember(model => model.PhoneEnabled, options => options.Ignore())
                .ForMember(model => model.FaxEnabled, options => options.Ignore())
                .ForMember(model => model.Fax, options => options.Ignore())
                .ForMember(model => model.CustomerAttributes, options => options.Ignore())
                .ForMember(model => model.RegisteredInStore, options => options.Ignore())
                .ForMember(model => model.DisplayRegisteredInStore, options => options.Ignore())
                .ForMember(model => model.AffiliateName, options => options.Ignore())
                .ForMember(model => model.TimeZoneId, options => options.Ignore())
                .ForMember(model => model.AllowCustomersToSetTimeZone, options => options.Ignore())
                .ForMember(model => model.AvailableTimeZones, options => options.Ignore())
                .ForMember(model => model.VatNumber, options => options.Ignore())
                .ForMember(model => model.VatNumberStatusNote, options => options.Ignore())
                .ForMember(model => model.DisplayVatNumber, options => options.Ignore())
                .ForMember(model => model.LastVisitedPage, options => options.Ignore())
                .ForMember(model => model.AvailableNewsletterSubscriptionStores, options => options.Ignore())
                .ForMember(model => model.SelectedNewsletterSubscriptionStoreIds, options => options.Ignore())
                .ForMember(model => model.DisplayRewardPointsHistory, options => options.Ignore())
                .ForMember(model => model.SendEmail, options => options.Ignore())
                .ForMember(model => model.SendPm, options => options.Ignore())
                .ForMember(model => model.AllowSendingOfPrivateMessage, options => options.Ignore())
                .ForMember(model => model.AllowSendingOfWelcomeMessage, options => options.Ignore())
                .ForMember(model => model.AllowReSendingOfActivationMessage, options => options.Ignore())
                .ForMember(model => model.GdprEnabled, options => options.Ignore())
                .ForMember(model => model.MultiFactorAuthenticationProvider, options => options.Ignore())
                .ForMember(model => model.CustomerAssociatedExternalAuthRecordsSearchModel, options => options.Ignore())
                .ForMember(model => model.CustomerAddressSearchModel, options => options.Ignore())
                .ForMember(model => model.CustomerActivityLogSearchModel, options => options.Ignore());
            
            CreateMap<CustomerModel, Customer>()
                .ForMember(entity => entity.CustomerGuid, options => options.Ignore())
                .ForMember(entity => entity.CreatedOnUtc, options => options.Ignore())
                .ForMember(entity => entity.LastActivityDateUtc, options => options.Ignore())
                .ForMember(entity => entity.EmailToRevalidate, options => options.Ignore())
                .ForMember(entity => entity.RequireReLogin, options => options.Ignore())
                .ForMember(entity => entity.FailedLoginAttempts, options => options.Ignore())
                .ForMember(entity => entity.CannotLoginUntilDateUtc, options => options.Ignore())
                .ForMember(entity => entity.Deleted, options => options.Ignore())
                .ForMember(entity => entity.IsSystemAccount, options => options.Ignore())
                .ForMember(entity => entity.SystemName, options => options.Ignore())
                .ForMember(entity => entity.LastLoginDateUtc, options => options.Ignore())
                .ForMember(entity => entity.VatNumberStatusId, options => options.Ignore())
                .ForMember(entity => entity.CustomCustomerAttributesXML, options => options.Ignore())
                .ForMember(entity => entity.CurrencyId, options => options.Ignore())
                .ForMember(entity => entity.LanguageId, options => options.Ignore())
                .ForMember(entity => entity.TaxDisplayTypeId, options => options.Ignore())
                .ForMember(entity => entity.RegisteredInStoreId, options => options.Ignore());

            CreateMap<Customer, OnlineCustomerModel>()
                .ForMember(model => model.LastActivityDate, options => options.Ignore())
                .ForMember(model => model.CustomerInfo, options => options.Ignore())
                .ForMember(model => model.LastIpAddress, options => options.Ignore())
                .ForMember(model => model.Location, options => options.Ignore())
                .ForMember(model => model.LastVisitedPage, options => options.Ignore());
        }

        /// <summary>
        /// Create directory maps 
        /// </summary>
        protected virtual void CreateDirectoryMaps()
        {
            CreateMap<Country, CountryModel>()
                .ForMember(model => model.NumberOfStates, options => options.Ignore())
                .ForMember(model => model.StateProvinceSearchModel, options => options.Ignore());
            CreateMap<CountryModel, Country>();

            CreateMap<Currency, CurrencyModel>()
                .ForMember(model => model.CreatedOn, options => options.Ignore())
                .ForMember(model => model.IsPrimaryExchangeRateCurrency, options => options.Ignore())
                .ForMember(model => model.IsPrimaryStoreCurrency, options => options.Ignore());
            CreateMap<CurrencyModel, Currency>()
                .ForMember(entity => entity.CreatedOnUtc, options => options.Ignore())
                .ForMember(entity => entity.RoundingType, options => options.Ignore())
                .ForMember(entity => entity.UpdatedOnUtc, options => options.Ignore());

            CreateMap<MeasureDimension, MeasureDimensionModel>()
                .ForMember(model => model.IsPrimaryDimension, options => options.Ignore());
            CreateMap<MeasureDimensionModel, MeasureDimension>();

            CreateMap<MeasureWeight, MeasureWeightModel>()
                .ForMember(model => model.IsPrimaryWeight, options => options.Ignore());
            CreateMap<MeasureWeightModel, MeasureWeight>();

            CreateMap<StateProvince, StateProvinceModel>();
            CreateMap<StateProvinceModel, StateProvince>();
        }

        /// <summary>
        /// Create forums maps 
        /// </summary>
        protected virtual void CreateForumsMaps()
        {
            CreateMap<Forum, ForumModel>()
                .ForMember(model => model.CreatedOn, options => options.Ignore())
                .ForMember(model => model.ForumGroups, options => options.Ignore());
            CreateMap<ForumModel, Forum>()
                .ForMember(entity => entity.CreatedOnUtc, options => options.Ignore())
                .ForMember(entity => entity.LastPostCustomerId, options => options.Ignore())
                .ForMember(entity => entity.LastPostId, options => options.Ignore())
                .ForMember(entity => entity.LastPostTime, options => options.Ignore())
                .ForMember(entity => entity.LastTopicId, options => options.Ignore())
                .ForMember(entity => entity.NumPosts, options => options.Ignore())
                .ForMember(entity => entity.NumTopics, options => options.Ignore())
                .ForMember(entity => entity.UpdatedOnUtc, options => options.Ignore());

            CreateMap<ForumGroup, ForumGroupModel>()
                .ForMember(model => model.CreatedOn, options => options.Ignore());
            CreateMap<ForumGroupModel, ForumGroup>()
                .ForMember(entity => entity.CreatedOnUtc, options => options.Ignore())
                .ForMember(entity => entity.UpdatedOnUtc, options => options.Ignore());

            CreateMap<ForumSettings, ForumSettingsModel>()
                .ForMember(model => model.ActiveDiscussionsFeedCount_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.ActiveDiscussionsFeedEnabled_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.ActiveDiscussionsPageSize_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.AllowCustomersToDeletePosts_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.AllowCustomersToEditPosts_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.AllowCustomersToManageSubscriptions_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.AllowGuestsToCreatePosts_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.AllowGuestsToCreateTopics_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.AllowPostVoting_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.AllowPrivateMessages_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.ForumEditorValues, options => options.Ignore())
                .ForMember(model => model.ForumEditor_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.ForumFeedCount_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.ForumFeedsEnabled_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.ForumsEnabled_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.MaxVotesPerDay_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.NotifyAboutPrivateMessages_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.PostsPageSize_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.RelativeDateTimeFormattingEnabled_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.SearchResultsPageSize_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.ShowAlertForPM_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.ShowCustomersPostCount_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.SignaturesEnabled_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.TopicsPageSize_OverrideForStore, options => options.Ignore());
            CreateMap<ForumSettingsModel, ForumSettings>()
                .ForMember(settings => settings.ForumSearchTermMinimumLength, options => options.Ignore())
                .ForMember(settings => settings.ForumSubscriptionsPageSize, options => options.Ignore())
                .ForMember(settings => settings.HomepageActiveDiscussionsTopicCount, options => options.Ignore())
                .ForMember(settings => settings.LatestCustomerPostsPageSize, options => options.Ignore())
                .ForMember(settings => settings.PMSubjectMaxLength, options => options.Ignore())
                .ForMember(settings => settings.PMTextMaxLength, options => options.Ignore())
                .ForMember(settings => settings.PostMaxLength, options => options.Ignore())
                .ForMember(settings => settings.PrivateMessagesPageSize, options => options.Ignore())
                .ForMember(settings => settings.StrippedTopicMaxLength, options => options.Ignore())
                .ForMember(settings => settings.TopicSubjectMaxLength, options => options.Ignore());
        }

        /// <summary>
        /// Create GDPR maps 
        /// </summary>
        protected virtual void CreateGdprMaps()
        {
            CreateMap<GdprSettings, GdprSettingsModel>()
                .ForMember(model => model.GdprConsentSearchModel, options => options.Ignore())
                .ForMember(model => model.GdprEnabled_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.LogNewsletterConsent_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.LogPrivacyPolicyConsent_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.LogUserProfileChanges_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.DeleteInactiveCustomersAfterMonths_OverrideForStore, options => options.Ignore());
            CreateMap<GdprSettingsModel, GdprSettings>();

            CreateMap<GdprConsent, GdprConsentModel>();
            CreateMap<GdprConsentModel, GdprConsent>();

            CreateMap<GdprLog, GdprLogModel>()
                .ForMember(model => model.CustomerInfo, options => options.Ignore())
                .ForMember(model => model.RequestType, options => options.Ignore())
                .ForMember(model => model.CreatedOn, options => options.Ignore());
        }

        /// <summary>
        /// Create localization maps 
        /// </summary>
        protected virtual void CreateLocalizationMaps()
        {
            CreateMap<Language, LanguageModel>()
                .ForMember(model => model.AvailableCurrencies, options => options.Ignore())
                .ForMember(model => model.LocaleResourceSearchModel, options => options.Ignore());
            CreateMap<LanguageModel, Language>();

            CreateMap<LocaleResourceModel, LocaleStringResource>()
                .ForMember(entity => entity.LanguageId, options => options.Ignore());
        }

        /// <summary>
        /// Create logging maps 
        /// </summary>
        protected virtual void CreateLoggingMaps()
        {
            CreateMap<ActivityLog, ActivityLogModel>()
                .ForMember(model => model.ActivityLogTypeName, options => options.Ignore())
                .ForMember(model => model.CreatedOn, options => options.Ignore())
                .ForMember(model => model.CustomerEmail, options => options.Ignore());
            CreateMap<ActivityLogModel, ActivityLog>()
                .ForMember(entity => entity.ActivityLogTypeId, options => options.Ignore())
                .ForMember(entity => entity.CreatedOnUtc, options => options.Ignore())
                .ForMember(entity => entity.EntityId, options => options.Ignore())
                .ForMember(entity => entity.EntityName, options => options.Ignore());

            CreateMap<ActivityLogType, ActivityLogTypeModel>();
            CreateMap<ActivityLogTypeModel, ActivityLogType>()
                .ForMember(entity => entity.SystemKeyword, options => options.Ignore());

            CreateMap<Log, LogModel>()
                .ForMember(model => model.CreatedOn, options => options.Ignore())
                .ForMember(model => model.FullMessage, options => options.Ignore())
                .ForMember(model => model.CustomerEmail, options => options.Ignore());
            CreateMap<LogModel, Log>()
                .ForMember(entity => entity.CreatedOnUtc, options => options.Ignore())
                .ForMember(entity => entity.LogLevelId, options => options.Ignore());
        }

        /// <summary>
        /// Create media maps 
        /// </summary>
        protected virtual void CreateMediaMaps()
        {
            CreateMap<MediaSettings, MediaSettingsModel>()
                .ForMember(model => model.AvatarPictureSize_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.DefaultImageQuality_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.DefaultPictureZoomEnabled_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.ImportProductImagesUsingHash_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.MaximumImageSize_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.MultipleThumbDirectories_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.PicturesStoredIntoDatabase, options => options.Ignore())
                .ForMember(model => model.AllowSVGUploads_OverrideForStore, options => options.Ignore());
            CreateMap<MediaSettingsModel, MediaSettings>()
                .ForMember(settings => settings.AutoCompleteSearchThumbPictureSize, options => options.Ignore())
                .ForMember(settings => settings.AzureCacheControlHeader, options => options.Ignore())
                .ForMember(settings => settings.UseAbsoluteImagePath, options => options.Ignore())
                .ForMember(settings => settings.ImageSquarePictureSize, options => options.Ignore())
                .ForMember(settings => settings.VideoIframeAllow, options => options.Ignore())
                .ForMember(settings => settings.VideoIframeHeight, options => options.Ignore())
                .ForMember(settings => settings.VideoIframeWidth, options => options.Ignore());
        }

        /// <summary>
        /// Create messages maps 
        /// </summary>
        protected virtual void CreateMessagesMaps()
        {
            CreateMap<Campaign, CampaignModel>()
                .ForMember(model => model.AllowedTokens, options => options.Ignore())
                .ForMember(model => model.AvailableCustomerRoles, options => options.Ignore())
                .ForMember(model => model.AvailableEmailAccounts, options => options.Ignore())
                .ForMember(model => model.AvailableStores, options => options.Ignore())
                .ForMember(model => model.CreatedOn, options => options.Ignore())
                .ForMember(model => model.DontSendBeforeDate, options => options.Ignore())
                .ForMember(model => model.EmailAccountId, options => options.Ignore())
                .ForMember(model => model.TestEmail, options => options.Ignore());
            CreateMap<CampaignModel, Campaign>()
                .ForMember(entity => entity.CreatedOnUtc, options => options.Ignore())
                .ForMember(entity => entity.DontSendBeforeDateUtc, options => options.Ignore());

            CreateMap<EmailAccount, EmailAccountModel>()
                .ForMember(model => model.IsDefaultEmailAccount, options => options.Ignore())
                .ForMember(model => model.Password, options => options.Ignore())
                .ForMember(model => model.SendTestEmailTo, options => options.Ignore());
            CreateMap<EmailAccountModel, EmailAccount>()
                .ForMember(entity => entity.Password, options => options.Ignore());

            CreateMap<MessageTemplate, MessageTemplateModel>()
                .ForMember(model => model.AllowedTokens, options => options.Ignore())
                .ForMember(model => model.AvailableEmailAccounts, options => options.Ignore())
                .ForMember(model => model.HasAttachedDownload, options => options.Ignore())
                .ForMember(model => model.ListOfStores, options => options.Ignore())
                .ForMember(model => model.SendImmediately, options => options.Ignore());
            CreateMap<MessageTemplateModel, MessageTemplate>()
                .ForMember(entity => entity.DelayPeriod, options => options.Ignore());

            CreateMap<NewsLetterSubscription, NewsletterSubscriptionModel>()
                .ForMember(model => model.CreatedOn, options => options.Ignore())
                .ForMember(model => model.StoreName, options => options.Ignore());
            CreateMap<NewsletterSubscriptionModel, NewsLetterSubscription>()
                .ForMember(entity => entity.CreatedOnUtc, options => options.Ignore())
                .ForMember(entity => entity.NewsLetterSubscriptionGuid, options => options.Ignore())
                .ForMember(entity => entity.StoreId, options => options.Ignore());

            CreateMap<QueuedEmail, QueuedEmailModel>()
                .ForMember(model => model.CreatedOn, options => options.Ignore())
                .ForMember(model => model.DontSendBeforeDate, options => options.Ignore())
                .ForMember(model => model.EmailAccountName, options => options.Ignore())
                .ForMember(model => model.PriorityName, options => options.Ignore())
                .ForMember(model => model.SendImmediately, options => options.Ignore())
                .ForMember(model => model.SentOn, options => options.Ignore());
            CreateMap<QueuedEmailModel, QueuedEmail>()
                .ForMember(entity => entity.AttachmentFileName, options => options.Ignore())
                .ForMember(entity => entity.AttachmentFilePath, options => options.Ignore())
                .ForMember(entity => entity.CreatedOnUtc, options => options.Ignore())
                .ForMember(entity => entity.DontSendBeforeDateUtc, options => options.Ignore())
                .ForMember(entity => entity.EmailAccountId, options => options.Ignore())
                .ForMember(entity => entity.Priority, options => options.Ignore())
                .ForMember(entity => entity.PriorityId, options => options.Ignore())
                .ForMember(entity => entity.SentOnUtc, options => options.Ignore());
        }

        /// <summary>
        /// Create news maps 
        /// </summary>
        protected virtual void CreateNewsMaps()
        {
            CreateMap<NewsComment, NewsCommentModel>()
                .ForMember(model => model.CustomerInfo, options => options.Ignore())
                .ForMember(model => model.CreatedOn, options => options.Ignore())
                .ForMember(model => model.CommentText, options => options.Ignore())
                .ForMember(model => model.NewsItemTitle, options => options.Ignore())
                .ForMember(model => model.StoreName, options => options.Ignore());
            CreateMap<NewsCommentModel, NewsComment>()
                .ForMember(entity => entity.CommentTitle, options => options.Ignore())
                .ForMember(entity => entity.CommentText, options => options.Ignore())
                .ForMember(entity => entity.CreatedOnUtc, options => options.Ignore())
                .ForMember(entity => entity.NewsItemId, options => options.Ignore())
                .ForMember(entity => entity.CustomerId, options => options.Ignore())
                .ForMember(entity => entity.StoreId, options => options.Ignore());

            CreateMap<NewsItem, NewsItemModel>()
                .ForMember(model => model.ApprovedComments, options => options.Ignore())
                .ForMember(model => model.AvailableLanguages, options => options.Ignore())
                .ForMember(model => model.CreatedOn, options => options.Ignore())
                .ForMember(model => model.LanguageName, options => options.Ignore())
                .ForMember(model => model.NotApprovedComments, options => options.Ignore())
                .ForMember(model => model.SeName, options => options.Ignore());
            CreateMap<NewsItemModel, NewsItem>()
                .ForMember(entity => entity.CreatedOnUtc, options => options.Ignore());

            CreateMap<NewsSettings, NewsSettingsModel>()
                .ForMember(model => model.AllowNotRegisteredUsersToLeaveComments_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.Enabled_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.MainPageNewsCount_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.NewsArchivePageSize_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.NewsCommentsMustBeApproved_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.NotifyAboutNewNewsComments_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.ShowHeaderRssUrl_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.ShowNewsOnMainPage_OverrideForStore, options => options.Ignore());
            CreateMap<NewsSettingsModel, NewsSettings>();
        }
        
        /// <summary>
        /// Create plugins maps 
        /// </summary>
        protected virtual void CreatePluginsMaps()
        {
            CreateMap<PluginDescriptor, PluginModel>()
                .ForMember(model => model.CanChangeEnabled, options => options.Ignore())
                .ForMember(model => model.IsEnabled, options => options.Ignore());
        }

        /// <summary>
        /// Create polls maps 
        /// </summary>
        protected virtual void CreatePollsMaps()
        {
            CreateMap<PollAnswer, PollAnswerModel>();
            CreateMap<PollAnswerModel, PollAnswer>();

            CreateMap<Poll, PollModel>()
                .ForMember(model => model.AvailableLanguages, options => options.Ignore())
                .ForMember(model => model.PollAnswerSearchModel, options => options.Ignore())
                .ForMember(model => model.LanguageName, options => options.Ignore());
            CreateMap<PollModel, Poll>();
        }

        /// <summary>
        /// Create security maps 
        /// </summary>
        protected virtual void CreateSecurityMaps()
        {
            CreateMap<CaptchaSettings, CaptchaSettingsModel>()
                .ForMember(model => model.Enabled_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.ReCaptchaPrivateKey_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.ReCaptchaPublicKey_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.ShowOnApplyVendorPage_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.ShowOnBlogCommentPage_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.ShowOnContactUsPage_OverrideForStore, options => options.Ignore())
                 .ForMember(model => model.ShowOnCareerPage_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.ShowOnEmailProductToFriendPage_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.ShowOnEmailWishlistToFriendPage_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.ShowOnLoginPage_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.ShowOnNewsCommentPage_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.ShowOnProductReviewPage_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.ShowOnRegistrationPage_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.ShowOnForgotPasswordPage_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.ShowOnForum_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.ShowOnCheckoutPageForGuests_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.CaptchaType_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.ReCaptchaV3ScoreThreshold_OverrideForStore, options => options.Ignore())
                .ForMember(model => model.CaptchaTypeValues, options => options.Ignore());
            CreateMap<CaptchaSettingsModel, CaptchaSettings>()
                .ForMember(settings => settings.AutomaticallyChooseLanguage, options => options.Ignore())
                .ForMember(settings => settings.ReCaptchaDefaultLanguage, options => options.Ignore())
                .ForMember(settings => settings.ReCaptchaRequestTimeout, options => options.Ignore())
                .ForMember(settings => settings.ReCaptchaTheme, options => options.Ignore())
                .ForMember(settings => settings.ReCaptchaApiUrl, options => options.Ignore());
        }

        /// <summary>
        /// Create SEO maps 
        /// </summary>
        protected virtual void CreateSeoMaps()
        {
            CreateMap<UrlRecord, UrlRecordModel>()
                .ForMember(model => model.DetailsUrl, options => options.Ignore())
                .ForMember(model => model.Language, options => options.Ignore())
                .ForMember(model => model.Name, options => options.Ignore());
            CreateMap<UrlRecordModel, UrlRecord>()
                .ForMember(entity => entity.LanguageId, options => options.Ignore())
                .ForMember(entity => entity.Slug, options => options.Ignore());
        }

        /// <summary>
        /// Create stores maps 
        /// </summary>
        protected virtual void CreateStoresMaps()
        {
            CreateMap<Store, StoreModel>()
                .ForMember(model => model.AvailableLanguages, options => options.Ignore());
            CreateMap<StoreModel, Store>()
                .ForMember(entity => entity.SslEnabled, options => options.Ignore())
                .ForMember(entity => entity.Deleted, options => options.Ignore());
        }

        /// <summary>
        /// Create tasks maps 
        /// </summary>
        protected virtual void CreateTasksMaps()
        {
            CreateMap<ScheduleTask, ScheduleTaskModel>();
            CreateMap<ScheduleTaskModel, ScheduleTask>()
                .ForMember(entity => entity.Type, options => options.Ignore())
                .ForMember(entity => entity.LastStartUtc, options => options.Ignore())
                .ForMember(entity => entity.LastEndUtc, options => options.Ignore())
                .ForMember(entity => entity.LastSuccessUtc, options => options.Ignore())
                .ForMember(entity => entity.LastEnabledUtc, options => options.Ignore());
        }

        /// <summary>
        /// Create topics maps 
        /// </summary>
        protected virtual void CreateTopicsMaps()
        {
            CreateMap<Topic, TopicModel>()
                .ForMember(model => model.AvailableTopicTemplates, options => options.Ignore())
                .ForMember(model => model.SeName, options => options.Ignore())
                .ForMember(model => model.TopicName, options => options.Ignore())
                .ForMember(model => model.Url, options => options.Ignore());
            CreateMap<TopicModel, Topic>();

            CreateMap<TopicTemplate, TopicTemplateModel>();
            CreateMap<TopicTemplateModel, TopicTemplate>();
        }

        /// <summary>
        /// Create holiday maps 
        /// </summary>
        protected virtual void CreateHolidayMaps()
        {
            CreateMap<Holiday, HolidayModel>();
            CreateMap<HolidayModel, Holiday>();
        }
        protected virtual void CreateLeaveManagementMaps()
        {
            CreateMap<LeaveManagement, LeaveManagementModel>()
                .ForMember(model => model.CreatedOnUTC, options => options.Ignore());

            CreateMap<LeaveManagementModel, LeaveManagement>()
                .ForMember(entity => entity.CreatedOnUTC, options => options.Ignore());

            CreateMap<LeaveSettings, LeaveSettingsModel>()
                .ForMember(model => model.CommonEmails, options => options.MapFrom(entity => entity.CommonEmails))
                .ForMember(model => model.HrEmail, options => options.MapFrom(entity => entity.HrEmail))
                .ForMember(model => model.SendEmailToAllProjectLeaders, options => options.MapFrom(entity => entity.SendEmailToAllProjectLeaders));

            CreateMap<LeaveSettingsModel, LeaveSettings>()
                .ForMember(entity => entity.CommonEmails, options => options.MapFrom(model => model.CommonEmails))
                .ForMember(entity => entity.HrEmail, options => options.MapFrom(model => model.HrEmail))
                .ForMember(entity => entity.SendEmailToAllProjectLeaders, options => options.MapFrom(model => model.SendEmailToAllProjectLeaders));
        }

        protected virtual void CreateLeaveTypeMaps()
        {
            CreateMap<Leave, LeaveTypeModel>();
            CreateMap<LeaveTypeModel, Leave>();
        }

        protected virtual void CreateLeaveTransactionLogMaps()
        {
            CreateMap<LeaveTransactionLog, LeaveTransactionLogModel>();
            CreateMap<LeaveTransactionLogModel, LeaveTransactionLog>();
        }
        protected virtual void CreateProjectMaps()
        {
            CreateMap<Project, ProjectModel>();
            CreateMap<ProjectModel, Project>();
        }
        protected virtual void CreateProjectEmpMappingMaps()
        {
            CreateMap<ProjectEmployeeMapping, ProjectEmployeeMappingModel>();
            CreateMap<ProjectEmployeeMappingModel, ProjectEmployeeMapping>();
        }
        protected virtual void CreateTimeSheetMaps()
        {
            CreateMap<TimeSheet, TimeSheetModel>();
            CreateMap<TimeSheetModel, TimeSheet>();
            CreateMap<TimeSheetSetting, TimeSheetSettingsModel>();


            CreateMap<TimeSheetSettingsModel, TimeSheetSetting>();
               
        }

        protected virtual void CreateTimeSheetReportMaps()
        {
            CreateMap<TimeSheetReport, TimesheetReportModel>();
            CreateMap<TimesheetReportModel, TimeSheetReport>();
        }

        protected virtual void CreateMonthlyPerformanceReportMaps()
        {
            CreateMap<MonthlyTimeSheetReport, MonthlyPerformanceReportModel>();
            CreateMap<MonthlyPerformanceReportModel, MonthlyTimeSheetReport>();

            CreateMap<MonthlyReportSetting, MonthlyReportSettingsModel>();

            CreateMap<MonthlyReportSettingsModel, MonthlyReportSetting>();

            CreateMap<EmailSettings, EmailSettingsModel>();
            CreateMap<EmailSettingsModel, EmailSettings>();

        }
        protected virtual void CreateJobPostingMaps()
        {
            CreateMap<JobPosting, JobPostingModel>();
            CreateMap<JobPostingModel, JobPosting>();
        }
        protected virtual void CreateCandiatesResumesMaps()
        {
            CreateMap<CandidatesResumes, CandiatesResumesModel>();
            CreateMap<CandiatesResumesModel, CandidatesResumes>();
        }
        protected virtual void CreateKPIMasterMaps()
        {
            CreateMap<KPIMaster, KPIMasterModel>();
            CreateMap<KPIMasterModel, KPIMaster>();
        }
        protected virtual void CreateKPIWeightageMaps()
        {
            CreateMap<KPIWeightage, KPIWeightageModel>();
            CreateMap<KPIWeightageModel, KPIWeightage>();
        }
        protected virtual void CreateTeamPerformanceMeasurementMaps()
        {
            CreateMap<TeamPerformanceMeasurement, TeamPerformanceMeasurementModel>();
            CreateMap<TeamPerformanceMeasurementModel, TeamPerformanceMeasurement>();

            CreateMap<TeamPerformanceSettings, TeamPerformanceSettingsModel>();
            CreateMap<TeamPerformanceSettingsModel, TeamPerformanceSettings>();

        }
        protected virtual void CreateEmployeeAttendanceMaps()
        {
            CreateMap<EmployeeAttendance, EmployeeAttendanceModel>();
            CreateMap<EmployeeAttendanceModel, EmployeeAttendance>();
            CreateMap<EmployeeAttendanceSetting, EmployeeAttendanceSettingsModel>();
            CreateMap<EmployeeAttendanceSettingsModel, EmployeeAttendanceSetting>();

        }

        /// <summary>
        /// Create employee maps 
        /// </summary>
        protected virtual void CreateEmployeeMaps()
        {
            CreateMap<Employee, EmployeeModel>();
            CreateMap<EmployeeModel, Employee>();



            CreateMap<EmployeeSettings, EmployeeSettingsModel>()
                .ForMember(model => model.OnBoardingEmail, options => options.MapFrom(entity => entity.OnBoardingEmail));


            CreateMap<EmployeeSettingsModel, EmployeeSettings>()
                .ForMember(entity => entity.OnBoardingEmail, options => options.MapFrom(model => model.OnBoardingEmail));
             
        }

        /// <summary>
        /// Create education maps 
        /// </summary>
        protected virtual void CreateEducationMaps()
        {
            CreateMap<Education, EducationModel>();
            CreateMap<EducationModel, Education>();
        }
        protected virtual void CreateExperienceMaps()
        {
            CreateMap<Experience, ExperienceModel>();
            CreateMap<ExperienceModel, Experience>();
        }
        protected virtual void CreateAssetsMaps()
        {
            CreateMap<Assets, AssetsModel>();
            CreateMap<AssetsModel, Assets>();
        }
        protected virtual void CreateAddressMaps()
        {
            CreateMap<EmpAddress,EmpAddressModel>();
            CreateMap<EmpAddressModel, EmpAddress>();
        }
        protected virtual void CreateInterviewQeations()
        {
            CreateMap<Questions, RecruitementModel>().ForMember(entity => entity.CreatedOn, options => options.Ignore());
            CreateMap<RecruitementModel, Questions>().ForMember(model => model.CreatedOn, options => options.Ignore());
        }
        protected virtual void CreateIresult()
        {
            CreateMap<CandidatesResult, CandiatesResumesModel>();
            CreateMap<CandiatesResumesModel, CandidatesResult>();
        }
        protected virtual void CreateIWeeklyQuestions()
        {
           
            CreateMap<WeeklyQuestions, WeeklyQuestionsModel>().ForMember(entity => entity.CreatedOn, options => options.Ignore());
            CreateMap<WeeklyQuestionsModel, WeeklyQuestions>().ForMember(model => model.CreatedOn, options => options.Ignore());
        }
        protected virtual void CreateWeeklyreport()
        {
            CreateMap<WeeklyReports, WeeklyReportModel>();
            CreateMap<WeeklyReportModel, WeeklyReports>();
        }

        protected virtual void CreateProjectTaskMaps()
        {
            CreateMap<ProjectTask, ProjectTaskModel>().ForMember(entity => entity.CreatedOnUtc, options => options.Ignore());
            CreateMap<ProjectTaskModel, ProjectTask>().ForMember(model => model.CreatedOnUtc, options => options.Ignore());

            CreateMap<ProjectTaskSetting, ProjectTaskSettingsModel>();
            CreateMap<ProjectTaskSettingsModel, ProjectTaskSetting>();

        }

        protected virtual void CreateTaskCommentsMaps()
        {
            CreateMap<TaskComments, TaskCommentsModel>().ForMember(entity => entity.CreatedOn, options => options.Ignore());
            CreateMap<TaskCommentsModel, TaskComments>().ForMember(model => model.CreatedOn, options => options.Ignore());
        }

        protected virtual void CreateTaskChangeLogMaps()
        {
            CreateMap<TaskChangeLog, TaskChangeLogModel>().ForMember(entity => entity.CreatedOn, options => options.Ignore());
            CreateMap<TaskChangeLogModel, TaskChangeLog>().ForMember(model => model.CreatedOn, options => options.Ignore());
        }

        protected virtual void CreateProcessWorkflowMaps()
        {
            CreateMap<ProcessWorkflow, ProcessWorkflowModel>().ForMember(entity => entity.CreatedOn, options => options.Ignore());
            CreateMap<ProcessWorkflowModel, ProcessWorkflow>().ForMember(model => model.CreatedOn, options => options.Ignore());
        }

        protected virtual void CreateProcessRulesMaps()
        {
            CreateMap<ProcessRules, ProcessRulesModel>().ForMember(entity => entity.CreatedOn, options => options.Ignore());
            CreateMap<ProcessRulesModel, ProcessRules>().ForMember(model => model.CreatedOn, options => options.Ignore());
        }

        protected virtual void CreateWorkflowStatusMaps()
        {
            CreateMap<WorkflowStatus, WorkflowStatusModel>().ForMember(entity => entity.CreatedOn, options => options.Ignore());
            CreateMap<WorkflowStatusModel, WorkflowStatus>().ForMember(model => model.CreatedOn, options => options.Ignore());
        }

        protected virtual void CreateActivityMaps()
        {
            CreateMap<Activity, ActivityModel>().ForMember(entity => entity.ActivityName, options => options.Ignore());
            CreateMap<ActivityModel, Activity>().ForMember(model => model.ActivityName, options => options.Ignore());
        }

        protected virtual void CreateAnnouncementMaps()
        {
            CreateMap<Announcement, AnnouncementModel>()
                .ForMember(model => model.CreatedOnUtc, options => options.Ignore()); 

            CreateMap<AnnouncementModel, Announcement>()
                .ForMember(entity => entity.CreatedOnUtc, options => options.Ignore()); 
        }


        protected virtual void CreateTaskCategoryMaps()
        {
            CreateMap<TaskCategory, TaskCategoryModel>()
                .ForMember(model => model.CreatedOn, options => options.Ignore());

            CreateMap<TaskCategoryModel, TaskCategory>()
                .ForMember(entity => entity.CreatedOn, options => options.Ignore());
        }

        protected virtual void CreateChecklistMasterMaps()
        {
            CreateMap<CheckListMaster, CheckListMasterModel>()
                .ForMember(model => model.CreatedOn, options => options.Ignore());

            CreateMap<CheckListMasterModel, CheckListMaster>()
                .ForMember(entity => entity.CreatedOn, options => options.Ignore());
        }
        protected virtual void CreateProjectTaskCategoryMappingMaps()
        {
            CreateMap<ProjectTaskCategoryMapping, ProjectTaskCategoryMappingModel>();
            CreateMap<ProjectTaskCategoryMappingModel, ProjectTaskCategoryMapping>();
              
        }


        protected virtual void CreateCheckListMappingMaps()
        {
            CreateMap<CheckListMapping, CheckListMappingModel>();
            CreateMap<CheckListMappingModel, CheckListMapping>();

        }

        #endregion

        #region Properties

        /// <summary>
        /// Order of this mapper implementation
        /// </summary>
        public int Order => 0;

        #endregion
    }
}