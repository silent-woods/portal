using App.Core;
using App.Core.Domain.Activities;
using App.Core.Domain.Extension.TimeSheets;
using App.Core.Domain.ProjectTasks;
using App.Core.Domain.TimeSheets;
using App.Services.Activities;
using App.Services.Customers;
using App.Services.EmployeeAttendances;
using App.Services.Employees;
using App.Services.Helpers;
using App.Services.Localization;
using App.Services.Messages;
using App.Services.ProjectEmployeeMappings;
using App.Services.Projects;
using App.Services.ProjectTasks;
using App.Services.Security;
using App.Services.TimeSheets;
using App.Web.Areas.Admin.Models.Extension.ProjectTasks;
using App.Web.Factories.Extensions;
using App.Web.Models.Extensions.TimeSheets;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Satyanam.Nop.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace App.Web.Controllers.Extensions
{
    public partial class TimeSheetController : Controller
    {

        #region Fields
        private readonly IPermissionService _permissionService;
        private readonly ITimeSheetModelFactory _timeSheetModelFactory;
        private readonly ITimeSheetsService _timeSheetsService;
        private readonly INotificationService _notificationService;
        private readonly ILocalizationService _localizationService;
        private readonly IProjectsService _projectsService;
        private readonly IProjectTaskService _projectTaskService;
        private readonly Areas.Admin.Factories.Extension.IProjectTaskModelFactory _projectTaskModelFactory;
        private readonly IEmployeeService _employeeService;
        private readonly MonthlyReportSetting _monthlyReportSettings;
        private readonly IWorkContext _workContext;
        private readonly ICustomerService _customerService;
        private readonly IProjectEmployeeMappingService _projectEmployeeMappingService;
        private readonly IEmployeeAttendanceService _employeeAttendanceService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IActivityService _activityService;
        private readonly IProcessWorkflowService _processWorkflowService;
        private readonly IWorkflowStatusService _workflowStatusService;
        #endregion

        #region Ctor

        public TimeSheetController(IPermissionService permissionService,
            ITimeSheetModelFactory timeSheetModelFactory,
            ITimeSheetsService timeSheetsService,
            INotificationService notificationService,
            ILocalizationService localizationService, IProjectsService projectsService, IProjectTaskService projectTaskService,
            Areas.Admin.Factories.Extension.IProjectTaskModelFactory projectTaskModelFactory,
            IEmployeeService employeeService, MonthlyReportSetting monthlyReportSettings,
            IWorkContext workContext,
            ICustomerService customerService,
            IProjectEmployeeMappingService projectEmployeeMappingService,
            IEmployeeAttendanceService employeeAttendanceService,
            IDateTimeHelper dateTimeHelper,
            IActivityService activityService,
            IProcessWorkflowService processWorkflowService,
            IWorkflowStatusService workflowStatusService
            )
        {
            _permissionService = permissionService;
            _timeSheetModelFactory = timeSheetModelFactory;
            _timeSheetsService = timeSheetsService;
            _notificationService = notificationService;
            _localizationService = localizationService;
            _projectsService = projectsService;
            _projectTaskService = projectTaskService;
            _projectTaskModelFactory = projectTaskModelFactory;
            _employeeService = employeeService;
            _monthlyReportSettings = monthlyReportSettings;
            _workContext = workContext;
            _customerService = customerService;
            _projectEmployeeMappingService = projectEmployeeMappingService;
            _employeeAttendanceService = employeeAttendanceService;
            _dateTimeHelper = dateTimeHelper;
            _activityService = activityService;
            _processWorkflowService = processWorkflowService;
            _workflowStatusService = workflowStatusService;
        }

        #endregion

        public virtual async Task<IActionResult> List()
        {
            var customer = await _workContext.GetCurrentCustomerAsync();

            if (!await _customerService.IsRegisteredAsync(customer))
                return Challenge();
            TimeSheetSearchModel timesheetmodel = new TimeSheetSearchModel();

            if (customer != null)
            {
                var employeeByCustomer = await _employeeService.GetEmployeeByCustomerIdAsync(customer.Id);
                if (employeeByCustomer != null)
                {
                    timesheetmodel.EmployeeId = employeeByCustomer.Id;
                }
            }
            var model = await _timeSheetModelFactory.PrepareTimeSheetSearchModelAsync(timesheetmodel);
            var employee = await _employeeService.GetEmployeeByIdAsync(model.EmployeeId);
            if (employee != null)
                model.EmployeeName = employee.FirstName + " " + employee.LastName;
            model.From = await _dateTimeHelper.GetUTCAsync();
            return View("/Themes/DefaultClean/Views/Extension/TimeSheets/TimeSheetList.cshtml", model);
        }

      public virtual async Task<IActionResult> TimesheetList(
      string projectIds,
      string employeeIds,
      string taskName,
      DateTime? fromDate,
      DateTime? toDate,
      int BillableType,
      int pageIndex = 0,
      int pageSize = 50
       )
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
            if (projectIds != null)
                projectIdsList = projectIds.Split(',').Select(int.Parse).ToList();

            var selectedEmployeeIds = new List<int>();
            if (employeeIds != null)
            {
                selectedEmployeeIds = employeeIds.Split(',').Select(int.Parse).ToList();
            }
            else
            {
                IList<int> juniorIds = await _projectEmployeeMappingService.GetJuniorsIdsByEmployeeIdAsync(currCustomer);
                selectedEmployeeIds = juniorIds.ToList();
            }
            var timesheets = await _timeSheetsService.GetAllTimeSheetAsync(
                employeeIds: selectedEmployeeIds,
                projectIds: projectIdsList,
                taskName: taskName,
                from: fromDate,
                to: toDate,
                SelectedBillable: BillableType,
                showHidden: true,
                pageIndex: pageIndex,
                pageSize: int.MaxValue
            );
            var result = new List<TimeSheetModel>();

            foreach (var timesheet in timesheets)
            {
                if (timesheet != null)
                {
                    var model = new TimeSheetModel();
                    model.EmployeeId = timesheet.EmployeeId;
                    model.ProjectId = timesheet.ProjectId;
                    model.TaskId = timesheet.TaskId;
                    model.SpentDate = timesheet.SpentDate;
                    model.SpentTime = await _timeSheetsService.ConvertSpentTimeAsync(timesheet.SpentHours, timesheet.SpentMinutes);
                    var activity = await _activityService.GetActivityByIdAsync(timesheet.ActivityId);
                    if (activity != null)
                        model.ActivityName = activity.ActivityName;
                    model.SpentDates = timesheet.SpentDate.ToString("d-MMM-yyyy");
                    model.Billable = timesheet.Billable;
                    var project = await _projectsService.GetProjectsByIdAsync(timesheet.ProjectId);
                    if (project != null)
                        model.ProjectName = project.ProjectTitle;
                    var task = await _projectTaskService.GetProjectTasksByIdAsync(timesheet.TaskId);
                    if (task != null)
                    {
                        model.TaskName = task.TaskTitle;
                        model.TotalSpent = await _timeSheetsService.ConvertSpentTimeAsync(task.SpentHours, task.SpentMinutes);
                        model.EstimatedHoursHHMM = await _timeSheetsService.ConvertToHHMMFormat(task.EstimatedTime);
                    }
                    var employee = await _employeeService.GetEmployeeByIdAsync(timesheet.EmployeeId);
                    if (employee != null)
                        model.EmployeeName = employee.FirstName + " " + employee.LastName;
                    if (timesheet.StartTime != null)
                        timesheet.StartTime = await _dateTimeHelper.ConvertToUserTimeAsync(timesheet.StartTime.Value, DateTimeKind.Utc);
                    if (timesheet.EndTime != null)
                        timesheet.EndTime = await _dateTimeHelper.ConvertToUserTimeAsync(timesheet.EndTime.Value, DateTimeKind.Utc);
                    if (timesheet.StartTime != null && timesheet.EndTime != null)
                    {
                        var istTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
                        if (timesheet.StartTime != null)
                            timesheet.StartTime = TimeZoneInfo.ConvertTimeFromUtc(timesheet.StartTime.Value, istTimeZone);
                        if (timesheet.EndTime != null)
                            timesheet.EndTime = TimeZoneInfo.ConvertTimeFromUtc(timesheet.EndTime.Value, istTimeZone);
                        model.Time = timesheet.StartTime.Value.ToString("h:mmtt") + "-" + timesheet.EndTime.Value.ToString("h:mmtt");
                    }
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

        public virtual async Task<IActionResult> UpdateTimeSheet()
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            if (!await _customerService.IsRegisteredAsync(customer))
                return Challenge();

            TimeSheetModel timesheetmodel = new TimeSheetModel();

            if (customer != null)
            {
                var employeeByCustomer = await _employeeService.GetEmployeeByCustomerIdAsync(customer.Id);
                if (employeeByCustomer != null)
                {
                    timesheetmodel.EmployeeId = employeeByCustomer.Id;
                }
            }
            var model = await _timeSheetModelFactory.PrepareTimeSheetModelAsync(timesheetmodel, null);
            model.SpentDate = await _dateTimeHelper.GetUTCAsync();
            return View("/Themes/DefaultClean/Views/Extension/TimeSheets/UpdateTimeSheet.cshtml", model);
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

        public virtual async Task<IActionResult> GetEstimatedTimeByTask(int taskId)
        {
            int employeeId = 0;
            var customer = await _workContext.GetCurrentCustomerAsync();
            if (!await _customerService.IsRegisteredAsync(customer))
                return Challenge();
            if (customer != null)
            {
                var employee = await _employeeService.GetEmployeeByCustomerIdAsync(customer.Id);
                if (employee != null)
                    employeeId = employee.Id;
            }
            var isQaLoggedIn = await _timeSheetsService.IsQALoggedIn(employeeId);
            int spentHours = 0;
            int spentMinutes = 0;
            if (isQaLoggedIn)
            {
                (spentHours, spentMinutes) = await _timeSheetsService.GetQATimeByTaskId(taskId);
            }
            else
            {
                (spentHours, spentMinutes) = await _timeSheetsService.GetDevelopmentTimeByTaskId(taskId);
            }
            var task = await _projectTaskService.GetProjectTasksByIdAsync(taskId);
            if (task != null)
            {
                var estimationTime = await _timeSheetsService.ConvertToHHMMFormat(task.EstimatedTime);
                var totalSpent = await _timeSheetsService.ConvertSpentTimeAsync(spentHours, spentMinutes);
                return Json(new { EstimatedTime = estimationTime, TotalSpent = totalSpent });
            }
            return Json(new { EstimatedTime = "", TotalSpent = "" });
        }

        [HttpGet]
        public async Task<IActionResult> GetPreviousEntries(string spentDate)
        {
            if (string.IsNullOrEmpty(spentDate))
            {
                return Json(new List<TimeSheetRowModel>()); 
            }
            if (!DateTime.TryParse(spentDate, out var date))
            {
                return BadRequest("Invalid date format");
            }
            int employeeId = 0;
            var customer = await _workContext.GetCurrentCustomerAsync();
            if (!await _customerService.IsRegisteredAsync(customer))
                return Challenge();
            if (customer != null)
            {
                var employee = await _employeeService.GetEmployeeByCustomerIdAsync(customer.Id);
                if (employee != null)
                    employeeId = employee.Id;
            }
            ViewBag.EmployeeId = employeeId;
            var projectTasks = await _projectTaskService.GetAllProjectTasksAsync(); 
            var projects = await _projectsService.GetAllProjectsListAsync();
            var entries = await _timeSheetsService.GetTimeSheetByEmpAndSpentDate(employeeId, date);
            ViewBag.EmployeeId = employeeId;
            var timeSheetRows = entries
                .Where(pt => projectTasks.Any(ptt => ptt.Id == pt.TaskId && ptt.ProjectId == pt.ProjectId))
                .Where(pt => projects.Any(p => p.Id == pt.ProjectId))
                .Select(pt => new TimeSheetRowModel
                {
                    Id = pt.Id,
                    ProjectId = pt.ProjectId,
                    TaskId = pt.TaskId,
                    SpentHours = pt.SpentHours,
                    SpentMinutes = pt.SpentMinutes,
                    Billable = pt.Billable,
                    ActivityId = pt.ActivityId,
                    EmployeeId = pt.EmployeeId
                })
            .ToList();
            var isQaLoggedIn = await _timeSheetsService.IsQALoggedIn(employeeId);
            for (int i = 0; i < timeSheetRows.Count; i++)
            {
                var task = await _projectTaskService.GetProjectTasksByIdAsync(timeSheetRows[i].TaskId);
                {
                    int spentHoursShow = 0;
                    int spentMinutesShow = 0;
                    if (isQaLoggedIn)
                    {
                        (spentHoursShow, spentMinutesShow) = await _timeSheetsService.GetQATimeByTaskId(timeSheetRows[i].TaskId);
                    }
                    else
                    {
                        (spentHoursShow, spentMinutesShow) = await _timeSheetsService.GetDevelopmentTimeByTaskId(timeSheetRows[i].TaskId);
                    }
                    timeSheetRows[i].TotalSpent = await _timeSheetsService.ConvertSpentTimeAsync(spentHoursShow, spentMinutesShow);
                    timeSheetRows[i].EstimatedHoursHHMM = await _timeSheetsService.ConvertToHHMMFormat(task.EstimatedTime);
                }
                var activity = await _activityService.GetActivityByIdAsync(timeSheetRows[i].ActivityId);
                if (activity != null)
                    timeSheetRows[i].ActivityName = activity.ActivityName;
                else
                    timeSheetRows[i].ActivityName = "";
                var time = await _timeSheetsService.ConvertSpentTimeAsync(timeSheetRows[i].SpentHours, timeSheetRows[i].SpentMinutes);
                if (time != null)
                    timeSheetRows[i].SpentTime = time;
            }
            return Json(timeSheetRows);
        }

        [HttpPost]
        public async Task<IActionResult> SaveNewChanges(List<TimeSheetRowModel> timeSheetEntries)
        {
            int employeeId = 0;
            var customer = await _workContext.GetCurrentCustomerAsync();
            if (!await _customerService.IsRegisteredAsync(customer))
                return Challenge();
            if (customer != null)
            {
                var employee = await _employeeService.GetEmployeeByCustomerIdAsync(customer.Id);
                if (employee != null)
                    employeeId = employee.Id;
            }
            var savedEntries = new List<object>();
            foreach (var entry in timeSheetEntries)
            {
                var activityId = 0;
                Activity activity = null;
                (entry.SpentHours, entry.SpentMinutes) = await _timeSheetsService.ConvertSpentTimeAsync(entry.SpentTime);
                if (!string.IsNullOrWhiteSpace(entry.ActivityName))
                {
                    var existingActivities = await _activityService.GetAllActivitiesByActivityNameTaskIdAsync(entry.ActivityName, entry.TaskId);
                    if (existingActivities.Any())
                    {
                        activity = existingActivities.First();
                        (activity.SpentHours, activity.SpentMinutes) = await _timeSheetsService.AddSpentTimeAsync(activity.SpentHours, activity.SpentMinutes, entry.SpentHours, entry.SpentMinutes);
                        activity.UpdateOnUtc = await _dateTimeHelper.GetUTCAsync();
                        await _activityService.UpdateActivityAsync(activity);
                        activityId = activity.Id;
                    }
                    else
                    {
                        activity = new Activity
                        {
                            ActivityName = entry.ActivityName,
                            EmployeeId = employeeId,
                            TaskId = entry.TaskId,
                            SpentHours = entry.SpentHours,
                            SpentMinutes = entry.SpentMinutes,
                            CreateOnUtc = await _dateTimeHelper.GetUTCAsync(),
                            UpdateOnUtc = await _dateTimeHelper.GetUTCAsync()
                        };
                        await _activityService.InsertActivityAsync(activity);
                        activityId = activity.Id;
                    }
                }
                var timeSheet = new TimeSheet
                {
                    EmployeeId = employeeId,
                    SpentDate = entry.SpentDate,
                    ProjectId = entry.ProjectId,
                    TaskId = entry.TaskId,
                    SpentHours = entry.SpentHours,
                    SpentMinutes = entry.SpentMinutes,
                    Billable = entry.Billable,
                    IsManualEntry = true,
                    ActivityId = activityId, 
                    CreateOnUtc = await _dateTimeHelper.GetUTCAsync(),
                    UpdateOnUtc = await _dateTimeHelper.GetUTCAsync()
                };

                await _timeSheetsService.InsertTimeSheetAsync(timeSheet);

                var projectTask = await _projectTaskService.GetProjectTasksByIdAsync(entry.TaskId);
                if (projectTask != null)
                {
                    (projectTask.SpentHours, projectTask.SpentMinutes) = await _timeSheetsService.AddSpentTimeAsync(projectTask.SpentHours, projectTask.SpentMinutes, entry.SpentHours, entry.SpentMinutes);
                    projectTask.DeliveryOnTime = await _timeSheetsService.IsTaskDeliveredOnTimeAsync(projectTask);
                    await _projectTaskService.UpdateProjectTaskAsync(projectTask);
                }

                savedEntries.Add(new
                {
                    newRowId = timeSheet.Id,
                    entry.ProjectId,
                    entry.TaskId,
                    entry.SpentHours,
                    entry.SpentMinutes,
                    entry.Billable,
                    entry.EstimatedHours,
                    entry.TotalSpent,
                    employeeId,
                    ActivityId = activityId
                });
            }
            return Json(new { success = true, message = "Changes saved successfully", savedEntries });
        }

        public async Task<IActionResult> GetRecentEntries(string spentDate)
        {
            if (string.IsNullOrEmpty(spentDate))
            {
                return Json(new List<TimeSheetRowModel>()); 
            }
            if (!DateTime.TryParse(spentDate, out var date))
            {
                return BadRequest("Invalid date format");
            }
            int employeeId = 0;
            var customer = await _workContext.GetCurrentCustomerAsync();
            if (!await _customerService.IsRegisteredAsync(customer))
                return Challenge();
            if (customer != null)
            {
                var employee = await _employeeService.GetEmployeeByCustomerIdAsync(customer.Id);
                if (employee != null)
                    employeeId = employee.Id;
            }
            var entries = await _timeSheetsService.GetTimeSheetByEmpAndSpentDate(employeeId, date);
            if (entries.Count == 0)
            {
                for (int i = 1; i <= 5; i++)
                {
                    var previousDate = date.AddDays(-i);
                    entries = await _timeSheetsService.GetTimeSheetByEmpAndSpentDate(employeeId, previousDate);
                    if (entries.Count > 0)
                    {
                        break;
                    }
                }
            }
            ViewBag.EmployeeId = employeeId;
            var projectTasks = await _projectTaskService.GetAllProjectTasksAsync();
            var projects = await _projectsService.GetAllProjectsListAsync();
            var timeSheetRows = entries
                .Where(pt => projectTasks.Any(ptt => ptt.Id == pt.TaskId && ptt.ProjectId == pt.ProjectId))
                .Where(pt => projects.Any(p => p.Id == pt.ProjectId)) // filter out rows where TaskId does not exist in ProjectTask table
                .Select(pt => new TimeSheetRowModel
                {
                    ProjectId = pt.ProjectId,
                    TaskId = pt.TaskId,
                    SpentHours = pt.SpentHours,
                    Billable = pt.Billable,
                    ActivityId = pt.ActivityId,
                    EmployeeId = pt.EmployeeId
                })
            .ToList();
            var isQaLoggedIn = await _timeSheetsService.IsQALoggedIn(employeeId);
            for (int i = 0; i < timeSheetRows.Count; i++)
            {
                var task = await _projectTaskService.GetProjectTasksByIdAsync(timeSheetRows[i].TaskId);
                {
                    int spentHoursShow = 0;
                    int spentMinutesShow = 0;
                    if (isQaLoggedIn)
                    {
                        (spentHoursShow, spentMinutesShow) = await _timeSheetsService.GetQATimeByTaskId(task.Id);
                    }
                    else
                    {
                        (spentHoursShow, spentMinutesShow) = await _timeSheetsService.GetDevelopmentTimeByTaskId(task.Id);
                    }
                    timeSheetRows[i].TotalSpent = await _timeSheetsService.ConvertSpentTimeAsync(spentHoursShow, spentMinutesShow);
                    timeSheetRows[i].EstimatedHours = task.EstimatedTime;
                }
                var activity = await _activityService.GetActivityByIdAsync(timeSheetRows[i].ActivityId);
                if (activity != null)
                    timeSheetRows[i].ActivityName = activity.ActivityName;
            }
            return Json(timeSheetRows);
        }

        [HttpPost]
        public virtual async Task<IActionResult> UpdateRow(
 int id,
 int projectId,
 int taskId,
 int prevTaskId,
 string activityName,
 string prevActivityName,
 decimal estimatedHours,
 decimal totalSpent,
 string spentTime,
 string prevspentTime,
 bool billable)
        {
            var (spentHours, spentMinutes) = await _timeSheetsService.ConvertSpentTimeAsync(spentTime);
            var (prevSpentHours, prevSpentMinutes) = await _timeSheetsService.ConvertSpentTimeAsync(prevspentTime);
            if (id <= 0 || projectId <= 0 || taskId <= 0 || estimatedHours < 0 || spentHours < 0 || spentMinutes < 0)
            {
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Common.InvalidParameters"));
                return BadRequest();
            }
            TimeSheet timeSheet = await _timeSheetsService.GetTimeSheetByIdAsync(id);
            if (timeSheet == null)
            {
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Catalog.TimeSheet.NotFound"));
                return NotFound();
            }
            timeSheet.EmployeeId = timeSheet.EmployeeId;
            timeSheet.SpentDate = timeSheet.SpentDate;
            var existingRows = await _timeSheetsService.GetTimeSheetByIdAsync(id);
            var existingRow = existingRows;
            if (existingRow != null)
            {
                existingRow.TaskId = taskId;
                existingRow.SpentHours = spentHours;
                existingRow.SpentMinutes = spentMinutes;
                existingRow.Billable = billable;
                existingRow.ProjectId = projectId;
                var oldProjectTask = await _projectTaskService.GetProjectTasksByIdAsync(prevTaskId);
                if (oldProjectTask != null)
                {
                    (oldProjectTask.SpentHours, oldProjectTask.SpentMinutes) = await _timeSheetsService.SubtractSpentTimeAsync(oldProjectTask.SpentHours, oldProjectTask.SpentMinutes, prevSpentHours, prevSpentMinutes);
                    await _projectTaskService.UpdateProjectTaskAsync(oldProjectTask);
                }
                var newProjectTask = await _projectTaskService.GetProjectTasksByIdAsync(taskId);
                if (newProjectTask != null)
                {
                    (newProjectTask.SpentHours, newProjectTask.SpentMinutes) = await _timeSheetsService.AddSpentTimeAsync(newProjectTask.SpentHours, newProjectTask.SpentMinutes, spentHours, spentMinutes);
                    await _projectTaskService.UpdateProjectTaskAsync(newProjectTask);
                }
                await _timeSheetsService.UpdateOrCreateActivityAsync(timeSheet, existingRow, activityName, prevSpentHours, spentHours, prevSpentMinutes, spentMinutes, taskId);
            }
            else
            {
                var newRow = new TimeSheet
                {
                    Id = id,
                    ProjectId = projectId,
                    TaskId = taskId,
                    SpentHours = spentHours,
                    SpentMinutes = spentMinutes,
                    Billable = billable
                };
                await _timeSheetsService.InsertTimeSheetAsync(newRow);
            }
            await _timeSheetsService.UpdateTimeSheetAsync(timeSheet);
            var updatedTask = await _projectTaskService.GetProjectTasksByIdAsync(taskId);
            if (updatedTask != null)
            {
                updatedTask.DeliveryOnTime = await _timeSheetsService.IsTaskDeliveredOnTimeAsync(updatedTask);
                await _projectTaskService.UpdateProjectTaskAsync(updatedTask);
            }

            await _employeeAttendanceService.UpdateEmployeeAttendanceBasedOnTimeSheetAsync(
                timeSheet.SpentDate, timeSheet.EmployeeId, timeSheet.SpentHours, timeSheet.SpentMinutes,
                timeSheet.SpentDate, timeSheet.EmployeeId);

            int employeeId = 0;
            var customer = await _workContext.GetCurrentCustomerAsync();
            if (!await _customerService.IsRegisteredAsync(customer))
                return Challenge();
            if (customer != null)
            {
                var employee = await _employeeService.GetEmployeeByCustomerIdAsync(customer.Id);
                if (employee != null)
                    employeeId = employee.Id;
            }
            var isQaLoggedIn = await _timeSheetsService.IsQALoggedIn(employeeId);
            int spentHoursShow = 0;
            int spentMinutesShow = 0;
            if (isQaLoggedIn)
            {
                (spentHoursShow, spentMinutesShow) = await _timeSheetsService.GetQATimeByTaskId(taskId);
            }
            else
            {
                (spentHoursShow, spentMinutesShow) = await _timeSheetsService.GetDevelopmentTimeByTaskId(taskId);
            }
            var updatedTotalSpent = await _timeSheetsService.ConvertSpentTimeAsync(spentHoursShow, spentMinutesShow);

            return Json(new { success = true, updatedTotalSpent = updatedTotalSpent });
        }

        [HttpPost]
        public virtual async Task<IActionResult> DeleteRow(int id)
        {
            var timeSheet = await _timeSheetsService.GetTimeSheetByIdWithoutCacheAsync(id);
            if (timeSheet == null)
                return RedirectToAction("List");
            await _timeSheetsService.DeleteTimeSheetAsync(timeSheet);
            return Ok("Row deleted successfully.");
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
            var items = new List<SelectListItem>
    {
        new SelectListItem
        {
            Text = "Select",
            Value = ""
        }
    };
            items.AddRange(workflows.Select(w => new SelectListItem
            {
                Text = w.Name,
                Value = w.Id.ToString()
            }));

            return Json(items);
        }
        public virtual async Task<IActionResult> CreateProjectTask(int employeeId)
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
            if (employeeId != 0)
                model.AssignedTo = employeeId;
            var employee = await _employeeService.GetEmployeeByIdAsync(employeeId);
            if (employee != null)
                model.AssignedEmployee = employee.FirstName + " " + employee.LastName;
            model.IsSync = true;
            return View("/Themes/DefaultClean/Views/Extension/TimeSheets/CreateProjectTask.cshtml", model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> InsertProjectTask(int projectId, int employeeId, string taskTitle, string description, string estimatedTimeHHMM, DateTime? DueDate, int taskTypeid, bool isQARequired, int processWorkflowId, int taskCategoryId, int parentTaskId, bool isSync)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            if (!await _customerService.IsRegisteredAsync(customer))
                return Challenge();
            if (projectId == 0)
            {
                return Json(new
                {
                    success = false,
                    message = "Please Select Project."
                });
            }
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

            if (taskTypeid == 0)
            {
                return Json(new
                {
                    success = false,
                    message = "Please Select Task Type."
                });
            }

            if (employeeId == 0)
            {
                return Json(new
                {
                    success = false,
                    message = "Please Select Assigned To Employee."
                });
            }

            if (parentTaskId == 0 && (taskTypeid == 3 || taskTypeid == 4))
            {
                return Json(new
                {
                    success = false,
                    message = "Please Select Parent Task In Case Of Bug or Change request"
                });
            }

            if (processWorkflowId == 0)
            {
                return Json(new
                {
                    success = false,
                    message = "Please Select Process Workflow"
                });
            }

            if (taskCategoryId == 0)
            {
                return Json(new
                {
                    success = false,
                    message = "Please Select Task Category"
                });
            }

            if (ModelState.IsValid)
            {
                var existingTask = await _projectTaskService.GetProjectTaskByTitleAndProjectAsync(taskTitle, projectId);
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

                var defaultStatus = await _workflowStatusService.GetDefaultStatusIdByWorkflowId(processWorkflowId);

                var projectTask = new ProjectTask
                {
                    TaskTitle = taskTitle,
                    QualityComments = null,
                    EstimatedTime = estimatedTime,
                    ProjectId = projectId,
                    Description = description,
                    StatusId = defaultStatus,
                    AssignedTo = employeeId,
                    DeveloperId = employeeId,
                    DueDate = DueDate,
                    Tasktypeid = taskTypeid,
                    ProcessWorkflowId = processWorkflowId,
                    TaskCategoryId = taskCategoryId,
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

        [HttpGet]
        public virtual async Task<IActionResult> SearchActivities(string searchText, int taskId)
        {
            if (string.IsNullOrWhiteSpace(searchText))
                return Json(new List<object>());

            var activitiesPaged = await _activityService.GetAllActivitiesAsync(
                searchText, 0, taskId, 0, int.MaxValue, false, null);

            var activities = activitiesPaged.Select(a => new { Value = a.Id, Text = a.ActivityName }).ToList();

            return Json(activities);
        }
        public async Task<IActionResult> GetByTaskId(int taskId , DateTime? from, DateTime? to)
        {
            var timeSheets = await _timeSheetsService.GetAllTimeSheetAsync(taskId:taskId,from:from,to:to);
            var model = new List<TimeSheetModel>();
            var task = await _projectTaskService.GetProjectTasksWithoutCacheByIdAsync(taskId);
            int projectId = task?.ProjectId ?? 0;
            int projectQAEmployeeId = 0;
            if (projectId > 0)
            {
                projectQAEmployeeId =
                    await _projectsService.GetProjectQAIdByIdAsync(projectId);
            }
            foreach (var ts in timeSheets
               .OrderByDescending(x => x.SpentDate))
            {
                model.Add(new TimeSheetModel
                {
                    Id = ts.Id,
                    SpentDate = ts.SpentDate,
                    SpentTime = await _timeSheetsService.ConvertSpentTimeAsync(
                        ts.SpentHours,
                        ts.SpentMinutes),
                    Billable = ts.Billable,
                    IsQAEntry = projectQAEmployeeId > 0
                        && ts.EmployeeId == projectQAEmployeeId
                });
            }
            return PartialView("/Themes/DefaultClean/Views/Extension/ProjectTasks/_TaskTimesheet.cshtml", model);
        }
        [HttpPost]
        public async Task<IActionResult> UpdateInlineTimesheet(int id, string spentTime, bool billable)
        {
            var timeSheet = await _timeSheetsService.GetTimeSheetByIdAsync(id);
            if (timeSheet == null)
                return Json(new { success = false, message = "Timesheet not found" });
            var parts = spentTime.Split(':');
            if (parts.Length != 2 ||
                !int.TryParse(parts[0], out int spentHours) ||
                !int.TryParse(parts[1], out int spentMinutes))
            {
                return Json(new { success = false, message = "Invalid time format (HH:MM)" });
            }
            var prevSpentHours = timeSheet.SpentHours;
            var prevSpentMinutes = timeSheet.SpentMinutes;
            timeSheet.SpentHours = spentHours;
            timeSheet.SpentMinutes = spentMinutes;
            timeSheet.Billable = billable;
            timeSheet.UpdateOnUtc = DateTime.UtcNow;
            await _timeSheetsService.UpdateTimeSheetAsync(timeSheet);

            var projectTask = await _projectTaskService.GetProjectTasksWithoutCacheByIdAsync(timeSheet.TaskId);
            if (projectTask != null)
            {
                (projectTask.SpentHours, projectTask.SpentMinutes) =
                    await _timeSheetsService.SubtractSpentTimeAsync(
                        projectTask.SpentHours,
                        projectTask.SpentMinutes,
                        prevSpentHours,
                        prevSpentMinutes);
                (projectTask.SpentHours, projectTask.SpentMinutes) =
                    await _timeSheetsService.AddSpentTimeAsync(
                        projectTask.SpentHours,
                        projectTask.SpentMinutes,
                        spentHours,
                        spentMinutes);
                projectTask.DeliveryOnTime = await _timeSheetsService.IsTaskDeliveredOnTimeAsync(projectTask);
                await _projectTaskService.UpdateProjectTaskAsync(projectTask);
            }

            var activity = await _activityService.GetActivityByIdAsync(timeSheet.ActivityId);
            await _timeSheetsService.UpdateOrCreateActivityAsync(
                timeSheet,
                timeSheet,
                activityName: activity?.ActivityName,
                prevSpentHours,
                spentHours,
                prevSpentMinutes,
                spentMinutes,
                timeSheet.TaskId
            );

            await _employeeAttendanceService.UpdateEmployeeAttendanceBasedOnTimeSheetAsync(
                timeSheet.SpentDate,
                timeSheet.EmployeeId,
                spentHours,
                spentMinutes,
                timeSheet.SpentDate,
                timeSheet.EmployeeId
            );

            var updatedSpentTime = $"{timeSheet.SpentHours:D2}:{timeSheet.SpentMinutes:D2}";
            return Json(new { success = true, updatedSpentTime, billable });
        }

        [HttpPost]
        public async Task<IActionResult> CreateInlineTimesheet(int taskId, DateTime spentDate, string spentTime, bool billable)
        {
            var (hours, minutes) = await _timeSheetsService.ConvertSpentTimeAsync(spentTime);
            if (hours < 0 || minutes < 0)
                return Json(new { success = false, message = "Invalid time format" });
            var customer = await _workContext.GetCurrentCustomerAsync();
            var employee = await _employeeService.GetEmployeeByCustomerIdAsync(customer.Id);
            var timesheet = new TimeSheet
            {
                TaskId = taskId,
                ProjectId = (await _projectTaskService.GetProjectTasksByIdAsync(taskId))?.ProjectId ?? 0,
                EmployeeId = employee.Id,
                SpentDate = spentDate,
                SpentHours = hours,
                SpentMinutes = minutes,
                Billable = billable,
                IsManualEntry =true,
                CreateOnUtc = DateTime.UtcNow
            };

            await _timeSheetsService.InsertTimeSheetAsync(timesheet);

            var task = await _projectTaskService.GetProjectTasksWithoutCacheByIdAsync(taskId);
            if (task != null)
            {
                (task.SpentHours, task.SpentMinutes) =
                    await _timeSheetsService.AddSpentTimeAsync(
                        task.SpentHours,
                        task.SpentMinutes,
                        hours,
                        minutes);

                task.DeliveryOnTime = await _timeSheetsService.IsTaskDeliveredOnTimeAsync(task);
                await _projectTaskService.UpdateProjectTaskAsync(task);
            }

            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteInlineTimesheet(int id)
        {
            var timeSheet = await _timeSheetsService.GetTimeSheetByIdAsync(id);
            if (timeSheet == null)
                return Json(new { success = false, message = "Not found" });

            await _timeSheetsService.DeleteTimeSheetAsync(timeSheet);

            return Json(new { success = true });
        }

    }
}
