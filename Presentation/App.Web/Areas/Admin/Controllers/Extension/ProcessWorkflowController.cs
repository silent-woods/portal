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
using App.Web.Areas.Admin.Models.Extension.ProcessWorkflows;
using App.Web.Framework.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Satyanam.Nop.Core.Domains;
using Satyanam.Nop.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace App.Web.Areas.Admin.Controllers
{
    public partial class ProcessWorkflowController : BaseAdminController
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
        private readonly IProcessWorkflowModelFactory _processWorkflowModelFactory;
        private readonly IProcessWorkflowService _processWorkflowService;
        private readonly IProcessRulesService _processRulesService;
        private readonly IWorkflowStatusService _workflowStatusService;
        #endregion

        #region Ctor

        public ProcessWorkflowController(IPermissionService permissionService,
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
            ITaskChangeLogModelFactory taskChangeLogModelFactory,
            IProcessWorkflowService processWorkflowService,
            IProcessWorkflowModelFactory processWorkflowModelFactory,
            IProcessRulesService processRulesService,
            IWorkflowStatusService workflowStatusService)
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
            _processWorkflowModelFactory = processWorkflowModelFactory;
            _processWorkflowService = processWorkflowService;
            _processRulesService = processRulesService;
            _workflowStatusService = workflowStatusService;
        }

        #endregion

        #region Methods

        public virtual async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProcessWorkflow, PermissionAction.View))
                return AccessDeniedView();
            var model = await _processWorkflowModelFactory.PrepareProcessWorkflowSearchModelAsync(new ProcessWorkflowSearchModel());
            return View("/Areas/Admin/Views/Extension/ProcessWorkflows/List.cshtml", model);
        }
        [HttpPost]
        public virtual async Task<IActionResult> List(ProcessWorkflowSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProcessWorkflow, PermissionAction.View))
                return AccessDeniedView();
            var model = await _processWorkflowModelFactory.PrepareProcessWorkflowListModelAsync(searchModel);
            return Json(model);
        }

        public virtual async Task<IActionResult> Create()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProcessWorkflow, PermissionAction.Add))
                return AccessDeniedView();
            var model = await _processWorkflowModelFactory.PrepareProcessWorkflowModelAsync(new ProcessWorkflowModel(), null);

            return View("/Areas/Admin/Views/Extension/ProcessWorkflows/Create.cshtml", model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Create(ProcessWorkflowModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProcessWorkflow, PermissionAction.Add))
                return AccessDeniedView();
            var processWorkflow = model.ToEntity<ProcessWorkflow>();
            if (ModelState.IsValid)
            {
                processWorkflow.CreatedOn = await _dateTimeHelper.GetUTCAsync();
                await _processWorkflowService.InsertProcessWorkflowAsync(processWorkflow);
                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.processWorkflow.Added"));
                if (!continueEditing)
                    return RedirectToAction("List");
                return RedirectToAction("Edit", new { id = processWorkflow.Id });
            }
            model = await _processWorkflowModelFactory.PrepareProcessWorkflowModelAsync(model, processWorkflow, true);
            return View("/Areas/Admin/Views/Extension/ProcessWorkflows/Create.cshtml", model);
        }

        public virtual async Task<IActionResult> Edit(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProcessWorkflow, PermissionAction.Edit))
                return AccessDeniedView();
            var processWorkflow = await _processWorkflowService.GetProcessWorkflowByIdAsync(id);
            if (processWorkflow == null)
                return RedirectToAction("List");
            var model = await _processWorkflowModelFactory.PrepareProcessWorkflowModelAsync(null, processWorkflow);
            return View("/Areas/Admin/Views/Extension/ProcessWorkflows/Edit.cshtml", model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Edit(ProcessWorkflowModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProcessWorkflow, PermissionAction.Edit))
                return AccessDeniedView();
            var processWorkflow = await _processWorkflowService.GetProcessWorkflowByIdAsync(model.Id);        
            if (processWorkflow == null)
                return RedirectToAction("List");
            if (ModelState.IsValid)
            {
                processWorkflow = model.ToEntity(processWorkflow);
                await _processWorkflowService.UpdateProcessWorkflowAsync(processWorkflow);
                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.processWorkflow.Updated"));
                if (!continueEditing)
                    return RedirectToAction("List");
                return RedirectToAction("Edit", new { id = processWorkflow.Id });
            }
            return View("/Areas/Admin/Views/Extension/ProcessWorkflows/Edit.cshtml", model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProcessWorkflow, PermissionAction.Delete))
                return AccessDeniedView();
            var processWorkflow = await _processWorkflowService.GetProcessWorkflowByIdAsync(id);
            if (processWorkflow == null)
                return RedirectToAction("List");
            await _processWorkflowService.DeleteProcessWorkflowAsync(processWorkflow);
            _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Catalog.processWorkflow.Deleted"));
            return RedirectToAction("List");
        }

        [HttpPost]
        public virtual async Task<IActionResult> DeleteSelected(ICollection<int> selectedIds)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProcessWorkflow, PermissionAction.Delete))
                return AccessDeniedView();
            if (selectedIds == null || selectedIds.Count == 0)
                return NoContent();
            var data = await _processWorkflowService.GetProcessWorkflowsByIdsAsync(selectedIds.ToArray());
            foreach (var item in data)
            {
                await _processWorkflowService.DeleteProcessWorkflowAsync(item);
            }
            return Json(new { Result = true });
        }

        [HttpPost]
        public async Task<IActionResult> Copy(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProcessWorkflow, PermissionAction.View))
                return AccessDeniedView();

            var original = await _processWorkflowService.GetProcessWorkflowByIdAsync(id);
            if (original == null)
                return Json(new { success = false, message = "Original workflow not found." });
 
            var newWorkflow = new ProcessWorkflow
            {
                Name = original.Name + " - Copy",
                IsActive = original.IsActive,
                Description = original.Description,
                DisplayOrder = original.DisplayOrder,
                CreatedOn = DateTime.UtcNow
            };

            await _processWorkflowService.InsertProcessWorkflowAsync(newWorkflow);
            var statuses = await _workflowStatusService.GetAllWorkflowStatusAsync(original.Id);
            var statusMap = new Dictionary<int, int>(); 

            foreach (var status in statuses)
            {
                var newStatus = new WorkflowStatus
                {
                    ProcessWorkflowId = newWorkflow.Id,
                    StatusName = status.StatusName,
                    IsDefaultDeveloperStatus = status.IsDefaultDeveloperStatus,
                    IsDefaultQAStatus = status.IsDefaultQAStatus,
                    ColorCode = status.ColorCode,
                    DisplayOrder = status.DisplayOrder,
                    CreatedOn = DateTime.UtcNow
                };

                await _workflowStatusService.InsertWorkflowStatusAsync(newStatus);
                statusMap[status.Id] = newStatus.Id;
            }
          
            var rules = await _processRulesService.GetAllProcessRulesAsync(original.Id);
            foreach (var rule in rules)
            {
                if (!statusMap.ContainsKey(rule.FromStateId) || !statusMap.ContainsKey(rule.ToStateId))
                    continue;
                var newRule = new ProcessRules
                {
                    ProcessWorkflowId = newWorkflow.Id,
                    FromStateId = statusMap[rule.FromStateId],
                    ToStateId = statusMap[rule.ToStateId],
                    IsCommentRequired = rule.IsCommentRequired,
                    IsActive = rule.IsActive,
                    CreatedOn = DateTime.UtcNow
                };
                await _processRulesService.InsertProcessRuleAsync(newRule);
            }
            return Json(new { success = true, newWorkflowId = newWorkflow.Id });
        }

        #endregion
    }
}