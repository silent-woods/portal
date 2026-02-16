using App.Core;
using App.Core.Domain.Employees;
using App.Core.Domain.Extension.Leaves;
using App.Core.Domain.Extension.TimeSheets;
using App.Core.Domain.Security;
using App.Services.Customers;
using App.Services.EmployeeAttendances;
using App.Services.Employees;
using App.Services.Helpers;
using App.Services.Holidays;
using App.Services.Leaves;
using App.Services.Localization;
using App.Services.Messages;
using App.Services.ProjectEmployeeMappings;
using App.Services.Projects;
using App.Services.ProjectTasks;
using App.Services.Security;
using App.Services.TimeSheets;
using App.Web.Factories.Extensions;
using App.Web.Models.Extension.TimesheetReports;
using Microsoft.AspNetCore.Mvc;
using NUglify.Helpers;
using Pipelines.Sockets.Unofficial.Arenas;
using Satyanam.Nop.Core.Services;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;

namespace App.Web.Controllers.Extensions
{
    public class ReportsController : Controller
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
        private readonly Factories.Extensions.IReportsModelFactory _reportsModelFactory;
        private readonly IHolidayService _holidayService;
        private readonly IWorkContext _workContext;
        private readonly ICustomerService _customerService;
        private readonly IProjectEmployeeMappingService _projectEmployeeMappingService;
        private readonly IEmployeeAttendanceService _employeeAttendanceService;
        private readonly ILeaveTypeService _leaveTypeService;
        private readonly ILeaveManagementService _leaveManagementService;
        private readonly ILeaveTransactionLogService _leaveTransactionLogService;
        private readonly LeaveSettings _leaveSettings;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IWorkflowStatusService _workflowStatusService;
        private readonly ICommonPluginService _commonPluginService;

        #endregion

        #region Ctor

        public ReportsController(IPermissionService permissionService,
            ITimeSheetModelFactory timeSheetModelFactory,
            ITimeSheetsService timeSheetsService,
            INotificationService notificationService,
            ILocalizationService localizationService, IProjectsService projectsService, IProjectTaskService projectTaskService,
            Areas.Admin.Factories.Extension.IProjectTaskModelFactory projectTaskModelFactory,
            IEmployeeService employeeService, Factories.Extensions.IReportsModelFactory reportsModelFactory, IHolidayService holidayService,
          IWorkContext workContext,
          ICustomerService customerService,
          IProjectEmployeeMappingService projectEmployeeMappingService,
          IEmployeeAttendanceService employeeAttendanceService,
          ILeaveTypeService leaveTypeService,
          ILeaveManagementService leaveManagementService,
          ILeaveTransactionLogService leaveTransactionLogService,
          LeaveSettings leaveSettings,
          IDateTimeHelper dateTimeHelper
, IWorkflowStatusService workflowStatusService, ICommonPluginService commonPluginService)
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

