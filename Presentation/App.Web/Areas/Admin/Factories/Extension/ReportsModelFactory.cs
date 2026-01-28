using App.Core.Domain.EmployeeAttendances;
using App.Core.Domain.Employees;
using App.Core.Domain.Extension.TimeSheets;
using App.Core.Domain.PerformanceMeasurements;
using App.Core.Domain.Projects;
using App.Core.Domain.ProjectTasks;
using App.Core.Domain.TimeSheets;
using App.Data.Extensions;
using App.Services;
using App.Services.Activities;
using App.Services.ActivityEvents;
using App.Services.EmployeeAttendances;
using App.Services.Employees;
using App.Services.Helpers;
using App.Services.Leaves;
using App.Services.Projects;
using App.Services.ProjectTasks;
using App.Services.TimeSheets;
using App.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using App.Web.Areas.Admin.Models.EmployeeAttendances;
using App.Web.Areas.Admin.Models.Extension.ActivityTracking;
using App.Web.Areas.Admin.Models.Extension.MonthlyPerformanceReports;
using App.Web.Areas.Admin.Models.Extension.TimesheetReports;
using App.Web.Areas.Admin.Models.TimeSheets;
using App.Web.Framework.Models.Extensions;
using DocumentFormat.OpenXml.EMMA;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Azure;
using Newtonsoft.Json;
using Satyanam.Nop.Core.Domains;
using Satyanam.Nop.Core.Models;
using Satyanam.Nop.Core.Services;
using StackExchange.Profiling.Internal;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace App.Web.Areas.Admin.Factories.Extension
{
    public partial class ReportsModelFactory : IReportsModelFactory
    {
        #region Fields

        private readonly IProjectsService _projectService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IEmployeeService _employeeService;
        private readonly ITimeSheetsService _timeSheetsService;
        private readonly IBaseAdminModelFactory _baseAdminModelFactory;
        private readonly IProjectTaskService _projectTaskService;
        private readonly IActivityService _activityService;
        private readonly IActivityEventService _activityEventService;
        private readonly ILeaveManagementService _leaveManagementService;
        private readonly IEmployeeAttendanceService _employeeAttendanceService;
        private readonly ILeaveTypeService _leaveTypeService;
        private readonly IActivityTrackingService _activityTrackingService;
        #endregion

        #region Ctor

        public ReportsModelFactory(IProjectsService projectsService,
            IDateTimeHelper dateTimeHelper,
            IEmployeeService employeeService,
            ITimeSheetsService timeSheetsService,
            IBaseAdminModelFactory baseAdminModelFactory,
            IProjectTaskService projectTaskService,
            IActivityService activityService,
            IActivityEventService activityEventService
,
            ILeaveManagementService leaveManagementService,
            IEmployeeAttendanceService employeeAttendanceService,
            ILeaveTypeService leaveTypeService,
            IActivityTrackingService activityTrackingService)
        {
            _projectService = projectsService;
            _dateTimeHelper = dateTimeHelper;
            _employeeService = employeeService;
            _timeSheetsService = timeSheetsService;
            _baseAdminModelFactory = baseAdminModelFactory;
            _projectTaskService = projectTaskService;
            _activityService = activityService;
            _activityEventService = activityEventService;
            _leaveManagementService = leaveManagementService;
            _employeeAttendanceService = employeeAttendanceService;
            _leaveTypeService = leaveTypeService;
            _activityTrackingService = activityTrackingService;
        }

        #endregion
        #region Utilities
        public virtual async Task PrepareEmployeeListAsync(TimesheetReportSearchModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            var employeeName = "";
            var employees = await _employeeService.GetAllEmployeeNameAsync(employeeName);



            foreach (var p in employees)
            {
                model.AvailableEmployees.Add(new SelectListItem
                {
                    Text = p.FirstName + " " + p.LastName,
                    Value = p.Id.ToString()
                });
            }
            model.AvailableEmployees.Add(new SelectListItem
            {
                Text = "All",
                Value = "0"
            });

        }

        public virtual async Task PrepareEmployeeListAsync(ActivityTrackingSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var employeeName = "";
            var employees = await _employeeService.GetAllEmployeeNameAsync(employeeName);


            
            foreach (var p in employees)
            {
                searchModel.AvailableEmployees.Add(new SelectListItem
                {
                    Text = p.FirstName + " " + p.LastName,
                    Value = p.Id.ToString()
                });
            }


        }
        public virtual async Task PrepareEmployeeListWithAllAsync(TimesheetReportSearchModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            var employeeName = "";
            var employees = await _employeeService.GetAllEmployeeNameAsync(employeeName);
            model.AvailableEmployees.Add(new SelectListItem
            {
                Text = "All",
                Value = "0"
            });
            foreach (var p in employees)
            {
                model.AvailableEmployees.Add(new SelectListItem
                {
                    Text = p.FirstName + " " + p.LastName,
                    Value = p.Id.ToString()
                });
            }
        }
        public virtual async Task PrepareProjectListAsync(TimeSheetModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));
            //model.Projects.Add(new SelectListItem
            //{
            //    Text = "Select",
            //    Value = null
            //});
            var projectName = "";
            var project = await _projectService.GetAllProjectsAsync(projectName);
            foreach (var p in project)
            {
                model.Projects.Add(new SelectListItem
                {
                    Text = p.ProjectTitle,
                    Value = p.Id.ToString()
                });
            }
        }

        public virtual async Task PrepareProjectListAsync(TimesheetReportSearchModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));
            //model.Projects.Add(new SelectListItem
            //{
            //    Text = "Select",
            //    Value = null
            //});

            var projectName = "";
            var project = await _projectService.GetAllProjectsAsync(projectName);
            foreach (var p in project)
            {
                model.AvailableProjects.Add(new SelectListItem
                {
                    Text = p.ProjectTitle,
                    Value = p.Id.ToString()
                });
            }
            model.AvailableProjects.Add(new SelectListItem
            {
                Text = "All",
                Value = "0"
            });
        }


        public virtual async Task PrepareProjectListWithAllAsync(TimesheetReportSearchModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));
            //model.Projects.Add(new SelectListItem
            //{
            //    Text = "Select",
            //    Value = null
            //});
            model.AvailableProjects.Add(new SelectListItem
            {
                Text = "All",
                Value = "0"
            });
            var projectName = "";
            var project = await _projectService.GetAllProjectsAsync(projectName);
            foreach (var p in project)
            {
                model.AvailableProjects.Add(new SelectListItem
                {
                    Text = p.ProjectTitle,
                    Value = p.Id.ToString()
                });
            }
        }

        public virtual async Task PrepareYearrListAsync(MonthlyPerformanceReportSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));
            searchModel.Years.Add(new SelectListItem
            {
                Text = "Select",
                Value = "0"
            });
            int startYear = DateTime.Now.Year - 110; // Start from 110 years ago
            int currentYear = DateTime.Now.Year;

            // Iterate over the range of years and add them to the model
            for (int year = currentYear; year >= startYear; year--)
            {
                searchModel.Years.Add(new SelectListItem
                {
                    Text = year.ToString(),
                    Value = year.ToString()
                });
            }
        }


        #endregion

        #region Methods

        public virtual async Task<TimesheetReportSearchModel> PrepareTimeSheetReportSearchModelAsync(TimesheetReportSearchModel searchModel)
        {
            await PrepareEmployeeListAsync(searchModel);
            searchModel.SetGridPageSize();
            var showBy = await ShowByEnum.Daily.ToSelectListAsync();
            var hours = await HoursEnum.TotalHours.ToSelectListAsync();
            var periods = await SearchPeriodEnum.Today.ToSelectListAsync();

            searchModel.PeriodList = periods.Select(store => new SelectListItem
            {
                Value = store.Value,
                Text = store.Text,
                Selected = searchModel.SearchPeriodId.ToString() == store.Value
            }).ToList();


            searchModel.ShowByList = showBy.Select(store => new SelectListItem
            {
                Value = store.Value,
                Text = store.Text,
                Selected = searchModel.ShowById.ToString() == store.Value
            }).ToList();

            searchModel.HoursList = hours.Select(store => new SelectListItem
            {
                Value = store.Value,
                Text = store.Text,
                Selected = searchModel.HoursId.ToString() == store.Value
            }).ToList();
            searchModel.IsHideEmpty = false;
            await PrepareProjectListAsync(searchModel);
            return searchModel;
        }

        public virtual async Task<TimesheetReportSearchModel> PrepareTimeSheetReportSearchModelForProjectAsync(TimesheetReportSearchModel searchModel)
        {
            await PrepareEmployeeListWithAllAsync(searchModel);
            searchModel.SetGridPageSize();
            var showBy = await ShowByEnum.Daily.ToSelectListAsync();
            var hours = await HoursEnum.TotalHours.ToSelectListAsync();
            var periods = await SearchPeriodEnum.Today.ToSelectListAsync();

            searchModel.PeriodList = periods.Select(store => new SelectListItem
            {
                Value = store.Value,
                Text = store.Text,
                Selected = searchModel.SearchPeriodId.ToString() == store.Value
            }).ToList();

            searchModel.ShowByList = showBy.Select(store => new SelectListItem
            {
                Value = store.Value,
                Text = store.Text,
                Selected = searchModel.ShowById.ToString() == store.Value
            }).ToList();

            searchModel.HoursList = hours.Select(store => new SelectListItem
            {
                Value = store.Value,
                Text = store.Text,
                Selected = searchModel.HoursId.ToString() == store.Value
            }).ToList();

            await PrepareProjectListWithAllAsync(searchModel);
            return searchModel;
        }
        public virtual async Task<TimesheetReportSearchModel> PrepareTimeSheetReportSearchModelForTaskAsync(TimesheetReportSearchModel searchModel)
        {
            await PrepareEmployeeListAsync(searchModel);
            searchModel.SetGridPageSize();
            var showBy = await ShowByEnum.Daily.ToSelectListAsync();
            var hours = await HoursEnum.TotalHours.ToSelectListAsync();
            var periods = await SearchPeriodEnum.Today.ToSelectListAsync();

            searchModel.PeriodList = periods.Select(store => new SelectListItem
            {
                Value = store.Value,
                Text = store.Text,
                Selected = searchModel.SearchPeriodId.ToString() == store.Value
            }).ToList();

            searchModel.ShowByList = showBy.Select(store => new SelectListItem
            {
                Value = store.Value,
                Text = store.Text,
                Selected = searchModel.ShowById.ToString() == store.Value
            }).ToList();

            searchModel.HoursList = hours.Select(store => new SelectListItem
            {
                Value = store.Value,
                Text = store.Text,
                Selected = searchModel.HoursId.ToString() == store.Value
            }).ToList();

            await PrepareProjectListAsync(searchModel);
            return searchModel;
        }
        public virtual async Task<TimesheetReportListModel> PrepareTimesheetReportListModelAsync
     (TimesheetReportSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get timesheet reports
            var timeSheetReports = await _timeSheetsService.GetReportByEmployeeAsync(searchModel.SearchEmployeeId, searchModel.From, searchModel.To, showHidden: false,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            var model = await new TimesheetReportListModel().PrepareToGridAsync(searchModel, timeSheetReports, () =>
            {
                return timeSheetReports.SelectAwait(async timeSheetReport =>
                {
                    if (timeSheetReport.EmployeeId == 0)
                    {
                        return null;
                    }

                    var employeeReportModel = timeSheetReport.ToModel<TimesheetReportModel>();
                    employeeReportModel.SpentDate = timeSheetReport.SpentDate;
                    employeeReportModel.SpentTime = timeSheetReport.SpentHours;
                    employeeReportModel.SpentDateFormat = timeSheetReport.SpentDate.ToString("dd-MMM-yyyy");

                    // Add a new column to display the weekday in three-character format
                    employeeReportModel.WeekDay = timeSheetReport.SpentDate.DayOfWeek.ToString();

                    if (timeSheetReport.TotalSpentHours > 0)
                        employeeReportModel.TotalSpentTime = $"Total: {timeSheetReport.TotalSpentHours.ToString("F2")}";

                    return employeeReportModel;
                }).Where(x => x != null);
            });


            return model;
        }
        public virtual async Task<TimeSheetModel> PrepareTimeSheetModelAsync(TimeSheetModel model, TimeSheet timeSheet, bool excludeProperties = false)
        {
            if (timeSheet != null)
            {
                //fill in model values from the entity
                if (model == null)
                {
                    model = timeSheet.ToModel<TimeSheetModel>();
                    model.EmployeeId = timeSheet.EmployeeId;
                    model.ProjectId = timeSheet.ProjectId;
                    var employee = await _employeeService.GetEmployeeByIdAsync(model.EmployeeId);
                    if (employee != null)
                    {
                        model.EmployeeName = employee.FirstName + " " + employee.LastName;
                    }

                }

                var emp = await _employeeService.GetEmployeeByIdAsync(model.EmployeeId);

                if (emp != null)
                {
                    model.SelectedEmployeeId.Add(emp.Id);
                }
            }

            await _baseAdminModelFactory.PrepareEmployeeAsync(model.AvailableEmployees, false);
            foreach (var employeeItem in model.AvailableEmployees)
            {
                employeeItem.Selected = int.TryParse(employeeItem.Value, out var employeeId)
                    && model.SelectedEmployeeId.Contains(employeeId);
            }

            await PrepareProjectListAsync(model);
            return model;
        }



        public virtual async Task<TimesheetReportListModel> PrepareActivityLogReportListModelAsync
         (TimesheetReportSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get timesheet
            var timeSheet = await _timeSheetsService.GetAllTimeSheetAsync(employeeIds: searchModel.SelectedEmployeeIds, projectIds: searchModel.SelectedProjectIds,
                taskName: searchModel.TaskName, to: searchModel.To, from: searchModel.From, SelectedBillable: searchModel.BillableType, activityName: searchModel.ActivityName,
                showHidden: true,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);
            //prepare grid model
            var model = await new TimesheetReportListModel().PrepareToGridAsync(searchModel, timeSheet, () =>
            {
                return timeSheet.SelectAwait(async timesheet =>
                {
                    //fill in model values from the entity

                    var TimeSheetModel = new TimesheetReportModel();
                    TimeSheetModel.SpentDateFormat = timesheet.SpentDate.ToString("d-MMM-yyyy", CultureInfo.InvariantCulture);
                    TimeSheetModel.Id = timesheet.Id;
                    TimeSheetModel.SpentTimeFormat = await _timeSheetsService.ConvertSpentTimeAsync(timesheet.SpentHours, timesheet.SpentMinutes);
                    TimeSheetModel.EmployeeId = timesheet.EmployeeId;
                    TimeSheetModel.ProjectId = timesheet.ProjectId;
                    TimeSheetModel.ActivityId = timesheet.ActivityId;
                    TimeSheetModel.TaskId = timesheet.TaskId;

                    if (TimeSheetModel.EmployeeId != 0 || TimeSheetModel.EmployeeId != null)
                    {
                        Employee emp = await _employeeService.GetEmployeeByIdAsync(TimeSheetModel.EmployeeId);
                        if (emp != null)
                            TimeSheetModel.EmployeeName = emp.FirstName + " " + emp.LastName;
                    }
                    if (TimeSheetModel.ProjectId != 0)
                    {
                        Project project = await _projectService.GetProjectsByIdAsync(TimeSheetModel.ProjectId);
                        if (project != null)
                            TimeSheetModel.ProjectName = project.ProjectTitle;
                    }

                    if (TimeSheetModel.TaskId != 0)
                    {
                        ProjectTask projectTask = await _projectTaskService.GetProjectTasksByIdAsync(TimeSheetModel.TaskId);
                        if (projectTask != null)
                        {
                            TimeSheetModel.TaskName = projectTask.TaskTitle;
                            TimeSheetModel.EstimatedTime = projectTask.EstimatedTime;
                        }
                    }
                    if (TimeSheetModel.ActivityId != 0)
                    {
                        var activity = await _activityService.GetActivityByIdAsync(TimeSheetModel.ActivityId);
                        if (activity != null)
                            TimeSheetModel.ActivityName = activity.ActivityName;
                    }
                    if (timesheet.StartTime != null && timesheet.EndTime != null &&
      timesheet.StartTime != DateTime.MinValue && timesheet.EndTime != DateTime.MinValue)
                    {
                        TimeSheetModel.Time = $"{timesheet.StartTime:hh:mmtt} - {timesheet.EndTime:hh:mmtt}".ToLower();
                    }
                    var activityEvents = await _activityEventService.GetActivityEventsByTimeSheetIdAsync(timesheet.Id);
                    if (activityEvents.Any())
                    {
                        TimeSheetModel.MouseHits = activityEvents.FirstOrDefault().MouseHits;
                        TimeSheetModel.KeyboardHits = activityEvents.FirstOrDefault().KeyboardHits;

                    }


                    return TimeSheetModel;
                }).Where(x => x != null);
            });
            //prepare grid model
            return model;
        }

        public virtual async Task<TimesheetReportModel> PrepareViewScreenShotsModel(int timesheetId)
        {

            TimesheetReportModel timesheetReportModel = new TimesheetReportModel();

            var timesheet = await _timeSheetsService.GetTimeSheetByIdAsync(timesheetId);
            if (timesheet != null)
            {
                var task = await _projectTaskService.GetProjectTasksByIdAsync(timesheet.TaskId);
                if (task != null)
                {
                    timesheetReportModel.TaskName = task.TaskTitle;

                }
                var project = await _projectService.GetProjectsByIdAsync(timesheet.ProjectId);
                if (project != null)
                    timesheetReportModel.ProjectName = project.ProjectTitle;

                var activity = await _activityService.GetActivityByIdAsync(timesheet.ActivityId);
                if (activity != null)
                    timesheetReportModel.ActivityName = activity.ActivityName;

                var activityEvents = await _activityEventService.GetActivityEventsByTimeSheetIdAsync(timesheetId);

                if (activityEvents.Any())
                {
                    timesheetReportModel.Screenshots = new List<ScreenshotModel>();

                    foreach (var eventItem in activityEvents)
                    {
                        if (!string.IsNullOrEmpty(eventItem.JsonString))
                        {
                            // Deserialize the JSON array into objects
                            var screenshots = JsonConvert.DeserializeObject<List<ScreenshotModel>>(eventItem.JsonString);
                            if (screenshots != null)
                                timesheetReportModel.Screenshots.AddRange(screenshots);
                        }
                    }
                }
            }


            return timesheetReportModel;
        }
        #endregion

        #region Activity Tracking 
        public virtual async Task<ActivityTrackingSearchModel> PrepareActivityTrackingSearchModelAsync(ActivityTrackingSearchModel searchModel)
        {

            searchModel.SearchDate = await _dateTimeHelper.GetIndianTimeAsync();


            return searchModel;
        }


        public virtual async Task<ActivityTrackingListModel> PrepareActivityTrackingListModelAsync(ActivityTrackingSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));



            var employeeIds = (await _employeeService.GetAllEmployeeIdsAsync()).ToList();

            // Fetch all timesheet data once
            var timeSheetList = await _timeSheetsService.GetAllTimeSheetAsync(
                employeeIds: employeeIds,
                projectIds: null,
                taskName: null,
                to: searchModel.SearchDate,
                from: searchModel.SearchDate,
                SelectedBillable: 0,
                activityName: null,
                showHidden: true,
                pageIndex: 0,
                pageSize: int.MaxValue
            );

            // Prepare grid model
            var model = await new ActivityTrackingListModel().PrepareToGridAsync(searchModel, timeSheetList, () =>
            {
                return employeeIds.SelectAwait(async employeeId =>
                {
                    var timesheet = timeSheetList.FirstOrDefault(ts => ts.EmployeeId == employeeId && ts.SpentDate == searchModel.SearchDate && ts.StartTime != null);

                    var activityTrackingModel = new ActivityTrackingModel();

                    var employee = await _employeeService.GetEmployeeByIdAsync(employeeId);
                    if (employee != null)
                    {
                        activityTrackingModel.EmployeeName = employee.FirstName + " " + employee.LastName;
                        activityTrackingModel.EmployeeId = employeeId;
                        activityTrackingModel.TodayDate = searchModel.SearchDate.Value;

                        var timeNow = await _dateTimeHelper.GetIndianTimeAsync();
                        if (searchModel.SearchDate.Value.Date == timeNow.Date)
                        {
                            activityTrackingModel.ActivityStatusId = employee.ActivityTrackingStatusId;

                            if (activityTrackingModel.ActivityStatusId == 1)
                                activityTrackingModel.ActivityStatusName = ((ActivityTrackingEnum)activityTrackingModel.ActivityStatusId).ToString() + "|||#28a745";
                            else if (activityTrackingModel.ActivityStatusId == 2)
                                activityTrackingModel.ActivityStatusName = ((ActivityTrackingEnum)activityTrackingModel.ActivityStatusId).ToString() + "|||#ffc107";
                            else if (activityTrackingModel.ActivityStatusId == 3)
                                activityTrackingModel.ActivityStatusName = ((ActivityTrackingEnum)activityTrackingModel.ActivityStatusId).ToString() + "|||#dc3545";
                            else if (activityTrackingModel.ActivityStatusId == 4)
                                activityTrackingModel.ActivityStatusName = ((ActivityTrackingEnum)activityTrackingModel.ActivityStatusId).ToString() + "|||#808080";
                        }
                    }

                    var attendances = await _employeeAttendanceService.GetAllEmployeeAttendanceAsync(employeeId, searchModel.SearchDate, searchModel.SearchDate, 0);
                    var attendanceStatus = attendances.FirstOrDefault()?.StatusId ?? 0;
                    if (attendanceStatus == 0)
                        attendanceStatus = 1;
                    var statusEnum = (Core.Domain.EmployeeAttendances.StatusEnum)attendanceStatus;
                    activityTrackingModel.PresentStatus = statusEnum.ToString();

                    // Show leave info if on leave or half-day
                    if (statusEnum == Core.Domain.EmployeeAttendances.StatusEnum.Leave || statusEnum == Core.Domain.EmployeeAttendances.StatusEnum.HalfLeave)
                    {
                        var leaves = await _leaveManagementService.GetLeaveManagementsAsync(employeeId, 0, searchModel.SearchDate.Value, searchModel.SearchDate.Value, 2);


                        var leave = leaves.FirstOrDefault();
                        if (leave != null)
                        {
                            var leaveType = await _leaveTypeService.GetLeaveTypeByIdAsync(leave.LeaveTypeId);
                            string leaveTypeName = leaveType != null ? leaveType.Type : string.Empty;
                            activityTrackingModel.ShowLeaveInfo = true;
                            activityTrackingModel.LeaveInfo = $"<strong>Type:</strong> {leaveTypeName},<br/>" +
                                                              $"<strong>From:</strong> {leave.From:dd MMM yyyy}, <strong>To:</strong> {leave.To:dd MMM yyyy},<br/>" +
                                                             $"<strong>Days:</strong> {leave.NoOfDays.ToString("0.0")},<br/>" +
                                                              $"<strong>Reason:</strong> {leave.ReasonForLeave}";
                        }
                    }



                    if (timesheet != null)
                    {
                        var project = await _projectService.GetProjectsByIdAsync(timesheet.ProjectId);
                        if (project != null)
                            activityTrackingModel.ProjectName = project.ProjectTitle;

                        var task = await _projectTaskService.GetProjectTasksByIdAsync(timesheet.TaskId);
                        if (task != null)
                            activityTrackingModel.TaskName = task.TaskTitle;

                        var activity = await _activityService.GetActivityByIdAsync(timesheet.ActivityId);
                        if (activity != null)
                            activityTrackingModel.ActivityName = activity.ActivityName;


                        //var checkInCheckOut = await _timeSheetsService.GetCheckInCheckOutAsync(employeeId, searchModel.SearchDate.Value.Date);
                        //if (checkInCheckOut.FirstCheckIn.HasValue && checkInCheckOut.LastCheckOut.HasValue)
                        //{
                        //    activityTrackingModel.Time = $"{checkInCheckOut.FirstCheckIn.Value:hh:mm tt} - {checkInCheckOut.LastCheckOut.Value:hh:mm tt}";
                        //}
                        //else
                        //{
                        //    activityTrackingModel.Time = "N/A";
                        //}

                        var activityTracking = await _activityTrackingService.GetByEmployeeAndDateAsync(employeeId, searchModel.SearchDate.Value.Date);
                    if (activityTracking != null)
                        {
                            var istTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");

                            activityTracking.StartTime = TimeZoneInfo.ConvertTimeFromUtc(activityTracking.StartTime, istTimeZone);
                            activityTracking.EndTime = TimeZoneInfo.ConvertTimeFromUtc(activityTracking.EndTime, istTimeZone);


                            activityTrackingModel.Time = $"{activityTracking.StartTime:hh:mm tt} - {activityTracking.EndTime:hh:mm tt}";
                        }
                        else
                        {
                            activityTrackingModel.Time = "";
                        }

                        //var activityEvents = await _activityEventService.GetActivityEventsByTimeSheetIdAsync(timesheet.Id);

                        //var activityEvent = activityEvents.FirstOrDefault();

                        //if (activityEvent != null && !string.IsNullOrEmpty(activityEvent.JsonString))
                        //{ 
                        //    try
                        //    {
                        //        var jsonArray = JsonConvert.DeserializeObject<List<ActivityEventJsonModel>>(activityEvent.JsonString);
                        //        if (jsonArray != null && jsonArray.Any())
                        //        {
                        //            var lastEntry = jsonArray
                        //                .Where(x => x.CreateOnUtc != null)
                        //                .OrderByDescending(x => x.CreateOnUtc)
                        //                .FirstOrDefault();

                        //            if (lastEntry != null)
                        //            {
                        //                activityTrackingModel.ActivityStatusId = lastEntry.StatusId;

                        //                if (lastEntry.StatusId == 1)
                        //                    activityTrackingModel.ActivityStatusName = ((ActivityTrackingEnum)lastEntry.StatusId).ToString() + "|||#28a745";
                        //                else if (lastEntry.StatusId == 2)
                        //                    activityTrackingModel.ActivityStatusName = ((ActivityTrackingEnum)lastEntry.StatusId).ToString() + "|||#ffc107";
                        //                else if (lastEntry.StatusId == 3)
                        //                    activityTrackingModel.ActivityStatusName = ((ActivityTrackingEnum)lastEntry.StatusId).ToString() + "|||#dc3545";
                        //            }
                        //        }

                        //    }
                        //    catch (Exception ex)
                        //    {

                        //        activityTrackingModel.ActivityStatusId = 0;
                        //    }
                        //}

                    }
                    else
                    {
                        // No timesheet: show default/empty values
                        activityTrackingModel.Time = "";
                        activityTrackingModel.SpentTime = "00:00";

                    }
                    var spentList = await _timeSheetsService.GetReportByEmployeeListAsync(employeeId, searchModel.SearchDate, searchModel.SearchDate, 0, 0, 0);
                    var spent = spentList.FirstOrDefault();
                    activityTrackingModel.SpentTime = spent != null
                        ? await _timeSheetsService.ConvertSpentTimeAsync(spent.SpentHours, spent.SpentMinutes)
                        : "00:00";

                    return activityTrackingModel;

                });
            });

            return model;
        }

        #region Activity Tracking Report

        public virtual async Task<ActivityTrackingSearchModel> PrepareActivityTrackingReportSearchModelAsync(ActivityTrackingSearchModel searchModel)
        {
            await PrepareEmployeeListAsync(searchModel);
            var periods = await SearchPeriodEnum.Today.ToSelectListAsync();

            searchModel.PeriodList = periods.Select(store => new SelectListItem
            {
                Value = store.Value,
                Text = store.Text,
                Selected = searchModel.SearchPeriodId.ToString() == store.Value
            }).ToList();
            return searchModel;
        }


        public virtual async Task<ActivityTrackingListModel> PrepareActivityTrackingReportListModelAsync(ActivityTrackingSearchModel searchModel)
        {
            if(searchModel.SelectedEmployeeIds.Count == 0)
            {
                var allEmployeeIds = await _employeeService.GetAllEmployeeIdsAsync();

                searchModel.SelectedEmployeeIds = allEmployeeIds.ToList();
            }
            var activityTrackings = await _activityTrackingService.GetGroupedActivityTrackingSummaryAsync(searchModel.SelectedEmployeeIds, searchModel.From, searchModel.To);

            //prepare grid model
            var model = await new ActivityTrackingListModel().PrepareToGridAsync(searchModel, activityTrackings, () =>
            {
                return activityTrackings.SelectAwait(async activityTracking =>
                {
                    //fill in model values from the entity

                    var activityTrackingModel = new ActivityTrackingModel();
                   
                    activityTrackingModel.EmployeeId = activityTracking.EmployeeId;
                    var employee = await _employeeService.GetEmployeeByIdAsync(activityTrackingModel.EmployeeId);
                    if (employee != null)
                    {
                        activityTrackingModel.EmployeeName = employee.FirstName + " " + employee.LastName;
                        
                    }

                    activityTrackingModel.ActiveDurationHHMM = await _timeSheetsService.ConvertToHHMMFormat(activityTracking.ActiveDuration);
                    activityTrackingModel.AwayDurationHHMM = await _timeSheetsService.ConvertToHHMMFormat(activityTracking.AwayDuration);
                    activityTrackingModel.StoppedDurationHHMM = await _timeSheetsService.ConvertToHHMMFormat(activityTracking.StoppedDuration);
                    activityTrackingModel.OfflineDurationHHMM = await _timeSheetsService.ConvertToHHMMFormat(activityTracking.OfflineDuration);
                    activityTrackingModel.TotalDurationHHMM =await _timeSheetsService.ConvertToHHMMFormat(activityTracking.TotalDuration);

                    return activityTrackingModel;
                }).Where(x => x != null);
            });
            //prepare grid model
            return model;

        }


        public virtual async Task<IList<ActivityTrackingModel>> PrepareDateWiseActivityTrackingModelAsync(
     int employeeId, DateTime? from, DateTime? to)
        {
            if (!from.HasValue || !to.HasValue)
                return new List<ActivityTrackingModel>();

            var activityData = await _activityTrackingService.GetDateWiseStatusSummaryByEmployeeAsync(employeeId, from.Value, to.Value);

            var model = new List<ActivityTrackingModel>();
            foreach (var item in activityData)
            {
                model.Add(new ActivityTrackingModel
                {
                    Date = item.Date.ToString("yyyy-MM-dd"),
                    ActiveDurationHHMM = await _timeSheetsService.ConvertToHHMMFormat(item.ActiveDuration),
                    AwayDurationHHMM = await _timeSheetsService.ConvertToHHMMFormat(item.AwayDuration),
                    OfflineDurationHHMM = await _timeSheetsService.ConvertToHHMMFormat(item.OfflineDuration),
                    StoppedDurationHHMM = await _timeSheetsService.ConvertToHHMMFormat(item.StoppedDuration),
                    TotalDurationHHMM = await _timeSheetsService.ConvertToHHMMFormat(item.TotalDuration),
                    
                });
            }

            return model;
        }



        public virtual async Task<IList<ActivityTrackingModel>> PrepareSubgridDataAsync(
    int employeeId, DateTime from, DateTime to, ShowByEnum showBy)
        {
            var chunks = new List<(DateTime Start, DateTime End)>();

            if (showBy == ShowByEnum.Monthly)
            {
                var current = new DateTime(from.Year, from.Month, 1);
                while (current <= to)
                {
                    var end = current.AddMonths(1).AddDays(-1);
                    chunks.Add((current, end > to ? to : end));
                    current = current.AddMonths(1);
                }
            }
            else if (showBy == ShowByEnum.Weekly)
            {
                var start = from.Date;
                var end = to.Date;

                var current = start;

                while (current <= end)
                {
                    DateTime chunkStart = current;
                    DateTime chunkEnd;

                    // First chunk: if not Monday, go until next Sunday or end
                    if (chunkStart.DayOfWeek != DayOfWeek.Monday)
                    {
                        int daysToSunday = DayOfWeek.Sunday - chunkStart.DayOfWeek;
                        if (daysToSunday < 0) daysToSunday += 7;

                        chunkEnd = chunkStart.AddDays(daysToSunday);
                    }
                    else
                    {
                        // Regular Monday-starting week
                        chunkEnd = chunkStart.AddDays(6);
                    }

                    // Ensure chunkEnd does not go beyond the final `to` date
                    if (chunkEnd > end)
                        chunkEnd = end;

                    chunks.Add((chunkStart, chunkEnd));

                    // Move to the next day after this chunk
                    current = chunkEnd.AddDays(1);
                }
            }


            else if (showBy == ShowByEnum.Daily)
            {
                for (var date = from.Date; date <= to; date = date.AddDays(1))
                    chunks.Add((date, date));
            }

            var model = new List<ActivityTrackingModel>();

            foreach (var chunk in chunks)
            {
                var summary = await _activityTrackingService.GetSummaryByEmployeeAndDateRangeAsync(employeeId, chunk.Start, chunk.End);
                model.Add(new ActivityTrackingModel
                {
                    Label = showBy switch
                    {
                        ShowByEnum.Monthly => chunk.Start.ToString("MMMM yyyy"),
                        ShowByEnum.Weekly => $"{chunk.Start:dd-MMM-yyyy} to {chunk.End:dd-MMM-yyyy}",
                        ShowByEnum.Daily => chunk.Start.ToString("yyyy-MM-dd"),
                        _ => ""
                    },
                    EmployeeId = employeeId,
                    From = chunk.Start,
                    To = chunk.End,
                    ShowBy = showBy,
                    ActiveDurationHHMM = await _timeSheetsService.ConvertToHHMMFormat(summary.ActiveDuration),
                    AwayDurationHHMM = await _timeSheetsService.ConvertToHHMMFormat(summary.AwayDuration),
                    OfflineDurationHHMM = await _timeSheetsService.ConvertToHHMMFormat(summary.OfflineDuration),
                    StoppedDurationHHMM = await _timeSheetsService.ConvertToHHMMFormat(summary.StoppedDuration),
                    TotalDurationHHMM = await _timeSheetsService.ConvertToHHMMFormat(summary.TotalDuration)
                });
            }

            return model;
        }

        #endregion


        #endregion


    }
}
