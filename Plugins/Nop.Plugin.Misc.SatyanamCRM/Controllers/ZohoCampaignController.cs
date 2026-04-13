using App.Core.Domain.Security;
using App.Services.Configuration;
using App.Services.Localization;
using App.Services.Messages;
using App.Services.Security;
using App.Web.Framework;
using App.Web.Framework.Controllers;
using App.Web.Framework.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Satyanam.Nop.Core.Services;
using Satyanam.Nop.Core.Settings;
using Satyanam.Nop.Plugin.Misc.SatyanamCRM.Models.Zoho;
using System.Threading.Tasks;

namespace Satyanam.Nop.Plugin.Misc.SatyanamCRM.Controllers
{
    [Area(AreaNames.Admin)]
    [AuthorizeAdmin]
    [AutoValidateAntiforgeryToken]
    public partial class ZohoCampaignController : BasePluginController
    {
        #region Fields

        private readonly IPermissionService   _permissionService;
        private readonly ISettingService      _settingService;
        private readonly INotificationService _notificationService;
        private readonly IZohoCampaignService _zohoCampaignService;
        private readonly ILocalizationService _localizationService;

        #endregion

        #region Ctor

        public ZohoCampaignController(
            IPermissionService permissionService,
            ISettingService settingService,
            INotificationService notificationService,
            IZohoCampaignService zohoCampaignService,
            ILocalizationService localizationService)
        {
            _permissionService = permissionService;
            _settingService = settingService;
            _notificationService = notificationService;
            _zohoCampaignService = zohoCampaignService;
            _localizationService = localizationService;
        }

        #endregion

        #region Methods

        public virtual async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageCampaigns))
                return AccessDeniedView();

            var settings = await _settingService.LoadSettingAsync<ZohoCampaignSettings>();
            var model = new ZohoCampaignSettingsModel
            {
                ClientId           = settings.ClientId,
                ClientSecret       = settings.ClientSecret,
                RefreshToken       = settings.RefreshToken,
                IsEnabled          = settings.IsEnabled,
                CampaignFetchLimit = settings.CampaignFetchLimit > 0 ? settings.CampaignFetchLimit : 10,
                LastSyncedUtc      = settings.LastSyncedUtc
            };
            return View("~/Plugins/Misc.SatyanamCRM/Views/ZohoCampaign/Configure.cshtml", model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> Configure(ZohoCampaignSettingsModel model)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageCampaigns))
                return AccessDeniedView();

            var settings = await _settingService.LoadSettingAsync<ZohoCampaignSettings>();
            if (!string.IsNullOrWhiteSpace(model.ClientId))
                settings.ClientId = model.ClientId.Trim();
            if (!string.IsNullOrWhiteSpace(model.ClientSecret))
                settings.ClientSecret = model.ClientSecret.Trim();
            if (!string.IsNullOrWhiteSpace(model.RefreshToken))
                settings.RefreshToken = model.RefreshToken.Trim();
            settings.IsEnabled          = model.IsEnabled;
            settings.CampaignFetchLimit = model.CampaignFetchLimit > 0 ? model.CampaignFetchLimit : 10;
            settings.AccessToken           = null;
            settings.AccessTokenExpiresUtc = null;
            await _settingService.SaveSettingAsync(settings);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Plugins.SatyanamCRM.ZohoCampaign.SettingsSaved"));

            return RedirectToAction(nameof(Configure));
        }

        [HttpPost]
        public virtual async Task<IActionResult> SyncNow()
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageCampaigns))
                return AccessDeniedView();

            try
            {
                await _zohoCampaignService.SyncAsync();
                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Plugins.SatyanamCRM.ZohoCampaign.SyncSuccess"));
            }
            catch (System.Exception ex)
            {
                _notificationService.ErrorNotification(string.Format(
    await _localizationService.GetResourceAsync("Plugins.SatyanamCRM.ZohoCampaign.SyncFailed"),
    ex.Message
));
            }
            return RedirectToAction(nameof(Configure));
        }

        #endregion
    }
}
