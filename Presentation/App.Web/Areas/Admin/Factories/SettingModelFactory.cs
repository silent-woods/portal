using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using App.Core;
using App.Core.Configuration;
using App.Core.Domain;
using App.Core.Domain.Blogs;
using App.Core.Domain.Catalog;
using App.Core.Domain.Common;
using App.Core.Domain.Customers;
using App.Core.Domain.Directory;
using App.Core.Domain.Forums;
using App.Core.Domain.Gdpr;
using App.Core.Domain.Localization;
using App.Core.Domain.Media;
using App.Core.Domain.News;
using App.Core.Domain.Security;
using App.Core.Domain.Seo;
using App.Core.Infrastructure;
using App.Data;
using App.Data.Configuration;
using App.Services;
using App.Services.Common;
using App.Services.Configuration;
using App.Services.Directory;
using App.Services.Gdpr;
using App.Services.Helpers;
using App.Services.Localization;
using App.Services.Media;
using App.Services.Stores;
using App.Services.Themes;
using App.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using App.Web.Areas.Admin.Models.Settings;
using App.Web.Areas.Admin.Models.Stores;
using App.Web.Framework.Factories;
using App.Web.Framework.Models.Extensions;
using App.Web.Framework.WebOptimizer;
using App.Data.Extensions;
using App.Core.Domain.Extension.Leaves;
using App.Core.Domain.Extension.TimeSheets;
using App.Core.Domain.Extension.EmployeeAttendanceSetting;
using App.Services.Leaves;
using App.Core.Domain.Extension.PerformanceMeasurements;
using App.Web.Models.Boards;
using App.Services.Departments;
using App.Core.Domain.Extension.Employees;
using App.Core.Domain.Extension.Alerts;
using App.Services.Projects;
using App.Core.Domain.Extension.ProjectTasks;
using App.Services.Designations;

namespace App.Web.Areas.Admin.Factories
{
    /// <summary>
    /// Represents the setting model factory implementation
    /// </summary>
    public partial class SettingModelFactory : ISettingModelFactory
    {
        #region Fields

        private readonly AppSettings _appSettings;
        private readonly CurrencySettings _currencySettings;
        private readonly IAddressModelFactory _addressModelFactory;
        private readonly IAddressAttributeModelFactory _addressAttributeModelFactory;
        private readonly IAddressService _addressService;
        private readonly IBaseAdminModelFactory _baseAdminModelFactory;
        private readonly ICurrencyService _currencyService;
        private readonly ICustomerAttributeModelFactory _customerAttributeModelFactory;
        private readonly INopDataProvider _dataProvider;
        private readonly INopFileProvider _fileProvider;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IGdprService _gdprService;
        private readonly ILocalizedModelFactory _localizedModelFactory;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly IPictureService _pictureService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly IStoreService _storeService;
        private readonly IThemeProvider _themeProvider;
        private readonly IWorkContext _workContext;
        private readonly ILeaveTypeService _leaveTypeService;
        private readonly IDepartmentService _departmentService;
        private readonly IProjectsService _projectsService;
        private readonly IDesignationService _designationService;

        #endregion

        #region Ctor

