using App.Services.Localization;
using App.Services.Messages;
using App.Services.Security;
using App.Web.Areas.Admin.Controllers;
using App.Web.Framework.Controllers;
using Microsoft.AspNetCore.Mvc;
using Satyanam.Plugin.Misc.AzureIntegration.Areas.Admin.Factories;
using Satyanam.Plugin.Misc.AzureIntegration.Areas.Admin.Models.AzureSyncLogs;
using Satyanam.Plugin.Misc.AzureIntegration.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Satyanam.Plugin.Misc.AzureIntegration.Areas.Admin.Controllers;

public partial class AzureIntegrationController : BaseAdminController
{
	#region Fields

    protected readonly IAzureIntegrationModelFactory _azureIntegrationModelFactory;
    protected readonly IAzureIntegrationService _azureIntegrationService;
	protected readonly ILocalizationService _localizationService;
	protected readonly INotificationService _notificationService;
    protected readonly IPermissionService _permissionService;

    #endregion

    #region Ctor

    public AzureIntegrationController(IAzureIntegrationModelFactory azureIntegrationModelFactory,
        IAzureIntegrationService azureIntegrationService,
        ILocalizationService localizationService,
		INotificationService notificationService,
        IPermissionService permissionService)
	{
        _azureIntegrationModelFactory = azureIntegrationModelFactory;
        _azureIntegrationService = azureIntegrationService;
		_permissionService = permissionService;
		_localizationService = localizationService;
		_notificationService = notificationService;
	}

	#endregion

    #region Azure Sync Log Methods

    public virtual async Task<IActionResult> List()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
            return AccessDeniedView();

        var model = await _azureIntegrationModelFactory.PrepareAzureSyncLogSearchModelAsync(new AzureSyncLogSearchModel());

        return View(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> List(AzureSyncLogSearchModel searchModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
            return AccessDeniedView();

        var model = await _azureIntegrationModelFactory.PrepareAzureSyncLogListModelAsync(searchModel);

        return Json(model);
    }

    [HttpPost, ActionName("List")]
    [FormValueRequired("clearall")]
    public virtual async Task<IActionResult> ClearAll()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
            return AccessDeniedView();

        await _azureIntegrationService.ClearAzureSyncLogAsync();

        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AzureIntegration.Admin.AzureSyncLog.Cleared"));

        return RedirectToAction(nameof(List));
    }

    [HttpPost]
    public virtual async Task<IActionResult> DeleteSelected(ICollection<int> selectedIds)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
            return AccessDeniedView();

        if (selectedIds == null || selectedIds.Count == 0)
            return NoContent();

        await _azureIntegrationService.DeleteAzureSyncLogsAsync((await _azureIntegrationService.GetAzureSyncLogByIdsAsync(selectedIds.ToArray())).ToList());

        return Json(new { Result = true });
    }

    #endregion
}
