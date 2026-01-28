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
using App.Web.Areas.Admin.Models.Extension.ProcessRules;
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
    public partial class ProcessRulesController : BaseAdminController
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
        private readonly IProcessRulesModelFactory _processRulesModelFactory;
        #endregion

        #region Ctor

        public ProcessRulesController(IPermissionService permissionService,
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
            IProcessWorkflowService processWorkflowService
,
            IProcessRulesService processRulesService,
            IProcessRulesModelFactory processRulesModelFactory)
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
            _processRulesModelFactory = processRulesModelFactory;
        }

        #endregion

        #region Methods

        public virtual async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProcessRules, PermissionAction.View))
                return AccessDeniedView();
            //prepare model
            var model = await _processRulesModelFactory.PrepareProcessRulesSearchModelAsync(new ProcessRulesSearchModel());
            return View("/Areas/Admin/Views/Extension/ProcessWorkflows/_CreateOrUpdateProcessRules.cshtml", model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> List(ProcessRulesSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProcessRules, PermissionAction.View))
                return AccessDeniedView();

            var model = await _processRulesModelFactory.PrepareProcessRulesListModelAsync(searchModel);
            return Json(model);
        }

        public virtual async Task<IActionResult> Create(int processWorkflowId)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProcessRules, PermissionAction.Add))
                return AccessDeniedView();
            ProcessRulesModel processRulesModel = new ProcessRulesModel();
            processRulesModel.ProcessWorkflowId = processWorkflowId;

            var model = await _processRulesModelFactory.PrepareProcessRulesModelAsync(processRulesModel, null);
            model.IsActive = true;
            var processWorkflow = await _processWorkflowService.GetProcessWorkflowByIdAsync(processWorkflowId);
            if (processWorkflow != null)
                model.ProcessWorkflowName = processWorkflow.Name;

            ViewBag.RefreshPage = false;

            return View("/Areas/Admin/Views/Extension/ProcessWorkflows/ProcessRulesCreate.cshtml", model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Create(ProcessRulesModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProcessRules, PermissionAction.Add))
                return AccessDeniedView();

            var processRules = model.ToEntity<ProcessRules>();

            if (ModelState.IsValid)
            {
                processRules.CreatedOn = await _dateTimeHelper.GetUTCAsync();

                await _processRulesService.InsertProcessRuleAsync(processRules);

                string successMessage = await _localizationService.GetResourceAsync("Admin.Catalog.processRules.Added");
                TempData["SuccessMessage"] = successMessage;
                ViewBag.RefreshPage = true;
            }

            //prepare model
            //model = await _projectEmployeeMappingModelFactory.PrepareProjectEmployeeMappingModelAsync(model, projectEmpMapping, true);

            //if we got this far, something failed, redisplay form
            return View("/Areas/Admin/Views/Extension/ProcessWorkflows/ProcessRulesCreate.cshtml", model);
        }

        public virtual async Task<IActionResult> Edit(int processWorkflowId, int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProcessRules, PermissionAction.Edit))
                return AccessDeniedView();

            var processRules = await _processRulesService.GetProcessRuleByIdAsync(id);
            ViewBag.RefreshPage = false;
            if (processRules == null)
                return RedirectToAction("List");

            //prepare model
            var model = await _processRulesModelFactory.PrepareProcessRulesModelAsync(null, processRules);
            model.ProcessWorkflowId = processWorkflowId;
            var processWorkflow = await
                _processWorkflowService.GetProcessWorkflowByIdAsync(processWorkflowId);
            if (processWorkflow != null)
                model.ProcessWorkflowName = processWorkflow.Name;

            return View("/Areas/Admin/Views/Extension/ProcessWorkflows/ProcessRulesEdit.cshtml", model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Edit(ProcessRulesModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProcessRules, PermissionAction.Edit))
                return AccessDeniedView();

            //try to get a project with the specified id
            var processRules = await _processRulesService.GetProcessRuleByIdAsync(model.Id);
            if (processRules == null)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                processRules = model.ToEntity(processRules);

                await _processRulesService.UpdateProcessRuleAsync(processRules);

                string successMessage = await _localizationService.GetResourceAsync("Admin.Catalog.processRules.Added");
                TempData["SuccessMessage"] = successMessage;

                ViewBag.RefreshPage = true;
            }
            //if we got this far, something failed, redisplay form
            return View("/Areas/Admin/Views/Extension/ProcessWorkflows/ProcessRulesEdit.cshtml", model);
        }
        [HttpPost]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProcessRules, PermissionAction.Delete))
                return AccessDeniedView();

            //try to get a leaveType with the specified id
            var processRule = await _processRulesService.GetProcessRuleByIdAsync(id);
            if (processRule == null)
                return RedirectToAction("List");

            await _processRulesService.DeleteProcessRuleAsync(processRule);

            return new NullJsonResult();
        }

        [HttpPost]
        public virtual async Task<IActionResult> DeleteSelected(ICollection<int> selectedIds)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProcessRules, PermissionAction.Delete))
                return AccessDeniedView();

            if (selectedIds == null || selectedIds.Count == 0)
                return NoContent();

            var data = await _processRulesService.GetProcessRulesByIdsAsync(selectedIds.ToArray());

            foreach (var item in data)
                await _processRulesService.DeleteProcessRuleAsync(item);

            return Json(new { Result = true });
        }

        #endregion
    }
}