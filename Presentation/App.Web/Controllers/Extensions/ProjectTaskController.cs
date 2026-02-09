using App.Core;
using App.Core.Domain.Extension.ProjectTasks;
using App.Core.Domain.Extension.TimeSheets;
using App.Core.Domain.ProjectTasks;
using App.Services.Configuration;
using App.Services.Customers;
using App.Services.Employees;
using App.Services.Helpers;
using App.Services.Leaves;
using App.Services.Localization;
using App.Services.Messages;
using App.Services.ProjectEmployeeMappings;
using App.Services.Projects;
using App.Services.ProjectTasks;
using App.Services.Security;
using App.Services.TaskAlerts;
using App.Services.TimeSheets;
using App.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using App.Web.Areas.Admin.Models.Extension.TaskChangeLogs;
using App.Web.Factories.Extensions;
using App.Web.Framework.Mvc.Filters;
using App.Web.Models.Extensions.ProjectTasks;
using AspNetCoreGeneratedDocument;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using Satyanam.Nop.Core.Domains;
using Satyanam.Nop.Core.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace App.Web.Controllers.Extensions
{
    public partial class ProjectTaskController : Controller
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
        private readonly IProjectEmployeeMappingService _projectEmployeeMappingService;
        private readonly IProcessWorkflowService _processWorkflowService;
        private readonly IWorkflowStatusService _workflowStatusService;
        private readonly IProcessRulesService _processRulesService;
        private readonly ICommonPluginService _commonPluginService;
        private readonly ICheckListMappingService _checkListMappingService;
        private readonly ICheckListMasterService _checkListMasterService;
        private readonly ITaskCheckListEntryService _taskCheckListEntryService;
        private readonly ITaskCategoryService _taskCategoryService;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly ISettingService _settingService;
        private readonly IProjectTaskCategoryMappingService _projectTaskCategoryMappingService;
        private readonly IFollowUpTaskService _followUpTaskService;
        private readonly ITaskAlertService _taskAlertService;

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
            ITaskChangeLogService taskChangeLogService,
            IProjectEmployeeMappingService projectEmployeeMappingService,
            IProcessWorkflowService processWorkflowService,
            IWorkflowStatusService workflowStatusService,
            IProcessRulesService processRulesService,
            ICommonPluginService commonPluginService,
            ICheckListMappingService checkListMappingService,
            ICheckListMasterService checkListMasterService,
            ITaskCheckListEntryService taskCheckListEntryService,
            IWebHostEnvironment hostingEnvironment,
            ISettingService settingService,
            IProjectTaskCategoryMappingService projectTaskCategoryMappingService,
            ITaskCategoryService taskCategoryService,
            IFollowUpTaskService followUpTaskService,
            ITaskAlertService taskAlertService)
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
            _projectEmployeeMappingService = projectEmployeeMappingService;
            _processWorkflowService = processWorkflowService;
            _workflowStatusService = workflowStatusService;
            _processRulesService = processRulesService;
            _commonPluginService = commonPluginService;
            _checkListMappingService = checkListMappingService;
            _checkListMasterService = checkListMasterService;
            _taskCheckListEntryService = taskCheckListEntryService;
            _hostingEnvironment = hostingEnvironment;
            _settingService = settingService;
            _projectTaskCategoryMappingService = projectTaskCategoryMappingService;
            _taskCategoryService = taskCategoryService;
            _followUpTaskService = followUpTaskService;
            _taskAlertService = taskAlertService;
        }

        #endregion
        public virtual async Task<IActionResult> List(int projectId)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            if (!await _customerService.IsRegisteredAsync(customer))
                return Challenge();
            ProjectTaskSearchModel projectTaskSearchModel = new ProjectTaskSearchModel();
            if (customer != null)
            {
                var employeeByCustomer = await _employeeService.GetEmployeeByCustomerIdAsync(customer.Id);
                if (employeeByCustomer != null)
                {
                    projectTaskSearchModel.EmployeeId = employeeByCustomer.Id;

                }
            }
            var allowedProjects = await _projectsService.GetProjectListByEmployee(projectTaskSearchModel.EmployeeId);
            if (!allowedProjects.Any(p => p.Id == projectId))
                return Unauthorized();
            
            var model = await _projectTaskModelFactory.PrepareProjectTaskSearchModelAsync(projectTaskSearchModel);
            model.SelectedProjectIds = new List<int> { projectId };
            var parentTask = await _projectTaskService.GetParentTasksByProjectIdAsync(projectId);
            model.AvailableParentTasks = parentTask
        .Select(x => new SelectListItem
        {
            Value = x.Id.ToString(),
            Text = x.TaskTitle
        })
        .ToList();
            model.AvailableParentTasks.Insert(0, new SelectListItem
            {
                Value = "0",
                Text = "Select Parent Task"
            });
            return View("/Themes/DefaultClean/Views/Extension/ProjectTasks/ProjectTaskList.cshtml", model);
        }

        public virtual async Task<IActionResult> ProjectManagement()
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            if (!await _customerService.IsRegisteredAsync(customer))
                return Challenge();
            ProjectTaskSearchModel projectTaskSearchModel = new ProjectTaskSearchModel();
            if (customer != null)
            {
                var employeeByCustomer = await _employeeService.GetEmployeeByCustomerIdAsync(customer.Id);
                if (employeeByCustomer != null)
                {
                    projectTaskSearchModel.EmployeeId = employeeByCustomer.Id;

                }
            }
            var project = await _projectsService.GetProjectListByEmployee(projectTaskSearchModel.EmployeeId);
            foreach (var p in project)
            {
                projectTaskSearchModel.AvailableProjects.Add(new SelectListItem
                {
                    Text = p.ProjectTitle,
                    Value = p.Id.ToString()
                });
            }
            return View("/Themes/DefaultClean/Views/Extension/ProjectTasks/ProjectManagement.cshtml", projectTaskSearchModel);
        }

        [HttpPost]
        public virtual async Task<IActionResult> List(ProjectTaskSearchModel searchModel)
        {
            var model = await _projectTaskModelFactory.PrepareProjectTaskListModelAsync(searchModel);
            return Json(model);
        }

        public virtual async Task<IActionResult> ProjectTaskList(
            int taskId,
            int taskTypeId,
            string projectIds,
            string employeeIds,
            string taskName,
            int searchProcessWorkflowId,
            DateTime? dueDate,
            int statusId,
            int searchDeliveryOnTime,
            int searchParentTaskId,
            int pageIndex = 0,
            int pageSize = 50)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();

            if (!await _customerService.IsRegisteredAsync(customer))
                return Challenge();

            int currCustomer = 0;
            if (customer != null)
            {
                var employeeByCustomer = await _employeeService.GetEmployeeByCustomerIdAsync(customer.Id);
                if (employeeByCustomer != null)
                {
                    currCustomer = employeeByCustomer.Id;
                }
            }
            var projectIdsList = new List<int>();
            if (string.IsNullOrWhiteSpace(projectIds))
                return Json(null);
            projectIdsList = projectIds.Split(',').Select(int.Parse).ToList();
            var selectedEmployeeIds = new List<int>();
            if (employeeIds != null)
            {
                selectedEmployeeIds = employeeIds.Split(',').Select(int.Parse).ToList();
            }
            var projectTasks = await _commonPluginService.GetAllProjectTasksAsync(
                taskId: taskId,
                taskTypeId: taskTypeId,
                employeeIds: selectedEmployeeIds,
                projectIds: projectIdsList,
                taskName: taskName,
                from: null,
                to: null,
                dueDate: dueDate,
                SelectedStatusId: statusId,
                processWorkflowId: searchProcessWorkflowId,
                showHidden: false,
                pageIndex: pageIndex,
                pageSize: int.MaxValue,
                filterDeliveryOnTime: searchDeliveryOnTime,
                searchParentTaskId: searchParentTaskId
            );

            var result = new List<ProjectTaskModel>();

            foreach (var projectTask in projectTasks)
            {
                if (projectTask != null)
                {
                    var model = new ProjectTaskModel();
                    model.Id = projectTask.Id;
                    model.EmployeeId = projectTask.AssignedTo;
                    model.ProjectId = projectTask.ProjectId;
                    model.Id = projectTask.Id;
                    model.CreatedOnUtc = projectTask.CreatedOnUtc;
                    model.SpentTime = await _timeSheetsService.ConvertSpentTimeAsync(projectTask.SpentHours, projectTask.SpentMinutes);
                    var workflowStatus = await _workflowStatusService.GetWorkflowStatusByIdAsync(projectTask.StatusId);
                    if (workflowStatus != null)
                        model.Status = workflowStatus.StatusName + "|||" + workflowStatus.ColorCode;

                    var project = await _projectsService.GetProjectsByIdAsync(projectTask.ProjectId);
                    if (project != null)
                        model.ProjectName = project.ProjectTitle;

                    var task = await _projectTaskService.GetProjectTasksWithoutCacheByIdAsync(projectTask.Id);
                    if (task != null)
                    {
                        model.TaskTitle = task.TaskTitle;
                        model.SpentTime = await _timeSheetsService.ConvertSpentTimeAsync(task.SpentHours, task.SpentMinutes);
                        model.EstimationTimeHHMM = await _timeSheetsService.ConvertToHHMMFormat(task.EstimatedTime);
                        model.DeliveryOnTime = task.DeliveryOnTime;
                        model.WorkQualityFormat = task.WorkQuality != null ? task.WorkQuality + "%" : "";
                        model.DOTPercentageFormat = task.DOTPercentage != null ? task.DOTPercentage + "%" : "";

                        if (task.DueDate != null)
                            model.DueDateFormat = task.DueDate.HasValue
              ? await _workflowStatusService.IsTaskOverdue(task.Id)
                  ? $"<span style='color: red;'>{task.DueDate.Value:dd-MMMM-yyyy}</span>"
                  : task.DueDate.Value.ToString("dd-MMMM-yyyy")
              : "";
                        var SelectedTypeOption = projectTask.Tasktypeid;
                        if (SelectedTypeOption != 0)
                        {
                            string taskType = ((TaskTypeEnum)SelectedTypeOption).ToString();
                            model.TaskTypeName = taskType;
                        }

                        var parentTask = await _projectTaskService.GetProjectTasksWithoutCacheByIdAsync(task.ParentTaskId);
                        if (parentTask != null)
                            model.ParentTaskName = parentTask.TaskTitle;

                    }
                    var employee = await _employeeService.GetEmployeeByIdAsync(projectTask.AssignedTo);
                    if (employee != null)
                        model.AssignedEmployee = employee.FirstName + " " + employee.LastName;

                    result.Add(model);
                }
            }
            return Json(new
            {
                data = result,
                pageIndex,
                pageSize
            });
        }
        public virtual async Task<IActionResult> Create()
        {
            var model = await _projectTaskModelFactory.PrepareProjectTaskModelAsync(new ProjectTaskModel(), null);
            return View("/Areas/Admin/Views/Extension/ProjectTasks/Create.cshtml", model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Create(ProjectTaskModel model, bool continueEditing)
        {
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

            if (ModelState.IsValid)
            {
                var projectTask = model.ToEntity<ProjectTask>();
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
                TaskComments taskComment = new TaskComments();
                taskComment.EmployeeId = currEmployeeId;
                taskComment.StatusId = projectTask.StatusId;
                taskComment.Description = model.StatusChangeComment;
                taskComment.TaskId = projectTask.Id;
                taskComment.CreatedOn = await _dateTimeHelper.GetUTCAsync();
                await _taskCommentsService.InsertTaskCommentsAsync(taskComment);
                await _workflowMessageService.SendEmployeeMentionMessageAsync((await _workContext.GetWorkingLanguageAsync()).Id, taskComment.EmployeeId, taskComment.TaskId, taskComment.Description);
                if (!continueEditing)
                    return RedirectToAction("List");
                return RedirectToAction("Edit", new { id = projectTask.Id });
            }
            model = await _projectTaskModelFactory.PrepareProjectTaskModelAsync(model, null, true);
            return View("/Areas/Admin/Views/Extension/ProjectTasks/Create.cshtml", model);
        }
        public virtual async Task<IActionResult> Edit(int id)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
                return Challenge();
            var projectTask = await _projectTaskService.GetProjectTasksWithoutCacheByIdAsync(id);
            if (projectTask == null)
                return RedirectToAction("List");
            var model = await _projectTaskModelFactory.PrepareProjectTaskModelAsync(null, projectTask);

            return View("/Themes/DefaultClean/Views/Extension/ProjectTasks/Edit.cshtml", model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Edit(ProjectTaskModel model, bool continueEditing)
        {
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
                TaskCategoryId = projectTask.TaskCategoryId
            };

            int selectedEmployeeId = model.SelectedEmployeeId.FirstOrDefault();
            model.AssignedTo = selectedEmployeeId;

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
            if (ModelState.IsValid)
            {
                projectTask = model.ToEntity(projectTask);
                (projectTask.SpentHours, projectTask.SpentMinutes) = await _timeSheetsService.ConvertSpentTimeAsync(model.SpentTime);
                projectTask.DeliveryOnTime = await _timeSheetsService.IsTaskDeliveredOnTimeAsync(projectTask, prevProjectTask);
                await _projectTaskService.UpdateProjectTaskAsync(projectTask);
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
                if (prevProjectTask.StatusId != projectTask.StatusId)
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
        public async Task<IActionResult> Delete(int id)
        {
            var projectTask = await _projectTaskService.GetProjectTasksByIdAsync(id);
            if (projectTask == null)
                return RedirectToAction("List");

            var projectId = projectTask.ProjectId;

            await _projectTaskService.DeleteProjectTaskAsync(projectTask);

            _notificationService.SuccessNotification("Task deleted successfully.");
            return RedirectToAction("List", "ProjectTask", new { projectId = projectId });
        }
        public virtual async Task<IActionResult> SyncTime()
        {
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

        [HttpPost]
        public async Task<IActionResult> SaveTask([FromBody] ProjectTaskModel model)
        {
            if (model == null)
                return BadRequest();

            var errors = new Dictionary<string, string>();
            if (string.IsNullOrWhiteSpace(model.TaskTitle))
                errors["TaskTitle"] = "Task title is required.";
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

            if (model.DueDate == null)
                errors["DueDate"] = "Due date is required.";

            if (model.ProcessWorkflowId == 0)
                errors["ProcessWorkflowId"] = "Please select a Process Workflow.";

            if (string.IsNullOrWhiteSpace(model.Description))
                errors["Description"] = "Description is required.";

            if (model.ParentTaskId == 0 && (model.Tasktypeid == 3 || model.Tasktypeid == 4))
                errors["ParentTaskId"] = "Please select Parent task in case of Bug or Change request ";

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
                TaskCategoryId = projectTask.TaskCategoryId

            };
            if (prevProjectTask.ProcessWorkflowId != model.ProcessWorkflowId)
            {
                model.StatusId = await _workflowStatusService.GetDefaultStatusIdByWorkflowId(model.ProcessWorkflowId);
            }

            if (await _processRulesService.IsCommentRequired(projectTask.ProcessWorkflowId, prevProjectTask.StatusId, model.StatusId)
      && (prevProjectTask.ProcessWorkflowId == model.ProcessWorkflowId))
            {
                var comment = model.StatusChangeComment ?? string.Empty;
                bool hasImage = Regex.IsMatch(comment, "<img[^>]*>", RegexOptions.IgnoreCase);
                var textOnly = Regex.Replace(comment, "<.*?>", string.Empty).Trim();
                if (string.IsNullOrWhiteSpace(textOnly) && !hasImage)
                {
                    errors["StatusChangeComment"] = "Comment is required for this status change.";
                }

            }

            var existingTask = await _projectTaskService.GetProjectTaskByTitleAndProjectAsync(model.TaskTitle, projectTask.ProjectId);
            if (existingTask != null && prevProjectTask.TaskTitle != model.TaskTitle)
            {
                errors["TaskTitle"] = "Task with the same name already exists within project.";
            }
            if (model.TaskCategoryId == 0)
            {
                errors["TaskCategoryId"] = "Please Select Task Category";
            }
            if (errors.Any())
            {
                return Json(new { success = false, validationErrors = errors });
            }

            if (projectTask == null)
                return Json(new { success = false });

            if (!string.IsNullOrWhiteSpace(model.EstimationTimeHHMM) &&
    System.Text.RegularExpressions.Regex.IsMatch(model.EstimationTimeHHMM, @"^([0-9]+):([0-5][0-9])$"))
            {
                model.EstimatedTime = await _timeSheetsService.ConvertHHMMToDecimal(model.EstimationTimeHHMM);
            }
            else
            {
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("admin.catalog.estimationTime.error.invalidhhmm"));

                model = await _projectTaskModelFactory.PrepareProjectTaskModelAsync(model, null, true);
                return Json(new { success = false, message = "invalid HHMM" });
            }

            if (ModelState.IsValid)
            {
                projectTask.Id = model.Id;
                projectTask.TaskTitle = model.TaskTitle;
                projectTask.IsManualDOT = model.IsManualDOT;
                projectTask.Description = model.Description;
                projectTask.EstimatedTime = model.EstimatedTime;
                projectTask.DueDate = model.DueDate;
                projectTask.StatusId = model.StatusId;
                projectTask.ProcessWorkflowId = model.ProcessWorkflowId;
                projectTask.AssignedTo = model.AssignedTo;
                projectTask.ParentTaskId = model.ParentTaskId;
                projectTask.IsSync = model.IsSync;
                projectTask.TaskCategoryId = model.TaskCategoryId;
                if (prevProjectTask.ProcessWorkflowId != model.ProcessWorkflowId)
                {
                    projectTask.StatusId = await _workflowStatusService.GetDefaultStatusIdByWorkflowId(model.ProcessWorkflowId);
                }

                if (model.ChecklistEntries != null && model.ChecklistEntries.Any())
                {
                    var validChecklistEntries = model.ChecklistEntries
                        .Where(e => e.CheckListId > 0)
                        .ToList();
                    if (validChecklistEntries.Any())
                    {
                        var checklistJson = JsonConvert.SerializeObject(validChecklistEntries);
                        var checklistEntry = new TaskCheckListEntry
                        {
                            TaskId = projectTask.Id,
                            StatusId = model.StatusId,
                            CheckListJson = checklistJson,
                            CheckedBy = currEmployeeId,
                            CreatedOn = await _dateTimeHelper.GetUTCAsync()
                        };
                        await _taskCheckListEntryService.InsertEntryAsync(checklistEntry);
                    }
                }
                projectTask.DeliveryOnTime = await _timeSheetsService.IsTaskDeliveredOnTimeAsync(projectTask, prevProjectTask);
                await _projectTaskService.UpdateProjectTaskAsync(projectTask);
                var htmlContent = model.StatusChangeComment;
                var plainText = Regex.Replace(htmlContent ?? string.Empty, "<(?!img\\b)[^>]*>", string.Empty, RegexOptions.IgnoreCase);
                plainText = HttpUtility.HtmlDecode(plainText).Trim();

                if (!string.IsNullOrWhiteSpace(plainText))
                {
                    var decodedHtml = HttpUtility.HtmlDecode(model.StatusChangeComment);
                    var taskComment = new TaskComments
                    {
                        EmployeeId = currEmployeeId,
                        StatusId = projectTask.StatusId,
                        Description = decodedHtml,
                        TaskId = model.Id,
                        CreatedOn = await _dateTimeHelper.GetUTCAsync()
                    };
                    await _taskCommentsService.InsertTaskCommentsAsync(taskComment);
                    await _workflowMessageService.SendEmployeeMentionMessageAsync(
                        (await _workContext.GetWorkingLanguageAsync()).Id,
                        taskComment.EmployeeId,
                        taskComment.TaskId,
                        decodedHtml
                    );
                }

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
                      await  _followUpTaskService.UpdateFollowUpTaskAsync(followuptask);
                    }
                }
                await _taskChangeLogService.InsertTaskChangeLogByUpdateTaskAsync(prevProjectTask, projectTask, currEmployeeId, model.ChecklistEntries != null ? model.ChecklistEntries.ToList() : null);
                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.projectTask.Updated"));
                return Json(new { success = true });
            }
            return Json(new { success = false, message = "Error" });
        }

        public async Task<IActionResult> GetTaskChangeLogs(int taskId)
        {
            var taskChangeLogs = await _taskChangeLogService.GetAllTaskChangeLogAsync(taskId, 0, 0, 0, null, null, 0);
            IList<TaskChangeLogModel> taskChangeLogModel = new List<TaskChangeLogModel>();
            foreach (var taskChangeLog in taskChangeLogs)
            {
                TaskChangeLogModel model = new TaskChangeLogModel();
                var employee = await _employeeService.GetEmployeeByIdAsync(taskChangeLog.EmployeeId);

                if (employee != null)
                    model.EmployeeName = employee.FirstName + " " + employee.LastName;

                var task = await _projectTaskService.GetProjectTasksByIdAsync(taskChangeLog.TaskId);
                if (task != null)
                {
                    model.TaskName = task.TaskTitle;
                }
                var assignedTo = await _employeeService.GetEmployeeByIdAsync(taskChangeLog.AssignedTo);
                if (assignedTo != null)
                    model.AssignedToName = assignedTo.FirstName + " " + assignedTo.LastName;

                model.Notes = taskChangeLog.Notes;
                model.CreatedOn = taskChangeLog.CreatedOn;

                var workflowStatus = await _workflowStatusService.GetWorkflowStatusByIdAsync(taskChangeLog.StatusId);
                if (workflowStatus != null)
                    model.StatusName = workflowStatus.StatusName;

                var SelectedLogTypeOption = taskChangeLog.LogTypeId;
                string logType = ((LogTypeEnum)SelectedLogTypeOption).ToString();
                if (logType != null)
                    model.LogTypeName = logType;

                taskChangeLogModel.Add(model);

            }
            return Json(taskChangeLogModel);
        }

        [HttpGet]
        public async Task<IActionResult> GetProcessWorkflowsByProjectIds([FromBody] int[] projectIds)
        {
            if (projectIds == null || !projectIds.Any())
                return Json(new List<SelectListItem>());

            var allWorkflowIds = new List<int>();

            foreach (var projectId in projectIds)
            {
                var project = await _projectsService.GetProjectsByIdAsync(projectId);
                if (project != null && !string.IsNullOrEmpty(project.ProcessWorkflowIds))
                {
                    var ids = project.ProcessWorkflowIds
                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(x => int.TryParse(x, out var id) ? id : (int?)null)
                        .Where(x => x.HasValue)
                        .Select(x => x.Value)
                        .ToList();

                    allWorkflowIds.AddRange(ids);
                }
            }
            var distinctWorkflowIds = allWorkflowIds.Distinct().ToArray();
            var workflows = await _processWorkflowService.GetProcessWorkflowsByIdsAsync(distinctWorkflowIds);
            var activeOrderedWorkflows = workflows
                .Where(w => w.IsActive)
                .OrderBy(w => w.DisplayOrder)
                .ToList();

            var items = new List<SelectListItem>
    {
        new SelectListItem { Text = "Select", Value = "" }
    };

            items.AddRange(activeOrderedWorkflows.Select(w => new SelectListItem
            {
                Text = w.Name,
                Value = w.Id.ToString()
            }));

            return Json(items);
        }

        [HttpGet]
        public async Task<IActionResult> GetStatusesByProcessWorkflow(int processWorkflowId)
        {
            var statuses = await _workflowStatusService.GetAllWorkflowStatusAsync(processWorkflowId);

            var items = new List<SelectListItem>();
            items.Add(new SelectListItem
            {
                Value = "0",
                Text = "All"
            });
            if (processWorkflowId != 0)
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
                taskId: searchModel.TaskId,
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

            var allStatuses = await _workflowStatusService.GetAllWorkflowStatusAsync(0);
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

        [HttpPost]
        public async Task<IActionResult> GetParentTasksByProjectIds([FromBody] List<int> projectIds)
        {
            if (projectIds == null || !projectIds.Any())
                return Json(new List<object>());
            var allTasks = new List<ProjectTask>();
            foreach (var projectId in projectIds)
            {
                var tasks = await _projectTaskService.GetParentTasksByProjectIdAsync(projectId);
                allTasks.AddRange(tasks);
            }
            var uniqueTasks = allTasks
                .GroupBy(t => t.Id)
                .Select(g => g.First())
                .ToList();
            var list = uniqueTasks.Select(t => new
            {
                Text = t.TaskTitle,
                Value = t.Id
            });
            return Json(list);
        }

        public async Task<IActionResult> GetStatusInfo(int taskId, int toStateId)
        {
            var task = await _projectTaskService.GetProjectTasksWithoutCacheByIdAsync(taskId);
            if (task == null)
                return Json(new { statusName = "", mentions = new List<string>(), commentTemplate = "" });
            var fromStateId = task.StatusId;
            var status = await _workflowStatusService.GetWorkflowStatusByIdAsync(toStateId);
            if (status == null)
                return Json(new { statusName = "", mentions = new List<string>(), commentTemplate = "" });
            task.StatusId = toStateId;
            int assignToId = await _workflowStatusService.GetAssignToIdByStatus(task);

            var mentions = new List<string>();
            string commentTemplate = "";

            var rule = await _processRulesService.GetRulesByStatesAsync(task.ProcessWorkflowId, fromStateId, toStateId);
            if (rule != null)
            {
                commentTemplate = rule.CommentTemplate ?? "";
            }
            if (status.StatusName.Equals("Ready to test", StringComparison.OrdinalIgnoreCase) || status.StatusName.Equals("QA on Live", StringComparison.OrdinalIgnoreCase))
            {
                var qaEmployeeId = await _projectsService.GetProjectQAIdByIdAsync(task.ProjectId);
                var qaEmployee = await _employeeService.GetEmployeeByIdAsync(qaEmployeeId);
                if (qaEmployee != null)
                    mentions.Add($"@<{qaEmployee.FirstName} {qaEmployee.LastName}>");
            }
            else if (status.StatusName.Equals("Code Review", StringComparison.OrdinalIgnoreCase))
            {
                int employeeId = 0;

                employeeId = await _projectsService.GetProjectCoordinatorIdByIdAsync(task.ProjectId);
                if (employeeId == 0)
                    employeeId = await _projectsService.GetProjectLeaderIdByIdAsync(task.ProjectId);
                if (employeeId == 0)
                    employeeId = await _projectsService.GetProjectManagerIdByIdAsync(task.ProjectId);
                var employee = await _employeeService.GetEmployeeByIdAsync(employeeId);
                if (employee != null)
                {
                    mentions.Add($"@<{employee.FirstName} {employee.LastName}>");
                }

            }
            else if (status.StatusName.Equals("Test Failed", StringComparison.OrdinalIgnoreCase) || status.StatusName.Equals("Code Review Done", StringComparison.OrdinalIgnoreCase) || status.StatusName.Equals("Ready for Live", StringComparison.OrdinalIgnoreCase) || status.StatusName.Equals("Active", StringComparison.OrdinalIgnoreCase))
            {
                var employee = await _employeeService.GetEmployeeByIdAsync(task.DeveloperId);
                if (employee != null)
                    mentions.Add($"@<{employee.FirstName} {employee.LastName}>");
            }
            return Json(new { statusName = status.StatusName, mentions, commentTemplate, assignToId });
        }

        [HttpGet]
        public async Task<IActionResult> GetCheckLists(int taskId, int statusId)
        {
            var task = await _projectTaskService.GetProjectTasksWithoutCacheByIdAsync(taskId);
            if (task == null)
                return Json(new { success = false, message = "Task not found" });

            var mappings = await _checkListMappingService.GetCheckListsByCategoryAndStatusAsync(task.TaskCategoryId, statusId);
            var items = new List<object>();
            foreach (var mapping in mappings)
            {
                var checklist = await _checkListMasterService.GetCheckListByIdAsync(mapping.CheckListId);
                if (checklist != null)
                {
                    items.Add(new
                    {
                        Id = checklist.Id,
                        Title = checklist.Title,
                        IsMandatory = mapping.IsMandatory
                    });
                }
            }
            bool showSelectAll = false;
            if (items.Count > 0)
            {
                var projectTaskSettings = await _settingService.LoadSettingAsync<ProjectTaskSetting>();
                showSelectAll = projectTaskSettings.IsShowSelctAllCheckList;
            }
            return Json(new
            {
                success = true,
                checklists = items,
                showSelectAll
            });
        }

        [HttpPost]
        public async Task<IActionResult> UploadCommentImage(IFormFile image)
        {
            if (image == null || image.Length == 0)
                return Json(new { success = false, message = "No file uploaded" });
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var extension = Path.GetExtension(image.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
                return Json(new { success = false, message = "Invalid file type" });
            var uploadsDir = Path.Combine(_hostingEnvironment.WebRootPath, "content", "images", "taskcomments");
            if (!Directory.Exists(uploadsDir))
                Directory.CreateDirectory(uploadsDir);
            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsDir, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await image.CopyToAsync(stream);
            }
            var fileUrl = $"/content/images/taskcomments/{fileName}";
            return Json(new { success = true, url = fileUrl });
        }


        [HttpGet]
        public async Task<IActionResult> GetTaskCategoryByProjectId(int projectId)
        {
            if (projectId <= 0)
                return Json(new List<SelectListItem>
        {
            new SelectListItem { Text = "Select", Value = "0" }
        });
            var taskCategories = await _projectTaskCategoryMappingService.GetAllMappingsAsync(projectId, isActive: true);

            var items = new List<SelectListItem>
    {
        new SelectListItem { Text = "Select", Value = "0" }
    };

            foreach (var mapping in taskCategories)
            {
                var category = await _taskCategoryService.GetTaskCategoryByIdAsync(mapping.TaskCategoryId);
                if (category != null)
                {
                    items.Add(new SelectListItem
                    {
                        Text = category.DisplayName,
                        Value = category.Id.ToString()
                    });
                }
            }
            return Json(items);
        }

        [HttpGet]
        public virtual async Task<IActionResult> CreateBugTask(int parentTaskId)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();

            if (!await _customerService.IsRegisteredAsync(customer))
                return Challenge();
            ProjectTaskModel projectTaskModel = new ProjectTaskModel();

            if (customer != null)
            {
                var employeeByCustomer = await _employeeService.GetEmployeeByCustomerIdAsync(customer.Id);
                if (employeeByCustomer != null)
                {
                    projectTaskModel.EmployeeId = employeeByCustomer.Id;
                }
            }
            var model = await _projectTaskModelFactory.PrepareProjectTaskModelByEmployeeAsync(projectTaskModel, null);
            ViewBag.RefreshPage = false;
            var parentTask = await _projectTaskService.GetProjectTasksWithoutCacheByIdAsync(parentTaskId);
            model.IsSync = true;
            model.ParentTaskId = parentTaskId;
            model.DueDate = parentTask != null ? parentTask.DueDate : null;
            model.EstimationTimeHHMM = await _timeSheetsService.ConvertToHHMMFormat(parentTask != null ? parentTask.EstimatedTime : 0);

            return View("/Themes/DefaultClean/Views/Extension/ProjectTasks/CreateBugTask.cshtml", model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> InsertBugTask(string taskTitle, string description, string estimatedTimeHHMM, DateTime? DueDate, int parentTaskId, bool isSync)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();

            if (!await _customerService.IsRegisteredAsync(customer))
                return Challenge();

            if (string.IsNullOrWhiteSpace(taskTitle))
            {
                return Json(new
                {
                    success = false,
                    message = "Please Enter Task Title."
                });
            }
            if (string.IsNullOrWhiteSpace(description))
            {
                return Json(new
                {
                    success = false,
                    message = "Please Enter Description."
                });
            }
            if (DueDate == null)
            {
                return Json(new
                {
                    success = false,
                    message = "Please Select DueDate."
                });
            }
            if (ModelState.IsValid)
            {
                var parentTask = await _projectTaskService.GetProjectTasksWithoutCacheByIdAsync(parentTaskId);
                if (parentTask == null)
                    return Json(new
                    {
                        success = false,
                        message = "Parnet Task is not found"
                    });

                var existingTask = await _projectTaskService.GetProjectTaskByTitleAndProjectAsync(taskTitle, parentTask.ProjectId);
                if (existingTask != null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "A task with the same title already exists for this project."
                    });
                }
                if (string.IsNullOrWhiteSpace(estimatedTimeHHMM) ||
     !System.Text.RegularExpressions.Regex.IsMatch(estimatedTimeHHMM, @"^\d+:[0-5][0-9]$"))
                {
                    return Json(new
                    {
                        success = false,
                        message = "Invalid time format. Please use HH:MM format."
                    });
                }
                decimal estimatedTime = await _timeSheetsService.ConvertHHMMToDecimal(estimatedTimeHHMM);
                if (estimatedTime <= 0)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Please Enter Valid Estimation"
                    });
                }

                var defaultStatus = await _workflowStatusService.GetDefaultStatusIdByWorkflowId(parentTask.ProcessWorkflowId);

                var projectTask = new ProjectTask
                {
                    TaskTitle = taskTitle,
                    QualityComments = null,
                    EstimatedTime = estimatedTime,
                    ProjectId = parentTask.ProjectId,
                    Description = description,
                    StatusId = defaultStatus,
                    AssignedTo = parentTask.AssignedTo,
                    DeveloperId = parentTask.DeveloperId,
                    DueDate = DueDate,
                    Tasktypeid = 3,
                    ProcessWorkflowId = parentTask.ProcessWorkflowId,
                    TaskCategoryId = parentTask.TaskCategoryId,
                    ParentTaskId = parentTaskId,
                    IsSync = isSync,
                    CreatedOnUtc = await _dateTimeHelper.GetUTCAsync()
                };
                await _projectTaskService.InsertProjectTaskAsync(projectTask);
                return Json(new { success = true, message = "Project task added successfully!" });
            }
            var errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList();
            return Json(new { success = false, errors });
        }

        public async Task<IActionResult> GetTaskAlertLogs(int taskId)
        {
            var pagedLogs = await _taskAlertService.GetAllTaskAlertLogsAsync(taskId: taskId);

            var ist = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
            var model = new List<TaskAlertLogRowModel>();

            foreach (var log in pagedLogs)
            {
                var employee = await _employeeService.GetEmployeeByIdAsync(log.EmployeeId);
                var reason = log.ReasonId > 0
                    ? await _taskAlertService.GetTaskAlertReasonByIdAsync(log.ReasonId)
                    : null;
                var alertConfig = log.AlertId > 0
                    ? await _taskAlertService.GetTaskAlertConfigurationByIdAsync(log.AlertId)
                    : null;

                model.Add(new TaskAlertLogRowModel
                {
                    FollowUpOn = TimeZoneInfo.ConvertTimeFromUtc(log.CreatedOnUtc, ist),
                    EmployeeName = employee?.FirstName + " " + employee?.LastName,
                    AlertPercentage = (int)(alertConfig?.Percentage ?? 0),
                    OnTrack = log.OnTrack,
                    ReasonName = reason?.Name ?? "",
                    Comment = log.Comment,
                    ETAHours = log.ETAHours,
                    AlertId=log.AlertId,
                    Type = log.AlertId == 0 ? "Manual" : "Auto"
                });
            }
            return PartialView("/Themes/DefaultClean/Views/Extension/ProjectTasks/_TaskAlertLogTable.cshtml", model);
        }
    }
}