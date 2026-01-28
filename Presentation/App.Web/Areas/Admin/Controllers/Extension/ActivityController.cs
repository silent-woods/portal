using App.Core;
using App.Core.Domain.Activities;
using App.Core.Domain.Extension.TimeSheets;
using App.Core.Domain.Security;
using App.Services.Activities;
using App.Services.Employees;
using App.Services.Helpers;
using App.Services.Leaves;
using App.Services.Localization;
using App.Services.Messages;
using App.Services.ProjectTasks;
using App.Services.Security;
using App.Services.TimeSheets;
using App.Web.Areas.Admin.Factories;
using App.Web.Areas.Admin.Factories.Extension;
using App.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using App.Web.Areas.Admin.Models.Extension.Activities;
using App.Web.Framework.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace App.Web.Areas.Admin.Controllers
{
    public partial class ActivityController : BaseAdminController
    {
        #region Fields
        private readonly IPermissionService _permissionService;
        private readonly IProjectTaskModelFactory _projectTaskModelFactory;
        private readonly ILeaveManagementService _leaveManagementService;
        private readonly INotificationService _notificationService;
        private readonly ILocalizationService _localizationService;
        private readonly IWorkContext _workContext;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly IEmployeeService _employeeService;
        private readonly IProjectTaskService _projectTaskService;
        private readonly MonthlyReportSetting _monthlyReportSettings;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IActivityModelFactory _activityModelFactory;
        private readonly IActivityService _activityService;
        private readonly ITimeSheetsService _timeSheetsService;

        #endregion

        #region Ctor

        public ActivityController(IPermissionService permissionService,
            IProjectTaskModelFactory projectTaskModelFactory,
            ILeaveManagementService leaveManagementService,
            INotificationService notificationService,
            ILocalizationService localizationService,
            IWorkContext workContext,
            IWorkflowMessageService workflowMessageService,
            IEmployeeService employeeService,
            IProjectTaskService projectTaskService,
            MonthlyReportSetting monthlyReportSettings,
            IDateTimeHelper dateTimeHelper,
            IActivityModelFactory activityModelFactory,
            IActivityService activityService ,
            ITimeSheetsService timeSheetsService
            )
        {
            _permissionService = permissionService;
            _projectTaskModelFactory = projectTaskModelFactory;
            _leaveManagementService = leaveManagementService;
            _notificationService = notificationService;
            _localizationService = localizationService;
            _workContext = workContext;
            _employeeService = employeeService;
            _workflowMessageService = workflowMessageService;
            _projectTaskService = projectTaskService;
            _monthlyReportSettings = monthlyReportSettings;
            _dateTimeHelper = dateTimeHelper;
            _activityModelFactory = activityModelFactory;
            _activityService = activityService;
            _timeSheetsService = timeSheetsService;
        }

        #endregion

        #region Methods

        public virtual async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageActivity, PermissionAction.View))
                return AccessDeniedView();
            //prepare model
            var model = await _activityModelFactory.PrepareActivitySearchModelAsync(new ActivitySearchModel());

            return View("/Areas/Admin/Views/Extension/Activities/List.cshtml", model);
        }
        [HttpPost]
        public virtual async Task<IActionResult> List(ActivitySearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageActivity, PermissionAction.View))
                return AccessDeniedView();

            //prepare model
            var model = await _activityModelFactory.PrepareActivityListModelAsync(searchModel);
            return Json(model);
        }

        public virtual async Task<IActionResult> Create()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageActivity, PermissionAction.Add))
                return AccessDeniedView();

            //prepare model
            var model = await _activityModelFactory.PrepareActivityModelAsync(new ActivityModel(), null);

            return View("/Areas/Admin/Views/Extension/Activities/Create.cshtml", model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Create(ActivityModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageActivity, PermissionAction.Add))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var activity = model.ToEntity<Activity>();


                activity.CreateOnUtc = await _dateTimeHelper.GetUTCAsync();
                activity.UpdateOnUtc = await _dateTimeHelper.GetUTCAsync();
                (activity.SpentHours, activity.SpentMinutes) = await _timeSheetsService.ConvertSpentTimeAsync(model.SpentTime);
                activity.ActivityName = model.ActivityName;


                await _activityService.InsertActivityWithTaskUpdateAsync(activity);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Activity.Added"));

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = activity.Id });
            }
            //prepare model
            model = await _activityModelFactory.PrepareActivityModelAsync(model, null, true);

            //if we got this far, something failed, redisplay form

            return View("/Areas/Admin/Views/Extension/Activities/Create.cshtml", model);
        }

        public virtual async Task<IActionResult> Edit(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageActivity, PermissionAction.Edit))
                return AccessDeniedView();

            var activity = await _activityService.GetActivityByIdAsync(id);
            if (activity == null)
                return RedirectToAction("List");

            //prepare model
            var model = await _activityModelFactory.PrepareActivityModelAsync(null, activity);

            return View("/Areas/Admin/Views/Extension/Activities/Edit.cshtml", model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Edit(ActivityModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageActivity, PermissionAction.Edit))
                return AccessDeniedView();

            //try to get a leaveManagement with the specified id
            var activity = await _activityService.GetActivityByIdAsync(model.Id);

            var prevActivity = new Activity
            {
                Id = activity.Id,
                ActivityName = activity.ActivityName,
                TaskId = activity.TaskId,
                SpentHours = activity.SpentHours,
                EmployeeId = activity.EmployeeId,
            };

            if (activity == null)
                return RedirectToAction("List");



            if (ModelState.IsValid)
            {

                activity = model.ToEntity(activity);
                activity.ActivityName = model.ActivityName;

                (activity.SpentHours, activity.SpentMinutes) = await _timeSheetsService.ConvertSpentTimeAsync(model.SpentTime);

                await _activityService.UpdateActivityWithTaskUpdateAsync(activity, prevActivity);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.activity.Updated"));

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = activity.Id });
            }
            //prepare model
            model = await _activityModelFactory.PrepareActivityModelAsync(model, null, true);
            //if we got this far, something failed, redisplay form
            return View("/Areas/Admin/Views/Extension/Activities/Edit.cshtml", model);
        }
        [HttpPost]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageActivity, PermissionAction.Delete))
                return AccessDeniedView();

            //try to get a leaveType with the specified id
            var activity = await _activityService.GetActivityByIdAsync(id);
            if (activity == null)
                return RedirectToAction("List");

            await _activityService.DeleteActivityWithTaskUpdateAsync(activity);

            _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Catalog.activity.Deleted"));

            return RedirectToAction("List");
        }
        [HttpPost]
        public virtual async Task<IActionResult> DeleteSelected(ICollection<int> selectedIds)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageActivity, PermissionAction.Delete))
                return AccessDeniedView();

            if (selectedIds == null || selectedIds.Count == 0)
                return NoContent();

            var data = await _activityService.GetActivitiesByIdsAsync(selectedIds.ToArray());

            foreach (var item in data)
            {
                await _activityService.DeleteActivityWithTaskUpdateAsync(item);
            }
            return Json(new { Result = true });
        }

        public virtual async Task<IActionResult> GetTasksByProject(int projectId, int? selectedTaskId = null)
        {
            if (projectId == 0)
                return Json(new List<object>());

            var tasks = await _projectTaskService.GetProjectTasksByProjectIdForTimeSheet(projectId);

            var taskList = tasks.Select(task => new
            {
                Value = task.Id.ToString(),
                Text = task.TaskTitle,
                Selected = task.Id == selectedTaskId
            }).ToList();

            return Json(taskList);
        }

        #endregion
    }
}