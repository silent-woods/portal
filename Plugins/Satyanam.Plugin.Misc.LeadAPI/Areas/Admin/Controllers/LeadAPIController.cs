using App.Core.Domain.Security;
using App.Services.Configuration;
using App.Services.Localization;
using App.Services.Messages;
using App.Services.Security;
using App.Web.Framework;
using App.Web.Framework.Controllers;
using Microsoft.AspNetCore.Mvc;
using Satyanam.Nop.Core.Services;
using Satyanam.Plugin.Misc.LeadAPI.Areas.Admin.Factories;
using Satyanam.Plugin.Misc.LeadAPI.Areas.Admin.Models.Configuration;
using Satyanam.Plugin.Misc.LeadAPI.Areas.Admin.Models.LeadAPILog;
using Satyanam.Plugin.Misc.LeadAPI.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Satyanam.Plugin.Misc.LeadAPI.Areas.Admin.Controllers;
[Area(AreaNames.Admin)]
public partial class LeadAPIController : BasePluginController
{
    #region Properties

    protected readonly ILocalizationService _localizationService;
    protected readonly INotificationService _notificationService;
    protected readonly IPermissionService _permissionService;
    protected readonly ISettingService _settingService;
    protected readonly ILeadAPIModelFactory _leadAPIModelFactory;
    protected readonly ILeadAPIService _leadAPIService;

    #endregion

    #region Ctor

    public LeadAPIController(ILocalizationService localizationService,
        INotificationService notificationService,
        IPermissionService permissionService,
        ISettingService settingService,
        ILeadAPIModelFactory leadAPIModelFactory,
        ILeadAPIService leadAPIService)
    {
        _permissionService = permissionService;
        _localizationService = localizationService;
        _notificationService = notificationService;
        _settingService = settingService;
        _leadAPIModelFactory = leadAPIModelFactory;
        _leadAPIService = leadAPIService;
    }

    #endregion

    #region Plugin Configuration Methods

    public virtual async Task<IActionResult> Configure()
    {
        if (!await _permissionService.AuthorizeAsync(LeadAPIPermissionProvider.ManageLeadAPIConfiguration, PermissionAction.View))
            return AccessDeniedView();

        var settings = await _settingService.LoadSettingAsync<LeadAPISettings>();

        var model = new ConfigurationModel()
        {
            APIKey = settings.APIKey,
            APISecretKey = settings.APISecretKey,
        };

        return View(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> Configure(ConfigurationModel model)
    {
        if (!await _permissionService.AuthorizeAsync(LeadAPIPermissionProvider.ManageLeadAPIConfiguration, PermissionAction.Edit))
            return AccessDeniedView();

        var settings = new LeadAPISettings()
        {
            APIKey = model.APIKey,
            APISecretKey = model.APISecretKey,
        };
        await _settingService.SaveSettingAsync(settings);

        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

        return await Configure();
    }

    #endregion

    #region Lead API Log Methods

    public virtual async Task<IActionResult> List()
    {
        if (!await _permissionService.AuthorizeAsync(LeadAPIPermissionProvider.ManageLeadAPILog, PermissionAction.View))
            return AccessDeniedView();

        var model = await _leadAPIModelFactory.PrepareLeadAPILogSearchModelAsync(new LeadAPILogSearchModel());

        return View(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> List(LeadAPILogSearchModel searchModel)
    {
        if (!await _permissionService.AuthorizeAsync(LeadAPIPermissionProvider.ManageLeadAPILog, PermissionAction.View))
            return AccessDeniedView();

        var model = await _leadAPIModelFactory.PrepareLeadAPILogListModelAsync(searchModel);

        return Json(model);
    }


    [HttpPost, ActionName("List")]
    [FormValueRequired("clearall")]
    public virtual async Task<IActionResult> ClearAll()
    {
        if (!await _permissionService.AuthorizeAsync(LeadAPIPermissionProvider.ManageLeadAPILog, PermissionAction.Delete))
            return AccessDeniedView();

        await _leadAPIService.ClearLeadAPILogAsync();

        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.LeadAPI.Admin.LeadAPILog.Cleared"));

        return RedirectToAction(nameof(List));
    }

    [HttpPost]
    public virtual async Task<IActionResult> DeleteSelected(ICollection<int> selectedIds)
    {
        if (!await _permissionService.AuthorizeAsync(LeadAPIPermissionProvider.ManageLeadAPILog, PermissionAction.Delete))
            return AccessDeniedView();

        if (selectedIds == null || selectedIds.Count == 0)
            return NoContent();

        await _leadAPIService.DeleteLeadAPILogsAsync((await _leadAPIService.GetLeadAPILogByIdsAsync(selectedIds.ToArray())).ToList());

        return Json(new { Result = true });
    }

    #endregion
}