            _reportsModelFactory = reportsModelFactory;
            _holidayService = holidayService;
            _workContext = workContext;
            _customerService = customerService;
            _projectEmployeeMappingService = projectEmployeeMappingService;
            _employeeAttendanceService = employeeAttendanceService;
            _leaveTypeService = leaveTypeService;
            _leaveManagementService = leaveManagementService;
            _leaveTransactionLogService = leaveTransactionLogService;
            _leaveSettings = leaveSettings;
            _dateTimeHelper = dateTimeHelper;
            _workflowStatusService = workflowStatusService;
            _commonPluginService = commonPluginService;
        }

        #endregion

        public virtual async Task<IActionResult> EmployeePerformanceReport()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.PublicStoreEmployeePerformaceReport, PermissionAction.View))
            {
                if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
                    return Challenge();

                return View("/Themes/DefaultClean/Views/Common/AccessDenied.cshtml");
            }

            //prepare model
            TimesheetReportSearchModel timesheetReportSearchModel = new TimesheetReportSearchModel();

            var customer = await _workContext.GetCurrentCustomerAsync();
            if (!await _customerService.IsRegisteredAsync(customer))
                return Challenge();

            if (customer != null)
            {
                var employee = await _employeeService.GetEmployeeByCustomerIdAsync(customer.Id);
                if (employee != null)
                    timesheetReportSearchModel.EmployeeId = employee.Id;
            }
            //prepare model
            var model = await _reportsModelFactory.PrepareTimeSheetSearchModelAsync(timesheetReportSearchModel);

                return View("/Themes/DefaultClean/Views/Extension/Reports/EmployeePerformanceReport.cshtml", model);
            }



            public virtual async Task<IActionResult> GetEmployeePerformanceReport(DateTime From,DateTime To,int EmployeeId, string ProjectIds, int OverEstimation)
        {
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
   
            var projectList = new List<int>();
            if (ProjectIds != "0" && ProjectIds !=null)
            {
                projectList = ProjectIds.Split(',').Select(int.Parse).ToList();
            }
            else
            {
                var projects = await _projectsService.GetProjectListByEmployee(currEmployee);
                if(projects!=null)
                    foreach(var project in projects)
                    {
                        projectList.Add(project.Id);
                    }
                
            }

            TimesheetReportSearchModel searchModel = new TimesheetReportSearchModel
            {
                SearchEmployeeId = EmployeeId,
                From = From,
                To = To,
                SelectedProjectIds= projectList,
                OverEstimation= OverEstimation

            };
            var model = await _reportsModelFactory.PrepareMonthlyPerformanceReportListModelAsync(searchModel);

            var ReportJson = model.Data.ToList();
            
            return Json(ReportJson);
        }

        [HttpPost]
        public async Task<IActionResult> GetSummary( DateTime? From, DateTime? To, int employeeId, string ProjectIds, int overEstimation)
            {
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

            var projectList = new List<int>();
            if (ProjectIds != "0" && ProjectIds != null)
            {
                projectList = ProjectIds.Split(',').Select(int.Parse).ToList();
            }
            else
            {
                var projects = await _projectsService.GetProjectListByEmployee(currEmployee);
                if (projects != null)
                    foreach (var project in projects)
                    {
                        projectList.Add(project.Id);
                    }
             
            }

            var timeSheetReports = await _timeSheetsService.GetAllEmployeePerformanceReportForParentTaskAsync(employeeId, From, To, projectList);

                var totalTask = 0;
                var totalDeliveredOnTime = 0;
                int overDueTask = 0;
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
                    if (overEstimation != 0)
                    {
                        if (overDuePercentage != overEstimation)
                        {
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
                                                                


                    if (overDuePercentage == overDueThresholds[0])
                        firstOverDueThresholdCount++;
                    else if (overDuePercentage == overDueThresholds[1])
                        secondOverDueThresholdCount++;
                    else if (overDuePercentage == overDueThresholds[2])
                        thirdOverDueThresholdCount++;
                    if (await _workflowStatusService.IsTaskOverdue(task.Id))
                        overDueTask++;


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
                        totalSpentHours += (decimal)spentTime; // Cast spentTime to decimal
                        extraTime += (decimal)(spentTime - estimatedHours >= 0 ? spentTime - estimatedHours : 0); // Cast the result to decimal
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
                    OverDueTask = overDueTask,
                                                                 
                    FirstOverDueThreshold = overDueThresholds[0],
                    SecondOverDueThreshold = overDueThresholds[1],
                    ThirdOverDueThreshold = overDueThresholds[2],
                    FirstOverDueThresholdCount = firstOverDueThresholdCount,
                    SecondOverDueThresholdCount = secondOverDueThresholdCount,
                    ThirdOverDueThresholdCount = thirdOverDueThresholdCount,
                    AvgWorkQuality = avgWorkQuality,

                });
            }

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

          
            [HttpPost]
            public virtual async Task<IActionResult> ReportByEmployee(TimesheetReportSearchModel searchModel)
            {
   
                //prepare model
                var model = await _reportsModelFactory.PrepareTimesheetReportListModelAsync(searchModel);

                return Json(model);
            }

            public virtual async Task<IActionResult> GetReportByEmployee(int searchEmployeeId, DateTime From, DateTime To, int ShowById, int ProjectId, int HoursId)
            {

                //prepare model
                var timeSheetReports = await _timeSheetsService.GetReportByEmployeeListAsync(searchEmployeeId, From, To, ShowById, ProjectId, HoursId);
                var holidayList = await _holidayService.GetAllHolidaysAsync();
                var employee = await _employeeService.GetEmployeeByIdAsync(searchEmployeeId);
                // Attach the employee name to each timesheet report entry
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


            public virtual async Task<IActionResult> GetAllEmployeesData(DateTime From, DateTime To, int ShowById, int ProjectId, int HoursId)
            {
                // Get all employee IDs
                var allEmployees = await _employeeService.GetAllEmployeesAsync();
                var holidayList = await _holidayService.GetAllHolidaysAsync();
                var allReports = new List<TimeSheetReport>(); 

                // Loop through all employees and fetch their timesheet data
                foreach (var employee in allEmployees)
                {
                    var timeSheetReports = await _timeSheetsService.GetReportByEmployeeListAsync(employee.Id, From, To, ShowById, ProjectId, HoursId);

                    // Attach the employee name to each timesheet report entry
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

           

            public virtual async Task<IActionResult> GetReportByProject(int searchEmployeeId, DateTime From, DateTime To, int ShowById, int ProjectId, int HoursId)
            {

                //prepare model
                var timeSheetReports = await _timeSheetsService.GetReportByProjectListAsync(searchEmployeeId, From, To, ShowById, ProjectId, HoursId);

                var project = await _projectsService.GetProjectsByIdAsync(ProjectId);
                var holidayList = await _holidayService.GetAllHolidaysAsync();
                // Attach the employee name to each timesheet report entry
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
                // Get all employee IDs
                var allProjects = await _projectsService.GetAllProjectsAsync("");
                var holidayList = await _holidayService.GetAllHolidaysAsync();
                var allReports = new List<TimeSheetReport>(); 

                // Loop through all employees and fetch their timesheet data
                foreach (var project in allProjects)
                {
                    var timeSheetReports = await _timeSheetsService.GetReportByProjectListAsync(EmployeeId, From, To, ShowById, project.Id, HoursId);

                    // Attach the employee name to each timesheet report entry
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

        [HttpPost]
            public virtual async Task<IActionResult> GetReportByTask( DateTime From, DateTime To, int ShowById, int ProjectId, string employeeIds,int HoursId, int TaskId, bool IsHideEmpty,int currCustomer)
            {
            var employeeList = new List<int>();
            if (employeeIds != "0")
            {
                employeeList = employeeIds.Split(',').Select(int.Parse).ToList();
            }
            else
            {
                IList<int> juniorIds = await _projectEmployeeMappingService.GetJuniorsIdsByEmployeeIdAsync(currCustomer);
                employeeList = juniorIds.ToList();
            }

            //var customer = await _workContext.GetCurrentCustomerAsync();

            //if (customer != null)
            //{
            //    var employee = await _employeeService.GetEmployeeByCustomerIdAsync(customer.Id);
            //    if (employee != null)

            //        employeeList.Add(employee.Id);
            //}

            //prepare model
            var timeSheetReports = await _timeSheetsService.GetReportByTaskListAsync(employeeList, From, To, ShowById, ProjectId, HoursId, TaskId, IsHideEmpty);
                var holidayList = await _holidayService.GetAllHolidaysAsync();
                var task = await _projectTaskService.GetProjectTasksByIdAsync(TaskId);
                // Attach the employee name to each timesheet report entry
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

                return Json(timeSheetReports);
            }

            public virtual async Task<IActionResult> GetTasksFromProjects(string projectIds)
            {
                var projectIdList = projectIds.Split(',').Select(int.Parse).ToList();

                var allTasks = new List<App.Core.Domain.ProjectTasks.ProjectTask>(); 
                                                                                     
                foreach (var projectId in projectIdList)
                {
                    var tasks = await _projectTaskService.GetProjectTasksByProjectId(projectId);  
                    allTasks.AddRange(tasks);
                }
                // Prepare the JSON response with the required fields(TaskId and TaskName)
                var taskList = allTasks.Select(t => new { TaskId = t.Id, TaskName = t.TaskTitle }).ToList().Distinct();

               
                return Json(taskList);
            }

        [HttpPost]
            public virtual async Task<IActionResult> GetAllTasksData(DateTime From, DateTime To, int ShowById, int HoursId, string projectIds, string employeeIds, int currCustomer)
            {

            List<int> projectIdList = new List<int>();

            // If projectIds is empty or has less than 1 project ID, consider all projects

            if (projectIds != "0")
            {
                projectIdList = projectIds.Split(',').Select(int.Parse).ToList();
            }
            else
            {
                var employeeProject = await _projectsService.GetProjectListByEmployee(currCustomer);
                if (employeeProject != null)
                    foreach (var project in employeeProject)
                    {
                        projectIdList.Add(project.Id);
                    }
            }

            var employeeIdList = new List<int>();
            if (employeeIds != "0")
            {
                employeeIdList = employeeIds.Split(',').Select(int.Parse).ToList();
            }
            else
            {
                IList<int> juniorIds = await _projectEmployeeMappingService.GetJuniorsIdsByEmployeeIdAsync(currCustomer);
                employeeIdList = juniorIds.ToList();
            }
            //var customer = await _workContext.GetCurrentCustomerAsync();

            //if (customer != null)
            //{
            //    var employee = await _employeeService.GetEmployeeByCustomerIdAsync(customer.Id);
            //    if (employee != null)

            //        employeeIdList.Add(employee.Id);
            //}


            // Rest of the method remains the same
            var allTasks = await _projectTaskService.GetAllProjectTasksByDateAsync(From, To);



                var holidayList = await _holidayService.GetAllHolidaysAsync();
                var allReports = new List<TimeSheetReport>();

                if (projectIdList.Count == 0 || projectIdList.Contains(0)) // Check if "All" projects is selected
                {
                    foreach (var task in allTasks)
                    {
                        var timeSheetReports = await _timeSheetsService.GetAllReportByTaskListAsync(employeeIdList, From, To, ShowById, 0, HoursId, task.Id);

                        // Attach the employee name to each timesheet report entry
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
                        // Check if the task's project ID is in the list of selected project IDs
                        if (projectIdList.Contains(task.ProjectId))
                        {
                            var timeSheetReports = await _timeSheetsService.GetAllReportByTaskListAsync(employeeIdList, From, To, ShowById, task.ProjectId, HoursId, task.Id);

                            // Attach the employee name to each timesheet report entry
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


        [HttpPost]
            public virtual async Task<IActionResult> GetAllEmployeesWithProjectsData(DateTime From, DateTime To, int ShowById, string ProjectIds, int HoursId, string TaskIds, int currCustomer)
            {
            var employeeIdList = new List<int>();
          
            
                IList<int> juniorIds = await _projectEmployeeMappingService.GetJuniorsIdsByEmployeeIdAsync(currCustomer);
            if(juniorIds !=null)
                employeeIdList = juniorIds.ToList();
            
            // Get all employee IDs
            var allEmployees = await _employeeService.GetAllEmployeesAsync();
                var holidayList = await _holidayService.GetAllHolidaysAsync();
                var allReports = new List<TimeSheetReport>(); // Assuming TimeSheetReport is the model for the timesheet data
                List<int> projectIdList = new List<int>();

                // If projectIds is empty or has less than 1 project ID, consider all projects
                if (string.IsNullOrEmpty(ProjectIds) || ProjectIds.Split(',').Count() < 1 || ProjectIds == "0")
                {
                var employeeProject = await _projectsService.GetProjectListByEmployee(currCustomer);
                if (employeeProject != null)
                    foreach (var project in employeeProject)
                    {
                        projectIdList.Add(project.Id);
                    }
            }
                else
                {
                    projectIdList = ProjectIds.Split(',').Select(int.Parse).ToList();
                }
                List<int> taskIdList = TaskIds.Split(',').Select(int.Parse).ToList();
                // Loop through all employees and fetch their timesheet data
                foreach (var employeeId in employeeIdList)
                {
                    var timeSheetReports = await _timeSheetsService.GetReportByEmployeeListWithProjectsAsync(employeeId, From, To, ShowById, projectIdList, HoursId, taskIdList);

                    // Attach the employee name to each timesheet report entry
                    timeSheetReports.ForEach(async report =>
                    {
                        var employee = await _employeeService.GetEmployeeByIdAsync(employeeId);
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
        [HttpPost]
        public virtual async Task<IActionResult> GetAllProjectsDataForSummary(DateTime From, DateTime To, int ShowById, int HoursId, string TaskIds, string EmployeeIds,int currCustomer)
        {
            // Get all employee IDs
           
            var employeeProjects = await _projectsService.GetProjectListByEmployee(currCustomer);
            var holidayList = await _holidayService.GetAllHolidaysAsync();
            var allReports = new List<TimeSheetReport>(); // Assuming TimeSheetReport is the model for the timesheet data
            var employeeIdList = new List<int>();
            if (EmployeeIds != "0")
            {
                employeeIdList = EmployeeIds.Split(',').Select(int.Parse).ToList();
            }
            else
            {
                IList<int> juniorIds = await _projectEmployeeMappingService.GetJuniorsIdsByEmployeeIdAsync(currCustomer);
                employeeIdList = juniorIds.ToList();
            }


            List<int> taskIdList = new List<int>();
            if (TaskIds !=null)
            taskIdList = TaskIds.Split(',').Select(int.Parse).ToList();
            // Loop through all employees and fetch their timesheet data
            if(employeeProjects !=null)
            foreach (var project in employeeProjects)
            {
                var timeSheetReports = await _timeSheetsService.GetReportByProjectListAsync(project.Id, From, To, ShowById, employeeIdList, HoursId, taskIdList);

                // Attach the employee name to each timesheet report entry
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

        [HttpPost]
        public virtual async Task<IActionResult> GetReportByWithProjectsEmployee(int searchEmployeeId, DateTime From, DateTime To, int ShowById, string ProjectIds, int HoursId, string TaskIds, int currCustomer)
            {
                List<int> projectIdList = new List<int>();

                // If projectIds is empty or has less than 1 project ID, consider all projects
                if (string.IsNullOrEmpty(ProjectIds) || ProjectIds.Split(',').Count() < 1 || ProjectIds == "0")
                {

                var employeeProject = await _projectsService.GetProjectListByEmployee(currCustomer);
                if(employeeProject != null)
                    foreach(var project in employeeProject)
                    {
                        projectIdList.Add(project.Id);
                    }
                  
                }
                else
                {
                    projectIdList = ProjectIds.Split(',').Select(int.Parse).ToList();
                }
                List<int> taskIdList = TaskIds.Split(',').Select(int.Parse).ToList();
                //prepare model
                var timeSheetReports = await _timeSheetsService.GetReportByEmployeeListWithProjectsAsync(searchEmployeeId, From, To, ShowById, projectIdList, HoursId, taskIdList);
                var holidayList = await _holidayService.GetAllHolidaysAsync();
                var employee = await _employeeService.GetEmployeeByIdAsync(searchEmployeeId);
                // Attach the employee name to each timesheet report entry
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

        [HttpPost]
        public virtual async Task<IActionResult> GetReportByProjectForSummary(int projectId, DateTime From, DateTime To, int ShowById, int HoursId, string TaskIds, string EmployeeIds, int currCustomer)
        {
            var employeeIdList = new List<int>();
            if (EmployeeIds != "0")
            {
                employeeIdList = EmployeeIds.Split(',').Select(int.Parse).ToList();
            }
            else
            {
                IList<int> juniorIds = await _projectEmployeeMappingService.GetJuniorsIdsByEmployeeIdAsync(currCustomer);
                employeeIdList = juniorIds.ToList();
            }
            //var customer = await _workContext.GetCurrentCustomerAsync();

            //if (customer != null)
            //{
            //    var employee = await _employeeService.GetEmployeeByCustomerIdAsync(customer.Id);
            //    if (employee != null)

            //        employeeIdList.Add(employee.Id);
            //}


            
            List<int> taskIdList = TaskIds.Split(',').Select(int.Parse).ToList();
            //prepare model
            var timeSheetReports = await _timeSheetsService.GetReportByProjectListAsync(projectId, From, To, ShowById, employeeIdList, HoursId, taskIdList);
            var holidayList = await _holidayService.GetAllHolidaysAsync();
            var project = await _projectsService.GetProjectsByIdAsync(projectId);
            // Attach the employee name to each timesheet report entry
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


        public virtual async Task<IActionResult> TimeSummaryReport()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.PublicStoreTimeSummaryReport, PermissionAction.View))
            {
                if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
                    return Challenge();

                return View("/Themes/DefaultClean/Views/Common/AccessDenied.cshtml");
            }

            //prepare model
            TimesheetReportSearchModel timesheetReportSearchModel = new TimesheetReportSearchModel();

            var customer = await _workContext.GetCurrentCustomerAsync();
            if (!await _customerService.IsRegisteredAsync(customer))
                return Challenge();

            if (customer != null)
            {
                var employee = await _employeeService.GetEmployeeByCustomerIdAsync(customer.Id);
                if (employee != null)
                    timesheetReportSearchModel.EmployeeId= employee.Id;


            }

            var model = await _reportsModelFactory.PrepareTimeSheetReportSearchModelAsync(timesheetReportSearchModel);


                return View("/Themes/DefaultClean/Views/Extension/Reports/TimeSummaryReport.cshtml", model);
            }

        [HttpPost]
            public virtual async Task<IActionResult> GetDateReportData(DateTime From, DateTime To, int ShowById, string projectIds, string taskIds,string EmployeeIds, int HoursId, int currCustomer)
            {

                List<int> projectIdList = new List<int>();

                // If projectIds is empty or has less than 1 project ID, consider all projects
                if (string.IsNullOrEmpty(projectIds) || projectIds.Split(',').Count() < 1 || projectIds == "0")
                {
                var employeeProject = await _projectsService.GetProjectListByEmployee(currCustomer);
                if (employeeProject != null)
                    foreach (var project in employeeProject)
                    {
                        projectIdList.Add(project.Id);
                    }
            }
                else
                {
                    projectIdList = projectIds.Split(',').Select(int.Parse).ToList();
                }
                List<int> taskIdList = new List<int>();
            var employeeIdList = new List<int>();
            if (EmployeeIds != "0")
            {
                employeeIdList = EmployeeIds.Split(',').Select(int.Parse).ToList();
            }
            else
            {
                IList<int> juniorIds = await _projectEmployeeMappingService.GetJuniorsIdsByEmployeeIdAsync(currCustomer);
                employeeIdList = juniorIds.ToList();
            }



            if (taskIds != null)
                    taskIdList = taskIds.Split(',').Select(int.Parse).ToList();


                //prepare model
                var timeSheetReports = await _timeSheetsService.GetReportByDateAsync(employeeIdList, From, To, projectIdList, HoursId, taskIdList);

            var reportTasks = timeSheetReports.Select(async report =>
            {   if (report != null)
                    {
                        var project = await _projectsService.GetProjectsByIdAsync(report.ProjectId);
                        if (project != null)
                            report.ProjectName = project.ProjectTitle;
                        var task = await _projectTaskService.GetProjectTasksByIdAsync(report.TaskId);
                        if (task != null)
                        {
                            report.TaskName = task.TaskTitle;
                        }
                        report.SpentDateFormat = report.SpentDate.ToString("d-MMM-yyyy");
                        //var employeeNames = new List<string>();
                        //foreach (var employeeId in report.EmployeeIds)
                        //{
                        var employee = await _employeeService.GetEmployeeByIdAsync(report.EmployeeId);
                        if (employee != null)
                        {
                            //employeeNames.Add(employee.FirstName + " " + employee.LastName);
                            report.EmployeeName = employee.FirstName + " " + employee.LastName;
                        }
                        //}
                        //report.EmployeeNames = string.Join(", ", employeeNames);
                    }
                });
            // Wait for all report processing tasks to complete
            await Task.WhenAll(reportTasks);


            return Json(timeSheetReports);
            }


        public virtual async Task<IActionResult> AttendanceReport()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.PublicStoreEmployeeAttendanceReport, PermissionAction.View))
            {
                if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
                    return Challenge();

                return View("/Themes/DefaultClean/Views/Common/AccessDenied.cshtml");
            }

            //prepare model
            TimesheetReportSearchModel timesheetReportSearchModel = new TimesheetReportSearchModel();

            var customer = await _workContext.GetCurrentCustomerAsync();
            if (!await _customerService.IsRegisteredAsync(customer))
                return Challenge();

            if (customer != null)
            {
                var employee = await _employeeService.GetEmployeeByCustomerIdAsync(customer.Id);
                if (employee != null)
                    timesheetReportSearchModel.EmployeeId = employee.Id;

            }

            //prepare model
            var model = await _reportsModelFactory.PrepareTimeSheetReportSearchModelAsync(timesheetReportSearchModel);
           


            return View("/Themes/DefaultClean/Views/Extension/Reports/AttendanceReport.cshtml", model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> GetEmployeeAtttendance(int searchEmployeeId, DateTime From, DateTime To, int ShowById)
        {

            //prepare model
            var timeSheetReports = await _timeSheetsService.GetAttendanceReportByEmployeeListAsync(searchEmployeeId, From, To);
            var holidayList = await _holidayService.GetAllHolidaysAsync();
           
            var employee = await _employeeService.GetEmployeeByIdAsync(searchEmployeeId);
            // Attach the employee name to each timesheet report entry
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


        [HttpPost]
        public virtual async Task<IActionResult> GetAllEmployeeAttendanceData(DateTime From, DateTime To, int ShowById, int currCustomer)
        {

            // Get all employee IDs
            var allEmployeeIds = await _projectEmployeeMappingService.GetJuniorsIdsByEmployeeIdAsync(currCustomer);
            var holidayList = await _holidayService.GetAllHolidaysAsync();
            var allReports = new List<TimeSheetReport>(); // Assuming TimeSheetReport is the model for the timesheet data

            // Loop through all employees and fetch their timesheet data
            foreach (var employeeId in allEmployeeIds)
            {
               

                var timeSheetReports = await _timeSheetsService.GetAttendanceReportByEmployeeListAsync(employeeId, From, To);

                // Attach the employee name to each timesheet report entry
                timeSheetReports.ForEach(async report =>
                {
                    var employee = await _employeeService.GetEmployeeByIdAsync(employeeId);

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

        [HttpPost]
        public async Task<IActionResult> GetEmployeeAttendanceSummary(string employeeIds, int currCustomer)
        {
            List<int> employeeIdList = new List<int>();

            if (employeeIds != null)
                employeeIdList = employeeIds.Split(',').Select(int.Parse).ToList();




            var currTime = await _dateTimeHelper.GetIndianTimeAsync();
            if (_leaveSettings.LeaveTestDate != DateTime.MinValue)
                currTime = _leaveSettings.LeaveTestDate;

            // Get all leave types
            var leaveTypes = await _leaveTypeService.GetLeaveTypesAsync();

            IList<Employee> employees = new List<Employee>();
            // Get all employees based on the provided IDs


            IList<int> employeeIdsList = new List<int>();
            if (employeeIdList.Contains(0))
            {
                employeeIdsList = await _projectEmployeeMappingService.GetJuniorsIdsByEmployeeIdAsync(currCustomer);
                if(employeeIdList !=null)
                foreach (var employeeId in employeeIdsList)
                {
                    var employee = await _employeeService.GetEmployeeByIdAsync(employeeId);
                    if(employee != null)
                    employees.Add(employee);
                }
            }
            else
            {
                employees = await _employeeService.GetEmployeeByIdsAsync(employeeIdList.ToArray());
            }


            // Initialize a list to store the leave balances
            var leaveBalances = new List<dynamic>();

            // Loop through each employee
            foreach (var employee in employees)
            {
                // Create a dynamic object to store leave balances for each employee
                var employeeLeaveBalance = new ExpandoObject() as IDictionary<string, object>;
                employeeLeaveBalance["EmployeeName"] = employee.FirstName + " " + employee.LastName;
                                                               
                // Loop through each leave type
                foreach (var leaveType in leaveTypes)
                {
                    // Get approved taken leaves for the current year for this employee and leave type
                    var approvedTakenLeaves = await _leaveManagementService
                        .GetApprovedTakenLeavesForCurrentYearAndEmployeeAsync(employee.Id, leaveType.Id);

                    // Calculate total taken leaves
                    var totalTakenLeaves = approvedTakenLeaves.Sum(l => l.NoOfDays);
                    decimal totalAllowed = 0;
                  
                    var leaveLog = await _leaveTransactionLogService.GetLeaveBalanceByLog(employee.Id, leaveType.Id);
                    if (leaveLog != null)
                        totalAllowed = await _leaveTransactionLogService.GetAddedLeaveBalanceForCurrentMonthForReport(employee.Id, leaveType.Id, currTime.Month, currTime.Year);


                    var remainingLeaves = totalAllowed;
                    // Calculate remaining leaves
                    //var remainingLeaves = leaveType.Total_Allowed - totalTakenLeaves;

                    // Add leave balance details to the employee object dynamically
                    //employeeLeaveBalance[$"Total{leaveType.Type}"] = leaveType.Total_Allowed;
                    employeeLeaveBalance[$"{leaveType.Type} Taken"] = totalTakenLeaves;
                    employeeLeaveBalance[$"{leaveType.Type} Remaining"] = remainingLeaves;
                }

                // Add the employee's leave balances to the main list
                leaveBalances.Add(employeeLeaveBalance);
            }

            string lastUpdateTime = _leaveSettings.LastUpdateBalance;
            // Return the data as JSON

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
        [HttpGet]
        public virtual async Task<IActionResult> GetPeriodDateRange(int periodId)
        {
            var (from, to) = await _timeSheetsService.GetDateRange(periodId);

            return Json(new
            {
                from = from?.ToString("yyyy-MM-dd"),
                to = to?.ToString("yyyy-MM-dd")
            });
        }

                 

        [HttpGet]
        public async Task<IActionResult> GetBugTasksByTaskId(int taskId)
        {
            var bugTasks = await _projectTaskService.GetBugChildTasksByParentTaskIdAsync(taskId);
            var modelList = new List<object>();

            foreach (var task in bugTasks)
            {
                var estimatedTimeFormat = await _timeSheetsService.ConvertToHHMMFormat(task.EstimatedTime);

                var devTime = await _timeSheetsService.GetDevelopmentTimeByTaskId(task.Id);
                var qaTime = await _timeSheetsService.GetQATimeByTaskId(task.Id);

                var spentTimeFormat = await _timeSheetsService.ConvertSpentTimeAsync(devTime.SpentHours, devTime.SpentMinutes);
                var qaTimeFormat = await _timeSheetsService.ConvertSpentTimeAsync(qaTime.SpentHours, qaTime.SpentMinutes);

                int spentTimeMin = devTime.SpentHours * 60 + devTime.SpentMinutes;
                int estTimeMin = (int)(task.EstimatedTime * 60);
                int extraTimeMin = Math.Max(spentTimeMin - estTimeMin, 0);

                int extraHours = extraTimeMin / 60;
                int extraMinutes = extraTimeMin % 60;
                var extraTime = await _timeSheetsService.ConvertSpentTimeAsync(extraHours, extraMinutes);

                string extraPercent = estTimeMin > 0 ? $"{(int)Math.Round((extraTimeMin * 100.0) / estTimeMin)}%" : "--";
                if (extraTimeMin > 0)
                    extraTime += $" ({extraPercent})";

                string statusName = "";
                var status = await _workflowStatusService.GetWorkflowStatusByIdAsync(task.StatusId);
                if (status != null)
                    statusName = status.StatusName + "|||" + status.ColorCode;

                string dueDateFormat = "";
                if (task.DueDate.HasValue)
                {
                    bool isOverdue = await _workflowStatusService.IsTaskOverdue(task.Id);
                    dueDateFormat = isOverdue
                        ? $"<span style='color: red; font-weight: bold;'>{task.DueDate.Value:dd-MMMM-yyyy}</span>"
                        : task.DueDate.Value.ToString("dd-MMMM-yyyy");
                }

                var parent = await _projectTaskService.GetProjectTasksByIdAsync(task.ParentTaskId);
                var parentTaskName = parent?.TaskTitle ?? "";

                var overduePercent = await _commonPluginService.GetOverduePercentageByTimeAsync(
                    spentTimeFormat, estimatedTimeFormat);

                modelList.Add(new
                {
                    TaskId= task.Id,
                    TaskName = task.TaskTitle,
                    EstimatedTimeFormat = estimatedTimeFormat,
                    SpentTimeFormat = spentTimeFormat,
                    QaTimeFormat = qaTimeFormat,
                    ExtraTime = extraTime,
                    DeliveredOnTime = task.DeliveryOnTime,
                    StatusName = statusName,
                    DueDateFormat = dueDateFormat,
                    ParentTaskName = parentTaskName,
                    DOTPercentage = task.DOTPercentage != null
    ? task.DOTPercentage
    : null,

                OverduePercentage = overduePercent
                });
            }

            return Json(modelList); 
        }

    }
}
