using App.Core;
using App.Core.Domain.Employees;
using App.Core.Domain.Extension.Leaves;
using App.Core.Domain.Extension.ProjectTasks;
using App.Core.Domain.Extension.TimeSheets;
using App.Core.Domain.ProjectTasks;
using App.Core.Domain.Security;
using App.Services.Customers;
using App.Services.EmployeeAttendances;
using App.Services.Employees;
using App.Services.Helpers;
using App.Services.Holidays;
using App.Services.Leaves;
using App.Services.Localization;
using App.Services.Messages;
using App.Services.Projects;
using App.Services.ProjectTasks;
using App.Services.Security;
using App.Services.TimeSheets;
using App.Web.Areas.Admin.Factories;
using App.Web.Areas.Admin.Factories.Extension;
using App.Web.Areas.Admin.Models.Extension.ActivityTracking;
using App.Web.Areas.Admin.Models.Extension.MonthlyPerformanceReports;
using App.Web.Areas.Admin.Models.Extension.TimesheetReports;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NUglify.Helpers;
using Pipelines.Sockets.Unofficial.Arenas;
using Satyanam.Nop.Core.Models;
using Satyanam.Nop.Core.Services;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;

namespace App.Web.Areas.Admin.Controllers.Extension
{
    public partial class ReportsController : BaseAdminController
    {
        #region Fields
        private readonly IPermissionService _permissionService;
        private readonly ITimeSheetModelFactory _timeSheetModelFactory;
        private readonly ITimeSheetsService _timeSheetsService;
        private readonly INotificationService _notificationService;
        private readonly ILocalizationService _localizationService;
        private readonly IProjectsService _projectsService;
        private readonly IProjectTaskService _projectTaskService;
        private readonly IProjectTaskModelFactory _projectTaskModelFactory;
        private readonly IEmployeeService _employeeService;
        private readonly IMonthlyPerformanceReportModelFactory _monthlyPerformanceReportModelFactory;
        private readonly IReportsModelFactory _reportsModelFactory;
        private readonly IHolidayService _holidayService;
        private readonly ILeaveManagementService _leaveManagementService;
        private readonly IEmployeeAttendanceService _employeeAttendanceService;
        private readonly ILeaveTypeService _leaveTypeService;
        private readonly LeaveSettings _leaveSettings;
        private readonly ILeaveTransactionLogService _leaveTransactionLogService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IActivityTrackingService _activityTrackingService;
        private readonly IWorkflowStatusService _workflowStatusService;
        private readonly ICommonPluginService _commonPluginService;
        private readonly MonthlyReportSetting _monthlyReportSetting;
        private readonly ITaskChangeLogService _taskChangeLogService;
        private readonly IWorkContext _workContext;
        private readonly ICustomerService _customerService;
        #endregion

        #region Ctor

        public ReportsController(IPermissionService permissionService,
            ITimeSheetModelFactory timeSheetModelFactory,
            ITimeSheetsService timeSheetsService,
            INotificationService notificationService,
            ILocalizationService localizationService, IProjectsService projectsService, IProjectTaskService projectTaskService,
            IProjectTaskModelFactory projectTaskModelFactory,
            IEmployeeService employeeService, IMonthlyPerformanceReportModelFactory monthlyPerformanceReportModelFactory, IReportsModelFactory reportsModelFactory, IHolidayService holidayService,
            ILeaveManagementService leaveManagementService,
            IEmployeeAttendanceService employeeAttendanceService,
            ILeaveTypeService leaveTypeService,
            LeaveSettings leaveSettings,
            ILeaveTransactionLogService leaveTransactionLogService,
            IDateTimeHelper dateTimeHelper
, IActivityTrackingService activityTrackingService, IWorkflowStatusService workflowStatusService, ICommonPluginService commonPluginService, MonthlyReportSetting monthlyReportSetting, ITaskChangeLogService taskChangeLogService, IWorkContext workContext, ICustomerService customerService)
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
            _monthlyPerformanceReportModelFactory = monthlyPerformanceReportModelFactory;
            _reportsModelFactory = reportsModelFactory;
            _holidayService = holidayService;
            _leaveManagementService = leaveManagementService;
            _employeeAttendanceService = employeeAttendanceService;
            _leaveTypeService = leaveTypeService;
            _leaveSettings = leaveSettings;
            _leaveTransactionLogService = leaveTransactionLogService;
            _dateTimeHelper = dateTimeHelper;
            _activityTrackingService = activityTrackingService;
            _workflowStatusService = workflowStatusService;
            _commonPluginService = commonPluginService;
            _monthlyReportSetting = monthlyReportSetting;
            _taskChangeLogService = taskChangeLogService;
            _workContext = workContext;
            _customerService = customerService;
        }

        #endregion