        public SettingModelFactory(AppSettings appSettings,
            CurrencySettings currencySettings,
            IAddressModelFactory addressModelFactory,
            IAddressAttributeModelFactory addressAttributeModelFactory,
            IAddressService addressService,
            IBaseAdminModelFactory baseAdminModelFactory,
            ICurrencyService currencyService,
            ICustomerAttributeModelFactory customerAttributeModelFactory,
            INopDataProvider dataProvider,
            INopFileProvider fileProvider,
            IDateTimeHelper dateTimeHelper,
            IGdprService gdprService,
            ILocalizedModelFactory localizedModelFactory,
            IGenericAttributeService genericAttributeService,
            ILanguageService languageService,
            ILocalizationService localizationService,
            IPictureService pictureService,
            ISettingService settingService,
            IStoreContext storeContext,
            IStoreService storeService,
            IThemeProvider themeProvider,
            IWorkContext workContext,
            ILeaveTypeService leaveTypeService,
            IDepartmentService departmentService,
            IProjectsService projectsService,
            IDesignationService designationService)
        {
            _appSettings = appSettings;
            _currencySettings = currencySettings;
            _addressModelFactory = addressModelFactory;
            _addressAttributeModelFactory = addressAttributeModelFactory;
            _addressService = addressService;
            _baseAdminModelFactory = baseAdminModelFactory;
            _currencyService = currencyService;
            _customerAttributeModelFactory = customerAttributeModelFactory;
            _dataProvider = dataProvider;
            _fileProvider = fileProvider;
            _dateTimeHelper = dateTimeHelper;
            _gdprService = gdprService;
            _localizedModelFactory = localizedModelFactory;
            _genericAttributeService = genericAttributeService;
            _languageService = languageService;
            _localizationService = localizationService;
            _pictureService = pictureService;
            _settingService = settingService;
            _storeContext = storeContext;
            _storeService = storeService;
            _themeProvider = themeProvider;
            _workContext = workContext;
            _leaveTypeService = leaveTypeService;
            _departmentService = departmentService;
            _projectsService = projectsService;
            _designationService = designationService;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Prepare store theme models
        /// </summary>
        /// <param name="models">List of store theme models</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected virtual async Task PrepareStoreThemeModelsAsync(IList<StoreInformationSettingsModel.ThemeModel> models)
        {
            if (models == null)
                throw new ArgumentNullException(nameof(models));

            //load settings for a chosen store scope
            var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var storeInformationSettings = await _settingService.LoadSettingAsync<StoreInformationSettings>(storeId);

            //get available themes
            var availableThemes = await _themeProvider.GetThemesAsync();
            foreach (var theme in availableThemes)
            {
                models.Add(new StoreInformationSettingsModel.ThemeModel
                {
                    FriendlyName = theme.FriendlyName,
                    SystemName = theme.SystemName,
                    PreviewImageUrl = theme.PreviewImageUrl,
                    PreviewText = theme.PreviewText,
                    SupportRtl = theme.SupportRtl,
                    Selected = theme.SystemName.Equals(storeInformationSettings.DefaultStoreTheme, StringComparison.InvariantCultureIgnoreCase)
                });
            }
        }

        /// <summary>
        /// Prepare sort option search model
        /// </summary>
        /// <param name="searchModel">Sort option search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the sort option search model
        /// </returns>
        protected virtual Task<SortOptionSearchModel> PrepareSortOptionSearchModelAsync(SortOptionSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //prepare page parameters
            searchModel.SetGridPageSize();

            return Task.FromResult(searchModel);
        }

        /// <summary>
        /// Prepare GDPR consent search model
        /// </summary>
        /// <param name="searchModel">GDPR consent search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the gDPR consent search model
        /// </returns>
        protected virtual Task<GdprConsentSearchModel> PrepareGdprConsentSearchModelAsync(GdprConsentSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //prepare page parameters
            searchModel.SetGridPageSize();

            return Task.FromResult(searchModel);
        }

        /// <summary>
        /// Prepare address settings model
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the address settings model
        /// </returns>
        protected virtual async Task<AddressSettingsModel> PrepareAddressSettingsModelAsync()
        {
            //load settings for a chosen store scope
            var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var addressSettings = await _settingService.LoadSettingAsync<AddressSettings>(storeId);

            //fill in model values from the entity
            var model = addressSettings.ToSettingsModel<AddressSettingsModel>();

            return model;
        }

        /// <summary>
        /// Prepare customer settings model
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the customer settings model
        /// </returns>
        protected virtual async Task<CustomerSettingsModel> PrepareCustomerSettingsModelAsync()
        {
            //load settings for a chosen store scope
            var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var customerSettings = await _settingService.LoadSettingAsync<CustomerSettings>(storeId);

            //fill in model values from the entity
            var model = customerSettings.ToSettingsModel<CustomerSettingsModel>();

            return model;
        }

        /// <summary>
        /// Prepare multi-factor authentication settings model
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the multiFactorAuthenticationSettingsModel
        /// </returns>
        protected virtual async Task<MultiFactorAuthenticationSettingsModel> PrepareMultiFactorAuthenticationSettingsModelAsync()
        {
            //load settings for a chosen store scope
            var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var multiFactorAuthenticationSettings = await _settingService.LoadSettingAsync<MultiFactorAuthenticationSettings>(storeId);

            //fill in model values from the entity
            var model = multiFactorAuthenticationSettings.ToSettingsModel<MultiFactorAuthenticationSettingsModel>();

            return model;

        }

        /// <summary>
        /// Prepare date time settings model
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the date time settings model
        /// </returns>
        protected virtual async Task<DateTimeSettingsModel> PrepareDateTimeSettingsModelAsync()
        {
            //load settings for a chosen store scope
            var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var dateTimeSettings = await _settingService.LoadSettingAsync<DateTimeSettings>(storeId);

            //fill in model values from the entity
            var model = new DateTimeSettingsModel
            {
                AllowCustomersToSetTimeZone = dateTimeSettings.AllowCustomersToSetTimeZone
            };

            //fill in additional values (not existing in the entity)
            model.DefaultStoreTimeZoneId = _dateTimeHelper.DefaultStoreTimeZone.Id;

            //prepare available time zones
            await _baseAdminModelFactory.PrepareTimeZonesAsync(model.AvailableTimeZones, false);

            return model;
        }

        /// <summary>
        /// Prepare external authentication settings model
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the external authentication settings model
        /// </returns>
        protected virtual async Task<ExternalAuthenticationSettingsModel> PrepareExternalAuthenticationSettingsModelAsync()
        {
            //load settings for a chosen store scope
            var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var externalAuthenticationSettings = await _settingService.LoadSettingAsync<ExternalAuthenticationSettings>(storeId);

            //fill in model values from the entity
            var model = new ExternalAuthenticationSettingsModel
            {
                AllowCustomersToRemoveAssociations = externalAuthenticationSettings.AllowCustomersToRemoveAssociations
            };

            return model;
        }

        /// <summary>
        /// Prepare store information settings model
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the store information settings model
        /// </returns>
        protected virtual async Task<StoreInformationSettingsModel> PrepareStoreInformationSettingsModelAsync()
        {
            //load settings for a chosen store scope
            var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var storeInformationSettings = await _settingService.LoadSettingAsync<StoreInformationSettings>(storeId);
            var commonSettings = await _settingService.LoadSettingAsync<CommonSettings>(storeId);

            //fill in model values from the entity
            var model = new StoreInformationSettingsModel
            {
                StoreClosed = storeInformationSettings.StoreClosed,
                DefaultStoreTheme = storeInformationSettings.DefaultStoreTheme,
                AllowCustomerToSelectTheme = storeInformationSettings.AllowCustomerToSelectTheme,
                LogoPictureId = storeInformationSettings.LogoPictureId,
                DisplayEuCookieLawWarning = storeInformationSettings.DisplayEuCookieLawWarning,
                FacebookLink = storeInformationSettings.FacebookLink,
                TwitterLink = storeInformationSettings.TwitterLink,
                YoutubeLink = storeInformationSettings.YoutubeLink,
                InstagramLink = storeInformationSettings.InstagramLink,
                SubjectFieldOnContactUsForm = commonSettings.SubjectFieldOnContactUsForm,
                UseSystemEmailForContactUsForm = commonSettings.UseSystemEmailForContactUsForm,
                PopupForTermsOfServiceLinks = commonSettings.PopupForTermsOfServiceLinks
            };

            //prepare available themes
            await PrepareStoreThemeModelsAsync(model.AvailableStoreThemes);

            if (storeId <= 0)
                return model;

            //fill in overridden values
            model.StoreClosed_OverrideForStore = await _settingService.SettingExistsAsync(storeInformationSettings, x => x.StoreClosed, storeId);
            model.DefaultStoreTheme_OverrideForStore = await _settingService.SettingExistsAsync(storeInformationSettings, x => x.DefaultStoreTheme, storeId);
            model.AllowCustomerToSelectTheme_OverrideForStore = await _settingService.SettingExistsAsync(storeInformationSettings, x => x.AllowCustomerToSelectTheme, storeId);
            model.LogoPictureId_OverrideForStore = await _settingService.SettingExistsAsync(storeInformationSettings, x => x.LogoPictureId, storeId);
            model.DisplayEuCookieLawWarning_OverrideForStore = await _settingService.SettingExistsAsync(storeInformationSettings, x => x.DisplayEuCookieLawWarning, storeId);
            model.FacebookLink_OverrideForStore = await _settingService.SettingExistsAsync(storeInformationSettings, x => x.FacebookLink, storeId);
            model.TwitterLink_OverrideForStore = await _settingService.SettingExistsAsync(storeInformationSettings, x => x.TwitterLink, storeId);
            model.YoutubeLink_OverrideForStore = await _settingService.SettingExistsAsync(storeInformationSettings, x => x.YoutubeLink, storeId);
            model.InstagramLink_OverrideForStore = await _settingService.SettingExistsAsync(storeInformationSettings, x => x.InstagramLink, storeId);
            model.SubjectFieldOnContactUsForm_OverrideForStore = await _settingService.SettingExistsAsync(commonSettings, x => x.SubjectFieldOnContactUsForm, storeId);
            model.UseSystemEmailForContactUsForm_OverrideForStore = await _settingService.SettingExistsAsync(commonSettings, x => x.UseSystemEmailForContactUsForm, storeId);
            model.PopupForTermsOfServiceLinks_OverrideForStore = await _settingService.SettingExistsAsync(commonSettings, x => x.PopupForTermsOfServiceLinks, storeId);

            return model;
        }

        /// <summary>
        /// Prepare Sitemap settings model
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the sitemap settings model
        /// </returns>
        protected virtual async Task<SitemapSettingsModel> PrepareSitemapSettingsModelAsync()
        {
            //load settings for a chosen store scope
            var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var sitemapSettings = await _settingService.LoadSettingAsync<SitemapSettings>(storeId);

            //fill in model values from the entity
            var model = new SitemapSettingsModel
            {
                SitemapEnabled = sitemapSettings.SitemapEnabled,
                SitemapPageSize = sitemapSettings.SitemapPageSize,
                SitemapIncludeCategories = sitemapSettings.SitemapIncludeCategories,
                SitemapIncludeManufacturers = sitemapSettings.SitemapIncludeManufacturers,
                SitemapIncludeProducts = sitemapSettings.SitemapIncludeProducts,
                SitemapIncludeProductTags = sitemapSettings.SitemapIncludeProductTags,
                SitemapIncludeBlogPosts = sitemapSettings.SitemapIncludeBlogPosts,
                SitemapIncludeNews = sitemapSettings.SitemapIncludeNews,
                SitemapIncludeTopics = sitemapSettings.SitemapIncludeTopics
            };

            if (storeId <= 0)
                return model;

            //fill in overridden values
            model.SitemapEnabled_OverrideForStore = await _settingService.SettingExistsAsync(sitemapSettings, x => x.SitemapEnabled, storeId);
            model.SitemapPageSize_OverrideForStore = await _settingService.SettingExistsAsync(sitemapSettings, x => x.SitemapPageSize, storeId);
            model.SitemapIncludeCategories_OverrideForStore = await _settingService.SettingExistsAsync(sitemapSettings, x => x.SitemapIncludeCategories, storeId);
            model.SitemapIncludeManufacturers_OverrideForStore = await _settingService.SettingExistsAsync(sitemapSettings, x => x.SitemapIncludeManufacturers, storeId);
            model.SitemapIncludeProducts_OverrideForStore = await _settingService.SettingExistsAsync(sitemapSettings, x => x.SitemapIncludeProducts, storeId);
            model.SitemapIncludeProductTags_OverrideForStore = await _settingService.SettingExistsAsync(sitemapSettings, x => x.SitemapIncludeProductTags, storeId);
            model.SitemapIncludeBlogPosts_OverrideForStore = await _settingService.SettingExistsAsync(sitemapSettings, x => x.SitemapIncludeBlogPosts, storeId);
            model.SitemapIncludeNews_OverrideForStore = await _settingService.SettingExistsAsync(sitemapSettings, x => x.SitemapIncludeNews, storeId);
            model.SitemapIncludeTopics_OverrideForStore = await _settingService.SettingExistsAsync(sitemapSettings, x => x.SitemapIncludeTopics, storeId);

            return model;
        }

        /// <summary>
        /// Prepare minification settings model
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the minification settings model
        /// </returns>
        protected virtual async Task<MinificationSettingsModel> PrepareMinificationSettingsModelAsync()
        {
            //load settings for a chosen store scope
            var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var minificationSettings = await _settingService.LoadSettingAsync<CommonSettings>(storeId);

            //fill in model values from the entity
            var model = new MinificationSettingsModel
            {
                EnableHtmlMinification = minificationSettings.EnableHtmlMinification,
                UseResponseCompression = minificationSettings.UseResponseCompression
            };

            if (storeId <= 0)
                return model;

            //fill in overridden values
            model.EnableHtmlMinification_OverrideForStore = await _settingService.SettingExistsAsync(minificationSettings, x => x.EnableHtmlMinification, storeId);
            model.UseResponseCompression_OverrideForStore = await _settingService.SettingExistsAsync(minificationSettings, x => x.UseResponseCompression, storeId);

            return model;
        }

        /// <summary>
        /// Prepare SEO settings model
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the sEO settings model
        /// </returns>
        protected virtual async Task<SeoSettingsModel> PrepareSeoSettingsModelAsync()
        {
            //load settings for a chosen store scope
            var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var seoSettings = await _settingService.LoadSettingAsync<SeoSettings>(storeId);

            //fill in model values from the entity
            var model = new SeoSettingsModel
            {
                PageTitleSeparator = seoSettings.PageTitleSeparator,
                PageTitleSeoAdjustment = (int)seoSettings.PageTitleSeoAdjustment,
                PageTitleSeoAdjustmentValues = await seoSettings.PageTitleSeoAdjustment.ToSelectListAsync(),
                GenerateProductMetaDescription = seoSettings.GenerateProductMetaDescription,
                ConvertNonWesternChars = seoSettings.ConvertNonWesternChars,
                CanonicalUrlsEnabled = seoSettings.CanonicalUrlsEnabled,
                WwwRequirement = (int)seoSettings.WwwRequirement,
                WwwRequirementValues = await seoSettings.WwwRequirement.ToSelectListAsync(),

                TwitterMetaTags = seoSettings.TwitterMetaTags,
                OpenGraphMetaTags = seoSettings.OpenGraphMetaTags,
                CustomHeadTags = seoSettings.CustomHeadTags,
                MicrodataEnabled = seoSettings.MicrodataEnabled
            };

            if (storeId <= 0)
                return model;

            //fill in overridden values
            model.PageTitleSeparator_OverrideForStore = await _settingService.SettingExistsAsync(seoSettings, x => x.PageTitleSeparator, storeId);
            model.PageTitleSeoAdjustment_OverrideForStore = await _settingService.SettingExistsAsync(seoSettings, x => x.PageTitleSeoAdjustment, storeId);
            model.GenerateProductMetaDescription_OverrideForStore = await _settingService.SettingExistsAsync(seoSettings, x => x.GenerateProductMetaDescription, storeId);
            model.ConvertNonWesternChars_OverrideForStore = await _settingService.SettingExistsAsync(seoSettings, x => x.ConvertNonWesternChars, storeId);
            model.CanonicalUrlsEnabled_OverrideForStore = await _settingService.SettingExistsAsync(seoSettings, x => x.CanonicalUrlsEnabled, storeId);
            model.WwwRequirement_OverrideForStore = await _settingService.SettingExistsAsync(seoSettings, x => x.WwwRequirement, storeId);
            model.TwitterMetaTags_OverrideForStore = await _settingService.SettingExistsAsync(seoSettings, x => x.TwitterMetaTags, storeId);
            model.OpenGraphMetaTags_OverrideForStore = await _settingService.SettingExistsAsync(seoSettings, x => x.OpenGraphMetaTags, storeId);
            model.CustomHeadTags_OverrideForStore = await _settingService.SettingExistsAsync(seoSettings, x => x.CustomHeadTags, storeId);
            model.MicrodataEnabled_OverrideForStore = await _settingService.SettingExistsAsync(seoSettings, x => x.MicrodataEnabled, storeId);

            return model;
        }

        /// <summary>
        /// Prepare security settings model
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the security settings model
        /// </returns>
        protected virtual async Task<SecuritySettingsModel> PrepareSecuritySettingsModelAsync()
        {
            //load settings for a chosen store scope
            var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var securitySettings = await _settingService.LoadSettingAsync<SecuritySettings>(storeId);

            //fill in model values from the entity
            var model = new SecuritySettingsModel
            {
                EncryptionKey = securitySettings.EncryptionKey,
                HoneypotEnabled = securitySettings.HoneypotEnabled
            };

            //fill in additional values (not existing in the entity)
            if (securitySettings.AdminAreaAllowedIpAddresses != null)
                model.AdminAreaAllowedIpAddresses = string.Join(",", securitySettings.AdminAreaAllowedIpAddresses);

            return model;
        }

        /// <summary>
        /// Prepare captcha settings model
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the captcha settings model
        /// </returns>
        protected virtual async Task<CaptchaSettingsModel> PrepareCaptchaSettingsModelAsync()
        {
            //load settings for a chosen store scope
            var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var captchaSettings = await _settingService.LoadSettingAsync<CaptchaSettings>(storeId);

            //fill in model values from the entity
            var model = captchaSettings.ToSettingsModel<CaptchaSettingsModel>();

            model.CaptchaTypeValues = await captchaSettings.CaptchaType.ToSelectListAsync();

            if (storeId <= 0)
                return model;

            model.Enabled_OverrideForStore = await _settingService.SettingExistsAsync(captchaSettings, x => x.Enabled, storeId);
            model.ShowOnLoginPage_OverrideForStore = await _settingService.SettingExistsAsync(captchaSettings, x => x.ShowOnLoginPage, storeId);
            model.ShowOnRegistrationPage_OverrideForStore = await _settingService.SettingExistsAsync(captchaSettings, x => x.ShowOnRegistrationPage, storeId);
            model.ShowOnContactUsPage_OverrideForStore = await _settingService.SettingExistsAsync(captchaSettings, x => x.ShowOnContactUsPage, storeId);
            model.ShowOnCareerPage_OverrideForStore = await _settingService.SettingExistsAsync(captchaSettings, x => x.ShowOnCareerPage, storeId);
            model.ShowOnEmailWishlistToFriendPage_OverrideForStore = await _settingService.SettingExistsAsync(captchaSettings, x => x.ShowOnEmailWishlistToFriendPage, storeId);
            model.ShowOnEmailProductToFriendPage_OverrideForStore = await _settingService.SettingExistsAsync(captchaSettings, x => x.ShowOnEmailProductToFriendPage, storeId);
            model.ShowOnBlogCommentPage_OverrideForStore = await _settingService.SettingExistsAsync(captchaSettings, x => x.ShowOnBlogCommentPage, storeId);
            model.ShowOnNewsCommentPage_OverrideForStore = await _settingService.SettingExistsAsync(captchaSettings, x => x.ShowOnNewsCommentPage, storeId);
            model.ShowOnProductReviewPage_OverrideForStore = await _settingService.SettingExistsAsync(captchaSettings, x => x.ShowOnProductReviewPage, storeId);
            model.ShowOnApplyVendorPage_OverrideForStore = await _settingService.SettingExistsAsync(captchaSettings, x => x.ShowOnApplyVendorPage, storeId);
            model.ShowOnForgotPasswordPage_OverrideForStore = await _settingService.SettingExistsAsync(captchaSettings, x => x.ShowOnForgotPasswordPage, storeId);
            model.ShowOnForum_OverrideForStore = await _settingService.SettingExistsAsync(captchaSettings, x => x.ShowOnForum, storeId);
            model.ShowOnCheckoutPageForGuests_OverrideForStore = await _settingService.SettingExistsAsync(captchaSettings, x => x.ShowOnCheckoutPageForGuests, storeId);
            model.ReCaptchaPublicKey_OverrideForStore = await _settingService.SettingExistsAsync(captchaSettings, x => x.ReCaptchaPublicKey, storeId);
            model.ReCaptchaPrivateKey_OverrideForStore = await _settingService.SettingExistsAsync(captchaSettings, x => x.ReCaptchaPrivateKey, storeId);
            model.CaptchaType_OverrideForStore = await _settingService.SettingExistsAsync(captchaSettings, x => x.CaptchaType, storeId);
            model.ReCaptchaV3ScoreThreshold_OverrideForStore = await _settingService.SettingExistsAsync(captchaSettings, x => x.ReCaptchaV3ScoreThreshold, storeId);

            return model;
        }

        /// <summary>
        /// Prepare PDF settings model
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the pDF settings model
        /// </returns>
        protected virtual async Task<PdfSettingsModel> PreparePdfSettingsModelAsync()
        {
            //load settings for a chosen store scope
            var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var pdfSettings = await _settingService.LoadSettingAsync<PdfSettings>(storeId);

            //fill in model values from the entity
            var model = new PdfSettingsModel
            {
                LetterPageSizeEnabled = pdfSettings.LetterPageSizeEnabled,
                LogoPictureId = pdfSettings.LogoPictureId,
                DisablePdfInvoicesForPendingOrders = pdfSettings.DisablePdfInvoicesForPendingOrders,
                InvoiceFooterTextColumn1 = pdfSettings.InvoiceFooterTextColumn1,
                InvoiceFooterTextColumn2 = pdfSettings.InvoiceFooterTextColumn2
            };

            if (storeId <= 0)
                return model;

            //fill in overridden values
            model.LetterPageSizeEnabled_OverrideForStore = await _settingService.SettingExistsAsync(pdfSettings, x => x.LetterPageSizeEnabled, storeId);
            model.LogoPictureId_OverrideForStore = await _settingService.SettingExistsAsync(pdfSettings, x => x.LogoPictureId, storeId);
            model.DisablePdfInvoicesForPendingOrders_OverrideForStore = await _settingService.SettingExistsAsync(pdfSettings, x => x.DisablePdfInvoicesForPendingOrders, storeId);
            model.InvoiceFooterTextColumn1_OverrideForStore = await _settingService.SettingExistsAsync(pdfSettings, x => x.InvoiceFooterTextColumn1, storeId);
            model.InvoiceFooterTextColumn2_OverrideForStore = await _settingService.SettingExistsAsync(pdfSettings, x => x.InvoiceFooterTextColumn2, storeId);

            return model;
        }

        /// <summary>
        /// Prepare localization settings model
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the localization settings model
        /// </returns>
        protected virtual async Task<LocalizationSettingsModel> PrepareLocalizationSettingsModelAsync()
        {
            //load settings for a chosen store scope
            var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var localizationSettings = await _settingService.LoadSettingAsync<LocalizationSettings>(storeId);

            //fill in model values from the entity
            var model = new LocalizationSettingsModel
            {
                UseImagesForLanguageSelection = localizationSettings.UseImagesForLanguageSelection,
                SeoFriendlyUrlsForLanguagesEnabled = localizationSettings.SeoFriendlyUrlsForLanguagesEnabled,
                AutomaticallyDetectLanguage = localizationSettings.AutomaticallyDetectLanguage,
                LoadAllLocaleRecordsOnStartup = localizationSettings.LoadAllLocaleRecordsOnStartup,
                LoadAllLocalizedPropertiesOnStartup = localizationSettings.LoadAllLocalizedPropertiesOnStartup,
                LoadAllUrlRecordsOnStartup = localizationSettings.LoadAllUrlRecordsOnStartup
            };

            return model;
        }

        /// <summary>
        /// Prepare admin area settings model
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the admin area settings model
        /// </returns>
        protected virtual async Task<AdminAreaSettingsModel> PrepareAdminAreaSettingsModelAsync()
        {
            //load settings for a chosen store scope
            var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var adminAreaSettings = await _settingService.LoadSettingAsync<AdminAreaSettings>(storeId);

            //fill in model values from the entity
            var model = new AdminAreaSettingsModel
            {
                UseRichEditorInMessageTemplates = adminAreaSettings.UseRichEditorInMessageTemplates
            };

            //fill in overridden values
            if (storeId > 0)
            {
                model.UseRichEditorInMessageTemplates_OverrideForStore = await _settingService.SettingExistsAsync(adminAreaSettings, x => x.UseRichEditorInMessageTemplates, storeId);
            }

            return model;
        }

        /// <summary>
        /// Prepare display default menu item settings model
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the display default menu item settings model
        /// </returns>
        protected virtual async Task<DisplayDefaultMenuItemSettingsModel> PrepareDisplayDefaultMenuItemSettingsModelAsync()
        {
            //load settings for a chosen store scope
            var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var displayDefaultMenuItemSettings = await _settingService.LoadSettingAsync<DisplayDefaultMenuItemSettings>(storeId);

            //fill in model values from the entity
            var model = new DisplayDefaultMenuItemSettingsModel
            {
                DisplayHomepageMenuItem = displayDefaultMenuItemSettings.DisplayHomepageMenuItem,
                DisplayNewProductsMenuItem = displayDefaultMenuItemSettings.DisplayNewProductsMenuItem,
                DisplayProductSearchMenuItem = displayDefaultMenuItemSettings.DisplayProductSearchMenuItem,
                DisplayCustomerInfoMenuItem = displayDefaultMenuItemSettings.DisplayCustomerInfoMenuItem,
                DisplayBlogMenuItem = displayDefaultMenuItemSettings.DisplayBlogMenuItem,
                DisplayForumsMenuItem = displayDefaultMenuItemSettings.DisplayForumsMenuItem,
                DisplayContactUsMenuItem = displayDefaultMenuItemSettings.DisplayContactUsMenuItem
            };

            if (storeId <= 0)
                return model;

            //fill in overridden values
            model.DisplayHomepageMenuItem_OverrideForStore = await _settingService.SettingExistsAsync(displayDefaultMenuItemSettings, x => x.DisplayHomepageMenuItem, storeId);
            model.DisplayNewProductsMenuItem_OverrideForStore = await _settingService.SettingExistsAsync(displayDefaultMenuItemSettings, x => x.DisplayNewProductsMenuItem, storeId);
            model.DisplayProductSearchMenuItem_OverrideForStore = await _settingService.SettingExistsAsync(displayDefaultMenuItemSettings, x => x.DisplayProductSearchMenuItem, storeId);
            model.DisplayCustomerInfoMenuItem_OverrideForStore = await _settingService.SettingExistsAsync(displayDefaultMenuItemSettings, x => x.DisplayCustomerInfoMenuItem, storeId);
            model.DisplayBlogMenuItem_OverrideForStore = await _settingService.SettingExistsAsync(displayDefaultMenuItemSettings, x => x.DisplayBlogMenuItem, storeId);
            model.DisplayForumsMenuItem_OverrideForStore = await _settingService.SettingExistsAsync(displayDefaultMenuItemSettings, x => x.DisplayForumsMenuItem, storeId);
            model.DisplayContactUsMenuItem_OverrideForStore = await _settingService.SettingExistsAsync(displayDefaultMenuItemSettings, x => x.DisplayContactUsMenuItem, storeId);

            return model;
        }

        /// <summary>
        /// Prepare display default footer item settings model
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the display default footer item settings model
        /// </returns>
        protected virtual async Task<DisplayDefaultFooterItemSettingsModel> PrepareDisplayDefaultFooterItemSettingsModelAsync()
        {
            //load settings for a chosen store scope
            var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var displayDefaultFooterItemSettings = await _settingService.LoadSettingAsync<DisplayDefaultFooterItemSettings>(storeId);

            //fill in model values from the entity
            var model = new DisplayDefaultFooterItemSettingsModel
            {
                DisplaySitemapFooterItem = displayDefaultFooterItemSettings.DisplaySitemapFooterItem,
                DisplayContactUsFooterItem = displayDefaultFooterItemSettings.DisplayContactUsFooterItem,
                DisplayProductSearchFooterItem = displayDefaultFooterItemSettings.DisplayProductSearchFooterItem,
                DisplayNewsFooterItem = displayDefaultFooterItemSettings.DisplayNewsFooterItem,
                DisplayBlogFooterItem = displayDefaultFooterItemSettings.DisplayBlogFooterItem,
                DisplayForumsFooterItem = displayDefaultFooterItemSettings.DisplayForumsFooterItem,
                DisplayRecentlyViewedProductsFooterItem = displayDefaultFooterItemSettings.DisplayRecentlyViewedProductsFooterItem,
                DisplayCompareProductsFooterItem = displayDefaultFooterItemSettings.DisplayCompareProductsFooterItem,
                DisplayNewProductsFooterItem = displayDefaultFooterItemSettings.DisplayNewProductsFooterItem,
                DisplayCustomerInfoFooterItem = displayDefaultFooterItemSettings.DisplayCustomerInfoFooterItem,
                DisplayCustomerOrdersFooterItem = displayDefaultFooterItemSettings.DisplayCustomerOrdersFooterItem,
                DisplayCustomerAddressesFooterItem = displayDefaultFooterItemSettings.DisplayCustomerAddressesFooterItem,
                DisplayShoppingCartFooterItem = displayDefaultFooterItemSettings.DisplayShoppingCartFooterItem,
                DisplayWishlistFooterItem = displayDefaultFooterItemSettings.DisplayWishlistFooterItem,
                DisplayApplyVendorAccountFooterItem = displayDefaultFooterItemSettings.DisplayApplyVendorAccountFooterItem
            };

            if (storeId <= 0)
                return model;

            //fill in overridden values
            model.DisplaySitemapFooterItem_OverrideForStore = await _settingService.SettingExistsAsync(displayDefaultFooterItemSettings, x => x.DisplaySitemapFooterItem, storeId);
            model.DisplayContactUsFooterItem_OverrideForStore = await _settingService.SettingExistsAsync(displayDefaultFooterItemSettings, x => x.DisplayContactUsFooterItem, storeId);
            model.DisplayProductSearchFooterItem_OverrideForStore = await _settingService.SettingExistsAsync(displayDefaultFooterItemSettings, x => x.DisplayProductSearchFooterItem, storeId);
            model.DisplayNewsFooterItem_OverrideForStore = await _settingService.SettingExistsAsync(displayDefaultFooterItemSettings, x => x.DisplayNewsFooterItem, storeId);
            model.DisplayBlogFooterItem_OverrideForStore = await _settingService.SettingExistsAsync(displayDefaultFooterItemSettings, x => x.DisplayBlogFooterItem, storeId);
            model.DisplayForumsFooterItem_OverrideForStore = await _settingService.SettingExistsAsync(displayDefaultFooterItemSettings, x => x.DisplayForumsFooterItem, storeId);
            model.DisplayRecentlyViewedProductsFooterItem_OverrideForStore = await _settingService.SettingExistsAsync(displayDefaultFooterItemSettings, x => x.DisplayRecentlyViewedProductsFooterItem, storeId);
            model.DisplayCompareProductsFooterItem_OverrideForStore = await _settingService.SettingExistsAsync(displayDefaultFooterItemSettings, x => x.DisplayCompareProductsFooterItem, storeId);
            model.DisplayNewProductsFooterItem_OverrideForStore = await _settingService.SettingExistsAsync(displayDefaultFooterItemSettings, x => x.DisplayNewProductsFooterItem, storeId);
            model.DisplayCustomerInfoFooterItem_OverrideForStore = await _settingService.SettingExistsAsync(displayDefaultFooterItemSettings, x => x.DisplayCustomerInfoFooterItem, storeId);
            model.DisplayCustomerOrdersFooterItem_OverrideForStore = await _settingService.SettingExistsAsync(displayDefaultFooterItemSettings, x => x.DisplayCustomerOrdersFooterItem, storeId);
            model.DisplayCustomerAddressesFooterItem_OverrideForStore = await _settingService.SettingExistsAsync(displayDefaultFooterItemSettings, x => x.DisplayCustomerAddressesFooterItem, storeId);
            model.DisplayShoppingCartFooterItem_OverrideForStore = await _settingService.SettingExistsAsync(displayDefaultFooterItemSettings, x => x.DisplayShoppingCartFooterItem, storeId);
            model.DisplayWishlistFooterItem_OverrideForStore = await _settingService.SettingExistsAsync(displayDefaultFooterItemSettings, x => x.DisplayWishlistFooterItem, storeId);
            model.DisplayApplyVendorAccountFooterItem_OverrideForStore = await _settingService.SettingExistsAsync(displayDefaultFooterItemSettings, x => x.DisplayApplyVendorAccountFooterItem, storeId);

            return model;
        }

        /// <summary>
        /// Prepare setting model to add
        /// </summary>
        /// <param name="model">Setting model to add</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected virtual async Task PrepareAddSettingModelAsync(SettingModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            //prepare available stores
            await _baseAdminModelFactory.PrepareStoresAsync(model.AvailableStores);
        }

