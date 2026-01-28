using App.Core;
using App.Core.Domain.Cms;
using App.Services.Common;
using App.Services.Configuration;
using App.Services.Localization;
using App.Services.Messages;
using App.Services.Plugins;
using App.Services.ScheduleTasks;
using App.Services.Stores;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Satyanam.Plugin.Misc.EmailVerification
{
    /// <summary>
    /// Represents the Sendinblue plugin
    /// </summary>
    public class EmailVerificationPlugin : BasePlugin, IMiscPlugin
    {
        #region Fields

        private readonly IEmailAccountService _emailAccountService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ILocalizationService _localizationService;
        private readonly IMessageTemplateService _messageTemplateService;
        private readonly IScheduleTaskService _scheduleTaskService;
        private readonly ISettingService _settingService;
        private readonly IStoreService _storeService;
        private readonly IWebHelper _webHelper;
        private readonly WidgetSettings _widgetSettings;

        #endregion

        #region Ctor

        public EmailVerificationPlugin(IEmailAccountService emailAccountService,
            IGenericAttributeService genericAttributeService,
            ILocalizationService localizationService,
            IMessageTemplateService messageTemplateService,
            IScheduleTaskService scheduleTaskService,
            ISettingService settingService,
            IStoreService storeService,
            IWebHelper webHelper,
            WidgetSettings widgetSettings)
        {
            _emailAccountService = emailAccountService;
            _genericAttributeService = genericAttributeService;
            _localizationService = localizationService;
            _messageTemplateService = messageTemplateService;
            _scheduleTaskService = scheduleTaskService;
            _settingService = settingService;
            _storeService = storeService;
            _webHelper = webHelper;
            _widgetSettings = widgetSettings;
        }

        #endregion

        #region Methods




        /// <summary>
        /// Gets a configuration page URL
        /// </summary>
        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/Configure/Configure";
        }

        /// <summary>
        /// Install the plugin
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        public override async Task InstallAsync()
        {
            //settings
            await _settingService.SaveSettingAsync(new EmailVerificationSettings
            {
                ApiKey = "cfce1ef159a7c90cdd6795d5a84f8623d0e881d00b0c5d9d94dde1042d07"

            });

            await _localizationService.AddOrUpdateLocaleResourceAsync(new Dictionary<string, string>
            {
                ["Plugins.Misc.EmailVerification.Fields.ApiKey"] = "ApiKey",
                ["Plugins.Misc.EmailVerification.Fields.ApiUrl"] = "ApiUrl",
                ["Plugins.Misc.EmailVerification.Fields.DisconnectOnUninstall"] = "DisconnectOnUninstall",
                ["Plugins.Misc.EmailVerification.Fields.Registartionpage"] = "Registartionpage",
                ["Plugins.Misc.EmailVerification.Fields.ContactUspage"] = "ContactUspage",
            });


            // Add localized resources
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.MyCompany.EmailVerification.ApiKey", "API Key");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.MyCompany.EmailVerification.EmailValidationMessage", "Invalid email address.");


            await base.InstallAsync();
        }

        /// <summary>
        /// Uninstall the plugin
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        /// <summary>
        /// Uninstall plugin
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        //public override async Task UninstallAsync()
        //{
        //    // Remove settings
        //    await _settingService.DeleteSettingAsync<EmailVerificationSettings>();

        //    // Remove localized resources
        //    await _localizationService.DeleteLocaleResourcesAsync("Plugins.Widgets.EmailVerification");

        //    await base.UninstallAsync();
        //}

        public override async Task UninstallAsync()
        {
            var zettleSettings = await _settingService.LoadSettingAsync<EmailVerificationSettings>();
            if (zettleSettings.DisconnectOnUninstall)

                await _settingService.DeleteSettingAsync<EmailVerificationSettings>();
            await _localizationService.DeleteLocaleResourcesAsync("Plugins.Widgets.EmailVerification");

            await base.UninstallAsync();
        }




        #endregion

        /// <summary>
        /// Gets a value indicating whether to hide this plugin on the widget list page in the admin area
        /// </summary>
        public bool HideInWidgetList => true;
    }
}
