using App.Core.Domain.Security;
using App.Core.Domain.TaskAlerts;
using App.Services.Localization;
using App.Services.Messages;
using App.Services.Security;
using App.Services.TaskAlerts;
using App.Web.Areas.Admin.Factories;
using App.Web.Areas.Admin.Models.TaskAlerts.TaskAlertConfiguration;
using App.Web.Areas.Admin.Models.TaskAlerts.TaskAlertReason;
using App.Web.Areas.Admin.Models.TaskAlerts.TaskAlertReport;
using App.Web.Framework.Mvc;
using App.Web.Framework.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace App.Web.Areas.Admin.Controllers;

public partial class TaskAlertController : BaseAdminController
{
	#region Fields

	protected readonly ILocalizationService _localizationService;
	protected readonly INotificationService _notificationService;
	protected readonly IPermissionService _permissionService;
	protected readonly ITaskAlertModelFactory _taskAlertModelFactory;
	protected readonly ITaskAlertService _taskAlertService;

	#endregion

	#region Ctor

	public TaskAlertController(ILocalizationService localizationService,
		INotificationService notificationService,
		IPermissionService permissionService,
		ITaskAlertModelFactory taskAlertModelFactory,
		ITaskAlertService taskAlertService)
	{
		_localizationService = localizationService;
		_notificationService = notificationService;
		_permissionService = permissionService;
		_taskAlertModelFactory = taskAlertModelFactory;
		_taskAlertService = taskAlertService;
	}

    #endregion

    #region Task Alert Configuration Methods