        public virtual async Task<IActionResult> EmployeePerformanceReport(int searchEmployeeId, DateTime? from, DateTime? to, string selectedProjectIds,
            int overEstimation, int searchPeriodId) 
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageTimeSheet))
                return AccessDeniedView();

            var searchModel = new MonthlyPerformanceReportSearchModel
            {
                SearchEmployeeId = searchEmployeeId,
                From = from ==null?await _dateTimeHelper.GetIndianTimeAsync():from,
                To = to ==null? await _dateTimeHelper.GetIndianTimeAsync():to,
                SelectedProjectIds = !string.IsNullOrEmpty(selectedProjectIds)
                    ? selectedProjectIds.Split(',').Select(int.Parse).ToList()
                    : new List<int>(),
                OverEstimation = overEstimation,
                SearchPeriodId = searchPeriodId   
            };

            var model = await _monthlyPerformanceReportModelFactory.PrepareTimeSheetSearchModelAsync(searchModel);

            return View("/Areas/Admin/Views/Extension/PerformanceReports/EmployeePerformanceReport.cshtml", model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> EmployeePerformanceReport(MonthlyPerformanceReportSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageTimeSheet))
                return AccessDeniedView();

                var model = await _monthlyPerformanceReportModelFactory.PrepareMonthlyPerformanceReportListModelAsync(searchModel);

                return Json(model);           
        }

        public async Task<IActionResult> GetSummary(int searchEmployeeId, DateTime? From, DateTime? To, string ProjectIds, int overEstimation)
        {

            var projectList = new List<int>();
            if (ProjectIds != "0" && ProjectIds != null)
            {
                projectList = ProjectIds.Split(',').Select(int.Parse).ToList();
            }

            var timeSheetReports = await _timeSheetsService.GetAllEmployeePerformanceReportForParentTaskAsync(searchEmployeeId, From, To, projectList);
            var totalTask = 0 ;
            var totalDeliveredOnTime = 0;
            int overDueCount = 0;
            decimal totalEstimatedHours = 0;
            decimal totalSpentHours = 0;
            decimal extraTime = 0;
            int firstOverDueThresholdCount = 0;
            int secondOverDueThresholdCount = 0;
            int thirdOverDueThresholdCount = 0;
            decimal totalWorkQuality = 0;
            int workQualityCount = 0;
            decimal totalDot = 0;
            int dotCount = 0;

            var overDueThresholds = await _commonPluginService.GetOverDueEmailPercentage();

            foreach (var report in timeSheetReports)
            {
                var task = await _projectTaskService.GetProjectTasksWithoutCacheByIdAsync(report.TaskId);
                if (task != null)
                {

                    decimal overDuePercentage = await _commonPluginService.GetOverduePercentageByTaskIdAsync(task.Id);
                    if(overEstimation != 0)
                    {
                        if(overDuePercentage != overEstimation) {
                            continue;
                        }
                    }
                    totalTask++;
                    var spentTime = await _timeSheetsService.GetSpeantTimeByEmployeeAndTaskAsync(report.EmployeeId, report.TaskId);
                    var estimatedHours = task.EstimatedTime;
                    var allowedVariations = report.AllowedVariations;

                    if (task.DeliveryOnTime)
                    {
                        totalDeliveredOnTime++;
                    }
                    if (await _workflowStatusService.IsTaskOverdue(task.Id))
                        overDueCount++;
                    if (overDuePercentage == overDueThresholds[0])
                        firstOverDueThresholdCount++;
                    else if (overDuePercentage == overDueThresholds[1])
                        secondOverDueThresholdCount++;
                    else if (overDuePercentage == overDueThresholds[2])
                        thirdOverDueThresholdCount++;

                    if (task.Tasktypeid == 3 && task.ParentTaskId != 0)
                    {
                        var parentTask = await _projectTaskService.GetProjectTasksWithoutCacheByIdAsync(task.ParentTaskId);
                        if (parentTask != null && parentTask.WorkQuality != null)
                        {
                            totalWorkQuality += parentTask.WorkQuality.Value;
                            workQualityCount += 1;
                        }
                    }
                    else if (task.WorkQuality != null)
                    {
                        totalWorkQuality += task.WorkQuality.Value;
                        workQualityCount += 1;
                    }

                    if (task.DOTPercentage != null)
                    {
                        totalDot += task.DOTPercentage.Value;
                        dotCount += 1;
                    }

                    totalEstimatedHours += estimatedHours;
                    totalSpentHours += (decimal)spentTime; 
                    extraTime += (decimal)(spentTime - estimatedHours >= 0 ? spentTime - estimatedHours : 0); 
                }
            }
            var resultPercentage = totalTask == 0 ? 0 : Math.Round((totalDeliveredOnTime / (double)totalTask) * 100, 2);
            decimal avgWorkQuality = workQualityCount != 0 ? totalWorkQuality / workQualityCount : 0;
            decimal avgDot = dotCount != 0 ? totalDot / dotCount : 0;

            return Json(new
            {
                TotalTask = totalTask,
                TotalDeliveredOnTime = totalDeliveredOnTime,
                ResultPercentage = avgDot,
                TotalEstimatedHours = totalEstimatedHours,
                TotalSpentHours = totalSpentHours,
                ExtraTime = extraTime,
                OverDueCount = overDueCount,
                FirstOverDueThreshold = overDueThresholds[0],
                SecondOverDueThreshold = overDueThresholds[1],
                ThirdOverDueThreshold = overDueThresholds[2],
                FirstOverDueThresholdCount = firstOverDueThresholdCount,
                SecondOverDueThresholdCount= secondOverDueThresholdCount,
                ThirdOverDueThresholdCount = thirdOverDueThresholdCount,
                AvgWorkQuality = avgWorkQuality,
                
            });
        }

        public virtual async Task<IActionResult> PerformanceSummaryReport()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePerformanceSummary, PermissionAction.View))
                return AccessDeniedView();

            var model = await _monthlyPerformanceReportModelFactory.PrepareTimeSheetSearchModelAsync(new MonthlyPerformanceReportSearchModel());

            model.SearchPeriodId = (int)SearchPeriodEnum.LastWeek;

            var (from, to) = await _timeSheetsService.GetDateRange(model.SearchPeriodId);

            model.From = from;
            model.To = to;

            return View("/Areas/Admin/Views/Extension/PerformanceReports/PerformanceSummaryReport.cshtml", model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> PerformanceSummaryReport(MonthlyPerformanceReportSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePerformanceSummary, PermissionAction.View))
                return AccessDeniedView();

            var model = await _monthlyPerformanceReportModelFactory.PreparePerformanceSummaryReportListModelAsync(searchModel);

            return Json(model);
        }

        [HttpPost]       
        public async Task<IActionResult> UpdateTaskEstimation(int taskId, string estimation)
        {
            try
            {
                var estimationHours = await _timeSheetsService.ConvertHHMMToDecimal(estimation);
                var projectTask = await _projectTaskService.GetProjectTasksWithoutCacheByIdAsync(taskId);
                if (projectTask == null)
                    return Json(new { success = false, message = "Task not found" });

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
                    IsSync =projectTask.IsSync,
                    TaskCategoryId =projectTask.TaskCategoryId
                };

                projectTask.EstimatedTime = estimationHours;
                int currEmployee = 0;
                var customer = await _workContext.GetCurrentCustomerAsync();
                if (!await _customerService.IsRegisteredAsync(customer))
                    return Challenge();
                if (customer != null)
                {
                    var employee = await _employeeService.GetEmployeeByCustomerIdAsync(customer.Id);
                    if (employee != null)
                        currEmployee = employee.Id;
                }

                projectTask.DeliveryOnTime = await _timeSheetsService.IsTaskDeliveredOnTimeAsync(projectTask);

                await _projectTaskService.UpdateProjectTaskAsync(projectTask);
                await _taskChangeLogService.InsertTaskChangeLogByUpdateTaskAsync(prevProjectTask, projectTask, currEmployee);

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ChangeDOT(int taskId)
        {
            var task = await _projectTaskService.GetProjectTasksByIdAsync(taskId);
            if (task != null)
            {
                if (task.DeliveryOnTime == false)
                {
                    task.DeliveryOnTime = true;
                }
                else
                {
                    task.DeliveryOnTime = false;
                }

                task.IsManualDOT = true;

                await _projectTaskService.UpdateProjectTaskAsync(task);
            }
            return Json("Success");
        }

        public virtual async Task<IActionResult> ReportByEmployee()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageEmployeeReport, PermissionAction.View))
                return AccessDeniedView();

            var model = await _reportsModelFactory.PrepareTimeSheetReportSearchModelAsync(new TimesheetReportSearchModel());
            
            return View("/Areas/Admin/Views/Extension/PerformanceReports/ReportByEmployee.cshtml", model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> ReportByEmployee(TimesheetReportSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageEmployeeReport, PermissionAction.View))
                return AccessDeniedView();
 
            var model = await _reportsModelFactory.PrepareTimesheetReportListModelAsync(searchModel);

            return Json(model);
        }

        public virtual async Task<IActionResult> GetReportByEmployee(int searchEmployeeId, DateTime From, DateTime To, int ShowById, int ProjectId, int HoursId)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageEmployeeReport, PermissionAction.View))
                return AccessDeniedView();

            var timeSheetReports = await _timeSheetsService.GetReportByEmployeeListAsync(searchEmployeeId,From,To, ShowById, ProjectId, HoursId);
            var holidayList = await _holidayService.GetAllHolidaysAsync();
            var leaveList = await _leaveManagementService.GetLeaveManagementsAsync(searchEmployeeId, 0, From, To, 2);

            var leaveDates = leaveList.SelectMany(leave =>
            {
                var dates = new List<(DateTime LeaveDate, bool IsHalf)>();
                var totalDays = (leave.To - leave.From).Days + 1;

                for (var date = leave.From; date <= leave.To; date = date.AddDays(1))
                {
                    bool isHalf = false;

                    if (leave.NoOfDays < totalDays && date == leave.To)
                    {
                        isHalf = true;
                    }
                    dates.Add((date, isHalf));
                }
                return dates;
            }).ToList();

            var employee = await _employeeService.GetEmployeeByIdAsync(searchEmployeeId);

            timeSheetReports.ForEach(report =>
            {
                if (employee != null)
                    report.EmployeeName = employee.FirstName+" "+employee.LastName;

                if (holidayList != null)
                {
                    report.IsHoliday = holidayList.Any(holiday => holiday.Date == report.SpentDate);
                }
                var leaveDay = leaveDates.FirstOrDefault(ld => ld.LeaveDate == report.SpentDate);
                if (leaveDay != default)
                {
                    report.IsLeave = true;
                    report.IsHalfLeave = leaveDay.IsHalf;
                    if (report.IsHalfLeave)
                        report.IsLeave = false;
                }
                else
                {
                    report.IsLeave = false;
                    report.IsHalfLeave = false;
                }

                report.IsWeekend = report.SpentDate.DayOfWeek == DayOfWeek.Saturday || report.SpentDate.DayOfWeek == DayOfWeek.Sunday;
            
            });

            return Json(timeSheetReports);
        }

        public virtual async Task<IActionResult> GetAllEmployeesData(DateTime From, DateTime To,int ShowById, int ProjectId, int HoursId)
        {
            var allEmployees = await _employeeService.GetAllEmployeesAsync();
            var holidayList = await _holidayService.GetAllHolidaysAsync();
            var allReports = new List<TimeSheetReport>(); 

            foreach (var employee in allEmployees)
            {
                var leaveList = await _leaveManagementService.GetLeaveManagementsAsync(employee.Id, 0, From, To, 2);

                var leaveDates = leaveList.SelectMany(leave =>
                {
                    var dates = new List<(DateTime LeaveDate, bool IsHalf)>();
                    var totalDays = (leave.To - leave.From).Days + 1;

                    for (var date = leave.From; date <= leave.To; date = date.AddDays(1))
                    {
                        bool isHalf = false;

                        if (leave.NoOfDays < totalDays && date == leave.To)
                        {
                            isHalf = true;
                        }

                        dates.Add((date, isHalf));
                    }
                    return dates;
                }).ToList();


                var timeSheetReports = await _timeSheetsService.GetReportByEmployeeListAsync(employee.Id, From, To,ShowById, ProjectId, HoursId);

                timeSheetReports.ForEach(report =>
                {
                    if (employee != null)
                        report.EmployeeName = employee.FirstName + " " + employee.LastName;

                    if (holidayList != null)
                    {
                        report.IsHoliday = holidayList.Any(holiday => holiday.Date == report.SpentDate);
                    }
                    var leaveDay = leaveDates.FirstOrDefault(ld => ld.LeaveDate == report.SpentDate);
                    if (leaveDay != default)
                    {
                        report.IsLeave = true;
                        report.IsHalfLeave = leaveDay.IsHalf;
                        if (report.IsHalfLeave)
                            report.IsLeave = false;
                    }
                    else
                    {
                        report.IsLeave = false;
                        report.IsHalfLeave = false;
                    }
                    report.IsWeekend = report.SpentDate.DayOfWeek == DayOfWeek.Saturday || report.SpentDate.DayOfWeek == DayOfWeek.Sunday;
                });

                allReports.AddRange(timeSheetReports);
            }

            return Json(allReports);
        }

        public virtual async Task<IActionResult> ReportByProject()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProjectReport, PermissionAction.View))
                return AccessDeniedView();

            var model = await _reportsModelFactory.PrepareTimeSheetReportSearchModelForProjectAsync(new TimesheetReportSearchModel());

            return View("/Areas/Admin/Views/Extension/PerformanceReports/ReportByProject.cshtml", model);
        }

        public virtual async Task<IActionResult> GetReportByProject(int searchEmployeeId, DateTime From, DateTime To, int ShowById, int ProjectId , int HoursId)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProjectReport, PermissionAction.View))
                return AccessDeniedView();

            var timeSheetReports = await _timeSheetsService.GetReportByProjectListAsync(searchEmployeeId, From, To, ShowById, ProjectId, HoursId);
            var project = await _projectsService.GetProjectsByIdAsync(ProjectId);
            var holidayList = await _holidayService.GetAllHolidaysAsync();

            timeSheetReports.ForEach(report =>
            {
                if (project != null)
                    report.ProjectName = project.ProjectTitle;

                if (holidayList != null)
                {
                    report.IsHoliday = holidayList.Any(holiday => holiday.Date == report.SpentDate);
                }
                report.IsWeekend = report.SpentDate.DayOfWeek == DayOfWeek.Saturday || report.SpentDate.DayOfWeek == DayOfWeek.Sunday;
            });

            return Json(timeSheetReports);
        }
        public virtual async Task<IActionResult> GetAllProjectsData(DateTime From, DateTime To, int ShowById, int EmployeeId, int HoursId)
        {
            var allProjects = await _projectsService.GetAllProjectsAsync();
            var holidayList = await _holidayService.GetAllHolidaysAsync();
            var allReports = new List<TimeSheetReport>();

            foreach (var project in allProjects)
            {
                var timeSheetReports = await _timeSheetsService.GetReportByProjectListAsync(EmployeeId, From, To, ShowById, project.Id, HoursId);

                timeSheetReports.ForEach(report =>
                {
                    if (project != null)
                        report.ProjectName = project.ProjectTitle;

                    if (holidayList != null)
                    {
                        report.IsHoliday = holidayList.Any(holiday => holiday.Date == report.SpentDate);
                    }
                    report.IsWeekend = report.SpentDate.DayOfWeek == DayOfWeek.Saturday || report.SpentDate.DayOfWeek == DayOfWeek.Sunday;
                });

                allReports.AddRange(timeSheetReports);
            }

            return Json(allReports);
        }

        public virtual async Task<IActionResult> ReportByTask()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageTaskReport, PermissionAction.View))
                return AccessDeniedView();

            var model = await _reportsModelFactory.PrepareTimeSheetReportSearchModelAsync(new TimesheetReportSearchModel());

            return View("/Areas/Admin/Views/Extension/PerformanceReports/ReportByTask.cshtml", model);
        }

        public virtual async Task<IActionResult> GetHolidays(string from, string to)
        {
            var fromDate = DateTime.Parse(from);
            var toDate = DateTime.Parse(to);

            var holidays = new List<object>();
            var holidayList = await _holidayService.GetAllHolidaysAsync();
            for (var date = fromDate; date <= toDate; date = date.AddDays(1))
            {
                var isWeekend = date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday;
                var isHoliday = holidayList.Any(holiday => holiday.Date == date);

                holidays.Add(new
                {
                    Date = date.ToString("yyyy-MM-dd"),
                    IsWeekend = isWeekend,
                    IsHoliday = isHoliday
                });
            }
            return Json(holidays);
        }

        public virtual async Task<IActionResult> GetReportByTask(string searchEmployeeId, DateTime From, DateTime To, int ShowById, int ProjectId, int HoursId, int TaskId, bool IsHideEmpty)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageTaskReport, PermissionAction.View))
                return AccessDeniedView();

            List<int> employeedList = searchEmployeeId.Split(',').Select(int.Parse).ToList();

            var timeSheetReports = await _timeSheetsService.GetReportByTaskListAsync(employeedList, From, To, ShowById, ProjectId, HoursId, TaskId, IsHideEmpty);
            var holidayList = await _holidayService.GetAllHolidaysAsync();
            var task = await _projectTaskService.GetProjectTasksByIdAsync(TaskId);

            timeSheetReports.ForEach(report =>
            {
                if (report != null)
                {
                    if(task !=null)
                    report.TaskName = task.TaskTitle;

                    if (holidayList != null)
                    {
                        report.IsHoliday = holidayList.Any(holiday => holiday.Date == report.SpentDate);
                    }
                    report.IsWeekend = report.SpentDate.DayOfWeek == DayOfWeek.Saturday || report.SpentDate.DayOfWeek== DayOfWeek.Sunday;
                }

            });

            return Json(timeSheetReports);
        }

        public virtual async Task<IActionResult> GetTasksFromProjects(string projectIds)
        {
            var projectIdList = projectIds.Split(',').Select(int.Parse).ToList();

            var allTasks = new List<ProjectTask>(); 

            foreach (var projectId in projectIdList)
            {
                var tasks = await _projectTaskService.GetProjectTasksByProjectId(projectId);  
                allTasks.AddRange(tasks);
            }

           var taskList = allTasks.Select(t => new { TaskId = t.Id, TaskName = t.TaskTitle }).ToList().Distinct();

            return Json(taskList);
        }

        public virtual async Task<IActionResult> GetAllTasksData(DateTime From, DateTime To, int ShowById, string EmployeeIds, int HoursId, string projectIds)
        {
            List<int> projectIdList = new List<int>();
          
            if(projectIdList!=null)
                projectIdList = projectIds.Split(',').Select(int.Parse).ToList();
            
            List<int> employeeIdList = new List<int>();

             if(EmployeeIds !=null)
                employeeIdList = EmployeeIds.Split(',').Select(int.Parse).ToList();
    
            var allTasks = await _projectTaskService.GetAllProjectTasksByDateAsync(From , To);
   
            var holidayList = await _holidayService.GetAllHolidaysAsync();
            var allReports = new List<TimeSheetReport>();

            if (projectIdList.Count == 0 || projectIdList.Contains(0)) 
            {
                foreach (var task in allTasks)
                {
                    var timeSheetReports = await _timeSheetsService.GetAllReportByTaskListAsync(employeeIdList, From, To, ShowById, 0, HoursId, task.Id);

                    timeSheetReports.ForEach(report =>
                    {
                        if (report != null)
                        {
                            if (task != null)
                                report.TaskName = task.TaskTitle;

                            if (holidayList != null)
                            {
                                report.IsHoliday = holidayList.Any(holiday => holiday.Date == report.SpentDate);
                            }
                            report.IsWeekend = report.SpentDate.DayOfWeek == DayOfWeek.Saturday || report.SpentDate.DayOfWeek == DayOfWeek.Sunday;
                        }
                    });

                    allReports.AddRange(timeSheetReports);
                }
            }
            else
            {
                foreach (var task in allTasks)
                {
                    if (projectIdList.Contains(task.ProjectId))
                    {
                        var timeSheetReports = await _timeSheetsService.GetAllReportByTaskListAsync(employeeIdList, From, To, ShowById, task.ProjectId, HoursId, task.Id);

                        timeSheetReports.ForEach(report =>
                        {
                            if (report != null)
                            {
                                if (task != null)
                                    report.TaskName = task.TaskTitle;

                                if (holidayList != null)
                                {
                                    report.IsHoliday = holidayList.Any(holiday => holiday.Date == report.SpentDate);
                                }
                                report.IsWeekend = report.SpentDate.DayOfWeek == DayOfWeek.Saturday || report.SpentDate.DayOfWeek == DayOfWeek.Sunday;
                            }
                        });

                        allReports.AddRange(timeSheetReports);
                    }
                }
            }

            return Json(allReports);
        }
        public virtual async Task<IActionResult> GetAllEmployeesWithProjectsData(DateTime From, DateTime To, int ShowById, string ProjectIds, int HoursId, string TaskIds)
        {

            var allEmployees = await _employeeService.GetAllEmployeesAsync();
            var holidayList = await _holidayService.GetAllHolidaysAsync();
            var allReports = new List<TimeSheetReport>(); 
            List<int> projectIdList;

            if (string.IsNullOrEmpty(ProjectIds) || ProjectIds.Split(',').Count() < 1 || ProjectIds == "0")
            {
                projectIdList = (await _projectsService.GetAllProjectIdsAsync()).ToList(); 
            }
            else
            {
                projectIdList = ProjectIds.Split(',').Select(int.Parse).ToList();
            }
            List<int> taskIdList = TaskIds.Split(',').Select(int.Parse).ToList();

            foreach (var employee in allEmployees)
            {
                var timeSheetReports = await _timeSheetsService.GetReportByEmployeeListWithProjectsAsync(employee.Id, From, To, ShowById, projectIdList, HoursId, taskIdList);

                timeSheetReports.ForEach(report =>
                {
                    if (employee != null)
                        report.EmployeeName = employee.FirstName + " " + employee.LastName;

                    if (holidayList != null)
                    {
                        report.IsHoliday = holidayList.Any(holiday => holiday.Date == report.SpentDate);
                    }
                    report.IsWeekend = report.SpentDate.DayOfWeek == DayOfWeek.Saturday || report.SpentDate.DayOfWeek == DayOfWeek.Sunday;
                });

                allReports.AddRange(timeSheetReports);
            }

            return Json(allReports);
        }

        public virtual async Task<IActionResult> GetReportByWithProjectsEmployee(int searchEmployeeId, DateTime From, DateTime To, int ShowById, string ProjectIds, int HoursId, string TaskIds)
        {
            List<int> projectIdList;

            if (string.IsNullOrEmpty(ProjectIds) || ProjectIds.Split(',').Count() < 1 || ProjectIds == "0")
            {
                projectIdList = (await _projectsService.GetAllProjectIdsAsync()).ToList(); 
            }
            else
            {
                projectIdList = ProjectIds.Split(',').Select(int.Parse).ToList();
            }
            List<int> taskIdList = TaskIds.Split(',').Select(int.Parse).ToList();

            var timeSheetReports = await _timeSheetsService.GetReportByEmployeeListWithProjectsAsync(searchEmployeeId, From, To, ShowById, projectIdList, HoursId, taskIdList);
            var holidayList = await _holidayService.GetAllHolidaysAsync();
            var employee = await _employeeService.GetEmployeeByIdAsync(searchEmployeeId);
 
            timeSheetReports.ForEach(report =>
            {
                if (employee != null)
                    report.EmployeeName = employee.FirstName + " " + employee.LastName;

                if (holidayList != null)
                {
                    report.IsHoliday = holidayList.Any(holiday => holiday.Date == report.SpentDate);
                }
                report.IsWeekend = report.SpentDate.DayOfWeek == DayOfWeek.Saturday || report.SpentDate.DayOfWeek == DayOfWeek.Sunday;

            });

            return Json(timeSheetReports);
        }

        public virtual async Task<IActionResult> TimeSummaryReport()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageTimeSummmaryReport, PermissionAction.View))
                return AccessDeniedView();
 
            var model = await _reportsModelFactory.PrepareTimeSheetReportSearchModelAsync(new TimesheetReportSearchModel());

            return View("/Areas/Admin/Views/Extension/PerformanceReports/TimeSummaryReport.cshtml", model);
        }

        public virtual async Task<IActionResult> GetDateReportData(DateTime From, DateTime To,int ShowById, string EmployeeIds,string projectIds,string taskIds, int HoursId)
        {
            List<int> projectIdList;

            if (string.IsNullOrEmpty(projectIds) || projectIds.Split(',').Count() < 1 || projectIds == "0")
            {
                projectIdList = (await _projectsService.GetAllProjectIdsAsync()).ToList(); 
            }
            else
            {
                projectIdList = projectIds.Split(',').Select(int.Parse).ToList();
            }
            List<int> taskIdList = new List<int>();
            List<int> employeeIdList = new List<int>();
           if (taskIds !=null)
            taskIdList = taskIds.Split(',').Select(int.Parse).ToList();

           if(employeeIdList !=null)
            employeeIdList = EmployeeIds.Split(',').Select(int.Parse).ToList();

            var timeSheetReports = await _timeSheetsService.GetReportByDateAsync(employeeIdList, From, To, projectIdList, HoursId, taskIdList);

            var reportTasks = timeSheetReports.Select(async report =>
            {
                var project = await _projectsService.GetProjectsByIdAsync(report.ProjectId);
                if (project != null)
                    report.ProjectName = project.ProjectTitle;
                var task = await _projectTaskService.GetProjectTasksByIdAsync(report.TaskId);
                if (task != null)
                {
                    report.TaskName = task.TaskTitle;
                }
                    var employee = await _employeeService.GetEmployeeByIdAsync(report.EmployeeId);
                    if (employee != null)
                    {
                    report.EmployeeName = employee.FirstName + " " + employee.LastName;
                    }
            });
            await Task.WhenAll(reportTasks);

            return Json(timeSheetReports);
        }


        public virtual async Task<IActionResult> EmployeeAttendance()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAttendanceReport, PermissionAction.View))
                return AccessDeniedView();

            var model = await _reportsModelFactory.PrepareTimeSheetReportSearchModelAsync(new TimesheetReportSearchModel());

            return View("/Areas/Admin/Views/Extension/PerformanceReports/AttendanceReport.cshtml", model);
        }

        public virtual async Task<IActionResult> GetEmployeeAtttendance(int searchEmployeeId, DateTime From, DateTime To, int ShowById)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAttendanceReport, PermissionAction.View))
                return AccessDeniedView();

            var timeSheetReports = await _timeSheetsService.GetAttendanceReportByEmployeeListAsync(searchEmployeeId, From, To);
            var holidayList = await _holidayService.GetAllHolidaysAsync();
            var employee = await _employeeService.GetEmployeeByIdAsync(searchEmployeeId);
 
            timeSheetReports.ForEach(report =>
            {
                if (employee != null)
                    report.EmployeeName = employee.FirstName + " " + employee.LastName;

                if (holidayList != null)
                {
                    report.IsHoliday = holidayList.Any(holiday => holiday.Date == report.SpentDate);
                }
               
                report.IsWeekend = report.SpentDate.DayOfWeek == DayOfWeek.Saturday || report.SpentDate.DayOfWeek == DayOfWeek.Sunday;
            });

            return Json(timeSheetReports);
        }

        public virtual async Task<IActionResult> GetAllEmployeeAttendanceData(DateTime From, DateTime To, int ShowById)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAttendanceReport, PermissionAction.View))
                return AccessDeniedView();

            var allEmployees = await _employeeService.GetAllEmployeesAsync();
            var holidayList = await _holidayService.GetAllHolidaysAsync();
            var allReports = new List<TimeSheetReport>(); 

            foreach (var employee in allEmployees)
            {

                var timeSheetReports = await _timeSheetsService.GetAttendanceReportByEmployeeListAsync(employee.Id, From, To);

                timeSheetReports.ForEach(report =>
                {
                    if (employee != null)
                        report.EmployeeName = employee.FirstName + " " + employee.LastName;

                    if (holidayList != null)
                    {
                        report.IsHoliday = holidayList.Any(holiday => holiday.Date == report.SpentDate);
                    }

                    report.IsWeekend = report.SpentDate.DayOfWeek == DayOfWeek.Saturday || report.SpentDate.DayOfWeek == DayOfWeek.Sunday;
                });

                allReports.AddRange(timeSheetReports);
            }

            return Json(allReports);
        }

        public async Task<IActionResult> GetEmployeeAttendanceSummary(string employeeIds)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAttendanceReport, PermissionAction.View))
                return AccessDeniedView();

            List<int> employeeIdList = new List<int>();

            if (employeeIds != null)
                employeeIdList = employeeIds.Split(',').Select(int.Parse).ToList();

            var leaveTypes = await _leaveTypeService.GetLeaveTypesAsync();

            IList<Employee> employees = new List<Employee>();

            if (employeeIdList.Contains(0))
                employees = await _employeeService.GetAllEmployeesAsync();
            else
            {
                employees = await _employeeService.GetEmployeeByIdsAsync(employeeIdList.ToArray());
            }

            var currTime = await _dateTimeHelper.GetIndianTimeAsync();
            if (_leaveSettings.LeaveTestDate != DateTime.MinValue)
                currTime = _leaveSettings.LeaveTestDate;

            var leaveBalances = new List<dynamic>();

            foreach (var employee in employees)
            {
                var employeeLeaveBalance = new ExpandoObject() as IDictionary<string, object>;
                employeeLeaveBalance["EmployeeName"] = employee.FirstName+" "+employee.LastName;

                foreach (var leaveType in leaveTypes)
                {
                    var approvedTakenLeaves = await _leaveManagementService
                        .GetApprovedTakenLeavesForCurrentYearAndEmployeeAsync(employee.Id, leaveType.Id);

                    var totalTakenLeaves = approvedTakenLeaves.Sum(l => l.NoOfDays);
                    decimal totalAllowed = 0;
                    
                    var leaveLog = await _leaveTransactionLogService.GetLeaveBalanceByLogForPreviousMonth(employee.Id, leaveType.Id, currTime.Month,currTime.Year);

                        totalAllowed = await _leaveTransactionLogService.GetAddedLeaveBalanceForCurrentMonthForReport(employee.Id, leaveType.Id, currTime.Month, currTime.Year);

                    var remainingLeaves = totalAllowed;
                    employeeLeaveBalance[$"{leaveType.Type} Taken"] = totalTakenLeaves;
                    employeeLeaveBalance[$"{leaveType.Type} Balance"] = remainingLeaves;
                }
                leaveBalances.Add(employeeLeaveBalance);
            }

            string lastUpdateTime = _leaveSettings.LastUpdateBalance;

            return Json(new
            {
                LastUpdateBalanceTime = lastUpdateTime,
                LeaveBalances = leaveBalances
            });
        }

        public async Task SyncDOT()
        {
            await _timeSheetsService.SyncDOT();
        }


        public virtual async Task<IActionResult> ActivityLogReport()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageTimeSheet))
                return AccessDeniedView();

            var model = await _reportsModelFactory.PrepareTimeSheetReportSearchModelAsync(new TimesheetReportSearchModel());
            var date = DateTime.Now.Date;
            model.From = date;

            return View("/Areas/Admin/Views/Extension/PerformanceReports/ActivityLogReport.cshtml", model);
        }
        [HttpPost]
        public virtual async Task<IActionResult> ActivityLogReport(TimesheetReportSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageTimeSheet))
                return AccessDeniedView();

            var model = await _reportsModelFactory.PrepareActivityLogReportListModelAsync(searchModel);
            return Json(model);
        }

        public virtual async Task<IActionResult> ViewScreenshots(int Id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageTimeSheet))
                return AccessDeniedView();

            var model = await _reportsModelFactory.PrepareViewScreenShotsModel(Id);

            return View("/Areas/Admin/Views/Extension/PerformanceReports/ViewScreenshots.cshtml", model);
        }

        public virtual async Task<IActionResult> ActivityTrackingReport()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageEmployeeActivities, PermissionAction.View))
                return AccessDeniedView();

            var model = await _reportsModelFactory.PrepareActivityTrackingReportSearchModelAsync(new ActivityTrackingSearchModel());

            return View("/Areas/Admin/Views/Extension/PerformanceReports/ActivityTrackingReport.cshtml", model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> ActivityTrackingReport(ActivityTrackingSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageEmployeeActivities, PermissionAction.View))
                return AccessDeniedView();

            var model = await _reportsModelFactory.PrepareActivityTrackingReportListModelAsync(searchModel);
            return Json(model);
        }

        public async Task<IActionResult> ActivityTrackingSubgridTable(int employeeId)
        {
            var model = new ActivityTrackingSearchModel
            {
                EmployeeId = employeeId
            };
            return View("_ActivityTrackingSubgridTable", model);
        }
        [HttpGet]
        public virtual async Task<IActionResult> GetDateWiseActivity(int employeeId, DateTime? from, DateTime? to)
        {
            var model = await _reportsModelFactory.PrepareDateWiseActivityTrackingModelAsync(employeeId, from, to);
            return PartialView("~/Areas/Admin/Views/Extension/PerformanceReports/_ActivityTrackingSubgridTable.cshtml", model);

        }

        public virtual async Task<IActionResult> GetSubgridActivity(int employeeId, DateTime from, DateTime to, ShowByEnum? showBy = null)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageEmployeeActivities, PermissionAction.View))
                return AccessDeniedView();

            var days = (to - from).TotalDays;

            var level = showBy ?? (
                days <= 14 ? ShowByEnum.Daily :
                days <= 60 ? ShowByEnum.Weekly :
                ShowByEnum.Monthly
            );

            var model = await _reportsModelFactory.PrepareSubgridDataAsync(employeeId, from, to, level);
            return PartialView("~/Areas/Admin/Views/Extension/PerformanceReports/_ActivityTrackingSubgridTable.cshtml", model);
        }

        [HttpGet]
        public virtual async Task<IActionResult>  GetPeriodDateRange(int periodId)
        {
            var (from, to) =await _timeSheetsService.GetDateRange(periodId);

            return Json(new
            {
                from = from,
                to = to
            });
        }

        public async Task<IActionResult> GetDailyTimelineData(int employeeId, DateTime date)
        {
            var entry = await _activityTrackingService.GetByEmployeeAndDateAsync(employeeId, date);
            if (entry == null)
                return Content("No data available.");

            var timelineEvents = JsonConvert.DeserializeObject<List<ActivityEventJsonModel>>(entry.JsonString);

            var istTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");

            foreach (var timelineEvent in timelineEvents)
            {
                var endTime = TimeZoneInfo.ConvertTimeFromUtc(timelineEvent.CreatedOnUtc, istTimeZone);

                var startTime = endTime.AddMinutes(-timelineEvent.Duration);

                timelineEvent.FormattedTime = $"{startTime:hh:mm tt} - {endTime:hh:mm tt}";

                timelineEvent.DurationHHMM = await _timeSheetsService.ConvertToHHMMFormat(timelineEvent.Duration);

                switch (timelineEvent.StatusId)
                {
                    case 1:
                        timelineEvent.StatusName = $"{ActivityTrackingEnum.Active}|||#28a745";
                        break;
                    case 2:
                        timelineEvent.StatusName = $"{ActivityTrackingEnum.Away}|||#ffc107";
                        break;
                    case 3:
                        timelineEvent.StatusName = $"{ActivityTrackingEnum.Offline}|||#dc3545";
                        break;
                    case 4:
                        timelineEvent.StatusName = $"{ActivityTrackingEnum.Stopped}|||#808080";
                        break; 
                }
            }
            return PartialView("~/Areas/Admin/Views/Extension/PerformanceReports/_TimelineSubgrid.cshtml", timelineEvents);
        }

        [HttpGet]
        public async Task<IActionResult> GetBugTasksByTaskId(int taskId)
        {
            var bugTasks = await _projectTaskService.GetBugChildTasksByParentTaskIdAsync(taskId);
            var modelList = new List<MonthlyPerformanceReportModel>();

            foreach (var task in bugTasks)
            {
                var reportModel = new MonthlyPerformanceReportModel();
                reportModel.TaskId = task.Id;
                reportModel.TaskName = task.TaskTitle;
                reportModel.EstimatedTime = task.EstimatedTime;
                reportModel.TaskTypeName = ((TaskTypeEnum)task.Tasktypeid).ToString();
                reportModel.QualityComments = task.QualityComments;
                reportModel.BugCount = task.BugCount;
                reportModel.EstimatedTimeFormat = await _timeSheetsService.ConvertToHHMMFormat(task.EstimatedTime);
                reportModel.DOTPercentage = task.DOTPercentage != null ? task.DOTPercentage : null;
                var devTime = await _timeSheetsService.GetDevelopmentTimeByTaskId(task.Id);
                var qaTime = await _timeSheetsService.GetQATimeByTaskId(task.Id);
                reportModel.SpentTimeFormat = await _timeSheetsService.ConvertSpentTimeAsync(devTime.SpentHours, devTime.SpentMinutes);
                reportModel.QaTimeFormat = await _timeSheetsService.ConvertSpentTimeAsync(qaTime.SpentHours, qaTime.SpentMinutes);
                int spentTimeMin = devTime.SpentHours * 60 + devTime.SpentMinutes;
                int estTimeMin = (int)(task.EstimatedTime * 60);
                int extraTimeMin = Math.Max(spentTimeMin - estTimeMin, 0);
                int extraHours = extraTimeMin / 60;
                int extraMinutes = extraTimeMin % 60;
                reportModel.ExtraTime = await _timeSheetsService.ConvertSpentTimeAsync(extraHours, extraMinutes);

                string extraPercent = estTimeMin > 0 ? $"{(int)Math.Round((extraTimeMin * 100.0) / estTimeMin)}%" : "--";
                if (extraTimeMin > 0)
                    reportModel.ExtraTime += $" ({extraPercent})";

                var status = await _workflowStatusService.GetWorkflowStatusByIdAsync(task.StatusId);
                if (status != null)
                    reportModel.StatusName = status.StatusName + "|||" + status.ColorCode;

                reportModel.DueDateFormat = task.DueDate.HasValue
                    ? await _workflowStatusService.IsTaskOverdue(task.Id)
                        ? $"<span style='color: red; font-weight: bold;'>{task.DueDate.Value:dd-MMMM-yyyy}</span>"
                        : task.DueDate.Value.ToString("dd-MMMM-yyyy")
                    : "";
                reportModel.DeliveredOnTime = task.DeliveryOnTime;
                reportModel.AllowedVariations = _monthlyReportSetting.AllowedVariations; 
                reportModel.OverduePercentage = await _commonPluginService.GetOverduePercentageByTimeAsync(
                reportModel.SpentTimeFormat, reportModel.EstimatedTimeFormat);

                var parent = await _projectTaskService.GetProjectTasksByIdAsync(task.ParentTaskId);
                if (parent != null)
                    reportModel.ParentTaskName = parent.TaskTitle;

                modelList.Add(reportModel);
            }
            return PartialView("~/Areas/Admin/Views/Extension/PerformanceReports/_SubgridPerformanceBugList.cshtml", modelList); 
        }
    }
}
