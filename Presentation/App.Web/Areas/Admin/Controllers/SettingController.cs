using App.Core;
using App.Core.Configuration;
using App.Core.Domain;
using App.Core.Domain.Blogs;
using App.Core.Domain.Common;
using App.Core.Domain.Customers;
using App.Core.Domain.Extension.Alerts;
using App.Core.Domain.Extension.EmployeeAttendanceSetting;
using App.Core.Domain.Extension.Employees;
using App.Core.Domain.Extension.Leaves;
using App.Core.Domain.Extension.PerformanceMeasurements;
using App.Core.Domain.Extension.ProjectTasks;
using App.Core.Domain.Extension.TimeSheets;
using App.Core.Domain.Forums;
using App.Core.Domain.Gdpr;
using App.Core.Domain.Localization;
using App.Core.Domain.Media;
using App.Core.Domain.News;
using App.Core.Domain.Security;
using App.Core.Domain.Seo;
using App.Core.Events;
using App.Core.Infrastructure;
using App.Data;
using App.Data.Configuration;
using App.Services.Authentication.MultiFactor;
using App.Services.Common;
using App.Services.Configuration;
using App.Services.Customers;
using App.Services.Gdpr;
using App.Services.Helpers;
using App.Services.Localization;
using App.Services.Logging;
using App.Services.Media;
using App.Services.Messages;
using App.Services.Plugins;
using App.Services.Security;
using App.Services.Stores;
using App.Web.Areas.Admin.Factories;
using App.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using App.Web.Areas.Admin.Models.Settings;
using App.Web.Framework;
using App.Web.Framework.Controllers;
using App.Web.Framework.Mvc;
using App.Web.Framework.Mvc.Filters;
using App.Web.Framework.Mvc.ModelBinding;
using App.Web.Framework.WebOptimizer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace App.Web.Areas.Admin.Controllers
{
    public partial class SettingController : BaseAdminController
    {
        #region Fields

        private readonly AppSettings _appSettings;
        private readonly IAddressService _addressService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ICustomerService _customerService;
        private readonly INopDataProvider _dataProvider;
        private readonly IEncryptionService _encryptionService;
        private readonly IEventPublisher _eventPublisher;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IGdprService _gdprService;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly ILocalizationService _localizationService;
        private readonly IMultiFactorAuthenticationPluginManager _multiFactorAuthenticationPluginManager;
        private readonly INopFileProvider _fileProvider;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly IPictureService _pictureService;
        private readonly ISettingModelFactory _settingModelFactory;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly IStoreService _storeService;
        private readonly IWorkContext _workContext;
        private readonly IUploadService _uploadService;

        #endregion

        #region Ctor

        public SettingController(AppSettings appSettings,
            IAddressService addressService,
            ICustomerActivityService customerActivityService,
            ICustomerService customerService,
            INopDataProvider dataProvider,
            IEncryptionService encryptionService,
            IEventPublisher eventPublisher,
            IGenericAttributeService genericAttributeService,
            IGdprService gdprService,
            ILocalizedEntityService localizedEntityService,
            ILocalizationService localizationService,
            IMultiFactorAuthenticationPluginManager multiFactorAuthenticationPluginManager,
            INopFileProvider fileProvider,
            INotificationService notificationService,
            IPermissionService permissionService,
            IPictureService pictureService,
            ISettingModelFactory settingModelFactory,
            ISettingService settingService,
            IStoreContext storeContext,
            IStoreService storeService,
            IWorkContext workContext,
            IUploadService uploadService)
        {
            _appSettings = appSettings;
            _addressService = addressService;
            _customerActivityService = customerActivityService;
            _customerService = customerService;
            _dataProvider = dataProvider;
            _encryptionService = encryptionService;
            _eventPublisher = eventPublisher;
            _genericAttributeService = genericAttributeService;
            _gdprService = gdprService;
            _localizedEntityService = localizedEntityService;
            _localizationService = localizationService;
            _multiFactorAuthenticationPluginManager = multiFactorAuthenticationPluginManager;
            _fileProvider = fileProvider;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _pictureService = pictureService;
            _settingModelFactory = settingModelFactory;
            _settingService = settingService;
            _storeContext = storeContext;
            _storeService = storeService;
            _workContext = workContext;
            _uploadService = uploadService;
        }

        #endregion

        #region Utilities

        protected virtual async Task UpdateGdprConsentLocalesAsync(GdprConsent gdprConsent, GdprConsentModel model)
        {
            foreach (var localized in model.Locales)
            {
                await _localizedEntityService.SaveLocalizedValueAsync(gdprConsent,
                    x => x.Message,
                    localized.Message,
                    localized.LanguageId);

                await _localizedEntityService.SaveLocalizedValueAsync(gdprConsent,
                    x => x.RequiredMessage,
                    localized.RequiredMessage,
                    localized.LanguageId);
            }
        }

        #endregion

        #region Methods

        public virtual async Task<IActionResult> ChangeStoreScopeConfiguration(int storeid, string returnUrl = "")
        {
            var store = await _storeService.GetStoreByIdAsync(storeid);
            if (store != null || storeid == 0)
            {
                await _genericAttributeService
                    .SaveAttributeAsync(await _workContext.GetCurrentCustomerAsync(), NopCustomerDefaults.AdminAreaStoreScopeConfigurationAttribute, storeid);
            }

            //home page
            if (string.IsNullOrEmpty(returnUrl))
                returnUrl = Url.Action("Index", "Home", new { area = AreaNames.Admin });

            //prevent open redirection attack
            if (!Url.IsLocalUrl(returnUrl))
                return RedirectToAction("Index", "Home", new { area = AreaNames.Admin });

            return Redirect(returnUrl);
        }

        public virtual async Task<IActionResult> AppSettings()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAppSettings))
                return AccessDeniedView();

            //prepare model
            var model = await _settingModelFactory.PrepareAppSettingsModel();

            return View(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> AppSettings(AppSettingsModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAppSettings))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var configurations = new List<IConfig>
                {
                    model.CacheConfigModel.ToConfig(_appSettings.Get<CacheConfig>()),
                    model.HostingConfigModel.ToConfig(_appSettings.Get<HostingConfig>()),
                    model.DistributedCacheConfigModel.ToConfig(_appSettings.Get<DistributedCacheConfig>()),
                    model.AzureBlobConfigModel.ToConfig(_appSettings.Get<AzureBlobConfig>()),
                    model.InstallationConfigModel.ToConfig(_appSettings.Get<InstallationConfig>()),
                    model.PluginConfigModel.ToConfig(_appSettings.Get<PluginConfig>()),
                    model.CommonConfigModel.ToConfig(_appSettings.Get<CommonConfig>()),
                    model.DataConfigModel.ToConfig(_appSettings.Get<DataConfig>()),
                    model.WebOptimizerConfigModel.ToConfig(_appSettings.Get<WebOptimizerConfig>())
                };

                await _eventPublisher.PublishAsync(new AppSettingsSavingEvent(configurations));

                AppSettingsHelper.SaveAppSettings(configurations, _fileProvider);

                await _customerActivityService.InsertActivityAsync("EditSettings",
                    await _localizationService.GetResourceAsync("ActivityLog.EditSettings"));

                _notificationService.SuccessNotification(
                    await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));

                var returnUrl = Url.Action("AppSettings", "Setting", new { area = AreaNames.Admin });
                return View("RestartApplication", returnUrl);
            }

            //prepare model
            model = await _settingModelFactory.PrepareAppSettingsModel(model);

            //if we got this far, something failed, redisplay form
            return View(model);
        }

        public virtual async Task<IActionResult> Blog()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            //prepare model
            var model = await _settingModelFactory.PrepareBlogSettingsModelAsync();

            return View(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> Blog(BlogSettingsModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                //load settings for a chosen store scope
                var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
                var blogSettings = await _settingService.LoadSettingAsync<BlogSettings>(storeScope);
                blogSettings = model.ToSettings(blogSettings);

                //we do not clear cache after each setting update.
                //this behavior can increase performance because cached settings will not be cleared 
                //and loaded from database after each update
                await _settingService.SaveSettingOverridablePerStoreAsync(blogSettings, x => x.Enabled, model.Enabled_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(blogSettings, x => x.PostsPageSize, model.PostsPageSize_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(blogSettings, x => x.AllowNotRegisteredUsersToLeaveComments, model.AllowNotRegisteredUsersToLeaveComments_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(blogSettings, x => x.NotifyAboutNewBlogComments, model.NotifyAboutNewBlogComments_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(blogSettings, x => x.NumberOfTags, model.NumberOfTags_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(blogSettings, x => x.ShowHeaderRssUrl, model.ShowHeaderRssUrl_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(blogSettings, x => x.BlogCommentsMustBeApproved, model.BlogCommentsMustBeApproved_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingAsync(blogSettings, x => x.ShowBlogCommentsPerStore, clearCache: false);

                //now clear settings cache
                await _settingService.ClearCacheAsync();

                //activity log
                await _customerActivityService.InsertActivityAsync("EditSettings", await _localizationService.GetResourceAsync("ActivityLog.EditSettings"));

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));

                return RedirectToAction("Blog");
            }

            //prepare model
            model = await _settingModelFactory.PrepareBlogSettingsModelAsync(model);

            //if we got this far, something failed, redisplay form
            return View(model);
        }

        public virtual async Task<IActionResult> Forum()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            //prepare model
            var model = await _settingModelFactory.PrepareForumSettingsModelAsync();

            return View(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> Forum(ForumSettingsModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                //load settings for a chosen store scope
                var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
                var forumSettings = await _settingService.LoadSettingAsync<ForumSettings>(storeScope);
                forumSettings = model.ToSettings(forumSettings);

                //we do not clear cache after each setting update.
                //this behavior can increase performance because cached settings will not be cleared 
                //and loaded from database after each update
                await _settingService.SaveSettingOverridablePerStoreAsync(forumSettings, x => x.ForumsEnabled, model.ForumsEnabled_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(forumSettings, x => x.RelativeDateTimeFormattingEnabled, model.RelativeDateTimeFormattingEnabled_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(forumSettings, x => x.ShowCustomersPostCount, model.ShowCustomersPostCount_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(forumSettings, x => x.AllowGuestsToCreatePosts, model.AllowGuestsToCreatePosts_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(forumSettings, x => x.AllowGuestsToCreateTopics, model.AllowGuestsToCreateTopics_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(forumSettings, x => x.AllowCustomersToEditPosts, model.AllowCustomersToEditPosts_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(forumSettings, x => x.AllowCustomersToDeletePosts, model.AllowCustomersToDeletePosts_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(forumSettings, x => x.AllowPostVoting, model.AllowPostVoting_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(forumSettings, x => x.MaxVotesPerDay, model.MaxVotesPerDay_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(forumSettings, x => x.AllowCustomersToManageSubscriptions, model.AllowCustomersToManageSubscriptions_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(forumSettings, x => x.TopicsPageSize, model.TopicsPageSize_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(forumSettings, x => x.PostsPageSize, model.PostsPageSize_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(forumSettings, x => x.ForumEditor, model.ForumEditor_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(forumSettings, x => x.SignaturesEnabled, model.SignaturesEnabled_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(forumSettings, x => x.AllowPrivateMessages, model.AllowPrivateMessages_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(forumSettings, x => x.ShowAlertForPM, model.ShowAlertForPM_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(forumSettings, x => x.NotifyAboutPrivateMessages, model.NotifyAboutPrivateMessages_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(forumSettings, x => x.ActiveDiscussionsFeedEnabled, model.ActiveDiscussionsFeedEnabled_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(forumSettings, x => x.ActiveDiscussionsFeedCount, model.ActiveDiscussionsFeedCount_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(forumSettings, x => x.ForumFeedsEnabled, model.ForumFeedsEnabled_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(forumSettings, x => x.ForumFeedCount, model.ForumFeedCount_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(forumSettings, x => x.SearchResultsPageSize, model.SearchResultsPageSize_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(forumSettings, x => x.ActiveDiscussionsPageSize, model.ActiveDiscussionsPageSize_OverrideForStore, storeScope, false);

                //now clear settings cache
                await _settingService.ClearCacheAsync();

                //activity log
                await _customerActivityService.InsertActivityAsync("EditSettings", await _localizationService.GetResourceAsync("ActivityLog.EditSettings"));

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));

                return RedirectToAction("Forum");
            }

            //prepare model
            model = await _settingModelFactory.PrepareForumSettingsModelAsync(model);

            //if we got this far, something failed, redisplay form
            return View(model);
        }

        public virtual async Task<IActionResult> News()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            //prepare model
            var model = await _settingModelFactory.PrepareNewsSettingsModelAsync();

            return View(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> News(NewsSettingsModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                //load settings for a chosen store scope
                var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
                var newsSettings = await _settingService.LoadSettingAsync<NewsSettings>(storeScope);
                newsSettings = model.ToSettings(newsSettings);

                //we do not clear cache after each setting update.
                //this behavior can increase performance because cached settings will not be cleared 
                //and loaded from database after each update
                await _settingService.SaveSettingOverridablePerStoreAsync(newsSettings, x => x.Enabled, model.Enabled_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(newsSettings, x => x.AllowNotRegisteredUsersToLeaveComments, model.AllowNotRegisteredUsersToLeaveComments_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(newsSettings, x => x.NotifyAboutNewNewsComments, model.NotifyAboutNewNewsComments_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(newsSettings, x => x.ShowNewsOnMainPage, model.ShowNewsOnMainPage_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(newsSettings, x => x.MainPageNewsCount, model.MainPageNewsCount_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(newsSettings, x => x.NewsArchivePageSize, model.NewsArchivePageSize_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(newsSettings, x => x.ShowHeaderRssUrl, model.ShowHeaderRssUrl_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(newsSettings, x => x.NewsCommentsMustBeApproved, model.NewsCommentsMustBeApproved_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingAsync(newsSettings, x => x.ShowNewsCommentsPerStore, clearCache: false);

                //now clear settings cache
                await _settingService.ClearCacheAsync();

                //activity log
                await _customerActivityService.InsertActivityAsync("EditSettings", await _localizationService.GetResourceAsync("ActivityLog.EditSettings"));

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));

                return RedirectToAction("News");
            }

            //prepare model
            model = await _settingModelFactory.PrepareNewsSettingsModelAsync(model);

            //if we got this far, something failed, redisplay form
            return View(model);
        }

        public virtual async Task<IActionResult> Media()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            //prepare model
            var model = await _settingModelFactory.PrepareMediaSettingsModelAsync();

            return View(model);
        }

        [HttpPost]
        [FormValueRequired("save")]
        public virtual async Task<IActionResult> Media(MediaSettingsModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                //load settings for a chosen store scope
                var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
                var mediaSettings = await _settingService.LoadSettingAsync<MediaSettings>(storeScope);
                mediaSettings = model.ToSettings(mediaSettings);

                //we do not clear cache after each setting update.
                //this behavior can increase performance because cached settings will not be cleared 
                //and loaded from database after each update
                await _settingService.SaveSettingOverridablePerStoreAsync(mediaSettings, x => x.AvatarPictureSize, model.AvatarPictureSize_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(mediaSettings, x => x.MaximumImageSize, model.MaximumImageSize_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(mediaSettings, x => x.MultipleThumbDirectories, model.MultipleThumbDirectories_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(mediaSettings, x => x.DefaultImageQuality, model.DefaultImageQuality_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(mediaSettings, x => x.ImportProductImagesUsingHash, model.ImportProductImagesUsingHash_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(mediaSettings, x => x.DefaultPictureZoomEnabled, model.DefaultPictureZoomEnabled_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(mediaSettings, x => x.AllowSVGUploads, model.AllowSVGUploads_OverrideForStore, storeScope, false);
               
                //now clear settings cache
                await _settingService.ClearCacheAsync();

                //activity log
                await _customerActivityService.InsertActivityAsync("EditSettings", await _localizationService.GetResourceAsync("ActivityLog.EditSettings"));

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));

                return RedirectToAction("Media");
            }

            //prepare model
            model = await _settingModelFactory.PrepareMediaSettingsModelAsync(model);

            //if we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpPost, ActionName("Media")]
        [FormValueRequired("change-picture-storage")]
        public virtual async Task<IActionResult> ChangePictureStorage()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            await _pictureService.SetIsStoreInDbAsync(!await _pictureService.IsStoreInDbAsync());

            //activity log
            await _customerActivityService.InsertActivityAsync("EditSettings", await _localizationService.GetResourceAsync("ActivityLog.EditSettings"));

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));

            return RedirectToAction("Media");
        }

        public virtual async Task<IActionResult> CustomerUser()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            //prepare model
            var model = await _settingModelFactory.PrepareCustomerUserSettingsModelAsync();

            return View(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> CustomerUser(CustomerUserSettingsModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
                var customerSettings = await _settingService.LoadSettingAsync<CustomerSettings>(storeScope);

                var lastUsernameValidationRule = customerSettings.UsernameValidationRule;
                var lastUsernameValidationEnabledValue = customerSettings.UsernameValidationEnabled;
                var lastUsernameValidationUseRegexValue = customerSettings.UsernameValidationUseRegex;

                //Phone number validation settings
                var lastPhoneNumberValidationRule = customerSettings.PhoneNumberValidationRule;
                var lastPhoneNumberValidationEnabledValue = customerSettings.PhoneNumberValidationEnabled;
                var lastPhoneNumberValidationUseRegexValue = customerSettings.PhoneNumberValidationUseRegex;

                var addressSettings = await _settingService.LoadSettingAsync<AddressSettings>(storeScope);
                var dateTimeSettings = await _settingService.LoadSettingAsync<DateTimeSettings>(storeScope);
                var externalAuthenticationSettings = await _settingService.LoadSettingAsync<ExternalAuthenticationSettings>(storeScope);
                var multiFactorAuthenticationSettings = await _settingService.LoadSettingAsync<MultiFactorAuthenticationSettings>(storeScope);

                customerSettings = model.CustomerSettings.ToSettings(customerSettings);

                if (customerSettings.UsernameValidationEnabled && customerSettings.UsernameValidationUseRegex)
                {
                    try
                    {
                        //validate regex rule
                        var unused = Regex.IsMatch("test_user_name", customerSettings.UsernameValidationRule);
                    }
                    catch (ArgumentException)
                    {
                        //restoring previous settings
                        customerSettings.UsernameValidationRule = lastUsernameValidationRule;
                        customerSettings.UsernameValidationEnabled = lastUsernameValidationEnabledValue;
                        customerSettings.UsernameValidationUseRegex = lastUsernameValidationUseRegexValue;

                        _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Settings.CustomerSettings.RegexValidationRule.Error"));
                    }
                }

                if (customerSettings.PhoneNumberValidationEnabled && customerSettings.PhoneNumberValidationUseRegex)
                {
                    try
                    {
                        //validate regex rule
                        var unused = Regex.IsMatch("123456789", customerSettings.PhoneNumberValidationRule);
                    }
                    catch (ArgumentException)
                    {
                        //restoring previous settings
                        customerSettings.PhoneNumberValidationRule = lastPhoneNumberValidationRule;
                        customerSettings.PhoneNumberValidationEnabled = lastPhoneNumberValidationEnabledValue;
                        customerSettings.PhoneNumberValidationUseRegex = lastPhoneNumberValidationUseRegexValue;

                        _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Settings.CustomerSettings.PhoneNumberRegexValidationRule.Error"));
                    }
                }

                await _settingService.SaveSettingAsync(customerSettings);

                addressSettings = model.AddressSettings.ToSettings(addressSettings);
                await _settingService.SaveSettingAsync(addressSettings);

                dateTimeSettings.DefaultStoreTimeZoneId = model.DateTimeSettings.DefaultStoreTimeZoneId;
                dateTimeSettings.AllowCustomersToSetTimeZone = model.DateTimeSettings.AllowCustomersToSetTimeZone;
                await _settingService.SaveSettingAsync(dateTimeSettings);

                externalAuthenticationSettings.AllowCustomersToRemoveAssociations = model.ExternalAuthenticationSettings.AllowCustomersToRemoveAssociations;
                await _settingService.SaveSettingAsync(externalAuthenticationSettings);

                multiFactorAuthenticationSettings = model.MultiFactorAuthenticationSettings.ToSettings(multiFactorAuthenticationSettings);
                await _settingService.SaveSettingAsync(multiFactorAuthenticationSettings);

                //activity log
                await _customerActivityService.InsertActivityAsync("EditSettings", await _localizationService.GetResourceAsync("ActivityLog.EditSettings"));

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));

                return RedirectToAction("CustomerUser");
            }

            //prepare model
            model = await _settingModelFactory.PrepareCustomerUserSettingsModelAsync(model);

            //if we got this far, something failed, redisplay form
            return View(model);
        }

        #region GDPR

        public virtual async Task<IActionResult> Gdpr()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            //prepare model
            var model = await _settingModelFactory.PrepareGdprSettingsModelAsync();

            return View(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> Gdpr(GdprSettingsModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                //load settings for a chosen store scope
                var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
                var gdprSettings = await _settingService.LoadSettingAsync<GdprSettings>(storeScope);
                gdprSettings = model.ToSettings(gdprSettings);

                //we do not clear cache after each setting update.
                //this behavior can increase performance because cached settings will not be cleared 
                //and loaded from database after each update
                await _settingService.SaveSettingOverridablePerStoreAsync(gdprSettings, x => x.GdprEnabled, model.GdprEnabled_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(gdprSettings, x => x.LogPrivacyPolicyConsent, model.LogPrivacyPolicyConsent_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(gdprSettings, x => x.LogNewsletterConsent, model.LogNewsletterConsent_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(gdprSettings, x => x.LogUserProfileChanges, model.LogUserProfileChanges_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(gdprSettings, x => x.DeleteInactiveCustomersAfterMonths, model.DeleteInactiveCustomersAfterMonths_OverrideForStore, storeScope, false);

                //now clear settings cache
                await _settingService.ClearCacheAsync();

                //activity log
                await _customerActivityService.InsertActivityAsync("EditSettings", await _localizationService.GetResourceAsync("ActivityLog.EditSettings"));

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));

                return RedirectToAction("Gdpr");
            }

            //prepare model
            model = await _settingModelFactory.PrepareGdprSettingsModelAsync(model);

            //if we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> GdprConsentList(GdprConsentSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
                return await AccessDeniedDataTablesJson();

            //prepare model
            var model = await _settingModelFactory.PrepareGdprConsentListModelAsync(searchModel);

            return Json(model);
        }

        public virtual async Task<IActionResult> CreateGdprConsent()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            //prepare model
            var model = await _settingModelFactory.PrepareGdprConsentModelAsync(new GdprConsentModel(), null);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> CreateGdprConsent(GdprConsentModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var gdprConsent = model.ToEntity<GdprConsent>();
                await _gdprService.InsertConsentAsync(gdprConsent);

                //locales                
                await UpdateGdprConsentLocalesAsync(gdprConsent, model);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Settings.Gdpr.Consent.Added"));

                return continueEditing ? RedirectToAction("EditGdprConsent", new { gdprConsent.Id }) : RedirectToAction("Gdpr");
            }

            //prepare model
            model = await _settingModelFactory.PrepareGdprConsentModelAsync(model, null, true);

            //if we got this far, something failed, redisplay form
            return View(model);
        }

        public virtual async Task<IActionResult> EditGdprConsent(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            //try to get a consent with the specified id
            var gdprConsent = await _gdprService.GetConsentByIdAsync(id);
            if (gdprConsent == null)
                return RedirectToAction("Gdpr");

            //prepare model
            var model = await _settingModelFactory.PrepareGdprConsentModelAsync(null, gdprConsent);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> EditGdprConsent(GdprConsentModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            //try to get a GDPR consent with the specified id
            var gdprConsent = await _gdprService.GetConsentByIdAsync(model.Id);
            if (gdprConsent == null)
                return RedirectToAction("Gdpr");

            if (ModelState.IsValid)
            {
                gdprConsent = model.ToEntity(gdprConsent);
                await _gdprService.UpdateConsentAsync(gdprConsent);

                //locales                
                await UpdateGdprConsentLocalesAsync(gdprConsent, model);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Settings.Gdpr.Consent.Updated"));

                return continueEditing ? RedirectToAction("EditGdprConsent", gdprConsent.Id) : RedirectToAction("Gdpr");
            }

            //prepare model
            model = await _settingModelFactory.PrepareGdprConsentModelAsync(model, gdprConsent, true);

            //if we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> DeleteGdprConsent(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            //try to get a GDPR consent with the specified id
            var gdprConsent = await _gdprService.GetConsentByIdAsync(id);
            if (gdprConsent == null)
                return RedirectToAction("Gdpr");

            await _gdprService.DeleteConsentAsync(gdprConsent);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Settings.Gdpr.Consent.Deleted"));

            return RedirectToAction("Gdpr");
        }

        #endregion

        public virtual async Task<IActionResult> GeneralCommon(bool showtour = false)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            //prepare model
            var model = await _settingModelFactory.PrepareGeneralCommonSettingsModelAsync();

            //show configuration tour
            if (showtour)
            {
                var customer = await _workContext.GetCurrentCustomerAsync();
                var hideCard = await _genericAttributeService.GetAttributeAsync<bool>(customer, NopCustomerDefaults.HideConfigurationStepsAttribute);
                var closeCard = await _genericAttributeService.GetAttributeAsync<bool>(customer, NopCustomerDefaults.CloseConfigurationStepsAttribute);

                if (!hideCard && !closeCard)
                    ViewBag.ShowTour = true;
            }

            return View(model);
        }

        [HttpPost]
        [FormValueRequired("save")]
        public virtual async Task<IActionResult> GeneralCommon(GeneralCommonSettingsModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                //load settings for a chosen store scope
                var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();

                //store information settings
                var storeInformationSettings = await _settingService.LoadSettingAsync<StoreInformationSettings>(storeScope);
                var commonSettings = await _settingService.LoadSettingAsync<CommonSettings>(storeScope);
                var sitemapSettings = await _settingService.LoadSettingAsync<SitemapSettings>(storeScope);

                storeInformationSettings.StoreClosed = model.StoreInformationSettings.StoreClosed;
                storeInformationSettings.DefaultStoreTheme = model.StoreInformationSettings.DefaultStoreTheme;
                storeInformationSettings.AllowCustomerToSelectTheme = model.StoreInformationSettings.AllowCustomerToSelectTheme;
                storeInformationSettings.LogoPictureId = model.StoreInformationSettings.LogoPictureId;
                //EU Cookie law
                storeInformationSettings.DisplayEuCookieLawWarning = model.StoreInformationSettings.DisplayEuCookieLawWarning;
                //social pages
                storeInformationSettings.FacebookLink = model.StoreInformationSettings.FacebookLink;
                storeInformationSettings.TwitterLink = model.StoreInformationSettings.TwitterLink;
                storeInformationSettings.YoutubeLink = model.StoreInformationSettings.YoutubeLink;
                storeInformationSettings.InstagramLink = model.StoreInformationSettings.InstagramLink;
                //contact us
                commonSettings.SubjectFieldOnContactUsForm = model.StoreInformationSettings.SubjectFieldOnContactUsForm;
                commonSettings.UseSystemEmailForContactUsForm = model.StoreInformationSettings.UseSystemEmailForContactUsForm;
                //terms of service
                commonSettings.PopupForTermsOfServiceLinks = model.StoreInformationSettings.PopupForTermsOfServiceLinks;
                //sitemap
                sitemapSettings.SitemapEnabled = model.SitemapSettings.SitemapEnabled;
                sitemapSettings.SitemapPageSize = model.SitemapSettings.SitemapPageSize;
                sitemapSettings.SitemapIncludeCategories = model.SitemapSettings.SitemapIncludeCategories;
                sitemapSettings.SitemapIncludeManufacturers = model.SitemapSettings.SitemapIncludeManufacturers;
                sitemapSettings.SitemapIncludeProducts = model.SitemapSettings.SitemapIncludeProducts;
                sitemapSettings.SitemapIncludeProductTags = model.SitemapSettings.SitemapIncludeProductTags;
                sitemapSettings.SitemapIncludeBlogPosts = model.SitemapSettings.SitemapIncludeBlogPosts;
                sitemapSettings.SitemapIncludeNews = model.SitemapSettings.SitemapIncludeNews;
                sitemapSettings.SitemapIncludeTopics = model.SitemapSettings.SitemapIncludeTopics;

                //minification
                commonSettings.EnableHtmlMinification = model.MinificationSettings.EnableHtmlMinification;
                //use response compression
                commonSettings.UseResponseCompression = model.MinificationSettings.UseResponseCompression;
                //custom header and footer HTML
                commonSettings.HeaderCustomHtml = model.CustomHtmlSettings.HeaderCustomHtml;
                commonSettings.FooterCustomHtml = model.CustomHtmlSettings.FooterCustomHtml;

                //we do not clear cache after each setting update.
                //this behavior can increase performance because cached settings will not be cleared 
                //and loaded from database after each update
                await _settingService.SaveSettingOverridablePerStoreAsync(storeInformationSettings, x => x.StoreClosed, model.StoreInformationSettings.StoreClosed_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(storeInformationSettings, x => x.DefaultStoreTheme, model.StoreInformationSettings.DefaultStoreTheme_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(storeInformationSettings, x => x.AllowCustomerToSelectTheme, model.StoreInformationSettings.AllowCustomerToSelectTheme_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(storeInformationSettings, x => x.LogoPictureId, model.StoreInformationSettings.LogoPictureId_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(storeInformationSettings, x => x.DisplayEuCookieLawWarning, model.StoreInformationSettings.DisplayEuCookieLawWarning_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(storeInformationSettings, x => x.FacebookLink, model.StoreInformationSettings.FacebookLink_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(storeInformationSettings, x => x.TwitterLink, model.StoreInformationSettings.TwitterLink_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(storeInformationSettings, x => x.YoutubeLink, model.StoreInformationSettings.YoutubeLink_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(storeInformationSettings, x => x.InstagramLink, model.StoreInformationSettings.InstagramLink_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(commonSettings, x => x.SubjectFieldOnContactUsForm, model.StoreInformationSettings.SubjectFieldOnContactUsForm_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(commonSettings, x => x.UseSystemEmailForContactUsForm, model.StoreInformationSettings.UseSystemEmailForContactUsForm_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(commonSettings, x => x.PopupForTermsOfServiceLinks, model.StoreInformationSettings.PopupForTermsOfServiceLinks_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(sitemapSettings, x => x.SitemapEnabled, model.SitemapSettings.SitemapEnabled_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(sitemapSettings, x => x.SitemapPageSize, model.SitemapSettings.SitemapPageSize_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(sitemapSettings, x => x.SitemapIncludeCategories, model.SitemapSettings.SitemapIncludeCategories_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(sitemapSettings, x => x.SitemapIncludeManufacturers, model.SitemapSettings.SitemapIncludeManufacturers_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(sitemapSettings, x => x.SitemapIncludeProducts, model.SitemapSettings.SitemapIncludeProducts_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(sitemapSettings, x => x.SitemapIncludeProductTags, model.SitemapSettings.SitemapIncludeProductTags_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(sitemapSettings, x => x.SitemapIncludeBlogPosts, model.SitemapSettings.SitemapIncludeBlogPosts_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(sitemapSettings, x => x.SitemapIncludeNews, model.SitemapSettings.SitemapIncludeNews_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(sitemapSettings, x => x.SitemapIncludeTopics, model.SitemapSettings.SitemapIncludeTopics_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(commonSettings, x => x.EnableHtmlMinification, model.MinificationSettings.EnableHtmlMinification_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(commonSettings, x => x.UseResponseCompression, model.MinificationSettings.UseResponseCompression_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(commonSettings, x => x.HeaderCustomHtml, model.CustomHtmlSettings.HeaderCustomHtml_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(commonSettings, x => x.FooterCustomHtml, model.CustomHtmlSettings.FooterCustomHtml_OverrideForStore, storeScope, false);

                //now clear settings cache
                await _settingService.ClearCacheAsync();

                //seo settings
                var seoSettings = await _settingService.LoadSettingAsync<SeoSettings>(storeScope);
                seoSettings.PageTitleSeparator = model.SeoSettings.PageTitleSeparator;
                seoSettings.PageTitleSeoAdjustment = (PageTitleSeoAdjustment)model.SeoSettings.PageTitleSeoAdjustment;
                seoSettings.GenerateProductMetaDescription = model.SeoSettings.GenerateProductMetaDescription;
                seoSettings.ConvertNonWesternChars = model.SeoSettings.ConvertNonWesternChars;
                seoSettings.CanonicalUrlsEnabled = model.SeoSettings.CanonicalUrlsEnabled;
                seoSettings.WwwRequirement = (WwwRequirement)model.SeoSettings.WwwRequirement;
                seoSettings.TwitterMetaTags = model.SeoSettings.TwitterMetaTags;
                seoSettings.OpenGraphMetaTags = model.SeoSettings.OpenGraphMetaTags;
                seoSettings.MicrodataEnabled = model.SeoSettings.MicrodataEnabled;
                seoSettings.CustomHeadTags = model.SeoSettings.CustomHeadTags;

                //we do not clear cache after each setting update.
                //this behavior can increase performance because cached settings will not be cleared 
                //and loaded from database after each update
                await _settingService.SaveSettingOverridablePerStoreAsync(seoSettings, x => x.PageTitleSeparator, model.SeoSettings.PageTitleSeparator_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(seoSettings, x => x.PageTitleSeoAdjustment, model.SeoSettings.PageTitleSeoAdjustment_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(seoSettings, x => x.GenerateProductMetaDescription, model.SeoSettings.GenerateProductMetaDescription_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(seoSettings, x => x.ConvertNonWesternChars, model.SeoSettings.ConvertNonWesternChars_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(seoSettings, x => x.CanonicalUrlsEnabled, model.SeoSettings.CanonicalUrlsEnabled_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(seoSettings, x => x.WwwRequirement, model.SeoSettings.WwwRequirement_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(seoSettings, x => x.TwitterMetaTags, model.SeoSettings.TwitterMetaTags_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(seoSettings, x => x.OpenGraphMetaTags, model.SeoSettings.OpenGraphMetaTags_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(seoSettings, x => x.CustomHeadTags, model.SeoSettings.CustomHeadTags_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(seoSettings, x => x.MicrodataEnabled, model.SeoSettings.MicrodataEnabled_OverrideForStore, storeScope, false);

                //now clear settings cache
                await _settingService.ClearCacheAsync();

                //security settings
                var securitySettings = await _settingService.LoadSettingAsync<SecuritySettings>(storeScope);
                if (securitySettings.AdminAreaAllowedIpAddresses == null)
                    securitySettings.AdminAreaAllowedIpAddresses = new List<string>();
                securitySettings.AdminAreaAllowedIpAddresses.Clear();
                if (!string.IsNullOrEmpty(model.SecuritySettings.AdminAreaAllowedIpAddresses))
                    foreach (var s in model.SecuritySettings.AdminAreaAllowedIpAddresses.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                        if (!string.IsNullOrWhiteSpace(s))
                            securitySettings.AdminAreaAllowedIpAddresses.Add(s.Trim());
                securitySettings.HoneypotEnabled = model.SecuritySettings.HoneypotEnabled;
                await _settingService.SaveSettingAsync(securitySettings);

                //robots.txt settings
                var robotsTxtSettings = await _settingService.LoadSettingAsync<RobotsTxtSettings>(storeScope);
                robotsTxtSettings.AllowSitemapXml = model.RobotsTxtSettings.AllowSitemapXml;
                robotsTxtSettings.AdditionsRules = model.RobotsTxtSettings.AdditionsRules?.Split(Environment.NewLine).ToList();
                robotsTxtSettings.DisallowLanguages = model.RobotsTxtSettings.DisallowLanguages?.ToList() ?? new List<int>(); 
                robotsTxtSettings.DisallowPaths = model.RobotsTxtSettings.DisallowPaths?.Split(Environment.NewLine).ToList();
                robotsTxtSettings.LocalizableDisallowPaths = model.RobotsTxtSettings.LocalizableDisallowPaths?.Split(Environment.NewLine).ToList();

                //we do not clear cache after each setting update.
                //this behavior can increase performance because cached settings will not be cleared 
                //and loaded from database after each update
                await _settingService.SaveSettingOverridablePerStoreAsync(robotsTxtSettings, x => x.AllowSitemapXml, model.RobotsTxtSettings.AllowSitemapXml_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(robotsTxtSettings, x => x.AdditionsRules, model.RobotsTxtSettings.AdditionsRules_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(robotsTxtSettings, x => x.DisallowLanguages, model.RobotsTxtSettings.DisallowLanguages_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(robotsTxtSettings, x => x.DisallowPaths, model.RobotsTxtSettings.DisallowPaths_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(robotsTxtSettings, x => x.LocalizableDisallowPaths, model.RobotsTxtSettings.LocalizableDisallowPaths_OverrideForStore, storeScope, false);

                // now clear settings cache
                await _settingService.ClearCacheAsync();

                //captcha settings
                var captchaSettings = await _settingService.LoadSettingAsync<CaptchaSettings>(storeScope);
                captchaSettings.Enabled = model.CaptchaSettings.Enabled;
                captchaSettings.ShowOnLoginPage = model.CaptchaSettings.ShowOnLoginPage;
                captchaSettings.ShowOnRegistrationPage = model.CaptchaSettings.ShowOnRegistrationPage;
                captchaSettings.ShowOnCareerPage = model.CaptchaSettings.ShowOnCareerPage;
                captchaSettings.ShowOnContactUsPage = model.CaptchaSettings.ShowOnContactUsPage;
                captchaSettings.ShowOnEmailWishlistToFriendPage = model.CaptchaSettings.ShowOnEmailWishlistToFriendPage;
                captchaSettings.ShowOnEmailProductToFriendPage = model.CaptchaSettings.ShowOnEmailProductToFriendPage;
                captchaSettings.ShowOnBlogCommentPage = model.CaptchaSettings.ShowOnBlogCommentPage;
                captchaSettings.ShowOnNewsCommentPage = model.CaptchaSettings.ShowOnNewsCommentPage;
                captchaSettings.ShowOnProductReviewPage = model.CaptchaSettings.ShowOnProductReviewPage;
                captchaSettings.ShowOnForgotPasswordPage = model.CaptchaSettings.ShowOnForgotPasswordPage;
                captchaSettings.ShowOnApplyVendorPage = model.CaptchaSettings.ShowOnApplyVendorPage;
                captchaSettings.ShowOnForum = model.CaptchaSettings.ShowOnForum;
                captchaSettings.ShowOnCheckoutPageForGuests = model.CaptchaSettings.ShowOnCheckoutPageForGuests;
                captchaSettings.ReCaptchaPublicKey = model.CaptchaSettings.ReCaptchaPublicKey;
                captchaSettings.ReCaptchaPrivateKey = model.CaptchaSettings.ReCaptchaPrivateKey;
                captchaSettings.CaptchaType = (CaptchaType)model.CaptchaSettings.CaptchaType;
                captchaSettings.ReCaptchaV3ScoreThreshold = model.CaptchaSettings.ReCaptchaV3ScoreThreshold;

                //we do not clear cache after each setting update.
                //this behavior can increase performance because cached settings will not be cleared 
                //and loaded from database after each update
                await _settingService.SaveSettingOverridablePerStoreAsync(captchaSettings, x => x.Enabled, model.CaptchaSettings.Enabled_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(captchaSettings, x => x.ShowOnLoginPage, model.CaptchaSettings.ShowOnLoginPage_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(captchaSettings, x => x.ShowOnRegistrationPage, model.CaptchaSettings.ShowOnRegistrationPage_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(captchaSettings, x => x.ShowOnContactUsPage, model.CaptchaSettings.ShowOnContactUsPage_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(captchaSettings, x => x.ShowOnCareerPage, model.CaptchaSettings.ShowOnCareerPage_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(captchaSettings, x => x.ShowOnEmailWishlistToFriendPage, model.CaptchaSettings.ShowOnEmailWishlistToFriendPage_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(captchaSettings, x => x.ShowOnEmailProductToFriendPage, model.CaptchaSettings.ShowOnEmailProductToFriendPage_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(captchaSettings, x => x.ShowOnBlogCommentPage, model.CaptchaSettings.ShowOnBlogCommentPage_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(captchaSettings, x => x.ShowOnNewsCommentPage, model.CaptchaSettings.ShowOnNewsCommentPage_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(captchaSettings, x => x.ShowOnProductReviewPage, model.CaptchaSettings.ShowOnProductReviewPage_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(captchaSettings, x => x.ShowOnApplyVendorPage, model.CaptchaSettings.ShowOnApplyVendorPage_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(captchaSettings, x => x.ShowOnForgotPasswordPage, model.CaptchaSettings.ShowOnForgotPasswordPage_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(captchaSettings, x => x.ShowOnForum, model.CaptchaSettings.ShowOnForum_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(captchaSettings, x => x.ShowOnCheckoutPageForGuests, model.CaptchaSettings.ShowOnCheckoutPageForGuests_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(captchaSettings, x => x.ReCaptchaPublicKey, model.CaptchaSettings.ReCaptchaPublicKey_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(captchaSettings, x => x.ReCaptchaPrivateKey, model.CaptchaSettings.ReCaptchaPrivateKey_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(captchaSettings, x => x.ReCaptchaV3ScoreThreshold, model.CaptchaSettings.ReCaptchaV3ScoreThreshold_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(captchaSettings, x => x.CaptchaType, model.CaptchaSettings.CaptchaType_OverrideForStore, storeScope, false);

                // now clear settings cache
                await _settingService.ClearCacheAsync();

                if (captchaSettings.Enabled &&
                    (string.IsNullOrWhiteSpace(captchaSettings.ReCaptchaPublicKey) || string.IsNullOrWhiteSpace(captchaSettings.ReCaptchaPrivateKey)))
                {
                    //captcha is enabled but the keys are not entered
                    _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Settings.GeneralCommon.CaptchaAppropriateKeysNotEnteredError"));
                }

                //PDF settings
                var pdfSettings = await _settingService.LoadSettingAsync<PdfSettings>(storeScope);
                pdfSettings.LetterPageSizeEnabled = model.PdfSettings.LetterPageSizeEnabled;
                pdfSettings.LogoPictureId = model.PdfSettings.LogoPictureId;
                pdfSettings.DisablePdfInvoicesForPendingOrders = model.PdfSettings.DisablePdfInvoicesForPendingOrders;
                pdfSettings.InvoiceFooterTextColumn1 = model.PdfSettings.InvoiceFooterTextColumn1;
                pdfSettings.InvoiceFooterTextColumn2 = model.PdfSettings.InvoiceFooterTextColumn2;

                //we do not clear cache after each setting update.
                //this behavior can increase performance because cached settings will not be cleared 
                //and loaded from database after each update
                await _settingService.SaveSettingOverridablePerStoreAsync(pdfSettings, x => x.LetterPageSizeEnabled, model.PdfSettings.LetterPageSizeEnabled_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(pdfSettings, x => x.LogoPictureId, model.PdfSettings.LogoPictureId_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(pdfSettings, x => x.DisablePdfInvoicesForPendingOrders, model.PdfSettings.DisablePdfInvoicesForPendingOrders_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(pdfSettings, x => x.InvoiceFooterTextColumn1, model.PdfSettings.InvoiceFooterTextColumn1_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(pdfSettings, x => x.InvoiceFooterTextColumn2, model.PdfSettings.InvoiceFooterTextColumn2_OverrideForStore, storeScope, false);

                //now clear settings cache
                await _settingService.ClearCacheAsync();

                //localization settings
                var localizationSettings = await _settingService.LoadSettingAsync<LocalizationSettings>(storeScope);
                localizationSettings.UseImagesForLanguageSelection = model.LocalizationSettings.UseImagesForLanguageSelection;
                if (localizationSettings.SeoFriendlyUrlsForLanguagesEnabled != model.LocalizationSettings.SeoFriendlyUrlsForLanguagesEnabled)
                {
                    localizationSettings.SeoFriendlyUrlsForLanguagesEnabled = model.LocalizationSettings.SeoFriendlyUrlsForLanguagesEnabled;
                }

                localizationSettings.AutomaticallyDetectLanguage = model.LocalizationSettings.AutomaticallyDetectLanguage;
                localizationSettings.LoadAllLocaleRecordsOnStartup = model.LocalizationSettings.LoadAllLocaleRecordsOnStartup;
                localizationSettings.LoadAllLocalizedPropertiesOnStartup = model.LocalizationSettings.LoadAllLocalizedPropertiesOnStartup;
                localizationSettings.LoadAllUrlRecordsOnStartup = model.LocalizationSettings.LoadAllUrlRecordsOnStartup;
                await _settingService.SaveSettingAsync(localizationSettings);

                //display default menu item
                var displayDefaultMenuItemSettings = await _settingService.LoadSettingAsync<DisplayDefaultMenuItemSettings>(storeScope);

                //we do not clear cache after each setting update.
                //this behavior can increase performance because cached settings will not be cleared 
                //and loaded from database after each update
                displayDefaultMenuItemSettings.DisplayHomepageMenuItem = model.DisplayDefaultMenuItemSettings.DisplayHomepageMenuItem;
                displayDefaultMenuItemSettings.DisplayNewProductsMenuItem = model.DisplayDefaultMenuItemSettings.DisplayNewProductsMenuItem;
                displayDefaultMenuItemSettings.DisplayProductSearchMenuItem = model.DisplayDefaultMenuItemSettings.DisplayProductSearchMenuItem;
                displayDefaultMenuItemSettings.DisplayCustomerInfoMenuItem = model.DisplayDefaultMenuItemSettings.DisplayCustomerInfoMenuItem;
                displayDefaultMenuItemSettings.DisplayBlogMenuItem = model.DisplayDefaultMenuItemSettings.DisplayBlogMenuItem;
                displayDefaultMenuItemSettings.DisplayForumsMenuItem = model.DisplayDefaultMenuItemSettings.DisplayForumsMenuItem;
                displayDefaultMenuItemSettings.DisplayContactUsMenuItem = model.DisplayDefaultMenuItemSettings.DisplayContactUsMenuItem;

                await _settingService.SaveSettingOverridablePerStoreAsync(displayDefaultMenuItemSettings, x => x.DisplayHomepageMenuItem, model.DisplayDefaultMenuItemSettings.DisplayHomepageMenuItem_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(displayDefaultMenuItemSettings, x => x.DisplayNewProductsMenuItem, model.DisplayDefaultMenuItemSettings.DisplayNewProductsMenuItem_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(displayDefaultMenuItemSettings, x => x.DisplayProductSearchMenuItem, model.DisplayDefaultMenuItemSettings.DisplayProductSearchMenuItem_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(displayDefaultMenuItemSettings, x => x.DisplayCustomerInfoMenuItem, model.DisplayDefaultMenuItemSettings.DisplayCustomerInfoMenuItem_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(displayDefaultMenuItemSettings, x => x.DisplayBlogMenuItem, model.DisplayDefaultMenuItemSettings.DisplayBlogMenuItem_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(displayDefaultMenuItemSettings, x => x.DisplayForumsMenuItem, model.DisplayDefaultMenuItemSettings.DisplayForumsMenuItem_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(displayDefaultMenuItemSettings, x => x.DisplayContactUsMenuItem, model.DisplayDefaultMenuItemSettings.DisplayContactUsMenuItem_OverrideForStore, storeScope, false);

                //now clear settings cache
                await _settingService.ClearCacheAsync();

                //display default footer item
                var displayDefaultFooterItemSettings = await _settingService.LoadSettingAsync<DisplayDefaultFooterItemSettings>(storeScope);

                //we do not clear cache after each setting update.
                //this behavior can increase performance because cached settings will not be cleared 
                //and loaded from database after each update
                displayDefaultFooterItemSettings.DisplaySitemapFooterItem = model.DisplayDefaultFooterItemSettings.DisplaySitemapFooterItem;
                displayDefaultFooterItemSettings.DisplayContactUsFooterItem = model.DisplayDefaultFooterItemSettings.DisplayContactUsFooterItem;
                displayDefaultFooterItemSettings.DisplayProductSearchFooterItem = model.DisplayDefaultFooterItemSettings.DisplayProductSearchFooterItem;
                displayDefaultFooterItemSettings.DisplayNewsFooterItem = model.DisplayDefaultFooterItemSettings.DisplayNewsFooterItem;
                displayDefaultFooterItemSettings.DisplayBlogFooterItem = model.DisplayDefaultFooterItemSettings.DisplayBlogFooterItem;
                displayDefaultFooterItemSettings.DisplayForumsFooterItem = model.DisplayDefaultFooterItemSettings.DisplayForumsFooterItem;
                displayDefaultFooterItemSettings.DisplayRecentlyViewedProductsFooterItem = model.DisplayDefaultFooterItemSettings.DisplayRecentlyViewedProductsFooterItem;
                displayDefaultFooterItemSettings.DisplayCompareProductsFooterItem = model.DisplayDefaultFooterItemSettings.DisplayCompareProductsFooterItem;
                displayDefaultFooterItemSettings.DisplayNewProductsFooterItem = model.DisplayDefaultFooterItemSettings.DisplayNewProductsFooterItem;
                displayDefaultFooterItemSettings.DisplayCustomerInfoFooterItem = model.DisplayDefaultFooterItemSettings.DisplayCustomerInfoFooterItem;
                displayDefaultFooterItemSettings.DisplayCustomerOrdersFooterItem = model.DisplayDefaultFooterItemSettings.DisplayCustomerOrdersFooterItem;
                displayDefaultFooterItemSettings.DisplayCustomerAddressesFooterItem = model.DisplayDefaultFooterItemSettings.DisplayCustomerAddressesFooterItem;
                displayDefaultFooterItemSettings.DisplayShoppingCartFooterItem = model.DisplayDefaultFooterItemSettings.DisplayShoppingCartFooterItem;
                displayDefaultFooterItemSettings.DisplayWishlistFooterItem = model.DisplayDefaultFooterItemSettings.DisplayWishlistFooterItem;
                displayDefaultFooterItemSettings.DisplayApplyVendorAccountFooterItem = model.DisplayDefaultFooterItemSettings.DisplayApplyVendorAccountFooterItem;

                await _settingService.SaveSettingOverridablePerStoreAsync(displayDefaultFooterItemSettings, x => x.DisplaySitemapFooterItem, model.DisplayDefaultFooterItemSettings.DisplaySitemapFooterItem_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(displayDefaultFooterItemSettings, x => x.DisplayContactUsFooterItem, model.DisplayDefaultFooterItemSettings.DisplayContactUsFooterItem_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(displayDefaultFooterItemSettings, x => x.DisplayProductSearchFooterItem, model.DisplayDefaultFooterItemSettings.DisplayProductSearchFooterItem_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(displayDefaultFooterItemSettings, x => x.DisplayNewsFooterItem, model.DisplayDefaultFooterItemSettings.DisplayNewsFooterItem_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(displayDefaultFooterItemSettings, x => x.DisplayBlogFooterItem, model.DisplayDefaultFooterItemSettings.DisplayBlogFooterItem_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(displayDefaultFooterItemSettings, x => x.DisplayForumsFooterItem, model.DisplayDefaultFooterItemSettings.DisplayForumsFooterItem_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(displayDefaultFooterItemSettings, x => x.DisplayRecentlyViewedProductsFooterItem, model.DisplayDefaultFooterItemSettings.DisplayRecentlyViewedProductsFooterItem_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(displayDefaultFooterItemSettings, x => x.DisplayCompareProductsFooterItem, model.DisplayDefaultFooterItemSettings.DisplayCompareProductsFooterItem_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(displayDefaultFooterItemSettings, x => x.DisplayNewProductsFooterItem, model.DisplayDefaultFooterItemSettings.DisplayNewProductsFooterItem_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(displayDefaultFooterItemSettings, x => x.DisplayCustomerInfoFooterItem, model.DisplayDefaultFooterItemSettings.DisplayCustomerInfoFooterItem_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(displayDefaultFooterItemSettings, x => x.DisplayCustomerOrdersFooterItem, model.DisplayDefaultFooterItemSettings.DisplayCustomerOrdersFooterItem_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(displayDefaultFooterItemSettings, x => x.DisplayCustomerAddressesFooterItem, model.DisplayDefaultFooterItemSettings.DisplayCustomerAddressesFooterItem_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(displayDefaultFooterItemSettings, x => x.DisplayShoppingCartFooterItem, model.DisplayDefaultFooterItemSettings.DisplayShoppingCartFooterItem_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(displayDefaultFooterItemSettings, x => x.DisplayWishlistFooterItem, model.DisplayDefaultFooterItemSettings.DisplayWishlistFooterItem_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(displayDefaultFooterItemSettings, x => x.DisplayApplyVendorAccountFooterItem, model.DisplayDefaultFooterItemSettings.DisplayApplyVendorAccountFooterItem_OverrideForStore, storeScope, false);

                //now clear settings cache
                await _settingService.ClearCacheAsync();

                //admin area
                var adminAreaSettings = await _settingService.LoadSettingAsync<AdminAreaSettings>(storeScope);

                //we do not clear cache after each setting update.
                //this behavior can increase performance because cached settings will not be cleared 
                //and loaded from database after each update
                adminAreaSettings.UseRichEditorInMessageTemplates = model.AdminAreaSettings.UseRichEditorInMessageTemplates;

                await _settingService.SaveSettingOverridablePerStoreAsync(adminAreaSettings, x => x.UseRichEditorInMessageTemplates, model.AdminAreaSettings.UseRichEditorInMessageTemplates_OverrideForStore, storeScope, false);

                //now clear settings cache
                await _settingService.ClearCacheAsync();

                //activity log
                await _customerActivityService.InsertActivityAsync("EditSettings", await _localizationService.GetResourceAsync("ActivityLog.EditSettings"));

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));

                return RedirectToAction("GeneralCommon");
            }

            //prepare model
            model = await _settingModelFactory.PrepareGeneralCommonSettingsModelAsync(model);

            //if we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpPost, ActionName("GeneralCommon")]
        [FormValueRequired("changeencryptionkey")]
        public virtual async Task<IActionResult> ChangeEncryptionKey(GeneralCommonSettingsModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var securitySettings = await _settingService.LoadSettingAsync<SecuritySettings>(storeScope);

            try
            {
                if (model.SecuritySettings.EncryptionKey == null)
                    model.SecuritySettings.EncryptionKey = string.Empty;

                model.SecuritySettings.EncryptionKey = model.SecuritySettings.EncryptionKey.Trim();

                var newEncryptionPrivateKey = model.SecuritySettings.EncryptionKey;
                if (string.IsNullOrEmpty(newEncryptionPrivateKey) || newEncryptionPrivateKey.Length != 16)
                    throw new NopException(await _localizationService.GetResourceAsync("Admin.Configuration.Settings.GeneralCommon.EncryptionKey.TooShort"));

                var oldEncryptionPrivateKey = securitySettings.EncryptionKey;
                if (oldEncryptionPrivateKey == newEncryptionPrivateKey)
                    throw new NopException(await _localizationService.GetResourceAsync("Admin.Configuration.Settings.GeneralCommon.EncryptionKey.TheSame"));

                //update password information
                //optimization - load only passwords with PasswordFormat.Encrypted
                var customerPasswords = await _customerService.GetCustomerPasswordsAsync(passwordFormat: PasswordFormat.Encrypted);
                foreach (var customerPassword in customerPasswords)
                {
                    var decryptedPassword = _encryptionService.DecryptText(customerPassword.Password, oldEncryptionPrivateKey);
                    var encryptedPassword = _encryptionService.EncryptText(decryptedPassword, newEncryptionPrivateKey);

                    customerPassword.Password = encryptedPassword;
                    await _customerService.UpdateCustomerPasswordAsync(customerPassword);
                }

                securitySettings.EncryptionKey = newEncryptionPrivateKey;
                await _settingService.SaveSettingAsync(securitySettings);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Settings.GeneralCommon.EncryptionKey.Changed"));
            }
            catch (Exception exc)
            {
                await _notificationService.ErrorNotificationAsync(exc);
            }

            return RedirectToAction("GeneralCommon");
        }

        [HttpPost]
        public virtual async Task<IActionResult> UploadLocalePattern()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            try
            {
                await _uploadService.UploadLocalePatternAsync();
                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Settings.GeneralCommon.LocalePattern.SuccessUpload"));
            }
            catch (Exception exc)
            {
                await _notificationService.ErrorNotificationAsync(exc);
            }

            return RedirectToAction("GeneralCommon");
        }

        [HttpPost]
        public virtual async Task<IActionResult> UploadIcons(IFormFile iconsFile)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            try
            {
                if (iconsFile == null || iconsFile.Length == 0)
                {
                    _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Common.UploadFile"));
                    return RedirectToAction("GeneralCommon");
                }

                //load settings for a chosen store scope
                var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
                var commonSettings = await _settingService.LoadSettingAsync<CommonSettings>(storeScope);

                switch (_fileProvider.GetFileExtension(iconsFile.FileName))
                {
                    case ".ico":
                        await _uploadService.UploadFaviconAsync(iconsFile);
                        commonSettings.FaviconAndAppIconsHeadCode = string.Format(NopCommonDefaults.SingleFaviconHeadLink, storeScope, iconsFile.FileName);

                        break;

                    case ".zip":
                        await _uploadService.UploadIconsArchiveAsync(iconsFile);

                        var headCodePath = _fileProvider.GetAbsolutePath(string.Format(NopCommonDefaults.FaviconAndAppIconsPath, storeScope), NopCommonDefaults.HeadCodeFileName);
                        if (!_fileProvider.FileExists(headCodePath))
                            throw new Exception(string.Format(await _localizationService.GetResourceAsync("Admin.Configuration.Settings.GeneralCommon.FaviconAndAppIcons.MissingFile"), NopCommonDefaults.HeadCodeFileName));

                        using (var sr = new StreamReader(headCodePath))
                            commonSettings.FaviconAndAppIconsHeadCode = await sr.ReadToEndAsync();

                        break;

                    default:
                        throw new InvalidOperationException("File is not supported.");
                }

                await _settingService.SaveSettingOverridablePerStoreAsync(commonSettings, x => x.FaviconAndAppIconsHeadCode, true, storeScope);

                //delete old favicon icon if exist
                var oldFaviconIconPath = _fileProvider.GetAbsolutePath(string.Format(NopCommonDefaults.OldFaviconIconName, storeScope));
                if (_fileProvider.FileExists(oldFaviconIconPath))
                {
                    _fileProvider.DeleteFile(oldFaviconIconPath);
                }

                //activity log
                await _customerActivityService.InsertActivityAsync("UploadIcons", string.Format(await _localizationService.GetResourceAsync("ActivityLog.UploadNewIcons"), storeScope));
                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.FaviconAndAppIcons.Uploaded"));
            }
            catch (Exception exc)
            {
                await _notificationService.ErrorNotificationAsync(exc);
            }

            return RedirectToAction("GeneralCommon");
        }

        public virtual async Task<IActionResult> AllSettings(string settingName)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            //prepare model
            var model = await _settingModelFactory.PrepareSettingSearchModelAsync(new SettingSearchModel { SearchSettingName = WebUtility.HtmlEncode(settingName) });

            return View(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> AllSettings(SettingSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
                return await AccessDeniedDataTablesJson();

            //prepare model
            var model = await _settingModelFactory.PrepareSettingListModelAsync(searchModel);

            return Json(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> SettingUpdate(SettingModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            if (model.Name != null)
                model.Name = model.Name.Trim();

            if (model.Value != null)
                model.Value = model.Value.Trim();

            if (!ModelState.IsValid)
                return ErrorJson(ModelState.SerializeErrors());

            //try to get a setting with the specified id
            var setting = await _settingService.GetSettingByIdAsync(model.Id)
                ?? throw new ArgumentException("No setting found with the specified id");

            if (!setting.Name.Equals(model.Name, StringComparison.InvariantCultureIgnoreCase))
            {
                //setting name has been changed
                await _settingService.DeleteSettingAsync(setting);
            }

            await _settingService.SetSettingAsync(model.Name, model.Value, setting.StoreId);

            //activity log
            await _customerActivityService.InsertActivityAsync("EditSettings", await _localizationService.GetResourceAsync("ActivityLog.EditSettings"), setting);

            return new NullJsonResult();
        }

        [HttpPost]
        public virtual async Task<IActionResult> SettingAdd(SettingModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            if (model.Name != null)
                model.Name = model.Name.Trim();

            if (model.Value != null)
                model.Value = model.Value.Trim();

            if (!ModelState.IsValid)
                return ErrorJson(ModelState.SerializeErrors());

            var storeId = model.StoreId;
            await _settingService.SetSettingAsync(model.Name, model.Value, storeId);

            //activity log
            await _customerActivityService.InsertActivityAsync("AddNewSetting",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.AddNewSetting"), model.Name),
                await _settingService.GetSettingAsync(model.Name, storeId));

            return Json(new { Result = true });
        }

        [HttpPost]
        public virtual async Task<IActionResult> SettingDelete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            //try to get a setting with the specified id
            var setting = await _settingService.GetSettingByIdAsync(id)
                ?? throw new ArgumentException("No setting found with the specified id", nameof(id));

            await _settingService.DeleteSettingAsync(setting);

            //activity log
            await _customerActivityService.InsertActivityAsync("DeleteSetting",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.DeleteSetting"), setting.Name), setting);

            return new NullJsonResult();
        }

        //action displaying notification (warning) to a store owner about a lot of traffic 
        //between the distributed cache server and the application when LoadAllLocaleRecordsOnStartup setting is set
        public async Task<IActionResult> DistributedCacheHighTrafficWarning(bool loadAllLocaleRecordsOnStartup)
        {
            //LoadAllLocaleRecordsOnStartup is set and distributed cache is used, so display warning
            if (_appSettings.Get<DistributedCacheConfig>().Enabled && _appSettings.Get<DistributedCacheConfig>().DistributedCacheType != DistributedCacheType.Memory && loadAllLocaleRecordsOnStartup)
            {
                return Json(new
                {
                    Result = await _localizationService
                        .GetResourceAsync("Admin.Configuration.Settings.GeneralCommon.LoadAllLocaleRecordsOnStartup.Warning")
                });
            }

            return Json(new { Result = string.Empty });
        }

        //Action that displays a notification (warning) to the store owner about the absence of active authentication providers
        public async Task<IActionResult> ForceMultifactorAuthenticationWarning(bool forceMultifactorAuthentication)
        {
            //ForceMultifactorAuthentication is set and the store haven't active Authentication provider , so display warning
            if (forceMultifactorAuthentication && !await _multiFactorAuthenticationPluginManager.HasActivePluginsAsync())
            {
                return Json(new
                {
                    Result = await _localizationService
                        .GetResourceAsync("Admin.Configuration.Settings.CustomerUser.ForceMultifactorAuthentication.Warning")
                });
            }

            return Json(new { Result = string.Empty });
        }

        //Action that displays a notification (warning) to the store owner about the need to restart the application after changing the setting
        public async Task<IActionResult> SeoFriendlyUrlsForLanguagesEnabledWarning(bool seoFriendlyUrlsForLanguagesEnabled)
        {
            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var localizationSettings = await _settingService.LoadSettingAsync<LocalizationSettings>(storeScope);

            if (seoFriendlyUrlsForLanguagesEnabled != localizationSettings.SeoFriendlyUrlsForLanguagesEnabled)
            {
                return Json(new
                {
                    Result = await _localizationService
                        .GetResourceAsync("Admin.Configuration.Settings.GeneralCommon.SeoFriendlyUrlsForLanguagesEnabled.Warning")
                });
            }

            return Json(new { Result = string.Empty });
        }

        public virtual async Task<IActionResult> Leave()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            //prepare model
            var model = await _settingModelFactory.PrepareLeaveSettingsModelAsync();

            return View(model);
        }
        [HttpPost]
        public virtual async Task<IActionResult> Leave(LeaveSettingsModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();
            if (model.HrEmail == null || model.HrEmail.Trim() == "")
            {
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Leave.Error.HrEmail"));
                model = await _settingModelFactory.PrepareLeaveSettingsModelAsync(model);

                //if we got this far, something failed, redisplay form
                return View(model);
            }
            if (ModelState.IsValid)
            {
                //load settings for a chosen store scope
                var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
                var leaveSettings = await _settingService.LoadSettingAsync<LeaveSettings>(storeScope);
                leaveSettings = model.ToSettings(leaveSettings);

                //we do not clear cache after each setting update.
                //this behavior can increase performance because cached settings will not be cleared 
                //and loaded from database after each update
                await _settingService.SaveSettingOverridablePerStoreAsync(leaveSettings, x => x.HrEmail, model.HrEmail_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(leaveSettings, x => x.SendEmailToAllProjectLeaders, model.SendEmailToAllProjectLeaders_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(leaveSettings, x => x.SendEmailToAllProjectManager, model.SendEmailToAllProjectManager_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(leaveSettings, x => x.CommonEmails, model.CommonEmails_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(leaveSettings, x => x.SendEmailToEmployeeManager, model.SendEmailToEmployeeManager_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(leaveSettings, x => x.SeletedLeaveTypeId, model.SeletedLeaveTypeId_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(leaveSettings, x => x.LastUpdateBalance, model.LastUpdateBalance_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(leaveSettings, x => x.LeaveTestDate, model.LeaveTestDate_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(leaveSettings, x => x.AddMonthlyLeaveDay, model.AddMonthlyLeaveDay_OverrideForStore, storeScope, false);


                await _settingService.ClearCacheAsync();

                //activity log
                //await _customerActivityService.InsertActivityAsync("EditSettings", await _localizationService.GetResourceAsync("ActivityLog.EditSettings"));

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));

                return RedirectToAction("Leave");
            }

            //prepare model
            model = await _settingModelFactory.PrepareLeaveSettingsModelAsync(model);

            //if we got this far, something failed, redisplay form
            return View(model);
        }


        public virtual async Task<IActionResult> MonthlyReport()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            //prepare model
            var model = await _settingModelFactory.PrepareMonthlyReportSettingsModelAsync();

            return View(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> MonthlyReport(MonthlyReportSettingsModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();
          
            if (ModelState.IsValid)
            {
                //load settings for a chosen store scope
                var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
                var monthlyReportSetting = await _settingService.LoadSettingAsync<MonthlyReportSetting>(storeScope);
                monthlyReportSetting = model.ToSettings(monthlyReportSetting);

                //we do not clear cache after each setting update.
                //this behavior can increase performance because cached settings will not be cleared 
                //and loaded from database after each update
                await _settingService.SaveSettingOverridablePerStoreAsync(monthlyReportSetting, x => x.AllowedVariations, model.AllowedVariations_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(monthlyReportSetting, x => x.AllowedQABillableHours, model.AllowedQABillableHours_OverrideForStore, storeScope, false);

                await _settingService.SaveSettingOverridablePerStoreAsync(monthlyReportSetting, x => x.DayTime_From, model.DayTime_From_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(monthlyReportSetting, x => x.DayTime_To, model.DayTime_To_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(monthlyReportSetting, x => x.WeekDay, model.WeekDay_OverrideForStore, storeScope, false);

                await _settingService.SaveSettingOverridablePerStoreAsync(monthlyReportSetting, x => x.SendReportToEmployee, model.SendReportToEmployee_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(monthlyReportSetting, x => x.SendReportToManager, model.SendReportToManager_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(monthlyReportSetting, x => x.SendReportToProjectLeader, model.SendReportToProjectLeader_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(monthlyReportSetting, x => x.ShowOnlyNotDOT, model.ShowOnlyNotDOT_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(monthlyReportSetting, x => x.SendReportToHR, model.SendReportToHR_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(monthlyReportSetting, x => x.LearningProjectId, model.LearningProjectId_OverrideForStore, storeScope, false);



                //for overdue email
                await _settingService.SaveSettingOverridablePerStoreAsync(monthlyReportSetting, x => x.OverDue_From, model.OverDue_From_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(monthlyReportSetting, x => x.OverDue_To, model.OverDue_To_OverrideForStore, storeScope, false);

                await _settingService.SaveSettingOverridablePerStoreAsync(monthlyReportSetting, x => x.SendOverdueEmailToEmployee, model.SendOverdueEmailToEmployee_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(monthlyReportSetting, x => x.SendOverdueReportToProjectLeader, model.SendOverdueReportToProjectLeader_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(monthlyReportSetting, x => x.SendOverdueReportToManager, model.SendOverdueReportToManager_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(monthlyReportSetting, x => x.SendOverdueReportToHR, model.SendOverdueReportToHR_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(monthlyReportSetting, x => x.OverdueCountCCThreshold, model.OverdueCountCCThreshold_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(monthlyReportSetting, x => x.IncludeProjectLeadersInCC, model.IncludeProjectLeadersInCC_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(monthlyReportSetting, x => x.IncludeProjectManagerInCC, model.IncludeProjectManagerInCC_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(monthlyReportSetting, x => x.IncludeManagementInCC, model.IncludeManagementInCC_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(monthlyReportSetting, x => x.IncludeHRInCC, model.IncludeHRInCC_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(monthlyReportSetting, x => x.IncludeProjectCoordinatorInCC, model.IncludeProjectCoordinatorInCC_OverrideForStore, storeScope, false);

                await _settingService.ClearCacheAsync();

                //activity log
                //await _customerActivityService.InsertActivityAsync("EditSettings", await _localizationService.GetResourceAsync("ActivityLog.EditSettings"));

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));

                return RedirectToAction("MonthlyReport");
            }

            //prepare model
            model = await _settingModelFactory.PrepareMonthlyReportSettingsModelAsync(model);

            //if we got this far, something failed, redisplay form
            return View(model);
        }


        public virtual async Task<IActionResult> Employee()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            //prepare model
            var model = await _settingModelFactory.PrepareEmployeeSettingsModelAsync();
            
           
            return View(model);
        }
        [HttpPost]
        public virtual async Task<IActionResult> Employee(EmployeeSettingsModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                //load settings for a chosen store scope
                var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
                var timesheetSettings = await _settingService.LoadSettingAsync<EmployeeSettings>(storeScope);
                timesheetSettings = model.ToSettings(timesheetSettings);
              
                await _settingService.SaveSettingOverridablePerStoreAsync(timesheetSettings, x => x.OnBoardingEmail, model.OnBoardingEmail_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(timesheetSettings, x => x.CoordinatorRoleId, model.CoordinatorRoleId_OverrideForStore, storeScope, false);

                await _settingService.ClearCacheAsync();

                //activity log
                //await _customerActivityService.InsertActivityAsync("EditSettings", await _localizationService.GetResourceAsync("ActivityLog.EditSettings"));

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));

                return RedirectToAction("Employee");
            }

            //prepare model
            model = await _settingModelFactory.PrepareEmployeeSettingsModelAsync(model);

            //if we got this far, something failed, redisplay form
            return View(model);
        }


        public virtual async Task<IActionResult> TimeSheet()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            //prepare model
            var model = await _settingModelFactory.PrepareTimeSheetSettingsModelAsync();
            var departmentIds = model.DepartmentIds;
            var departmentList = new List<int>();
            if (departmentIds != null && departmentIds != "")
            {
                departmentList = model.DepartmentIds.Split(',').Select(int.Parse).ToList();
                model.SelectedDepartmentIds = departmentList;
            }
            return View(model);
        }
        [HttpPost]
        public virtual async Task<IActionResult> TimeSheet(TimeSheetSettingsModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();
            if (model.Reminder1_To == null || model.Reminder1_To ==null || model.Reminder2_From== null || model.Reminder2_To == null)
            {
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.TimeSheet.Error.FromTo"));
                model = await _settingModelFactory.PrepareTimeSheetSettingsModelAsync(model);

                //if we got this far, something failed, redisplay form
                return View(model);
            }
            if (ModelState.IsValid)
            {
                //load settings for a chosen store scope
                var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
                var timesheetSettings = await _settingService.LoadSettingAsync<TimeSheetSetting>(storeScope);
                timesheetSettings = model.ToSettings(timesheetSettings);
                var departmentList = model.SelectedDepartmentIds;
                timesheetSettings.DepartmentIds = "";
                if(departmentList != null)
                timesheetSettings.DepartmentIds = string.Join(",", departmentList.Select(d => d.ToString()));


                await _settingService.SaveSettingOverridablePerStoreAsync(timesheetSettings, x => x.Reminder1_From, model.Reminder1_From_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(timesheetSettings, x => x.Reminder1_To, model.Reminder1_To_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(timesheetSettings, x => x.Reminder2_From, model.Reminder2_From_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(timesheetSettings, x => x.Reminder2_To, model.Reminder2_To_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(timesheetSettings, x => x.DepartmentIds, model.DepartmentIds_OverrideForStore, storeScope, false);

                await _settingService.SaveSettingOverridablePerStoreAsync(timesheetSettings, x => x.SendEmailToAllProjectLeaders, model.SendEmailToAllProjectLeaders_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(timesheetSettings, x => x.SendEmailToAllProjectManager, model.SendEmailToAllProjectManager_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(timesheetSettings, x => x.CommonEmails, model.CommonEmails_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(timesheetSettings, x => x.SendEmailToEmployeeManager, model.SendEmailToEmployeeManager_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(timesheetSettings, x => x.SendEmailToHr, model.SendEmailToHr_OverrideForStore, storeScope, false);

                await _settingService.SaveSettingOverridablePerStoreAsync(timesheetSettings, x => x.ConsiderBeforeDay, model.ConsiderBeforeDay_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(timesheetSettings, x => x.SendWithCCAfterDay, model.SendWithCCAfterDay_OverrideForStore, storeScope, false);

                await _settingService.ClearCacheAsync();

                //activity log
                //await _customerActivityService.InsertActivityAsync("EditSettings", await _localizationService.GetResourceAsync("ActivityLog.EditSettings"));

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));

                return RedirectToAction("TimeSheet");
            }

            //prepare model
            model = await _settingModelFactory.PrepareTimeSheetSettingsModelAsync(model);

            //if we got this far, something failed, redisplay form
            return View(model);
        }

        public virtual async Task<IActionResult> ProjectTask()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            var model = await _settingModelFactory.PrepareProjectTaskSettingsModelAsync();
            return View(model);
        }
        [HttpPost]
        public virtual async Task<IActionResult> ProjectTask(ProjectTaskSettingsModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
                var projectTaskSettings = await _settingService.LoadSettingAsync<ProjectTaskSetting>(storeScope);
                projectTaskSettings = model.ToSettings(projectTaskSettings);
    
                await _settingService.SaveSettingOverridablePerStoreAsync(projectTaskSettings, x => x.IsShowSelctAllCheckList, model.IsShowSelctAllCheckList_OverrideForStore, storeScope, false);

                await _settingService.ClearCacheAsync();

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));

                return RedirectToAction("ProjectTask");
            }

            model = await _settingModelFactory.PrepareProjectTaskSettingsModelAsync(model);
            return View(model);
        }





        public virtual async Task<IActionResult> EmployeeAttendance()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            //prepare model
            var model = await _settingModelFactory.PrepareEmployeeAttendanceSettingsModelAsync();

            return View(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> EmployeeAttendance(EmployeeAttendanceSettingsModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            if (model.OfficeTime_To == null || model.OfficeTime_From == null)
            {
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.EmployeeAttendance.Error.DateRequired"));
                model = await _settingModelFactory.PrepareEmployeeAttendanceSettingsModelAsync(model);

                //if we got this far, something failed, redisplay form
                return View(model);
            }
            if (ModelState.IsValid)
            {
                //load settings for a chosen store scope
                var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
                var attendanceSettings = await _settingService.LoadSettingAsync<EmployeeAttendanceSetting>(storeScope);
                attendanceSettings = model.ToSettings(attendanceSettings);


                await _settingService.SaveSettingOverridablePerStoreAsync(attendanceSettings, x => x.OfficeTime_From, model.OfficeTime_From_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(attendanceSettings, x => x.OfficeTime_To, model.OfficeTime_To_OverrideForStore, storeScope, false);
                

                await _settingService.ClearCacheAsync();

                //activity log
                //await _customerActivityService.InsertActivityAsync("EditSettings", await _localizationService.GetResourceAsync("ActivityLog.EditSettings"));

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));

                return RedirectToAction("EmployeeAttendance");
            }

            //prepare model
            model = await _settingModelFactory.PrepareEmployeeAttendanceSettingsModelAsync(model);

            //if we got this far, something failed, redisplay form
            return View(model);
        }


        public virtual async Task<IActionResult> TeamPerformance()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            //prepare model
            var model = await _settingModelFactory.PrepareTeamPerformanceSettingsModelAsync();

            return View(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> TeamPerformance(TeamPerformanceSettingsModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

           
            if (ModelState.IsValid)
            {
                //load settings for a chosen store scope
                var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
                var attendanceSettings = await _settingService.LoadSettingAsync<TeamPerformanceSettings>(storeScope);
                attendanceSettings = model.ToSettings(attendanceSettings);


                await _settingService.SaveSettingOverridablePerStoreAsync(attendanceSettings, x => x.FeedbackShowId, model.FeedbackShowId_OverrideForStore, storeScope, false);

                await _settingService.SaveSettingOverridablePerStoreAsync(attendanceSettings, x => x.StartReminderDate, model.StartReminderDate_OverrideForStore, storeScope, false);

                await _settingService.SaveSettingOverridablePerStoreAsync(attendanceSettings, x => x.StartCCDate, model.StartCCDate_OverrideForStore, storeScope, false);



                await _settingService.ClearCacheAsync();

                //activity log
                //await _customerActivityService.InsertActivityAsync("EditSettings", await _localizationService.GetResourceAsync("ActivityLog.EditSettings"));

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));

                return RedirectToAction("TeamPerformance");
            }

            //prepare model
            model = await _settingModelFactory.PrepareTeamPerformanceSettingsModelAsync(model);

            //if we got this far, something failed, redisplay form
            return View(model);
        }
        #endregion


        #region Email Settings Methods

        public virtual async Task<IActionResult> EmailSettings()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            var model = await _settingModelFactory.PrepareEmailSettingsModelAsync();

            return View(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> EmailSettings(EmailSettingsModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
                var emailSettings = await _settingService.LoadSettingAsync<EmailSettings>(storeScope);
                emailSettings = model.ToSettings(emailSettings);

                await _settingService.SaveSettingOverridablePerStoreAsync(emailSettings, x => x.FirstMailVariation, model.FirstMailVariation_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(emailSettings, x => x.SecondMailVariation, model.SecondMailVariation_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(emailSettings, x => x.ThirdMailVariation, model.ThirdMailVariation_OverrideForStore, storeScope, false);

                await _settingService.ClearCacheAsync();

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));

                return RedirectToAction(nameof(EmailSettings));
            }

            model = await _settingModelFactory.PrepareEmailSettingsModelAsync(model);

            return View(model);
        }

        #endregion
    }
}