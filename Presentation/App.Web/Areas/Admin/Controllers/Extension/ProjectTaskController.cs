using App.Core;
using App.Core.Domain.Blogs;
using App.Core.Domain.Extension.TimeSheets;
using App.Core.Domain.Leaves;
using App.Core.Domain.ProjectTasks;
using App.Core.Domain.Security;
using App.Core.Domain.TimeSheets;
using App.Data;
using App.Services.Customers;
using App.Services.Employees;
using App.Services.Helpers;
using App.Services.Leaves;
using App.Services.Localization;
using App.Services.Messages;
using App.Services.Projects;
using App.Services.ProjectTasks;
using App.Services.Security;
using App.Services.TimeSheets;
using App.Web.Areas.Admin.Factories;
using App.Web.Areas.Admin.Factories.Extension;
using App.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using App.Web.Areas.Admin.Models.Extension.ProjectTasks;
using App.Web.Areas.Admin.Models.LeaveManagement;
using App.Web.Framework.Mvc.Filters;
using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.Office2013.Drawing.ChartStyle;
using DocumentFormat.OpenXml.Wordprocessing;
using MaxMind.GeoIP2.Responses;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Satyanam.Nop.Core.Domains;
using Satyanam.Nop.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Threading.Tasks;

namespace App.Web.Areas.Admin.Controllers
{
    public partial class ProjectTaskController : BaseAdminController
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
        private readonly ITimeSheetsService _timeSheetsService;
        private readonly IProjectsService _projectsService;
        private readonly ICustomerService _customerService;
        private readonly ITaskCommentsService _taskCommentsService;
        private readonly ITaskChangeLogService _taskChangeLogService;
        private readonly IProcessWorkflowService _processWorkflowService;
        private readonly IWorkflowStatusService _workflowStatusService;
        private readonly IProcessRulesService _processRulesService;
        private readonly ICommonPluginService _commonPluginService;
        private readonly IProjectTaskCategoryMappingService _projectTaskCategoryMappingService;
        private readonly ITaskCategoryService _taskCategoryService;
        private readonly IFollowUpTaskService _followUpTaskService;
        #endregion

        #region Ctor

