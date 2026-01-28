using App.Core;
using App.Core.Caching;
using App.Core.Domain.Messages;
using App.Services.Common;
using App.Services.Configuration;
using App.Services.Localization;
using App.Services.Logging;
using App.Services.Messages;
using App.Services.Stores;
using App.Web.Framework;
using App.Web.Framework.Controllers;
using App.Web.Framework.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Satyanam.Plugin.Misc.EmailVerification.Models;
using System.Threading.Tasks;

namespace Satyanam.Plugin.Misc.EmailVerification.Controllers
{
    [AutoValidateAntiforgeryToken]
    public class ConfigureController : BasePluginController
    {
        #region Fields

        private readonly IEmailAccountService _emailAccountService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ILocalizationService _localizationService;
        private readonly ILogger _logger;
        private readonly IMessageTemplateService _messageTemplateService;
        private readonly IMessageTokenProvider _messageTokenProvider;
        private readonly INotificationService _notificationService;
        private readonly ISettingService _settingService;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly IStoreContext _storeContext;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IStoreService _storeService;
        private readonly IWorkContext _workContext;
        private readonly EmailVerificationSettings _verificationSettings;
        #endregion

        #region Ctor

        public ConfigureController(IEmailAccountService emailAccountService,
            IGenericAttributeService genericAttributeService,
            ILocalizationService localizationService,
            ILogger logger,
            IMessageTemplateService messageTemplateService,
            IMessageTokenProvider messageTokenProvider,
            INotificationService notificationService,
            ISettingService settingService,
            IStaticCacheManager staticCacheManager,
            IStoreContext storeContext,
            IStoreMappingService storeMappingService,
            IStoreService storeService,
            IWorkContext workContext,
            MessageTemplatesSettings messageTemplatesSetting,
            EmailVerificationSettings verificationSettings)
        {
            _emailAccountService = emailAccountService;
            _genericAttributeService = genericAttributeService;
            _localizationService = localizationService;
            _logger = logger;
            _messageTemplateService = messageTemplateService;
            _messageTokenProvider = messageTokenProvider;
            _notificationService = notificationService;
            _settingService = settingService;
            _staticCacheManager = staticCacheManager;
            _storeContext = storeContext;
            _storeMappingService = storeMappingService;
            _storeService = storeService;
            _workContext = workContext;
            _verificationSettings = verificationSettings;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Prepare SendinblueModel
        /// </summary>
        /// <param name="model">Model</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        private async Task PrepareModelAsync(ConfigurationModel model)
        {
            //load settings for active store scope
            var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var emailcverificationSettings = await _settingService.LoadSettingAsync<EmailVerificationSettings>(storeId);

            //whether plugin is configured
            //if (string.IsNullOrEmpty(sendinblueSettings.ApiKey))
            //    return;

            //prepare common properties
            model.ActiveStoreScopeConfiguration = storeId;
            model.ApiKey = emailcverificationSettings.ApiKey;
            model.ApiUrl = emailcverificationSettings.ApiUrl;
            model.DisconnectOnUninstall = emailcverificationSettings.DisconnectOnUninstall;
            model.ContactUspage = emailcverificationSettings.ContactUspages;
            model.Registartionpage = emailcverificationSettings.Registartionpage;
            model.EBookspage = emailcverificationSettings.EBookspage;

        }

        #endregion

        #region Methods

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public async Task<IActionResult> Configure()
        {
            var model = new ConfigurationModel();
            await PrepareModelAsync(model);

            return View("~/Plugins/Misc.EmailVerification/Views/Configure.cshtml", model);
        }

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        [HttpPost, ActionName("Configure")]
        [FormValueRequired("save")]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!ModelState.IsValid)
                return await Configure();

            var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var emailcverificationSettings = await _settingService.LoadSettingAsync<EmailVerificationSettings>(storeId);

            //set API key
            emailcverificationSettings.ApiKey = model.ApiKey;
            emailcverificationSettings.ApiUrl = model.ApiUrl;
            emailcverificationSettings.ContactUspages = model.ContactUspage;
            emailcverificationSettings.EBookspage = model.EBookspage;
            emailcverificationSettings.Registartionpage = model.Registartionpage;
            await _settingService.SaveSettingAsync(emailcverificationSettings, settings => settings.ApiKey, clearCache: false);
            await _settingService.SaveSettingAsync(emailcverificationSettings);
            await _settingService.ClearCacheAsync();

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

            return await Configure();
        }

        //[AuthorizeAdmin]
        //[Area(AreaNames.Admin)]
        //[HttpPost, ActionName("Configure")]
        //[FormValueRequired("saveSync")]
        //public async Task<IActionResult> SaveSynchronization(ConfigurationModel model)
        //{
        //    if (!ModelState.IsValid)
        //        return await Configure();

        //    var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        //    var sendinblueSettings = await _settingService.LoadSettingAsync<EmailVerificationSettings>(storeId);



        //    //set list of contacts to synchronize
        //    sendinblueSettings.ApiUrl = model.ApiUrl;

        //    //now clear settings cache
        //    await _settingService.ClearCacheAsync();

        //    _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

        //    return await Configure();
        //}



        #endregion
    }


}