        /// <summary>
        /// Prepare custom HTML settings model
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the custom HTML settings model
        /// </returns>
        protected virtual async Task<CustomHtmlSettingsModel> PrepareCustomHtmlSettingsModelAsync()
        {
            //load settings for a chosen store scope
            var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var commonSettings = await _settingService.LoadSettingAsync<CommonSettings>(storeId);

            //fill in model values from the entity
            var model = new CustomHtmlSettingsModel
            {
                HeaderCustomHtml = commonSettings.HeaderCustomHtml,
                FooterCustomHtml = commonSettings.FooterCustomHtml
            };

            //fill in overridden values
            if (storeId > 0)
            {
                model.HeaderCustomHtml_OverrideForStore = await _settingService.SettingExistsAsync(commonSettings, x => x.HeaderCustomHtml, storeId);
                model.FooterCustomHtml_OverrideForStore = await _settingService.SettingExistsAsync(commonSettings, x => x.FooterCustomHtml, storeId);
            }

            return model;
        }

        /// <summary>
        /// Prepare robots.txt settings model
        /// </summary>
        /// <param name="model">robots.txt model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the robots.txt settings model
        /// </returns>
        protected virtual async Task<RobotsTxtSettingsModel> PrepareRobotsTxtSettingsModelAsync(RobotsTxtSettingsModel model = null)
        {
            var additionsInstruction =
                string.Format(
                    await _localizationService.GetResourceAsync("Admin.Configuration.Settings.GeneralCommon.RobotsAdditionsInstruction"),
                    RobotsTxtDefaults.RobotsAdditionsFileName);

            if (_fileProvider.FileExists(_fileProvider.Combine(_fileProvider.MapPath("~/wwwroot"), RobotsTxtDefaults.RobotsCustomFileName)))
                return new RobotsTxtSettingsModel { CustomFileExists = string.Format(await _localizationService.GetResourceAsync("Admin.Configuration.Settings.GeneralCommon.RobotsCustomFileExists"), RobotsTxtDefaults.RobotsCustomFileName), AdditionsInstruction = additionsInstruction };

            //load settings for a chosen store scope
            var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var robotsTxtSettings = await _settingService.LoadSettingAsync<RobotsTxtSettings>(storeId);
            
            model ??= new RobotsTxtSettingsModel
            {
                AllowSitemapXml = robotsTxtSettings.AllowSitemapXml,
                DisallowPaths = string.Join(Environment.NewLine, robotsTxtSettings.DisallowPaths),
                LocalizableDisallowPaths =
                    string.Join(Environment.NewLine, robotsTxtSettings.LocalizableDisallowPaths),
                DisallowLanguages = robotsTxtSettings.DisallowLanguages.ToList(),
                AdditionsRules = string.Join(Environment.NewLine, robotsTxtSettings.AdditionsRules),
                AvailableLanguages = new List<SelectListItem>()
            };

            if (!model.AvailableLanguages.Any())
                (model.AvailableLanguages as List<SelectListItem>)?.AddRange((await _languageService.GetAllLanguagesAsync(storeId: storeId)).Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = p.Name
                }));