        public ProjectTaskController(IPermissionService permissionService,
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
            ITimeSheetsService timeSheetsService,
            IProjectsService projectsService,
            ICustomerService customerService,
            ITaskCommentsService taskCommentsService,
            ITaskChangeLogService taskChangeLogService
,
            ITaskCommentsModelFactory taskCommentsModelFactory,
            IProcessWorkflowService processWorkflowService,
            IWorkflowStatusService workflowStatusService,
            IProcessRulesService processRulesService,
            ICommonPluginService commonPluginService,
            IProjectTaskCategoryMappingService projectTaskCategoryMappingService,
            ITaskCategoryService taskCategoryService,
            IFollowUpTaskService followUpTaskService)
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
            _timeSheetsService = timeSheetsService;
            _projectsService = projectsService;
            _customerService = customerService;
            _taskCommentsService = taskCommentsService;
            _taskChangeLogService = taskChangeLogService;
            _processWorkflowService = processWorkflowService;
            _workflowStatusService = workflowStatusService;
            _processRulesService = processRulesService;
            _commonPluginService = commonPluginService;
            _projectTaskCategoryMappingService = projectTaskCategoryMappingService;
            _taskCategoryService = taskCategoryService;
            _followUpTaskService = followUpTaskService;
        }

        #endregion
        public virtual async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProjectTask, PermissionAction.View))
                return AccessDeniedView();

            var model = await _projectTaskModelFactory.PrepareProjectTaskSearchModelAsync(new ProjectTaskSearchModel());
            return View("/Areas/Admin/Views/Extension/ProjectTasks/List.cshtml", model);
        }
        [HttpPost]
        public virtual async Task<IActionResult> List(ProjectTaskSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProjectTask, PermissionAction.View))
                return AccessDeniedView();

            var model = await _projectTaskModelFactory.PrepareProjectTaskListModelAsync(searchModel);
            return Json(model);
        }

        public virtual async Task<IActionResult> Create()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProjectTask, PermissionAction.Add))
                return AccessDeniedView();

            var model = await _projectTaskModelFactory.PrepareProjectTaskModelAsync(new ProjectTaskModel(), null);
            model.IsSync = true;
            model.SpentTime = "00:00";

            return View("/Areas/Admin/Views/Extension/ProjectTasks/Create.cshtml", model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Create(ProjectTaskModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProjectTask, PermissionAction.Add))
                return AccessDeniedView();

            int selectedEmployeeId = model.SelectedEmployeeId.FirstOrDefault();
            model.AssignedTo = selectedEmployeeId;

            if (!string.IsNullOrWhiteSpace(model.EstimationTimeHHMM) &&
                System.Text.RegularExpressions.Regex.IsMatch(model.EstimationTimeHHMM, @"^([0-9]{1,2}):([0-5][0-9])$"))
            {
                model.EstimatedTime = await _timeSheetsService.ConvertHHMMToDecimal(model.EstimationTimeHHMM);
            }
            else
            {            
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("admin.catalog.estimationTime.error.invalidhhmm"));

                model = await _projectTaskModelFactory.PrepareProjectTaskModelAsync(model, null, true);
                return View("/Areas/Admin/Views/Extension/ProjectTasks/Create.cshtml", model);
            }

            if (string.IsNullOrWhiteSpace(model.SpentTime) ||
                 !System.Text.RegularExpressions.Regex.IsMatch(model.SpentTime, @"^([0-9]{1,2}):([0-5][0-9])$"))
            {
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("admin.catalog.spentTime.error.invalidhhmm"));

                model = await _projectTaskModelFactory.PrepareProjectTaskModelAsync(model, null, true);
                return View("/Areas/Admin/Views/Extension/ProjectTasks/Create.cshtml", model);
            }

            if (model.ParentTaskId == 0 && (model.Tasktypeid == 3 || model.Tasktypeid == 4))
            {
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("admin.catalog.parenttask.error.required"));

                model = await _projectTaskModelFactory.PrepareProjectTaskModelAsync(model, null, true);
                return View("/Areas/Admin/Views/Extension/ProjectTasks/Create.cshtml", model);
            }

            if (ModelState.IsValid)
            {

                var existingTask = await _projectTaskService.GetProjectTaskByTitleAndProjectAsync(model.TaskTitle, model.ProjectId);
                if (existingTask != null)
                {
                    _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("admin.catalog.ProjectTask.error.TaskAlreadyExist"));
                    model = await _projectTaskModelFactory.PrepareProjectTaskModelAsync(model, null, true);

                    return View("/Areas/Admin/Views/Extension/ProjectTasks/Create.cshtml", model);
                }

                var projectTask = model.ToEntity<ProjectTask>();
                projectTask.DeveloperId = model.AssignedTo;
                projectTask.CreatedOnUtc = await _dateTimeHelper.GetUTCAsync();
                (projectTask.SpentHours, projectTask.SpentMinutes) = await _timeSheetsService.ConvertSpentTimeAsync(model.SpentTime);


                await _projectTaskService.InsertProjectTaskAsync(projectTask);

                projectTask.DeliveryOnTime = await _timeSheetsService.IsTaskDeliveredOnTimeAsync(projectTask);

                await _projectTaskService.UpdateProjectTaskAsync(projectTask);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.ProjectTask.Added"));

          
                    var customer = await _workContext.GetCurrentCustomerAsync();
                    int currEmployeeId = 0;

                    if (!await _customerService.IsRegisteredAsync(customer))
                        return Challenge();
                    if (customer != null)
                    {
                        var currEmployee = await _employeeService.GetEmployeeByCustomerIdAsync(customer.Id);
                        if (currEmployee != null)
                            currEmployeeId = currEmployee.Id;
                    }

                if (!string.IsNullOrWhiteSpace(model.StatusChangeComment))
                {
                    TaskComments taskComment = new TaskComments();

                    taskComment.EmployeeId = currEmployeeId;
                    taskComment.StatusId = projectTask.StatusId;
                    taskComment.Description = model.StatusChangeComment;
                    taskComment.TaskId = projectTask.Id;
                    taskComment.CreatedOn = await _dateTimeHelper.GetUTCAsync();

                    await _taskCommentsService.InsertTaskCommentsAsync(taskComment);
                    await _workflowMessageService.SendEmployeeMentionMessageAsync((await _workContext.GetWorkingLanguageAsync()).Id, taskComment.EmployeeId, taskComment.TaskId, taskComment.Description);
                }
                
                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = projectTask.Id });
            }
            model = await _projectTaskModelFactory.PrepareProjectTaskModelAsync(model, null, true);

            return View("/Areas/Admin/Views/Extension/ProjectTasks/Create.cshtml", model);
        }

        public virtual async Task<IActionResult> Edit(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProjectTask, PermissionAction.Edit))
                return AccessDeniedView();

            var projectTask = await _projectTaskService.GetProjectTasksWithoutCacheByIdAsync(id);
            if (projectTask == null)
                return RedirectToAction("List");

            var model = await _projectTaskModelFactory.PrepareProjectTaskModelAsync(null, projectTask);

            return View("/Areas/Admin/Views/Extension/ProjectTasks/Edit.cshtml", model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Edit(ProjectTaskModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProjectTask, PermissionAction.Edit))
                return AccessDeniedView();

            var projectTask = await _projectTaskService.GetProjectTasksWithoutCacheByIdAsync(model.Id);
            ProjectTask prevProjectTask = new ProjectTask()
            {
                StatusId = projectTask.StatusId,
                AssignedTo = projectTask.AssignedTo,
                DueDate = projectTask.DueDate,
                EstimatedTime = projectTask.EstimatedTime,
                ProcessWorkflowId = projectTask.ProcessWorkflowId,
                Tasktypeid = projectTask.Tasktypeid,
                TaskTitle = projectTask.TaskTitle,
                ParentTaskId = projectTask.ParentTaskId,
                IsSync = projectTask.IsSync,
                TaskCategoryId=projectTask.TaskCategoryId
            };

            int selectedEmployeeId = model.SelectedEmployeeId.FirstOrDefault();

            if (projectTask == null)
                return RedirectToAction("List");


            if (!string.IsNullOrWhiteSpace(model.EstimationTimeHHMM) &&
                System.Text.RegularExpressions.Regex.IsMatch(model.EstimationTimeHHMM, @"^([0-9]{1,2}):([0-5][0-9])$"))
            {
                model.EstimatedTime = await _timeSheetsService.ConvertHHMMToDecimal(model.EstimationTimeHHMM);
            }
            else
            {
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("admin.catalog.estimationTime.error.invalidhhmm"));

                model = await _projectTaskModelFactory.PrepareProjectTaskModelAsync(model, null, true);
  
                return View("/Areas/Admin/Views/Extension/ProjectTasks/Edit.cshtml", model);
            }

            if (string.IsNullOrWhiteSpace(model.SpentTime) ||
                 !System.Text.RegularExpressions.Regex.IsMatch(model.SpentTime, @"^([0-9]{1,2}):([0-5][0-9])$"))
            {
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("admin.catalog.spentTime.error.invalidhhmm"));

                model = await _projectTaskModelFactory.PrepareProjectTaskModelAsync(model, null, true);

                return View("/Areas/Admin/Views/Extension/ProjectTasks/Edit.cshtml", model);
            }
            if (model.ParentTaskId == 0 && (model.Tasktypeid == 3 || model.Tasktypeid == 4))
            {
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("admin.catalog.parenttask.error.required"));

                model = await _projectTaskModelFactory.PrepareProjectTaskModelAsync(model, null, true);

                return View("/Areas/Admin/Views/Extension/ProjectTasks/Edit.cshtml", model);
            }
           
            if (await _processRulesService.IsCommentRequired(projectTask.ProcessWorkflowId, prevProjectTask.StatusId, model.StatusId) && (prevProjectTask.ProcessWorkflowId == model.ProcessWorkflowId))    
            {
                if (string.IsNullOrWhiteSpace(model.StatusChangeComment))
                {
                    _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("admin.catalog.projectTask.error.commentrequired"));

                    model = await _projectTaskModelFactory.PrepareProjectTaskModelAsync(model, null, true);
   
                    return View("/Areas/Admin/Views/Extension/ProjectTasks/Edit.cshtml", model);
                }
            }

            if (ModelState.IsValid)
            {
                var existingTask = await _projectTaskService.GetProjectTaskByTitleAndProjectAsync(model.TaskTitle, model.ProjectId);
                if (existingTask != null && prevProjectTask.TaskTitle != model.TaskTitle)
                {
                    _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("admin.catalog.projecttask.error.tasknameexists"));

                    model = await _projectTaskModelFactory.PrepareProjectTaskModelAsync(model, null, true);

                    return View("/Areas/Admin/Views/Extension/ProjectTasks/Edit.cshtml", model);
                }

                projectTask = model.ToEntity(projectTask);
                (projectTask.SpentHours, projectTask.SpentMinutes) = await _timeSheetsService.ConvertSpentTimeAsync(model.SpentTime);

                projectTask.DeliveryOnTime = await _timeSheetsService.IsTaskDeliveredOnTimeAsync(projectTask, prevProjectTask);
                projectTask.AssignedTo = await _workflowStatusService.GetAssignToIdByStatus(projectTask);
                await _projectTaskService.UpdateProjectTaskAsync(projectTask);

                var status = await _workflowStatusService.GetWorkflowStatusByIdAsync(projectTask.StatusId);
                if (status != null &&
    string.Equals(
        status.StatusName?.Trim(),
        "Closed",
        StringComparison.OrdinalIgnoreCase))
                {
                    var followuptask =
                        await _followUpTaskService.GetFollowUpTaskByTaskIdAsync(projectTask.Id);

                    if (followuptask != null && !followuptask.IsCompleted)
                    {
                        followuptask.IsCompleted = true;
                        await _followUpTaskService.UpdateFollowUpTaskAsync(followuptask);

                    }
                }

                var customer = await _workContext.GetCurrentCustomerAsync();
                int currEmployeeId = 0;

                if (!await _customerService.IsRegisteredAsync(customer))
                    return Challenge();
                if (customer != null)
                {
                    var currEmployee = await _employeeService.GetEmployeeByCustomerIdAsync(customer.Id);
                    if (currEmployee != null)
                        currEmployeeId = currEmployee.Id;
                }

                if (!string.IsNullOrWhiteSpace(model.StatusChangeComment))
                {
                    
                    TaskComments taskComment = new TaskComments();
                    taskComment.EmployeeId = currEmployeeId;
                    taskComment.StatusId = projectTask.StatusId;
                    taskComment.Description = model.StatusChangeComment;
                    taskComment.TaskId = model.Id;
                    taskComment.CreatedOn = await _dateTimeHelper.GetUTCAsync();

                    await _taskCommentsService.InsertTaskCommentsAsync(taskComment);
                    await _workflowMessageService.SendEmployeeMentionMessageAsync((await _workContext.GetWorkingLanguageAsync()).Id, taskComment.EmployeeId, taskComment.TaskId, taskComment.Description);
                }

                await _taskChangeLogService.InsertTaskChangeLogByUpdateTaskAsync(prevProjectTask, projectTask, currEmployeeId);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.projectTask.Updated"));

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = projectTask.Id });
            }

            model = await _projectTaskModelFactory.PrepareProjectTaskModelAsync(model, null, true);

            return View("/Areas/Admin/Views/Extension/ProjectTasks/Edit.cshtml", model);
        }
        [HttpPost]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProjectTask, PermissionAction.Delete))
                return AccessDeniedView();

            var projectTask = await _projectTaskService.GetProjectTasksByIdAsync(id);
            if (projectTask == null)
                return RedirectToAction("List");

            await _projectTaskService.DeleteProjectTaskAsync(projectTask);

            _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Catalog.projectTask.Deleted"));

            return RedirectToAction("List");
        }
        [HttpPost]
        public virtual async Task<IActionResult> DeleteSelected(ICollection<int> selectedIds)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProjectTask, PermissionAction.Delete))
                return AccessDeniedView();

            if (selectedIds == null || selectedIds.Count == 0)
                return NoContent();

            var data = await _projectTaskService.GetProjectsTasksByIdsAsync(selectedIds.ToArray());

            foreach (var item in data)
            {
                await _projectTaskService.DeleteProjectTaskAsync(item);
            }
            return Json(new { Result = true });
        }


        public virtual async Task<IActionResult> SyncTime()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProjectTask, PermissionAction.View))
                return AccessDeniedView();

            var model = await _projectTaskModelFactory.PrepareProjectTaskModelAsync(new ProjectTaskModel(), null);

            ViewBag.RefreshPage = false;

            return View("/Areas/Admin/Views/Extension/ProjectTasks/SyncTime.cshtml", model);
        }


        public async Task<IActionResult> SearchMismatchEntires(DateTime From, DateTime To, int employeeId, int projectId)
        {
            var mismatches = await _timeSheetsService.GetMismatchEntriesAsync(From, To, employeeId, projectId);

            return Json(new { mismatches });
        }

        public async Task<IActionResult> CorrectMismatchedEntries(DateTime From, DateTime To, int employeeId, int projectId)
        {
            var mismatches = await _timeSheetsService.GetMismatchEntriesAsync(From, To, employeeId, projectId);

            foreach (var entry in mismatches)
            {
                var projectTask = await _projectTaskService.GetProjectTasksByIdAsync(entry.TaskId);
                if (projectTask != null)
                {
                    (int actualHours, int actualMinutes) = await _timeSheetsService.ConvertSpentTimeAsync(entry.ActualTime);
                    projectTask.SpentHours = actualHours;
                    projectTask.SpentMinutes = actualMinutes;
                    await _projectTaskService.UpdateProjectTaskAsync(projectTask);
                }
            }
            return Json(new { Result = true });
        }

        [HttpGet]
        public async Task<IActionResult> GetProcessWorkflowsByProjectId(int projectId)
        {
            var project = await _projectsService.GetProjectsByIdAsync(projectId);
            if (project == null || string.IsNullOrEmpty(project.ProcessWorkflowIds))
                return Json(new List<SelectListItem>());

            var ids = project.ProcessWorkflowIds
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(x => int.TryParse(x, out var id) ? id : (int?)null)
                .Where(x => x.HasValue)
                .Select(x => x.Value)
                .ToArray();

            var workflows = await _processWorkflowService.GetProcessWorkflowsByIdsAsync(ids);

            var items = workflows.Select(w => new SelectListItem
            {
                Text = w.Name,
                Value = w.Id.ToString()
            }).ToList();

            return Json(items);
        }

        [HttpGet]
        public async Task<IActionResult> GetTaskCategoryByProjectId(int projectId)
        {
            if (projectId <= 0)
                return Json(new List<SelectListItem>());

            var taskCategories = await _projectTaskCategoryMappingService.GetAllMappingsAsync(projectId);

            var items = new List<SelectListItem>();
            foreach (var mapping in taskCategories)
            {
                var category = await _taskCategoryService.GetTaskCategoryByIdAsync(mapping.TaskCategoryId);
                if (category != null)
                {
                    items.Add(new SelectListItem
                    {
                        Text = category.CategoryName,
                        Value = category.Id.ToString()
                    });
                }
            }
            return Json(items);
        }

        [HttpGet]
        public async Task<IActionResult> GetStatusesByProcessWorkflow(int processWorkflowId)
        {
           
            var statuses = await _workflowStatusService.GetAllWorkflowStatusAsync(processWorkflowId);
            var items = new List<SelectListItem>();
            foreach (var status in statuses)
            {              
                if (status != null)
                {
                    items.Add(new SelectListItem
                    {
                        Value = status.Id.ToString(),
                        Text = status.StatusName + "|||" + status.ColorCode
                    });
                }
            }

            return Json(items);
        }


        [HttpPost]
        public async Task<IActionResult> GetStatusSummary(ProjectTaskSearchModel searchModel)
        {
            var filteredTasks = await _commonPluginService.GetAllProjectTasksAsync(
                taskId: searchModel.SearchTaskId,
                taskTypeId: searchModel.SearchTaskTypeId,
                employeeIds: searchModel.SelectedEmployeeIds,
                projectIds: searchModel.SelectedProjectIds,
                taskName: searchModel.SearchTaskTitle,
                from: null,
                to: null,
                dueDate: searchModel.DueDate,
                SelectedStatusId: searchModel.SearchStatusId,
                processWorkflowId: searchModel.SearchProcessWorkflowId,
                pageIndex: searchModel.Page - 1,
                pageSize: int.MaxValue,
                showHidden: false,
                filterDeliveryOnTime: searchModel.SearchDeliveryOnTime,
                searchParentTaskId: searchModel.SearchParentTaskId);

            var allStatuses = await _workflowStatusService.GetAllWorkflowStatusAsync();
            var pagedStatuses = new PagedList<WorkflowStatus>(allStatuses, 0, int.MaxValue); 

            var allWorkflows = await _processWorkflowService.GetAllProcessWorkflowsAsync();

            if (searchModel.SearchProcessWorkflowId > 0)
                allStatuses = new PagedList<WorkflowStatus>(
     allStatuses.Where(s => s.ProcessWorkflowId == searchModel.SearchProcessWorkflowId).ToList(),
     0, 
     int.MaxValue 
 );

            if (searchModel.SearchStatusId > 0)
                allStatuses = new PagedList<WorkflowStatus>(allStatuses.Where(s => s.Id == searchModel.SearchStatusId).ToList(), 0, int.MaxValue);

            var taskCounts = filteredTasks
                .GroupBy(t => t.StatusId)
                .ToDictionary(g => g.Key, g => g.Count());

            var result = allStatuses
                .Where(s => !string.IsNullOrEmpty(s.StatusName))
                .Select(s =>
                {
                    var count = taskCounts.TryGetValue(s.Id, out var c) ? c : 0;
                    var workflow = allWorkflows.FirstOrDefault(w => w.Id == s.ProcessWorkflowId);
                    if (workflow == null)
                        return null;

                    return new
                    {
                        StatusId = s.Id,
                        StatusName = s.StatusName,
                        ColorCode = s.ColorCode ?? "#999",
                        WorkflowId = s.ProcessWorkflowId,
                        WorkflowName = workflow.Name,
                        TaskCount = count
                    };
                })
                .Where(x => x != null)
                .ToList();

            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetParentTasksByProjectId(int projectId)
        {
            var tasks = await _projectTaskService.GetParentTasksByProjectIdAsync(projectId);

            var list = tasks.Select(t => new
            {
                Text = t.TaskTitle,
                Value = t.Id
            });

            return Json(list);
        }


        [HttpGet]
        public async Task<IActionResult> GetParentTasksByFilter(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return Json(Enumerable.Empty<object>()); 

            var allParentTasks = await _projectTaskService.GetParentTasksByProjectIdAsync(0);
            var filtered = allParentTasks
                .Where(t => t.TaskTitle.Contains(term, StringComparison.OrdinalIgnoreCase))
                .Select(t => new
                {
                    Text = t.TaskTitle,
                    Value = t.Id
                })
                .ToList();

            return Json(filtered);
        }
    }
}