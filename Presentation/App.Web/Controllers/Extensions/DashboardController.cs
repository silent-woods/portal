using App.Core;
using App.Core.Domain.ProjectTasks;
using App.Core.Domain.TaskAlerts;
using App.Data.Extensions;
using App.Services.Customers;
using App.Services.Employees;
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
        #endregion

        #region Ctor
        public DashboardController(IFollowUpTaskService followUpTaskService, IDashboardModelFactory dashboardModelFactory, IWorkContext workContext, IEmployeeService employeeService, ICustomerService customerService, ITaskAlertService taskAlertService, IProjectTaskService projectTaskService, IProjectsService projectsService, IProjectEmployeeMappingService projectEmployeeMappingService)
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
            var model = await _dashboardModelFactory.PrepareFollowUpDashboardModelAsync(
                statusType: 2,
                currEmployeeId: currEmployeeId,
                visibleProjectIds: visibleProjectIds,
                managedProjectIds: manageableProjectIds);
            return View("/Themes/DefaultClean/Views/Extension/Dashboard/FollowUp.cshtml", model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> SaveFollowUp(int id, DateTime? nextDate, string comment, bool isCompleted)
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
                if (nextDate.HasValue)
                {
                    var istZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
                    var istDateTime = DateTime.SpecifyKind(nextDate.Value, DateTimeKind.Unspecified);
                    follow.NextFollowupDateTime =
                        TimeZoneInfo.ConvertTimeToUtc(istDateTime, istZone);
                }
                follow.IsCompleted = isCompleted;
                follow.LastComment = comment;
                follow.ReviewerId = currEmployeeId;
                follow.AlertId = 0;

                var task = await _projectTaskService.GetProjectTasksWithoutCacheByIdAsync(follow.TaskId);
                if (task == null)
                    return Json(new { success = false, message = "Task not found" });

                var managedProjectIds =
await _projectEmployeeMappingService.GetProjectIdsManagedOrCoordinateByEmployeeIdAsync(currEmployeeId);
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
        public virtual async Task<IActionResult> SearchFollowup(string taskName, int projectId, int employeeId, bool showOnlyNotTrack, string sourceType, DateTime? from = null, DateTime? to = null)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            if (!await _customerService.IsRegisteredAsync(customer))
                return Challenge();

            var currEmployee = await _employeeService.GetEmployeeByCustomerIdAsync(customer.Id);
            var currEmployeeId = currEmployee?.Id ?? 0;
            var visibleProjectIds =
      await _projectEmployeeMappingService.GetVisibleProjectIdsForDashboardAsync(currEmployeeId);
            var managedProjectIds =
                await _projectEmployeeMappingService.GetProjectIdsManagedOrCoordinateByEmployeeIdAsync(currEmployeeId);
            var model = await _dashboardModelFactory.PrepareFollowUpDashboardModelAsync(
                taskName: taskName,
                statusType: 2,
                projectId: projectId,
                employeeId: employeeId,
                currEmployeeId: currEmployeeId,
                visibleProjectIds: visibleProjectIds,
                managedProjectIds: managedProjectIds,
                showOnlyNotOnTrack:showOnlyNotTrack,
                sourceType: sourceType,
                from :from,
                to:to
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

            var visibleProjectIds =
     await _projectEmployeeMappingService.GetVisibleProjectIdsForDashboardAsync(currEmployeeId);
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
        #endregion
    }
}