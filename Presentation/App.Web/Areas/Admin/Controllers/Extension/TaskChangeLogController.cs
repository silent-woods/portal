using App.Core;
using App.Core.Domain.Security;
using App.Services.Designations;
using App.Services.Employees;
using App.Services.Helpers;
using App.Services.Localization;
using App.Services.Messages;
using App.Services.ProjectEmployeeMappings;
using App.Services.Projects;
using App.Services.Security;
using App.Web.Areas.Admin.Factories;
using App.Web.Areas.Admin.Models.Extension.TaskChangeLogs;
using App.Web.Framework.Mvc;
using Microsoft.AspNetCore.Mvc;
using Satyanam.Nop.Core.Services;
using System.Threading.Tasks;

namespace App.Web.Areas.Admin.Controllers
{
    public partial class TaskChangeLogController : BaseAdminController
    {
        #region Fields
        private readonly IPermissionService _permissionService;
        private readonly IProjectEmployeeMappingModelFactory _projectEmployeeMappingModelFactory;
        private readonly IProjectEmployeeMappingService _projectEmployeeMappingService;
        private readonly INotificationService _notificationService;
        private readonly ILocalizationService _localizationService;
        private readonly IWorkContext _workContext;
        private readonly IProjectsService _projectsService;
        private readonly IDesignationService _designationService;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IEmployeeService _employeeService;
        private readonly ITaskCommentsModelFactory _taskCommentsModelFactory;
        private readonly ITaskChangeLogModelFactory _taskChangeLogModelFactory;
        private readonly ITaskChangeLogService _taskChangeLogService;
        #endregion

        #region Ctor

        public TaskChangeLogController(IPermissionService permissionService,
            IProjectEmployeeMappingModelFactory projectEmployeeMappingModelFactory,
            IProjectEmployeeMappingService projectEmployeeMappingService,
            INotificationService notificationService,
            ILocalizationService localizationService,
            IWorkContext workContext,
            IProjectsService projectsService,
            IDesignationService designationService,
            IWorkflowMessageService workflowMessageService,
            IDateTimeHelper dateTimeHelper,
            IEmployeeService employeeService,
            ITaskCommentsModelFactory taskCommentsModelFactory,
            ITaskChangeLogService taskChangeLogService,
            ITaskChangeLogModelFactory taskChangeLogModelFactory
            )
        {
            _permissionService = permissionService;
            _projectEmployeeMappingModelFactory = projectEmployeeMappingModelFactory;
            _projectEmployeeMappingService = projectEmployeeMappingService;
            _notificationService = notificationService;
            _localizationService = localizationService;
            _workContext = workContext;
            _projectsService = projectsService;
            _designationService = designationService;
            _workflowMessageService = workflowMessageService;
            _dateTimeHelper = dateTimeHelper;
            _employeeService = employeeService;
            _taskCommentsModelFactory = taskCommentsModelFactory;
            _taskChangeLogModelFactory = taskChangeLogModelFactory;
            _taskChangeLogService = taskChangeLogService;
        }

        #endregion

        #region Methods

        public virtual async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProjectTaskChangeLog, PermissionAction.View))
                return AccessDeniedView();
            //prepare model
            var model = await _taskChangeLogModelFactory.PrepareTaskChangeLogSearchModelAsync(new TaskChangeLogSearchModel());
            return View("/Areas/Admin/Views/Extension/Projects/_CreateOrUpdateProjectEmpMapping.cshtml", model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> List(TaskChangeLogSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProjectTaskChangeLog, PermissionAction.View))
                return AccessDeniedView();
            
            var model = await _taskChangeLogModelFactory.PrepareTaskChangeLogListModelAsync(searchModel);

            return Json(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProjectTaskChangeLog, PermissionAction.Delete))
                return AccessDeniedView();

            var taskChangeLog = await _taskChangeLogService.GetTaskChangeLogByIdAsync(id);
            if (taskChangeLog == null)
                return RedirectToAction("List");

            await _taskChangeLogService.DeleteTaskChangeLogAsync(taskChangeLog);

            return new NullJsonResult();
        }

        #endregion
    }
}