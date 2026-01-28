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
using App.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using App.Web.Areas.Admin.Models.Extension.WorkflowStatus;
using App.Web.Framework.Mvc;
using App.Web.Framework.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Satyanam.Nop.Core.Domains;
using Satyanam.Nop.Core.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace App.Web.Areas.Admin.Controllers
{
    public partial class WorkflowStatusController : BaseAdminController
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
        private readonly IWorkflowStatusService _workflowStatusService;
        private readonly IWorkflowStatusModelFactory _workflowStatusModelFactory;
        private readonly IProcessWorkflowService _processWorkflowService;
        private readonly IProcessRulesService _processRulesService;
        #endregion

        #region Ctor

        public WorkflowStatusController(IPermissionService permissionService,
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
            IWorkflowStatusService workflowStatusService,
            IWorkflowStatusModelFactory workflowStatusModelFactory,
            IProcessWorkflowService processWorkflowService,
            IProcessRulesService processRulesService
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
            _workflowStatusService = workflowStatusService;
            _workflowStatusModelFactory = workflowStatusModelFactory;
            _processWorkflowService = processWorkflowService;
            _processRulesService = processRulesService;
        }

        #endregion

        #region Methods

        public virtual async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageWorkflowStatus, PermissionAction.View))
                return AccessDeniedView();

            //prepare model
            var model = await _workflowStatusModelFactory.PrepareWorkflowStatusSearchModelAsync(new WorkflowStatusSearchModel());

            return View("/Areas/Admin/Views/Extension/ProcessWorkflows/_CreateOrUpdateWorkflowStatus.cshtml", model);
        }
        [HttpPost]
        public virtual async Task<IActionResult> List(WorkflowStatusSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageWorkflowStatus, PermissionAction.View))
                return AccessDeniedView();

            //prepare model
            var model = await _workflowStatusModelFactory.PrepareWorkflowStatusListModelAsync(searchModel);

            return Json(model);
        }
        public virtual async Task<IActionResult> Create(int processWorkflowId)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageWorkflowStatus, PermissionAction.Add))
                return AccessDeniedView();

            //prepare model
            var model = await _workflowStatusModelFactory.PrepareWorkflowStatusModelAsync(new WorkflowStatusModel(), null);
            model.ProcessWorkflowId = processWorkflowId;
            var processWorkflow = await _processWorkflowService.GetProcessWorkflowByIdAsync(processWorkflowId);
            if (processWorkflow != null)
                model.ProcessWorkflowName = processWorkflow.Name;

            ViewBag.RefreshPage = false;

            return View("/Areas/Admin/Views/Extension/ProcessWorkflows/WorkflowStatusCreate.cshtml", model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Create(WorkflowStatusModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageWorkflowStatus, PermissionAction.Add))
                return AccessDeniedView();

            var workflowStatus = model.ToEntity<WorkflowStatus>();

            if (ModelState.IsValid)
            {
                workflowStatus.CreatedOn = await _dateTimeHelper.GetUTCAsync();

                await _workflowStatusService.InsertWorkflowStatusAsync(workflowStatus);

                string successMessage = await _localizationService.GetResourceAsync("Admin.Catalog.WorkflowStatus.Added");
                TempData["SuccessMessage"] = successMessage;
                ViewBag.RefreshPage = true;
            }

            //prepare model
            //model = await _projectEmployeeMappingModelFactory.PrepareProjectEmployeeMappingModelAsync(model, projectEmpMapping, true);

            //if we got this far, something failed, redisplay form
            return View("/Areas/Admin/Views/Extension/ProcessWorkflows/WorkflowStatusCreate.cshtml", model);
        }

        public virtual async Task<IActionResult> Edit(int processWorkflowId, int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageWorkflowStatus, PermissionAction.Edit))
                return AccessDeniedView();

            var workflowStatus = await _workflowStatusService.GetWorkflowStatusByIdAsync(id);
            ViewBag.RefreshPage = false;
            if (workflowStatus == null)
                return RedirectToAction("List");

            //prepare model
            var model = await _workflowStatusModelFactory.PrepareWorkflowStatusModelAsync(null, workflowStatus);
            model.ProcessWorkflowId = processWorkflowId;
            var processWorkflow = await
                _processWorkflowService.GetProcessWorkflowByIdAsync(processWorkflowId);
            if (processWorkflow != null)
                model.ProcessWorkflowName = processWorkflow.Name;

            return View("/Areas/Admin/Views/Extension/ProcessWorkflows/WorkflowStatusEdit.cshtml", model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Edit(WorkflowStatusModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageWorkflowStatus, PermissionAction.Edit))
                return AccessDeniedView();

            //try to get a project with the specified id
            var workflowStatus = await _workflowStatusService.GetWorkflowStatusByIdAsync(model.Id);
            if (workflowStatus == null)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                workflowStatus = model.ToEntity(workflowStatus);

                await _workflowStatusService.UpdateWorkflowStatusAsync(workflowStatus);

                string successMessage = await _localizationService.GetResourceAsync("Admin.Catalog.workflowStatus.Added");
                TempData["SuccessMessage"] = successMessage;

                ViewBag.RefreshPage = true;
            }
            //if we got this far, something failed, redisplay form
            return View("/Areas/Admin/Views/Extension/ProcessWorkflows/WorkflowStatusEdit.cshtml", model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageWorkflowStatus, PermissionAction.Delete))
                return AccessDeniedView();

            //try to get a leaveType with the specified id
            var workflowStatus = await _workflowStatusService.GetWorkflowStatusByIdAsync(id);
            if (workflowStatus == null)
                return RedirectToAction("List");

            await _workflowStatusService.DeleteWorkflowStatusAsync(workflowStatus);

            return new NullJsonResult();
        }
        [HttpPost]
        public virtual async Task<IActionResult> DeleteSelected(ICollection<int> selectedIds)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageWorkflowStatus, PermissionAction.Delete))
                return AccessDeniedView();

            if (selectedIds == null || selectedIds.Count == 0)
                return NoContent();

            var data = await _workflowStatusService.GetWorkflowStatusByIdsAsync(selectedIds.ToArray());

            foreach (var item in data)
                await _workflowStatusService.DeleteWorkflowStatusAsync(item);

            return Json(new { Result = true });
        }

        [HttpGet]
        public virtual async Task<IActionResult> GetEmployeeDesignation(int employeeId)
        {
            // Fetch the employee designation based on employeeId
            var employee = await _employeeService.GetEmployeeByIdAsync(employeeId);
            if (employee == null)
                return Json(new { success = false, message = "Employee not found" });

            // Return the designation information
            return Json(new { success = true, roleId = employee.DesignationId });
        }

        #endregion
    }
}