            model.AdditionsInstruction = additionsInstruction;

            if (storeId <= 0)
                return model;

            model.AdditionsRules_OverrideForStore = await _settingService.SettingExistsAsync(robotsTxtSettings, x => x.AdditionsRules, storeId);
            model.AllowSitemapXml_OverrideForStore = await _settingService.SettingExistsAsync(robotsTxtSettings, x => x.AllowSitemapXml, storeId);
            model.DisallowLanguages_OverrideForStore = await _settingService.SettingExistsAsync(robotsTxtSettings, x => x.DisallowLanguages, storeId);
            model.DisallowPaths_OverrideForStore = await _settingService.SettingExistsAsync(robotsTxtSettings, x => x.DisallowPaths, storeId);
            model.LocalizableDisallowPaths_OverrideForStore = await _settingService.SettingExistsAsync(robotsTxtSettings, x => x.LocalizableDisallowPaths, storeId);

            return model;
        }


        protected virtual async Task PrepareLeaveTypeListAsync(LeaveSettingsModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            model.AvailableLeaveType.Add(new SelectListItem
            {
                Text = "Select",
                Value = null
            });
            var leaveTypeName = "";
            var leaves = await _leaveTypeService.GetAllLeaveTypeAsync(leaveTypeName);
            foreach (var p in leaves)
            {
                model.AvailableLeaveType.Add(new SelectListItem
                {
                    Text = p.Type,
                    Value = p.Id.ToString()
                });
            }
        }

        protected virtual async Task PrepareDepartmentListAsync(TimeSheetSettingsModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));
        
         
            var leaves = await _departmentService.GetAllDepartmentsAsync();
            foreach (var p in leaves)
            {
                model.AvailableDepartments.Add(new SelectListItem
                {
                    Text = p.Name,
                    Value = p.Id.ToString()
                });
            }
        }

        protected virtual async Task PrepareFeedBackShowListAsync(TeamPerformanceSettingsModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));



            var feedback = Enum.GetValues(typeof(FeedBackShow))
                .Cast<FeedBackShow>()
                .Select(e => new SelectListItem
                {
                    Value = ((int)e).ToString(),
                    Text = e.ToString()
                }).ToList();


           

            model.AvailableFeedBackShow = feedback;
           
        }

        protected virtual async Task PrepareWeekDayListAsync(MonthlyReportSettingsModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            model.WeekDayList = Enum.GetValues(typeof(DayOfWeek))
       .Cast<DayOfWeek>()
       .Select(d => new SelectListItem
       {
           Text = d.ToString(), 
           Value = ((int)d).ToString() 
       })
       .ToList();

        }

        public virtual async Task PrepareProjectListAsync(MonthlyReportSettingsModel searchmodel)
        {
            if (searchmodel == null)
                throw new ArgumentNullException(nameof(searchmodel));


            searchmodel.ProjectList.Add(new SelectListItem {
            Text="Select",
            Value="0"
            
            });
            var projects = await _projectsService.GetAllProjectsAsync("");
            foreach (var p in projects)
            {
                
                    searchmodel.ProjectList.Add(new SelectListItem
                    {
                        Text = p.ProjectTitle,
                        Value = p.Id.ToString()
                    });
                
            }
        }

        protected virtual async Task PrepareDateListAsync(TeamPerformanceSettingsModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            // Create a list of days from 1 to 28
            var dateList = Enumerable.Range(1, 28)
                                     .Select(day => new SelectListItem
                                     {
                                         Text = day.ToString(),
                                         Value = day.ToString()
                                     })
                                     .ToList();

            // Assign the date list to the model
            model.AvailableDates = dateList;






        }

        protected virtual async Task PrepareDesignationListAsync(EmployeeSettingsModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));
            model.AvailableRoles.Add(new SelectListItem
            {
                Text = "Select",
                Value = "0"
            });
            var designations = await _designationService.GetAllDesignationAsync();
            foreach (var d in designations)
            {
                model.AvailableRoles.Add(new SelectListItem
                {
                    Text = d.Name,
                    Value = d.Id.ToString()
                });
            }
        }
        #endregion

        #region Methods

        /// <summary>
        /// Prepare app settings model
        /// </summary>
        /// <param name="model">AppSettings model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the app settings model
        /// </returns>
        public virtual async Task<AppSettingsModel> PrepareAppSettingsModel(AppSettingsModel model = null)
        {
            model ??= new AppSettingsModel
            {
                CacheConfigModel = _appSettings.Get<CacheConfig>().ToConfigModel<CacheConfigModel>(),
                HostingConfigModel = _appSettings.Get<HostingConfig>().ToConfigModel<HostingConfigModel>(),
                DistributedCacheConfigModel = _appSettings.Get<DistributedCacheConfig>().ToConfigModel<DistributedCacheConfigModel>(),
                AzureBlobConfigModel = _appSettings.Get<AzureBlobConfig>().ToConfigModel<AzureBlobConfigModel>(),
                InstallationConfigModel = _appSettings.Get<InstallationConfig>().ToConfigModel<InstallationConfigModel>(),
                PluginConfigModel = _appSettings.Get<PluginConfig>().ToConfigModel<PluginConfigModel>(),
                CommonConfigModel = _appSettings.Get<CommonConfig>().ToConfigModel<CommonConfigModel>(),
                DataConfigModel = _appSettings.Get<DataConfig>().ToConfigModel<DataConfigModel>(),
                WebOptimizerConfigModel = _appSettings.Get<WebOptimizerConfig>().ToConfigModel<WebOptimizerConfigModel>(),
            };

            model.DistributedCacheConfigModel.DistributedCacheTypeValues = await _appSettings.Get<DistributedCacheConfig>().DistributedCacheType.ToSelectListAsync();

            model.DataConfigModel.DataProviderTypeValues = await _appSettings.Get<DataConfig>().DataProvider.ToSelectListAsync();

            //Since we decided to use the naming of the DB connections section as in the .net core - "ConnectionStrings",
            //we are forced to adjust our internal model naming to this convention in this check.
            model.EnvironmentVariables.AddRange(from property in model.GetType().GetProperties()
                                                where property.Name != nameof(AppSettingsModel.EnvironmentVariables)
                                                from pp in property.PropertyType.GetProperties()
                                                where Environment.GetEnvironmentVariables().Contains($"{property.Name.Replace("Model", "").Replace("DataConfig", "ConnectionStrings")}__{pp.Name}")
                                                select $"{property.Name}_{pp.Name}");
            return model;
        }

        /// <summary>
        /// Prepare blog settings model
        /// </summary>
        /// <param name="model">Blog settings model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the blog settings model
        /// </returns>
        public virtual async Task<BlogSettingsModel> PrepareBlogSettingsModelAsync(BlogSettingsModel model = null)
        {
            //load settings for a chosen store scope
            var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var blogSettings = await _settingService.LoadSettingAsync<BlogSettings>(storeId);

            //fill in model values from the entity
            model ??= blogSettings.ToSettingsModel<BlogSettingsModel>();

            //fill in additional values (not existing in the entity)
            model.ActiveStoreScopeConfiguration = storeId;

            if (storeId <= 0)
                return model;

            //fill in overridden values
            model.Enabled_OverrideForStore = await _settingService.SettingExistsAsync(blogSettings, x => x.Enabled, storeId);
            model.PostsPageSize_OverrideForStore = await _settingService.SettingExistsAsync(blogSettings, x => x.PostsPageSize, storeId);
            model.AllowNotRegisteredUsersToLeaveComments_OverrideForStore = await _settingService.SettingExistsAsync(blogSettings, x => x.AllowNotRegisteredUsersToLeaveComments, storeId);
            model.NotifyAboutNewBlogComments_OverrideForStore = await _settingService.SettingExistsAsync(blogSettings, x => x.NotifyAboutNewBlogComments, storeId);
            model.NumberOfTags_OverrideForStore = await _settingService.SettingExistsAsync(blogSettings, x => x.NumberOfTags, storeId);
            model.ShowHeaderRssUrl_OverrideForStore = await _settingService.SettingExistsAsync(blogSettings, x => x.ShowHeaderRssUrl, storeId);
            model.BlogCommentsMustBeApproved_OverrideForStore = await _settingService.SettingExistsAsync(blogSettings, x => x.BlogCommentsMustBeApproved, storeId);

            return model;
        }

        /// <summary>
        /// Prepare forum settings model
        /// </summary>
        /// <param name="model">Forum settings model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the forum settings model
        /// </returns>
        public virtual async Task<ForumSettingsModel> PrepareForumSettingsModelAsync(ForumSettingsModel model = null)
        {
            //load settings for a chosen store scope
            var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var forumSettings = await _settingService.LoadSettingAsync<ForumSettings>(storeId);

            //fill in model values from the entity
            model ??= forumSettings.ToSettingsModel<ForumSettingsModel>();

            //fill in additional values (not existing in the entity)
            model.ActiveStoreScopeConfiguration = storeId;
            model.ForumEditorValues = await forumSettings.ForumEditor.ToSelectListAsync();

            if (storeId <= 0)
                return model;

            //fill in overridden values
            model.ForumsEnabled_OverrideForStore = await _settingService.SettingExistsAsync(forumSettings, x => x.ForumsEnabled, storeId);
            model.RelativeDateTimeFormattingEnabled_OverrideForStore = await _settingService.SettingExistsAsync(forumSettings, x => x.RelativeDateTimeFormattingEnabled, storeId);
            model.ShowCustomersPostCount_OverrideForStore = await _settingService.SettingExistsAsync(forumSettings, x => x.ShowCustomersPostCount, storeId);
            model.AllowGuestsToCreatePosts_OverrideForStore = await _settingService.SettingExistsAsync(forumSettings, x => x.AllowGuestsToCreatePosts, storeId);
            model.AllowGuestsToCreateTopics_OverrideForStore = await _settingService.SettingExistsAsync(forumSettings, x => x.AllowGuestsToCreateTopics, storeId);
            model.AllowCustomersToEditPosts_OverrideForStore = await _settingService.SettingExistsAsync(forumSettings, x => x.AllowCustomersToEditPosts, storeId);
            model.AllowCustomersToDeletePosts_OverrideForStore = await _settingService.SettingExistsAsync(forumSettings, x => x.AllowCustomersToDeletePosts, storeId);
            model.AllowPostVoting_OverrideForStore = await _settingService.SettingExistsAsync(forumSettings, x => x.AllowPostVoting, storeId);
            model.MaxVotesPerDay_OverrideForStore = await _settingService.SettingExistsAsync(forumSettings, x => x.MaxVotesPerDay, storeId);
            model.AllowCustomersToManageSubscriptions_OverrideForStore = await _settingService.SettingExistsAsync(forumSettings, x => x.AllowCustomersToManageSubscriptions, storeId);
            model.TopicsPageSize_OverrideForStore = await _settingService.SettingExistsAsync(forumSettings, x => x.TopicsPageSize, storeId);
            model.PostsPageSize_OverrideForStore = await _settingService.SettingExistsAsync(forumSettings, x => x.PostsPageSize, storeId);
            model.ForumEditor_OverrideForStore = await _settingService.SettingExistsAsync(forumSettings, x => x.ForumEditor, storeId);
            model.SignaturesEnabled_OverrideForStore = await _settingService.SettingExistsAsync(forumSettings, x => x.SignaturesEnabled, storeId);
            model.AllowPrivateMessages_OverrideForStore = await _settingService.SettingExistsAsync(forumSettings, x => x.AllowPrivateMessages, storeId);
            model.ShowAlertForPM_OverrideForStore = await _settingService.SettingExistsAsync(forumSettings, x => x.ShowAlertForPM, storeId);
            model.NotifyAboutPrivateMessages_OverrideForStore = await _settingService.SettingExistsAsync(forumSettings, x => x.NotifyAboutPrivateMessages, storeId);
            model.ActiveDiscussionsFeedEnabled_OverrideForStore = await _settingService.SettingExistsAsync(forumSettings, x => x.ActiveDiscussionsFeedEnabled, storeId);
            model.ActiveDiscussionsFeedCount_OverrideForStore = await _settingService.SettingExistsAsync(forumSettings, x => x.ActiveDiscussionsFeedCount, storeId);
            model.ForumFeedsEnabled_OverrideForStore = await _settingService.SettingExistsAsync(forumSettings, x => x.ForumFeedsEnabled, storeId);
            model.ForumFeedCount_OverrideForStore = await _settingService.SettingExistsAsync(forumSettings, x => x.ForumFeedCount, storeId);
            model.SearchResultsPageSize_OverrideForStore = await _settingService.SettingExistsAsync(forumSettings, x => x.SearchResultsPageSize, storeId);
            model.ActiveDiscussionsPageSize_OverrideForStore = await _settingService.SettingExistsAsync(forumSettings, x => x.ActiveDiscussionsPageSize, storeId);

            return model;
        }

        /// <summary>
        /// Prepare news settings model
        /// </summary>
        /// <param name="model">News settings model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the news settings model
        /// </returns>
        public virtual async Task<NewsSettingsModel> PrepareNewsSettingsModelAsync(NewsSettingsModel model = null)
        {
            //load settings for a chosen store scope
            var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var newsSettings = await _settingService.LoadSettingAsync<NewsSettings>(storeId);

            //fill in model values from the entity
            model ??= newsSettings.ToSettingsModel<NewsSettingsModel>();

            //fill in additional values (not existing in the entity)
            model.ActiveStoreScopeConfiguration = storeId;

            if (storeId <= 0)
                return model;

            //fill in overridden values
            model.Enabled_OverrideForStore = await _settingService.SettingExistsAsync(newsSettings, x => x.Enabled, storeId);
            model.AllowNotRegisteredUsersToLeaveComments_OverrideForStore = await _settingService.SettingExistsAsync(newsSettings, x => x.AllowNotRegisteredUsersToLeaveComments, storeId);
            model.NotifyAboutNewNewsComments_OverrideForStore = await _settingService.SettingExistsAsync(newsSettings, x => x.NotifyAboutNewNewsComments, storeId);
            model.ShowNewsOnMainPage_OverrideForStore = await _settingService.SettingExistsAsync(newsSettings, x => x.ShowNewsOnMainPage, storeId);
            model.MainPageNewsCount_OverrideForStore = await _settingService.SettingExistsAsync(newsSettings, x => x.MainPageNewsCount, storeId);
            model.NewsArchivePageSize_OverrideForStore = await _settingService.SettingExistsAsync(newsSettings, x => x.NewsArchivePageSize, storeId);
            model.ShowHeaderRssUrl_OverrideForStore = await _settingService.SettingExistsAsync(newsSettings, x => x.ShowHeaderRssUrl, storeId);
            model.NewsCommentsMustBeApproved_OverrideForStore = await _settingService.SettingExistsAsync(newsSettings, x => x.NewsCommentsMustBeApproved, storeId);

            return model;
        }

        /// <summary>
        /// Prepare media settings model
        /// </summary>
        /// <param name="model">Media settings model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the media settings model
        /// </returns>
        public virtual async Task<MediaSettingsModel> PrepareMediaSettingsModelAsync(MediaSettingsModel model = null)
        {
            //load settings for a chosen store scope
            var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var mediaSettings = await _settingService.LoadSettingAsync<MediaSettings>(storeId);

            //fill in model values from the entity
            model ??= mediaSettings.ToSettingsModel<MediaSettingsModel>();

            //fill in additional values (not existing in the entity)
            model.ActiveStoreScopeConfiguration = storeId;
            model.PicturesStoredIntoDatabase = await _pictureService.IsStoreInDbAsync();

            if (storeId <= 0)
                return model;

            //fill in overridden values
            model.AvatarPictureSize_OverrideForStore = await _settingService.SettingExistsAsync(mediaSettings, x => x.AvatarPictureSize, storeId);
            model.MaximumImageSize_OverrideForStore = await _settingService.SettingExistsAsync(mediaSettings, x => x.MaximumImageSize, storeId);
            model.MultipleThumbDirectories_OverrideForStore = await _settingService.SettingExistsAsync(mediaSettings, x => x.MultipleThumbDirectories, storeId);
            model.DefaultImageQuality_OverrideForStore = await _settingService.SettingExistsAsync(mediaSettings, x => x.DefaultImageQuality, storeId);
            model.ImportProductImagesUsingHash_OverrideForStore = await _settingService.SettingExistsAsync(mediaSettings, x => x.ImportProductImagesUsingHash, storeId);
            model.DefaultPictureZoomEnabled_OverrideForStore = await _settingService.SettingExistsAsync(mediaSettings, x => x.DefaultPictureZoomEnabled, storeId);
            model.AllowSVGUploads_OverrideForStore = await _settingService.SettingExistsAsync(mediaSettings, x => x.AllowSVGUploads, storeId);
          
            return model;
        }

        /// <summary>
        /// Prepare customer user settings model
        /// </summary>
        /// <param name="model">Customer user settings model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the customer user settings model
        /// </returns>
        public virtual async Task<CustomerUserSettingsModel> PrepareCustomerUserSettingsModelAsync(CustomerUserSettingsModel model = null)
        {
            model ??= new CustomerUserSettingsModel
            {
                ActiveStoreScopeConfiguration = await _storeContext.GetActiveStoreScopeConfigurationAsync()
            };

            //prepare customer settings model
            model.CustomerSettings = await PrepareCustomerSettingsModelAsync();

            //prepare multi-factor authentication settings model
            model.MultiFactorAuthenticationSettings = await PrepareMultiFactorAuthenticationSettingsModelAsync();

            //prepare address settings model
            model.AddressSettings = await PrepareAddressSettingsModelAsync();

            //prepare date time settings model
            model.DateTimeSettings = await PrepareDateTimeSettingsModelAsync();

            //prepare external authentication settings model
            model.ExternalAuthenticationSettings = await PrepareExternalAuthenticationSettingsModelAsync();

            //prepare nested search models
            await _customerAttributeModelFactory.PrepareCustomerAttributeSearchModelAsync(model.CustomerAttributeSearchModel);
            await _addressAttributeModelFactory.PrepareAddressAttributeSearchModelAsync(model.AddressAttributeSearchModel);

            return model;
        }

        /// <summary>
        /// Prepare GDPR settings model
        /// </summary>
        /// <param name="model">Gdpr settings model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the gDPR settings model
        /// </returns>
        public virtual async Task<GdprSettingsModel> PrepareGdprSettingsModelAsync(GdprSettingsModel model = null)
        {
            //load settings for a chosen store scope
            var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var gdprSettings = await _settingService.LoadSettingAsync<GdprSettings>(storeId);

            //fill in model values from the entity
            model ??= gdprSettings.ToSettingsModel<GdprSettingsModel>();

            //fill in additional values (not existing in the entity)
            model.ActiveStoreScopeConfiguration = storeId;

            //prepare nested search model
            await PrepareGdprConsentSearchModelAsync(model.GdprConsentSearchModel);

            if (storeId <= 0)
                return model;

            //fill in overridden values
            model.GdprEnabled_OverrideForStore = await _settingService.SettingExistsAsync(gdprSettings, x => x.GdprEnabled, storeId);
            model.LogPrivacyPolicyConsent_OverrideForStore = await _settingService.SettingExistsAsync(gdprSettings, x => x.LogPrivacyPolicyConsent, storeId);
            model.LogNewsletterConsent_OverrideForStore = await _settingService.SettingExistsAsync(gdprSettings, x => x.LogNewsletterConsent, storeId);
            model.LogUserProfileChanges_OverrideForStore = await _settingService.SettingExistsAsync(gdprSettings, x => x.LogUserProfileChanges, storeId);
            model.DeleteInactiveCustomersAfterMonths_OverrideForStore = await _settingService.SettingExistsAsync(gdprSettings, x => x.DeleteInactiveCustomersAfterMonths, storeId);

            return model;
        }

        /// <summary>
        /// Prepare paged GDPR consent list model
        /// </summary>
        /// <param name="searchModel">GDPR search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the gDPR consent list model
        /// </returns>
        public virtual async Task<GdprConsentListModel> PrepareGdprConsentListModelAsync(GdprConsentSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get sort options
            var consentList = (await _gdprService.GetAllConsentsAsync()).ToPagedList(searchModel);

            //prepare list model
            var model = await new GdprConsentListModel().PrepareToGridAsync(searchModel, consentList, () =>
            {
                return consentList.SelectAwait(async consent =>
                {
                    var gdprConsentModel = consent.ToModel<GdprConsentModel>();

                    var gdprConsent = await _gdprService.GetConsentByIdAsync(gdprConsentModel.Id);
                    gdprConsentModel.Message = await _localizationService.GetLocalizedAsync(gdprConsent, entity => entity.Message);
                    gdprConsentModel.RequiredMessage = await _localizationService.GetLocalizedAsync(gdprConsent, entity => entity.RequiredMessage);

                    return gdprConsentModel;
                });
            });

            return model;
        }

        /// <summary>
        /// Prepare GDPR consent model
        /// </summary>
        /// <param name="model">GDPR consent model</param>
        /// <param name="gdprConsent">GDPR consent</param>
        /// <param name="excludeProperties">Whether to exclude populating of some properties of model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the gDPR consent model
        /// </returns>
        public virtual async Task<GdprConsentModel> PrepareGdprConsentModelAsync(GdprConsentModel model, GdprConsent gdprConsent, bool excludeProperties = false)
        {
            Func<GdprConsentLocalizedModel, int, Task> localizedModelConfiguration = null;

            //fill in model values from the entity
            if (gdprConsent != null)
            {
                model ??= gdprConsent.ToModel<GdprConsentModel>();

                //define localized model configuration action
                localizedModelConfiguration = async (locale, languageId) =>
                {
                    locale.Message = await _localizationService.GetLocalizedAsync(gdprConsent, entity => entity.Message, languageId, false, false);
                    locale.RequiredMessage = await _localizationService.GetLocalizedAsync(gdprConsent, entity => entity.RequiredMessage, languageId, false, false);
                };
            }

            //set default values for the new model
            if (gdprConsent == null)
                model.DisplayOrder = 1;

            //prepare localized models
            if (!excludeProperties)
                model.Locales = await _localizedModelFactory.PrepareLocalizedModelsAsync(localizedModelConfiguration);

            return model;
        }

        /// <summary>
        /// Prepare general and common settings model
        /// </summary>
        /// <param name="model">General common settings model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the general and common settings model
        /// </returns>
        public virtual async Task<GeneralCommonSettingsModel> PrepareGeneralCommonSettingsModelAsync(GeneralCommonSettingsModel model = null)
        {
            model ??= new GeneralCommonSettingsModel
            {
                ActiveStoreScopeConfiguration = await _storeContext.GetActiveStoreScopeConfigurationAsync()
            };

            //prepare store information settings model
            model.StoreInformationSettings = await PrepareStoreInformationSettingsModelAsync();

            //prepare Sitemap settings model
            model.SitemapSettings = await PrepareSitemapSettingsModelAsync();

            //prepare Minification settings model
            model.MinificationSettings = await PrepareMinificationSettingsModelAsync();

            //prepare SEO settings model
            model.SeoSettings = await PrepareSeoSettingsModelAsync();

            //prepare security settings model
            model.SecuritySettings = await PrepareSecuritySettingsModelAsync();

            //prepare robots.txt settings model
            model.RobotsTxtSettings = await PrepareRobotsTxtSettingsModelAsync();

            //prepare captcha settings model
            model.CaptchaSettings = await PrepareCaptchaSettingsModelAsync();

            //prepare PDF settings model
            model.PdfSettings = await PreparePdfSettingsModelAsync();

            //prepare localization settings model
            model.LocalizationSettings = await PrepareLocalizationSettingsModelAsync();

            //prepare admin area settings model
            model.AdminAreaSettings = await PrepareAdminAreaSettingsModelAsync();

            //prepare display default menu item settings model
            model.DisplayDefaultMenuItemSettings = await PrepareDisplayDefaultMenuItemSettingsModelAsync();

            //prepare display default footer item settings model
            model.DisplayDefaultFooterItemSettings = await PrepareDisplayDefaultFooterItemSettingsModelAsync();

            //prepare custom HTML settings model
            model.CustomHtmlSettings = await PrepareCustomHtmlSettingsModelAsync();

            return model;
        }

        /// <summary>
        /// Prepare setting search model
        /// </summary>
        /// <param name="searchModel">Setting search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the setting search model
        /// </returns>
        public virtual async Task<SettingSearchModel> PrepareSettingSearchModelAsync(SettingSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //prepare model to add
            await PrepareAddSettingModelAsync(searchModel.AddSetting);

            //prepare page parameters
            searchModel.SetGridPageSize();

            return searchModel;
        }

        /// <summary>
        /// Prepare paged setting list model
        /// </summary>
        /// <param name="searchModel">Setting search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the setting list model
        /// </returns>
        public virtual async Task<SettingListModel> PrepareSettingListModelAsync(SettingSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get settings
            var settings = (await _settingService.GetAllSettingsAsync()).AsQueryable();

            //filter settings
            if (!string.IsNullOrEmpty(searchModel.SearchSettingName))
                settings = settings.Where(setting => setting.Name.ToLowerInvariant().Contains(searchModel.SearchSettingName.ToLowerInvariant()));
            if (!string.IsNullOrEmpty(searchModel.SearchSettingValue))
                settings = settings.Where(setting => setting.Value.ToLowerInvariant().Contains(searchModel.SearchSettingValue.ToLowerInvariant()));

            var pagedSettings = settings.ToList().ToPagedList(searchModel);

            //prepare list model
            var model = await new SettingListModel().PrepareToGridAsync(searchModel, pagedSettings, () =>
            {
                return pagedSettings.SelectAwait(async setting =>
                {
                    //fill in model values from the entity
                    var settingModel = setting.ToModel<SettingModel>();

                    //fill in additional values (not existing in the entity)
                    settingModel.Store = setting.StoreId > 0
                        ? (await _storeService.GetStoreByIdAsync(setting.StoreId))?.Name ?? "Deleted"
                        : await _localizationService.GetResourceAsync("Admin.Configuration.Settings.AllSettings.Fields.StoreName.AllStores");

                    return settingModel;
                });
            });

            return model;
        }

        /// <summary>
        /// Prepare setting mode model
        /// </summary>
        /// <param name="modeName">Mode name</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the setting mode model
        /// </returns>
        public virtual async Task<SettingModeModel> PrepareSettingModeModelAsync(string modeName)
        {
            var model = new SettingModeModel
            {
                ModeName = modeName,
                Enabled = await _genericAttributeService.GetAttributeAsync<bool>(await _workContext.GetCurrentCustomerAsync(), modeName)
            };

            return model;
        }

        /// <summary>
        /// Prepare store scope configuration model
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the store scope configuration model
        /// </returns>
        public virtual async Task<StoreScopeConfigurationModel> PrepareStoreScopeConfigurationModelAsync()
        {
            var model = new StoreScopeConfigurationModel
            {
                Stores = (await _storeService.GetAllStoresAsync()).Select(store => store.ToModel<StoreModel>()).ToList(),
                StoreId = await _storeContext.GetActiveStoreScopeConfigurationAsync()
            };

            return model;
        }


        public virtual async Task<LeaveSettingsModel> PrepareLeaveSettingsModelAsync(LeaveSettingsModel model = null)
        {
            //load settings for a chosen store scope
            var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var blogSettings = await _settingService.LoadSettingAsync<LeaveSettings>(storeId);

            //fill in model values from the entity
            model ??= blogSettings.ToSettingsModel<LeaveSettingsModel>();

            //fill in additional values (not existing in the entity)
            model.ActiveStoreScopeConfiguration = storeId;
            await PrepareLeaveTypeListAsync(model);

            if (storeId <= 0)
                return model;

            //fill in overridden values
            model.SendEmailToAllProjectLeaders_OverrideForStore = await _settingService.SettingExistsAsync(blogSettings, x => x.SendEmailToAllProjectLeaders, storeId);
            model.SendEmailToAllProjectManager_OverrideForStore = await _settingService.SettingExistsAsync(blogSettings, x => x.SendEmailToAllProjectManager, storeId);
            model.CommonEmails_OverrideForStore = await _settingService.SettingExistsAsync(blogSettings, x => x.CommonEmails, storeId);
            model.HrEmail_OverrideForStore = await _settingService.SettingExistsAsync(blogSettings, x => x.HrEmail, storeId);
            model.SeletedLeaveTypeId_OverrideForStore = await _settingService.SettingExistsAsync(blogSettings, x => x.SeletedLeaveTypeId, storeId);
            model.LastUpdateBalance_OverrideForStore = await _settingService.SettingExistsAsync(blogSettings, x => x.LastUpdateBalance, storeId);
            model.LeaveTestDate_OverrideForStore = await _settingService.SettingExistsAsync(blogSettings, x => x.LeaveTestDate, storeId);
            model.AddMonthlyLeaveDay_OverrideForStore = await _settingService.SettingExistsAsync(blogSettings, x => x.AddMonthlyLeaveDay, storeId);

            await _settingService.ClearCacheAsync();

            return model;
        }


        public virtual async Task<MonthlyReportSettingsModel> PrepareMonthlyReportSettingsModelAsync(MonthlyReportSettingsModel model = null)
        {
            //load settings for a chosen store scope
            var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var blogSettings = await _settingService.LoadSettingAsync<MonthlyReportSetting>(storeId);

            //fill in model values from the entity
            model ??= blogSettings.ToSettingsModel<MonthlyReportSettingsModel>();

            //fill in additional values (not existing in the entity)
            model.ActiveStoreScopeConfiguration = storeId;
            await PrepareWeekDayListAsync(model);
            await PrepareProjectListAsync(model);
            if (storeId <= 0)
                return model;

            //fill in overridden values
            model.AllowedVariations_OverrideForStore = await _settingService.SettingExistsAsync(blogSettings, x => x.AllowedVariations, storeId);
            model.AllowedQABillableHours_OverrideForStore = await _settingService.SettingExistsAsync(blogSettings, x => x.AllowedQABillableHours, storeId);
            model.DayTime_From_OverrideForStore = await _settingService.SettingExistsAsync(blogSettings, x => x.DayTime_From, storeId);
            model.DayTime_To_OverrideForStore = await _settingService.SettingExistsAsync(blogSettings, x => x.DayTime_To, storeId);
            model.WeekDay_OverrideForStore = await _settingService.SettingExistsAsync(blogSettings, x => x.WeekDay, storeId);
            model.SendReportToEmployee_OverrideForStore = await _settingService.SettingExistsAsync(blogSettings, x => x.SendReportToEmployee, storeId);
            model.SendReportToManager_OverrideForStore = await _settingService.SettingExistsAsync(blogSettings, x => x.SendReportToManager, storeId);
            model.SendReportToProjectLeader_OverrideForStore = await _settingService.SettingExistsAsync(blogSettings, x => x.SendReportToProjectLeader, storeId);
            model.ShowOnlyNotDOT_OverrideForStore = await _settingService.SettingExistsAsync(blogSettings, x => x.ShowOnlyNotDOT, storeId);
            model.SendReportToHR_OverrideForStore = await _settingService.SettingExistsAsync(blogSettings, x => x.SendReportToHR, storeId);
            model.LearningProjectId_OverrideForStore = await _settingService.SettingExistsAsync(blogSettings, x => x.LearningProjectId, storeId);



            //for overdue email
            model.OverDue_From_OverrideForStore = await _settingService.SettingExistsAsync(blogSettings, x => x.OverDue_From, storeId);
            model.OverDue_To_OverrideForStore = await _settingService.SettingExistsAsync(blogSettings, x => x.OverDue_To, storeId);
            model.SendOverdueEmailToEmployee_OverrideForStore = await _settingService.SettingExistsAsync(blogSettings, x => x.SendOverdueEmailToEmployee, storeId);
            model.SendOverdueReportToProjectLeader_OverrideForStore = await _settingService.SettingExistsAsync(blogSettings, x => x.SendOverdueReportToProjectLeader, storeId);
            model.SendOverdueReportToManager_OverrideForStore = await _settingService.SettingExistsAsync(blogSettings, x => x.SendOverdueReportToManager, storeId);
            model.SendOverdueReportToHR_OverrideForStore = await _settingService.SettingExistsAsync(blogSettings, x => x.SendOverdueReportToHR, storeId);
            model.OverdueCountCCThreshold_OverrideForStore = await _settingService.SettingExistsAsync(blogSettings, x => x.OverdueCountCCThreshold, storeId);
            model.IncludeProjectLeadersInCC_OverrideForStore = await _settingService.SettingExistsAsync(blogSettings, x => x.IncludeProjectLeadersInCC, storeId);
            model.IncludeProjectManagerInCC_OverrideForStore = await _settingService.SettingExistsAsync(blogSettings, x => x.IncludeProjectManagerInCC, storeId);
            model.IncludeManagementInCC_OverrideForStore = await _settingService.SettingExistsAsync(blogSettings, x => x.IncludeManagementInCC, storeId);
            model.IncludeHRInCC_OverrideForStore = await _settingService.SettingExistsAsync(blogSettings, x => x.IncludeHRInCC, storeId);
            model.IncludeProjectCoordinatorInCC_OverrideForStore = await _settingService.SettingExistsAsync(blogSettings, x => x.IncludeProjectCoordinatorInCC, storeId);




            await _settingService.ClearCacheAsync();

            return model;
        }

        public virtual async Task<TimeSheetSettingsModel> PrepareTimeSheetSettingsModelAsync(TimeSheetSettingsModel model = null)
        {
            //load settings for a chosen store scope
            var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var blogSettings = await _settingService.LoadSettingAsync<TimeSheetSetting>(storeId);
            //fill in model values from the entity
            model ??= blogSettings.ToSettingsModel<TimeSheetSettingsModel>();

            //fill in additional values (not existing in the entity)
            model.ActiveStoreScopeConfiguration = storeId;
            await PrepareDepartmentListAsync(model);

            if (storeId <= 0)
                return model;

            //fill in overridden values
            model.Reminder1_From_OverrideForStore = await _settingService.SettingExistsAsync(blogSettings, x => x.Reminder1_From, storeId);
            model.Reminder1_To_OverrideForStore = await _settingService.SettingExistsAsync(blogSettings, x => x.Reminder1_To, storeId);
            model.Reminder2_From_OverrideForStore = await _settingService.SettingExistsAsync(blogSettings, x => x.Reminder2_From, storeId);
            model.Reminder2_To_OverrideForStore = await _settingService.SettingExistsAsync(blogSettings, x => x.Reminder2_To, storeId);
            model.DepartmentIds_OverrideForStore = await _settingService.SettingExistsAsync(blogSettings, x => x.DepartmentIds, storeId);
            model.SendEmailToAllProjectLeaders_OverrideForStore = await _settingService.SettingExistsAsync(blogSettings, x => x.SendEmailToAllProjectLeaders, storeId);
            model.SendEmailToAllProjectManager_OverrideForStore = await _settingService.SettingExistsAsync(blogSettings, x => x.SendEmailToAllProjectManager, storeId);
            model.SendEmailToHr_OverrideForStore = await _settingService.SettingExistsAsync(blogSettings, x => x.SendEmailToHr, storeId);
            model.CommonEmails_OverrideForStore = await _settingService.SettingExistsAsync(blogSettings, x => x.CommonEmails, storeId);
            model.ConsiderBeforeDay_OverrideForStore = await _settingService.SettingExistsAsync(blogSettings, x => x.ConsiderBeforeDay, storeId);
            model.SendWithCCAfterDay_OverrideForStore = await _settingService.SettingExistsAsync(blogSettings, x => x.SendWithCCAfterDay, storeId);





            await _settingService.ClearCacheAsync();

            return model;
        }


        public virtual async Task<ProjectTaskSettingsModel> PrepareProjectTaskSettingsModelAsync(ProjectTaskSettingsModel model = null)
        {

            var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var projectTaskSettings = await _settingService.LoadSettingAsync<ProjectTaskSetting>(storeId);
            model ??= projectTaskSettings.ToSettingsModel<ProjectTaskSettingsModel>();
            model.ActiveStoreScopeConfiguration = storeId;
            if (storeId <= 0)
                return model; 
            model.IsShowSelctAllCheckList_OverrideForStore = await _settingService.SettingExistsAsync(projectTaskSettings, x => x.IsShowSelctAllCheckList, storeId);
            model.EnableProjectTaskDebugLog_OverrideForStore = await _settingService.SettingExistsAsync(projectTaskSettings, x => x.EnableProjectTaskDebugLog, storeId);
            await _settingService.ClearCacheAsync();
            return model;
        }

        public virtual async Task<EmployeeSettingsModel> PrepareEmployeeSettingsModelAsync(EmployeeSettingsModel model = null)
        {
            var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var employeeSettings = await _settingService.LoadSettingAsync<EmployeeSettings>(storeId);
            model ??= employeeSettings.ToSettingsModel<EmployeeSettingsModel>();
            await PrepareDesignationListAsync(model);
            model.ActiveStoreScopeConfiguration = storeId;
            if (storeId <= 0)
                return model;
            model.OnBoardingEmail_OverrideForStore = await _settingService.SettingExistsAsync(employeeSettings, x => x.OnBoardingEmail, storeId);
            model.CoordinatorRoleId_OverrideForStore = await _settingService.SettingExistsAsync(employeeSettings, x => x.CoordinatorRoleId, storeId);
            await _settingService.ClearCacheAsync();
            return model;
        }


        public virtual async Task<EmployeeAttendanceSettingsModel> PrepareEmployeeAttendanceSettingsModelAsync(EmployeeAttendanceSettingsModel model = null)
        {
            //load settings for a chosen store scope
            var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var blogSettings = await _settingService.LoadSettingAsync<EmployeeAttendanceSetting>(storeId);

            //fill in model values from the entity
            model ??= blogSettings.ToSettingsModel<EmployeeAttendanceSettingsModel>();

            //fill in additional values (not existing in the entity)
            model.ActiveStoreScopeConfiguration = storeId;

            if (storeId <= 0)
                return model;

            //fill in overridden values
            model.OfficeTime_From_OverrideForStore = await _settingService.SettingExistsAsync(blogSettings, x => x.OfficeTime_From, storeId);
            model.OfficeTime_To_OverrideForStore = await _settingService.SettingExistsAsync(blogSettings, x => x.OfficeTime_To, storeId);
           

            await _settingService.ClearCacheAsync();

            return model;
        }


        public virtual async Task<TeamPerformanceSettingsModel> PrepareTeamPerformanceSettingsModelAsync(TeamPerformanceSettingsModel model = null)
        {
            //load settings for a chosen store scope
            var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var blogSettings = await _settingService.LoadSettingAsync<TeamPerformanceSettings>(storeId);

            //fill in model values from the entity
            model ??= blogSettings.ToSettingsModel<TeamPerformanceSettingsModel>();

            //fill in additional values (not existing in the entity)
            model.ActiveStoreScopeConfiguration = storeId;
            await PrepareFeedBackShowListAsync(model);
            await PrepareDateListAsync(model);
            if (storeId <= 0)
                return model;

            //fill in overridden values
            model.FeedbackShowId_OverrideForStore = await _settingService.SettingExistsAsync(blogSettings, x => x.FeedbackShowId, storeId);

            model.StartReminderDate_OverrideForStore = await _settingService.SettingExistsAsync(blogSettings, x => x.StartReminderDate, storeId);

            model.StartCCDate_OverrideForStore = await _settingService.SettingExistsAsync(blogSettings, x => x.StartCCDate, storeId);

            await _settingService.ClearCacheAsync();

            return model;
        }
        #endregion

        #region Email Settings Methods

        public virtual async Task<EmailSettingsModel> PrepareEmailSettingsModelAsync(EmailSettingsModel model = null)
        {
            var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var emailSettings = await _settingService.LoadSettingAsync<EmailSettings>(storeId);

            model ??= emailSettings.ToSettingsModel<EmailSettingsModel>();

            model.ActiveStoreScopeConfiguration = storeId;

            if (storeId <= 0)
                return model;

            model.FirstMailVariation_OverrideForStore = await _settingService.SettingExistsAsync(emailSettings, x => x.FirstMailVariation, storeId);
            model.SecondMailVariation_OverrideForStore = await _settingService.SettingExistsAsync(emailSettings, x => x.SecondMailVariation, storeId);
            model.ThirdMailVariation_OverrideForStore = await _settingService.SettingExistsAsync(emailSettings, x => x.ThirdMailVariation, storeId);

            await _settingService.ClearCacheAsync();

            return model;
        }

        #endregion
    }
}
