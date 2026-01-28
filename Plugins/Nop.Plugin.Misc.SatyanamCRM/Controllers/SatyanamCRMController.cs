using App.Services.Configuration;
using App.Services.Localization;
using App.Services.Messages;
using App.Services.Security;
using App.Web.Framework;
using App.Web.Framework.Controllers;
using Microsoft.AspNetCore.Mvc;
using Satyanam.Nop.Plugin.SatyanamCRM.Models;
using Satyanam.Nop.Core.Settings;
using System.Threading.Tasks;

namespace Nop.Plugin.Misc.SatyanamCRM.Controllers
{
    [Area(AreaNames.Admin)]
    [AutoValidateAntiforgeryToken]
    public class SatyanamCRMController : BasePluginController
    {
        #region Fields
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;
        private readonly INotificationService _notificationService;
        private readonly ILocalizationService _localizationService;
        #endregion

        #region Ctor 
        public SatyanamCRMController(IPermissionService permissionService, ISettingService settingService, INotificationService notificationService, ILocalizationService localizationService)
        {
            _permissionService = permissionService;
            _settingService = settingService;
            _notificationService = notificationService;
            _localizationService = localizationService;
        }
        #endregion

        #region Methods
        public virtual async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();
            var settings = await _settingService.LoadSettingAsync<CampaignEmailSettings>();
            var APIsettings = await _settingService.LoadSettingAsync<SatyanamAPISettings>();

            var model = new SatyanamCRMSettingsModel
            {
                LinkedInUrl = settings.LinkedInUrl,
                WebsiteUrl = settings.WebsiteUrl,
                FooterText = settings.FooterText,
                APIKey = APIsettings.APIKey,
                APISecret = APIsettings.APISecret,
                AllowedDomains = APIsettings.AllowedDomains,
                InquiryEmailSendTo = APIsettings.InquiryEmailSendTo
            };
            return View("~/Plugins/Misc.SatyanamCRM/Views/Configure.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> Configure(SatyanamCRMSettingsModel model)
        {
            if (!ModelState.IsValid)
                return await Configure();

            var settings = await _settingService.LoadSettingAsync<CampaignEmailSettings>();
            var APIsettings = await _settingService.LoadSettingAsync<SatyanamAPISettings>();

            settings.LinkedInUrl = model.LinkedInUrl;
            settings.WebsiteUrl = model.WebsiteUrl;
            settings.FooterText = model.FooterText;
            APIsettings.APIKey = model.APIKey;
            APIsettings.APISecret = model.APISecret;
            APIsettings.AllowedDomains = model.AllowedDomains;
            APIsettings.InquiryEmailSendTo = model.InquiryEmailSendTo;

            await _settingService.SaveSettingAsync(settings);
            await _settingService.SaveSettingAsync(APIsettings);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Campaign email settings add successfully"));
            return RedirectToAction("Configure");
        }
        #endregion
    }
}
