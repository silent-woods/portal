using App.Core;
using App.Core.Domain.ProjectTasks;
using App.Core.Domain.TaskAlerts;
using App.Data.Extensions;
using App.Services.Customers;
using App.Services.Employees;
using App.Services.Helpers;
using App.Services.Messages;
using App.Services.ProjectEmployeeMappings;
using App.Services.Projects;
using App.Services.ProjectTasks;
using App.Services.TaskAlerts;
using App.Web.Areas.Admin.Models.TaskAlerts;
using App.Web.Factories.Extensions;
using App.Web.Models.Dashboard;
using DocumentFormat.OpenXml.Bibliography;
using Microsoft.AspNetCore.Mvc;
using Satyanam.Nop.Core.Domains;
using Satyanam.Nop.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace App.Web.Controllers
{
    [AutoValidateAntiforgeryToken]
    public partial class DashboardController : BasePublicController
    {
        #region Fields
        private readonly IFollowUpTaskService _followUpTaskService;
        private readonly IDashboardModelFactory _dashboardModelFactory;
        private readonly IWorkContext _workContext;
        private readonly IEmployeeService _employeeService;
        private readonly ICustomerService _customerService;
        private readonly IProjectTaskService _projectTaskService;
        private readonly ITaskAlertService _taskAlertService;
        private readonly IProjectsService _projectsService;
        private readonly IProjectEmployeeMappingService _projectEmployeeMappingService;
        private readonly ITaskCommentsService _taskCommentsService;
        private readonly IWorkflowStatusService _workflowStatusService;
        private readonly IProcessRulesService _processRulesService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly IProcessWorkflowService _processWorkflowService;
        #endregion

        #region Ctor
        public DashboardController(IFollowUpTaskService followUpTaskService, IDashboardModelFactory dashboardModelFactory, IWorkContext workContext, IEmployeeService employeeService, ICustomerService customerService, ITaskAlertService taskAlertService, IProjectTaskService projectTaskService, IProjectsService projectsService, IProjectEmployeeMappingService projectEmployeeMappingService, ITaskCommentsService taskCommentsService, IWorkflowStatusService workflowStatusService, IProcessRulesService processRulesService, IDateTimeHelper dateTimeHelper, IWorkflowMessageService workflowMessageService, IProcessWorkflowService processWorkflowService)
        {
            _followUpTaskService = followUpTaskService;
            _dashboardModelFactory = dashboardModelFactory;
            _workContext = workContext;
            _employeeService = employeeService;
            _customerService = customerService;
            _taskAlertService = taskAlertService;
            _projectTaskService = projectTaskService;
            _projectsService = projectsService;
            _projectEmployeeMappingService = projectEmployeeMappingService;
            _taskCommentsService = taskCommentsService;
            _workflowStatusService = workflowStatusService;
            _processRulesService = processRulesService;
            _dateTimeHelper = dateTimeHelper;
            _workflowMessageService = workflowMessageService;
            _processWorkflowService = processWorkflowService;
        }

        #endregion

        #region Methods
        public virtual async Task<IActionResult> FollowUp()
        {
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
            var visibleProjectIds =
                await _projectEmployeeMappingService.GetVisibleProjectIdsForDashboardAsync(currEmployeeId);
            var manageableProjectIds =
                await _projectEmployeeMappingService.GetProjectIdsManagedOrCoordinateByEmployeeIdAsync(currEmployeeId);
            var workflows = (await _processWorkflowService.GetAllProcessWorkflowsAsync()).OrderBy(w => w.DisplayOrder).ToList();
            var defaultWorkflowId = workflows.Any()
                ? workflows.First().Id
                : 0;
            var model = await _dashboardModelFactory.PrepareFollowUpDashboardModelAsync(
                statusType: 2,
                currEmployeeId: currEmployeeId,
                visibleProjectIds: visibleProjectIds,
                managedProjectIds: manageableProjectIds,
                processWorkflow: defaultWorkflowId);

            return View("/Themes/DefaultClean/Views/Extension/Dashboard/FollowUp.cshtml", model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> SaveFollowUp(int id, int durationMinutes, string comment, bool isCompleted)
        {
            if (id == 0)
                return Json(new { success = false, message = "Invalid request" });
            try
            {
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
                var follow = await _followUpTaskService.GetFollowUpTaskByIdAsync(id);
                if (follow == null)
                    return Json(new { success = false, message = "Followup not found" });
                follow.LastFollowupDateTime = DateTime.UtcNow;
                if (!isCompleted && durationMinutes > 0)
                {
                    var officeTimeZone =
                        TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");

                    DateTime officeNow =
                        TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, officeTimeZone);

                    DateTime adjustedOfficeTime =
                       _followUpTaskService.AdjustToOfficeHours(officeNow, durationMinutes, officeTimeZone);

                    follow.NextFollowupDateTime =
                        TimeZoneInfo.ConvertTimeToUtc(adjustedOfficeTime, officeTimeZone);
                }
                follow.IsCompleted = isCompleted;
                follow.LastComment = comment;
                follow.ReviewerId = currEmployeeId;
                follow.AlertId = 0;

                var task = await _projectTaskService.GetProjectTasksWithoutCacheByIdAsync(follow.TaskId);
                if (task == null)
                    return Json(new { success = false, message = "Task not found" });

                var managedProjectIds = await _projectEmployeeMappingService.GetProjectIdsManagedOrCoordinateByEmployeeIdAsync(currEmployeeId);
                bool isAllowed =
                    managedProjectIds.Contains(task.ProjectId);
                if (!isAllowed)
                {
                    return Json(new { success = false, message = "You are not allowed to take follow-up for this task." });
                }

                await _followUpTaskService.UpdateFollowUpTaskAsync(follow);

                TaskAlertLog taskalertlog = new TaskAlertLog();
                taskalertlog.Comment = comment;
                taskalertlog.CreatedOnUtc = follow.LastFollowupDateTime.Value;
                taskalertlog.NextFollowupDateTime = follow.NextFollowupDateTime !=null ?  follow.NextFollowupDateTime.Value :null;
                taskalertlog.ReviewerId = currEmployeeId;
                taskalertlog.TaskId = follow.TaskId;
                taskalertlog.EmployeeId = task.AssignedTo;
                taskalertlog.FollowUpTaskId = follow.Id;
                taskalertlog.AlertId = 0;
                await _taskAlertService.InsertTaskAlertLogAsync(taskalertlog);

                if (!string.IsNullOrWhiteSpace(comment))
                {
                    TaskComments taskComment = new TaskComments();

                    taskComment.EmployeeId = currEmployeeId;
                    taskComment.StatusId = task.StatusId;
                    taskComment.TaskId = task.Id;
                    taskComment.CreatedOn = await _dateTimeHelper.GetUTCAsync();

                    var assignTo = await _employeeService.GetEmployeeByIdAsync(task.AssignedTo);

                    var assignedUserName = assignTo != null
                        ? $"{assignTo.FirstName} {assignTo.LastName}".Trim()
                        : "";

                    taskComment.Description = $"@<{assignedUserName}> {comment}";

                    await _taskCommentsService.InsertTaskCommentsAsync(taskComment);
                    await _workflowMessageService.SendEmployeeMentionMessageAsync((await _workContext.GetWorkingLanguageAsync()).Id, taskComment.EmployeeId, taskComment.TaskId, taskComment.Description);
                }
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        public virtual async Task<IActionResult> ReloadFollowupTables()
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            if (!await _customerService.IsRegisteredAsync(customer))
                return Challenge();

            var emp = await _employeeService.GetEmployeeByCustomerIdAsync(customer.Id);
            var currEmployeeId = emp?.Id ?? 0;
            var visibleProjectIds =
                await _projectEmployeeMappingService.GetVisibleProjectIdsForDashboardAsync(currEmployeeId);
            var managedProjectIds =
                await _projectEmployeeMappingService.GetProjectIdsManagedOrCoordinateByEmployeeIdAsync(currEmployeeId);
            var model = await _dashboardModelFactory.PrepareFollowUpDashboardModelAsync(
                statusType: 2,
                currEmployeeId: currEmployeeId,
                visibleProjectIds: visibleProjectIds,
                managedProjectIds: managedProjectIds
            );

            return PartialView(
                "/Themes/DefaultClean/Views/Extension/Dashboard/_FollowupTables.cshtml",
                model
            );
        }
        [HttpPost]
        public virtual async Task<IActionResult> SearchFollowup(string taskName,string projectIds, string employeeIds, bool showOnlyNotTrack, string sourceType, DateTime? from = null, DateTime? to = null,int percentageFilter = 0,int processWorkflow = 0,int statusId =0)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            if (!await _customerService.IsRegisteredAsync(customer))
                return Challenge();

            var currEmployee = await _employeeService.GetEmployeeByCustomerIdAsync(customer.Id);
            var currEmployeeId = currEmployee?.Id ?? 0;
            var visibleProjectIds =await _projectEmployeeMappingService.GetVisibleProjectIdsForDashboardAsync(currEmployeeId);
            var managedProjectIds =
                await _projectEmployeeMappingService.GetProjectIdsManagedOrCoordinateByEmployeeIdAsync(currEmployeeId);
            var projectIdList = string.IsNullOrEmpty(projectIds)? new List<int>(): projectIds.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList();
            var employeeIdList = string.IsNullOrEmpty(employeeIds) ? new List<int>(): employeeIds.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList();
            var model = await _dashboardModelFactory.PrepareFollowUpDashboardModelAsync(
                taskName: taskName,
                statusType: 2,
                projectIds: projectIdList,
                employeeIds: employeeIdList,
                currEmployeeId: currEmployeeId,
                visibleProjectIds: visibleProjectIds,
                managedProjectIds: managedProjectIds,
                showOnlyNotOnTrack:showOnlyNotTrack,
                sourceType: sourceType,
                from :from,
                to:to,
                percentageFilter: percentageFilter,
                processWorkflow: processWorkflow,
                statusId: statusId
            );

            return PartialView(
                "/Themes/DefaultClean/Views/Extension/Dashboard/_FollowupTables.cshtml",
                model
            );
        }
        public async Task<IActionResult> GetFollowUpSubGrid(int followUpTaskId)
        {
            var logs = await _taskAlertService.GetAllTaskAlertLogsAsync(followUpTaskid:followUpTaskId);
            var istTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
            var model = await logs.SelectAwait(async x =>
            {
                var employee = await _employeeService.GetEmployeeByIdAsync(x.EmployeeId);
                var reviewer = x.ReviewerId !=0 ?(await _employeeService.GetEmployeeByIdAsync(x.ReviewerId)):null;
                var reason = x.ReasonId != 0 ? (await _taskAlertService.GetTaskAlertReasonByIdAsync(x.ReasonId)):null;
                bool isAutoFollowup = x.AlertId == 0 ? false : true;
                var alertConfig = x.AlertId != 0 ? (await _taskAlertService.GetTaskAlertConfigurationByIdAsync(x.AlertId)):null;

                return new FollowUpSubGridModel
                {
                    Id = x.Id,
                    EmployeeId = x.EmployeeId,
                    EmployeeName = employee != null ? $"{employee.FirstName} {employee.LastName}" : "",
                    AlertId = x.AlertId,
                    ReasonName = reason != null ? reason.Name : "",
                    Comment = x.Comment,
                    OnTrack = x.OnTrack,
                    ETAHours = x.ETAHours,
                    FollowUpTaskId = x.FollowUpTaskId,
                    IsAutomatic = isAutoFollowup,
                    ReviewerId = x.ReviewerId,
                    AlertType = alertConfig !=null ? Enum.GetName(typeof(TaskAlertsEnum), alertConfig.TaskAlertTypeId):"",
                    AlertPercentage = alertConfig != null
    ? Math.Round(alertConfig.Percentage).ToString()
    : "",
                    ReviewerName = reviewer != null ? $"{reviewer.FirstName} {reviewer.LastName}" : "",
                    NextFollowupDateTime = x.NextFollowupDateTime != null ? TimeZoneInfo.ConvertTimeFromUtc(x.NextFollowupDateTime.Value, istTimeZone) : null,
                    CreatedOnUtc =TimeZoneInfo.ConvertTimeFromUtc(x.CreatedOnUtc, istTimeZone),
                };
            }).ToListAsync();

            return PartialView("/Themes/DefaultClean/Views/Extension/Dashboard/_FollowUpSubGrid.cshtml", model);
        }

        public virtual async Task<IActionResult> CompletedFollowUp(int page = 1, int pageSize = 1)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            if (!await _customerService.IsRegisteredAsync(customer))
                return Challenge();

            var emp = await _employeeService.GetEmployeeByCustomerIdAsync(customer.Id);
            var currEmployeeId = emp?.Id ?? 0;

            var visibleProjectIds = await _projectEmployeeMappingService.GetVisibleProjectIdsForDashboardAsync(currEmployeeId);
            var managedProjectIds =
                await _projectEmployeeMappingService.GetProjectIdsManagedOrCoordinateByEmployeeIdAsync(currEmployeeId);

            var model = await _dashboardModelFactory.PrepareFollowUpDashboardModelAsync(
                currEmployeeId: currEmployeeId,
                statusType: 1,
                page: page,
                pageSize: pageSize,
                visibleProjectIds: visibleProjectIds,
                managedProjectIds: managedProjectIds
            );
            return PartialView("/Themes/DefaultClean/Views/Extension/Dashboard/_CompletedTableBody.cshtml", model);
        }

        public async Task<IActionResult> CodeReview()
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            if (!await _customerService.IsRegisteredAsync(customer))
                return Challenge();

            var emp = await _employeeService.GetEmployeeByCustomerIdAsync(customer.Id);

            var model = await _dashboardModelFactory
                .PrepareCodeReviewDashboardModelAsync(
                    currEmployeeId: emp?.Id ?? 0
                );

            return View("/Themes/DefaultClean/Views/Extension/Dashboard/CodeReview.cshtml",model);
        }


        [HttpGet]
        public async Task<IActionResult> SearchCodeReviewTasks(int projectId = 0,int employeeId = 0,string taskName = null,int statusId= 0)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            if (!await _customerService.IsRegisteredAsync(customer))
                return Challenge();

            var emp = await _employeeService.GetEmployeeByCustomerIdAsync(customer.Id);
            var model = await _dashboardModelFactory
                .PrepareCodeReviewDashboardModelAsync(
                    currEmployeeId: emp?.Id ?? 0,
                    projectId: projectId,
                    employeeId: employeeId,
                    taskName: taskName,
                    statusId: statusId
                );

            return PartialView("/Themes/DefaultClean/Views/Extension/Dashboard/_CodeReviewTaskList.cshtml", model);
        }


        public async Task<IActionResult> GetLastTaskComment(int taskId)
        {
            if (taskId <= 0)
                return Content("No comment found");
            var comment = await _taskCommentsService.GetLastCommentByTaskIdAsync(taskId);
            if (comment == null)
                return Content("No comment found");
            return PartialView(
                "/Themes/DefaultClean/Views/Extension/Dashboard/_LastTaskComment.cshtml",
                comment
            );
        }

        public async Task<IActionResult> GetReviewStatuses(int taskId,int processWorkflowId,int currentStatusId)
        {
            var statusIds = await _processRulesService
                .GetPossibleStatusIds(processWorkflowId, currentStatusId);

            var statuses = await _workflowStatusService.GetWorkflowStatusByIdsAsync(statusIds.ToArray());
            return Json(statuses.Select(s => new {
                id = s.Id,
                text = s.StatusName
            }));
        }
        [HttpPost]
        public async Task<IActionResult> SaveReview(int taskId,int statusId,string comment)
        {
            var projectTask = await _projectTaskService.GetProjectTasksWithoutCacheByIdAsync(taskId);
            if (projectTask != null)
            {
                projectTask.StatusId = statusId;
                
                await _projectTaskService.UpdateProjectTaskAsync(projectTask);
            } 
            if (!string.IsNullOrWhiteSpace(comment))
            {
                TaskComments taskComment = new TaskComments();
                taskComment.StatusId = statusId;
                taskComment.UpdatedOn = DateTime.UtcNow;
                taskComment.CreatedOn = DateTime.UtcNow;
                taskComment.TaskId = taskId;
                taskComment.Description = comment;

                var customer = await _workContext.GetCurrentCustomerAsync();
                if (!await _customerService.IsRegisteredAsync(customer))
                    return Challenge();

                var emp = await _employeeService.GetEmployeeByCustomerIdAsync(customer.Id);

                taskComment.EmployeeId = emp != null ? emp.Id : 0;
                await _taskCommentsService.InsertTaskCommentsAsync(taskComment);
            }
            return Ok();
        }

        public async Task<IActionResult> ReadyToTest(int projectId = 0,int employeeId = 0,string taskName = null)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            if (!await _customerService.IsRegisteredAsync(customer))
                return Challenge();

            var emp = await _employeeService.GetEmployeeByCustomerIdAsync(customer.Id);
            var model = await _dashboardModelFactory
                .PrepareReadyToTestDashboardModelAsync(
                    emp?.Id ?? 0, projectId, employeeId, taskName
                );

            return View("/Themes/DefaultClean/Views/Extension/Dashboard/ReadyToTest.cshtml", model);
        }

        [HttpGet]
        public async Task<IActionResult> SearchReadyToTestTasks(int projectId = 0,int employeeId = 0,string taskName = null,int statusId = 0)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            if (!await _customerService.IsRegisteredAsync(customer))
                return Challenge();

            var emp = await _employeeService.GetEmployeeByCustomerIdAsync(customer.Id);
            var model = await _dashboardModelFactory
                .PrepareReadyToTestDashboardModelAsync(
                    currEmployeeId: emp?.Id ?? 0,
                    projectId: projectId,
                    employeeId: employeeId,
                    taskName: taskName,
                    statusId: statusId
                );
            return PartialView("/Themes/DefaultClean/Views/Extension/Dashboard/_ReadyToTestTaskList.cshtml",model);
        }
        [HttpGet]
        public async Task<IActionResult> SearchOverdueTasks(int projectId = 0,int employeeId = 0,string taskName = null,int statusId = 0)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            if (!await _customerService.IsRegisteredAsync(customer))
                return Challenge();

            var emp = await _employeeService.GetEmployeeByCustomerIdAsync(customer.Id);
            var model = await _dashboardModelFactory
                .PrepareOverdueDashboardModelAsync(
                    currEmployeeId: emp?.Id ?? 0,
                    projectId: projectId,
                    employeeId: employeeId,
                    taskName: taskName,
                    statusId: statusId
                );

            return PartialView("/Themes/DefaultClean/Views/Extension/Dashboard/_OverdueTaskList.cshtml",model);
        }

        public async Task<IActionResult> Overdue(int projectId = 0, int employeeId = 0, string taskName = null)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            if (!await _customerService.IsRegisteredAsync(customer))
                return Challenge();

            var emp = await _employeeService.GetEmployeeByCustomerIdAsync(customer.Id);
            var model = await _dashboardModelFactory
                .PrepareOverdueDashboardModelAsync(
                    currEmployeeId: emp.Id,
                    projectId: projectId,
                    employeeId: employeeId,
                    taskName: taskName);

            return View("~/Themes/DefaultClean/Views/Extension/Dashboard/Overdue.cshtml", model);
        }
        #endregion
    }
}