    public virtual async Task<IActionResult> TaskAlertConfigurations()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAlertConfiguration, PermissionAction.View))
            return AccessDeniedView();

        var model = await _taskAlertModelFactory.PrepareTaskAlertConfigurationSearchModelAsync(new TaskAlertConfigurationSearchModel());

        return View("/Areas/Admin/Views/Extension/TaskAlerts/TaskAlertConfigurations.cshtml", model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> TaskAlertConfigurations(TaskAlertConfigurationSearchModel searchModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAlertConfiguration, PermissionAction.View))
            return AccessDeniedView();

        var model = await _taskAlertModelFactory.PrepareTaskAlertConfigurationListModelAsync(searchModel);

        return Json(model);
    }

    public virtual async Task<IActionResult> TaskAlertConfigurationCreate()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAlertConfiguration, PermissionAction.Add))
            return AccessDeniedView();

        var model = await _taskAlertModelFactory.PrepareTaskAlertConfigurationModelAsync(new TaskAlertConfigurationModel(), null);

        return View("/Areas/Admin/Views/Extension/TaskAlerts/TaskAlertConfigurationCreate.cshtml", model);
    }

    [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
    public virtual async Task<IActionResult> TaskAlertConfigurationCreate(TaskAlertConfigurationModel model, bool continueEditing)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAlertConfiguration, PermissionAction.Add))
            return AccessDeniedView();

        if (ModelState.IsValid)
        {
            var taskAlertConfiguration = new TaskAlertConfiguration()
            {
                TaskAlertTypeId = model.TaskAlertTypeId,
                Message = model.Message,
                Percentage = model.Percentage,
                EnableComment = model.EnableComment,
                CommentRequired = model.CommentRequired,
                EnableReason = model.EnableReason,
                ReasonRequired = model.ReasonRequired,
                EnableCoordinatorMail = model.EnableCoordinatorMail,
                EnableLeaderMail = model.EnableLeaderMail,
                EnableManagerMail = model.EnableManagerMail,
                EnableDeveloperMail = model.EnableDeveloperMail,
                NewETA = model.NewETA,
                EnableOnTrack = model.EnableOnTrack,
                IsActive = model.IsActive,
                DisplayOrder = model.DisplayOrder,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow
            };
            await _taskAlertService.InsertTaskAlertConfigurationAsync(taskAlertConfiguration);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.TaskAlert.TaskAlertConfiguration.Added"));

            if (!continueEditing)
                return RedirectToAction(nameof(TaskAlertConfigurations));

            return RedirectToAction(nameof(TaskAlertConfigurationEdit), new { id = taskAlertConfiguration.Id });
        }

        model = await _taskAlertModelFactory.PrepareTaskAlertConfigurationModelAsync(model, null);

        return View("/Areas/Admin/Views/Extension/TaskAlerts/TaskAlertConfigurationCreate.cshtml", model);
    }

    public virtual async Task<IActionResult> TaskAlertConfigurationEdit(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAlertConfiguration, PermissionAction.Edit))
            return AccessDeniedView();

        var existingTaskAlertConfiguration = await _taskAlertService.GetTaskAlertConfigurationByIdAsync(id);
        if (existingTaskAlertConfiguration == null)
            return RedirectToAction(nameof(TaskAlertConfigurations));

        var model = await _taskAlertModelFactory.PrepareTaskAlertConfigurationModelAsync(null, existingTaskAlertConfiguration);

        return View("/Areas/Admin/Views/Extension/TaskAlerts/TaskAlertConfigurationEdit.cshtml", model);
    }

    [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
    public virtual async Task<IActionResult> TaskAlertConfigurationEdit(TaskAlertConfigurationModel model, bool continueEditing)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAlertConfiguration, PermissionAction.Edit))
            return AccessDeniedView();

        var existingTaskAlertConfiguration = await _taskAlertService.GetTaskAlertConfigurationByIdAsync(model.Id);
        if (existingTaskAlertConfiguration == null)
            return RedirectToAction(nameof(TaskAlertConfigurations));

        if (ModelState.IsValid)
        {
            existingTaskAlertConfiguration.TaskAlertTypeId = model.TaskAlertTypeId;
            existingTaskAlertConfiguration.Message = model.Message;
            existingTaskAlertConfiguration.Percentage = model.Percentage;
            existingTaskAlertConfiguration.EnableComment = model.EnableComment;
            existingTaskAlertConfiguration.CommentRequired = model.CommentRequired;
            existingTaskAlertConfiguration.EnableReason = model.EnableReason;
            existingTaskAlertConfiguration.ReasonRequired = model.ReasonRequired;
            existingTaskAlertConfiguration.EnableCoordinatorMail = model.EnableCoordinatorMail;
            existingTaskAlertConfiguration.EnableLeaderMail = model.EnableLeaderMail;
            existingTaskAlertConfiguration.EnableManagerMail = model.EnableManagerMail;
            existingTaskAlertConfiguration.EnableDeveloperMail = model.EnableDeveloperMail;
            existingTaskAlertConfiguration.NewETA = model.NewETA;
            existingTaskAlertConfiguration.EnableOnTrack = model.EnableOnTrack;
            existingTaskAlertConfiguration.IsActive = model.IsActive;
            existingTaskAlertConfiguration.DisplayOrder = model.DisplayOrder;
            existingTaskAlertConfiguration.UpdatedOnUtc = DateTime.UtcNow;
            await _taskAlertService.UpdateTaskAlertConfigurationAsync(existingTaskAlertConfiguration);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.TaskAlert.TaskAlertConfiguration.Updated"));

            if (!continueEditing)
                return RedirectToAction(nameof(TaskAlertConfigurations));

            return RedirectToAction(nameof(TaskAlertConfigurationEdit), new { id = existingTaskAlertConfiguration.Id });
        }

        model = await _taskAlertModelFactory.PrepareTaskAlertConfigurationModelAsync(model, existingTaskAlertConfiguration);

        return View("/Areas/Admin/Views/Extension/TaskAlerts/TaskAlertConfigurationEdit.cshtml", model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> TaskAlertConfigurationDelete(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAlertConfiguration, PermissionAction.Delete))
            return AccessDeniedView();

        var existingTaskAlertConfiguration = await _taskAlertService.GetTaskAlertConfigurationByIdAsync(id);
        if (existingTaskAlertConfiguration == null)
            return RedirectToAction(nameof(TaskAlertConfigurations));

        await _taskAlertService.DeleteTaskAlertConfigurationAsync(existingTaskAlertConfiguration);

        return new NullJsonResult();
    }

    #endregion

    #region Task Alert Reasons Methods

    public virtual async Task<IActionResult> TaskAlertReasons()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAlertReason, PermissionAction.View))
            return AccessDeniedView();

        var model = await _taskAlertModelFactory.PrepareTaskAlertReasonSearchModelAsync(new TaskAlertReasonSearchModel());

        return View("/Areas/Admin/Views/Extension/TaskAlerts/TaskAlertReasons.cshtml", model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> TaskAlertReasons(TaskAlertReasonSearchModel searchModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAlertReason, PermissionAction.View))
            return AccessDeniedView();

        var model = await _taskAlertModelFactory.PrepareTaskAlertReasonListModelAsync(searchModel);

        return Json(model);
    }

    public virtual async Task<IActionResult> TaskAlertReasonCreate()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAlertReason, PermissionAction.Add))
            return AccessDeniedView();

        var model = await _taskAlertModelFactory.PrepareTaskAlertReasonModelAsync(new TaskAlertReasonModel(), null);

        return View("/Areas/Admin/Views/Extension/TaskAlerts/TaskAlertReasonCreate.cshtml", model);
    }

    [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
    public virtual async Task<IActionResult> TaskAlertReasonCreate(TaskAlertReasonModel model, bool continueEditing)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAlertReason, PermissionAction.Add))
            return AccessDeniedView();

        if (ModelState.IsValid)
        {
            var taskAlertReason = new TaskAlertReason()
            {
                Name = model.Name,
                IsActive = model.IsActive,
                DisplayOrder = model.DisplayOrder,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow
            };
            await _taskAlertService.InsertTaskAlertReasonAsync(taskAlertReason);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.TaskAlert.TaskAlertReason.Added"));

            if (!continueEditing)
                return RedirectToAction(nameof(TaskAlertReasons));

            return RedirectToAction(nameof(TaskAlertReasonEdit), new { id = taskAlertReason.Id });
        }

        model = await _taskAlertModelFactory.PrepareTaskAlertReasonModelAsync(model, null);

        return View("/Areas/Admin/Views/Extension/TaskAlerts/TaskAlertReasonCreate.cshtml", model);
    }

    public virtual async Task<IActionResult> TaskAlertReasonEdit(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAlertReason, PermissionAction.Edit))
            return AccessDeniedView();

        var existingTaskAlertReason = await _taskAlertService.GetTaskAlertReasonByIdAsync(id);
        if (existingTaskAlertReason == null)
            return RedirectToAction(nameof(TaskAlertReasons));

        var model = await _taskAlertModelFactory.PrepareTaskAlertReasonModelAsync(null, existingTaskAlertReason);

        return View("/Areas/Admin/Views/Extension/TaskAlerts/TaskAlertReasonEdit.cshtml", model);
    }

    [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
    public virtual async Task<IActionResult> TaskAlertReasonEdit(TaskAlertReasonModel model, bool continueEditing)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAlertReason, PermissionAction.Edit))
            return AccessDeniedView();

        var existingTaskAlertReason = await _taskAlertService.GetTaskAlertReasonByIdAsync(model.Id);
        if (existingTaskAlertReason == null)
            return RedirectToAction(nameof(TaskAlertReasons));

        if (ModelState.IsValid)
        {
            existingTaskAlertReason.Name = model.Name;
            existingTaskAlertReason.IsActive = model.IsActive;
            existingTaskAlertReason.DisplayOrder = model.DisplayOrder;
            existingTaskAlertReason.UpdatedOnUtc = DateTime.UtcNow;
            await _taskAlertService.UpdateTaskAlertReasonAsync(existingTaskAlertReason);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.TaskAlert.TaskAlertReason.Updated"));

            if (!continueEditing)
                return RedirectToAction(nameof(TaskAlertReasons));

            return RedirectToAction(nameof(TaskAlertReasonEdit), new { id = existingTaskAlertReason.Id });
        }

        model = await _taskAlertModelFactory.PrepareTaskAlertReasonModelAsync(model, existingTaskAlertReason);

        return View("/Areas/Admin/Views/Extension/TaskAlerts/TaskAlertReasonEdit.cshtml", model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> TaskAlertReasonDelete(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAlertReason, PermissionAction.Delete))
            return AccessDeniedView();

        var existingTaskAlertReason = await _taskAlertService.GetTaskAlertReasonByIdAsync(id);
        if (existingTaskAlertReason == null)
            return RedirectToAction(nameof(TaskAlertReasons));

        await _taskAlertService.DeleteTaskAlertReasonAsync(existingTaskAlertReason);

        return new NullJsonResult();
    }

    #endregion

    #region Task Alert Report Methods

    public virtual async Task<IActionResult> TaskAlertReports()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAlertReason, PermissionAction.View))
            return AccessDeniedView();

        var model = await _taskAlertModelFactory.PrepareTaskAlertReportSearchModelAsync(new TaskAlertReportSearchModel());

        return View("/Areas/Admin/Views/Extension/TaskAlerts/TaskAlertReports.cshtml", model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> TaskAlertReports(TaskAlertReportSearchModel searchModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAlertReason, PermissionAction.View))
            return AccessDeniedView();

        var model = await _taskAlertModelFactory.PrepareTaskAlertReportListModelAsync(searchModel);

        return Json(model);
    }

    public virtual async Task<IActionResult> ViewTaskAlertLogDetails(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAlertReason, PermissionAction.View))
            return AccessDeniedView();

        var existingTaskAlertLog = await _taskAlertService.GetTaskAlertLogByIdAsync(id);
        if (existingTaskAlertLog == null)
            return RedirectToAction(nameof(TaskAlertReports));

        var model = await _taskAlertModelFactory.PrepareTaskAlertReportModelAsync(null, existingTaskAlertLog);

        return View("/Areas/Admin/Views/Extension/TaskAlerts/ViewTaskAlertLogDetails.cshtml", model);
    }

    public virtual async Task<IActionResult> SelectedTaskAlertValues(int taskAlertTypeId)
    {
        var availableTaskAlertConfigurations = await _taskAlertService.GetAllTaskAlertConfigurationsAsync(taskAlertTypeId: taskAlertTypeId);
        var taskAlertConfigurations = availableTaskAlertConfigurations.Select(availableTaskAlertConfiguration => new {
            id = availableTaskAlertConfiguration.Id,
            name = availableTaskAlertConfiguration.Percentage
        });

        return Json(taskAlertConfigurations);
    }

    #endregion
}
