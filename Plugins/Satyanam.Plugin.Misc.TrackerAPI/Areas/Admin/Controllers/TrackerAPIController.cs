using App.Core.Domain.Security;
using App.Services.Configuration;
using App.Services.Localization;
using App.Services.Messages;
using App.Services.Security;
using App.Web.Areas.Admin.Controllers;
using App.Web.Framework.Controllers;
using Microsoft.AspNetCore.Mvc;
using Satyanam.Plugin.Misc.TrackerAPI.Areas.Admin.Factories;
using Satyanam.Plugin.Misc.TrackerAPI.Areas.Admin.Models.Configuration;
using Satyanam.Plugin.Misc.TrackerAPI.Areas.Admin.Models.TrackerAPILog;
using Satyanam.Plugin.Misc.TrackerAPI.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Satyanam.Plugin.Misc.TrackerAPI.Areas.Admin.Controllers;

public partial class TrackerAPIController : BaseAdminController
{
	#region Properties

	protected readonly ILocalizationService _localizationService;
	protected readonly INotificationService _notificationService;
    protected readonly IPermissionService _permissionService;
	protected readonly ISettingService _settingService;
    protected readonly ITrackerAPIModelFactory _trackerAPIModelFactory;
    protected readonly ITrackerAPIService _trackerAPIService;

    #endregion

    #region Ctor

    public TrackerAPIController(ILocalizationService localizationService,
		INotificationService notificationService,
        IPermissionService permissionService,
		ISettingService settingService,
        ITrackerAPIModelFactory trackerAPIModelFactory,
        ITrackerAPIService trackerAPIService)
	{
		_permissionService = permissionService;
		_localizationService = localizationService;
		_notificationService = notificationService;
		_settingService = settingService;
        _trackerAPIModelFactory = trackerAPIModelFactory;
        _trackerAPIService = trackerAPIService;
	}

	#endregion

	#region Plugin Configuration Methods

	public virtual async Task<IActionResult> Configure()
	{
        if (!await _permissionService.AuthorizeAsync(TrackerAPIPermissionProvider.ManageTrackerAPIConfiguration, PermissionAction.View))
            return AccessDeniedView();

        var settings = await _settingService.LoadSettingAsync<TrackerAPISettings>();

        var model = new ConfigurationModel()
        {
            APIKey = settings.APIKey,
            APISecretKey = settings.APISecretKey,
            EnableKeyboardClick = settings.EnableKeyboardClick,
            EnableMouseClick = settings.EnableMouseClick,
            MinimumKeyboardMouseClick = settings.MinimumKeyboardMouseClick,
            EnableScreenShot = settings.EnableScreenShot,
            LastAwayCount = settings.LastAwayCount,
            TrackingDuration = settings.TrackingDuration,
            AlertDuration = settings.AlertDuration,
            AlertMinimumDuration = settings.AlertMinimumDuration,
            SwitchTaskDuration = settings.SwitchTaskDuration,
            EnableLogging = settings.EnableLogging,
            ClientId = settings.ClientId,
            ClientSecret = settings.ClientSecret,
            TenantId = settings.TenantId,
            UserId = settings.UserId,
            UploadTimeTrackerId = settings.UploadTimeTrackerId
        };

        return View(model);
    }

	[HttpPost]
    public virtual async Task<IActionResult> Configure(ConfigurationModel model)
    {
        if (!await _permissionService.AuthorizeAsync(TrackerAPIPermissionProvider.ManageTrackerAPIConfiguration, PermissionAction.Edit))
            return AccessDeniedView();

        var settings = new TrackerAPISettings()
        {
            APIKey = model.APIKey,
            APISecretKey = model.APISecretKey,
            EnableKeyboardClick = model.EnableKeyboardClick,
            EnableMouseClick = model.EnableMouseClick,
            MinimumKeyboardMouseClick = model.MinimumKeyboardMouseClick,
            EnableScreenShot = model.EnableScreenShot,
            LastAwayCount = model.LastAwayCount,
            TrackingDuration = model.TrackingDuration,
            AlertDuration = model.AlertDuration,
            AlertMinimumDuration = model.AlertMinimumDuration,
            SwitchTaskDuration = model.SwitchTaskDuration,
            EnableLogging = model.EnableLogging,
            ClientId = model.ClientId,
            ClientSecret = model.ClientSecret,
            TenantId = model.TenantId,
            UserId = model.UserId,
            UploadTimeTrackerId = model.UploadTimeTrackerId
        };
        await _settingService.SaveSettingAsync(settings);

        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

        return await Configure();
    }

    #endregion

    #region Tracker API Log Methods

    public virtual async Task<IActionResult> List()
    {
        if (!await _permissionService.AuthorizeAsync(TrackerAPIPermissionProvider.ManageTrackerAPILog, PermissionAction.View))
            return AccessDeniedView();

        var model = await _trackerAPIModelFactory.PrepareTrackerAPILogSearchModelAsync(new TrackerAPILogSearchModel());

        return View(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> List(TrackerAPILogSearchModel searchModel)
    {
        if (!await _permissionService.AuthorizeAsync(TrackerAPIPermissionProvider.ManageTrackerAPILog, PermissionAction.View))
            return AccessDeniedView();

        var model = await _trackerAPIModelFactory.PrepareTrackerAPILogListModelAsync(searchModel);

        return Json(model);
    }


    [HttpPost, ActionName("List")]
    [FormValueRequired("clearall")]
    public virtual async Task<IActionResult> ClearAll()
    {
        if (!await _permissionService.AuthorizeAsync(TrackerAPIPermissionProvider.ManageTrackerAPILog, PermissionAction.Delete))
            return AccessDeniedView();

        await _trackerAPIService.ClearTrackerAPILogAsync();

        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.TrackerAPI.Admin.TrackerAPILog.Cleared"));

        return RedirectToAction(nameof(List));
    }

    [HttpPost]
    public virtual async Task<IActionResult> DeleteSelected(ICollection<int> selectedIds)
    {
        if (!await _permissionService.AuthorizeAsync(TrackerAPIPermissionProvider.ManageTrackerAPILog, PermissionAction.Delete))
            return AccessDeniedView();

        if (selectedIds == null || selectedIds.Count == 0)
            return NoContent();

        await _trackerAPIService.DeleteTrackerAPILogsAsync((await _trackerAPIService.GetTrackerAPILogByIdsAsync(selectedIds.ToArray())).ToList());

        return Json(new { Result = true });
    }

    #endregion
}
