using App.Core.Domain.PerformanceMeasurements;
using App.Core.Domain.TimeSheets;
using App.Services.Employees;
using App.Services.Helpers;
using App.Services.Projects;
using App.Services.ProjectTasks;
using App.Services.TimeSheets;
using App.Web.Areas.Admin.Models.Extension.MonthlyPerformanceReports;

using Microsoft.AspNetCore.Mvc.Rendering;
using System.Threading.Tasks;
using System;
using App.Services;
using System.Linq;

using App.Web.Models.Extension.TimesheetReports;
using App.Web.Framework.Models.Extensions;
using App.Data.Extensions;

using System.Globalization;
using App.Core.Domain.Extension.TimeSheets;
using DocumentFormat.OpenXml.EMMA;

using App.Web.Models.Extensions.TimeSheets;
using App.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using App.Services.ProjectEmployeeMappings;
using Satyanam.Nop.Core.Services;

namespace App.Web.Factories.Extensions
{
    public partial class ReportsModelFactory : IReportsModelFactory
    {
        #region Fields

        private readonly IProjectsService _projectService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IEmployeeService _employeeService;
        private readonly ITimeSheetsService _timeSheetsService;
        private readonly IProjectEmployeeMappingService _projectEmployeeMappingService;
        private readonly IProjectTaskService _projectTaskService;
        private readonly IWorkflowStatusService _workflowStatusService;
        private readonly ICommonPluginService _commonPluginService;
        #endregion

        #region Ctor

        public ReportsModelFactory(IProjectsService projectsService,
            IDateTimeHelper dateTimeHelper,
            IEmployeeService employeeService,
            ITimeSheetsService timeSheetsService,
            IProjectEmployeeMappingService projectEmployeeMappingService,

            IProjectTaskService projectTaskService
,
            IWorkflowStatusService workflowStatusService,
            ICommonPluginService commonPluginService)
        {
            _projectService = projectsService;
            _dateTimeHelper = dateTimeHelper;
            _employeeService = employeeService;
            _timeSheetsService = timeSheetsService;
            _projectEmployeeMappingService = projectEmployeeMappingService;
            _projectTaskService = projectTaskService;
            _workflowStatusService = workflowStatusService;
            _commonPluginService = commonPluginService;
        }

        #endregion
        #region Utilities
        public virtual async Task PrepareEmployeeListAsync(TimesheetReportSearchModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            var employees = await _projectEmployeeMappingService.GetJuniorsIdsByEmployeeIdAsync(model.EmployeeId);
          
         if(employees.Count >1)
            model.AvailableEmployees.Add(new SelectListItem
            {
                Text = "All",
                Value = "0"
            });
            foreach (var p in employees)
            {
                var employee = await _employeeService.GetEmployeeByIdAsync(p);
                if (employee != null)
                    model.AvailableEmployees.Add(new SelectListItem
                {
                    Text = employee.FirstName + " " + employee.LastName,
                    Value = employee.Id.ToString()
                });
            }
        }


        public virtual async Task PrepareEmployeeListWithoutAllAsync(TimesheetReportSearchModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            var employees = await _projectEmployeeMappingService.GetJuniorsIdsByEmployeeIdAsync(model.EmployeeId);

            foreach (var p in employees)
            {
                var employee = await _employeeService.GetEmployeeByIdAsync(p);
                if (employee != null)
                    model.AvailableEmployees.Add(new SelectListItem
                    {
                        Text = employee.FirstName + " " + employee.LastName,
                        Value = employee.Id.ToString()
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
         
            model.AvailableProjects.Add(new SelectListItem
            {
                Text = "All",
                Value = "0"
            });
            
            var project = await _projectService.GetProjectListByEmployee(model.EmployeeId);
            if (project != null)
            {
                
                foreach (var p in project)
                {
                    model.AvailableProjects.Add(new SelectListItem
                    {
                        Text = p.ProjectTitle,
                        Value = p.Id.ToString()
                    });
                }
            }
        }


        public virtual async Task PrepareProjectListWithAllAsync(TimesheetReportSearchModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));
          
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

        public virtual async Task PrepareOverEstimationListAsync(TimesheetReportSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var overEstimations = await _commonPluginService.GetOverDueEmailPercentage();
            searchModel.AvailableOverEstimations.Add(new SelectListItem
            {
                Text = "All",
                Value = "0"
            });

            foreach (var overEstimation in overEstimations)
            {
                searchModel.AvailableOverEstimations.Add(new SelectListItem
                {
                    Text = overEstimation.ToString() + "%",
                    Value = overEstimation.ToString()
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
                    var employeeReportModel = new TimesheetReportModel();

                    employeeReportModel.SpentDate = timeSheetReport.SpentDate;
                    employeeReportModel.SpentTime = timeSheetReport.SpentHours;
                    employeeReportModel.SpentDateFormat = timeSheetReport.SpentDate.ToString("dd-MMM-yyyy");

                    // Add a new column to display the weekday in three-character format
                    employeeReportModel.WeekDay = timeSheetReport.SpentDate.DayOfWeek.ToString();

                    if (timeSheetReport.TotalSpentHours > 0)
                        employeeReportModel.TotalSpentTime = $"Total: {timeSheetReport.TotalSpentHours.ToString("F2")}";
                 
                    return employeeReportModel;
                }).Where(x => x != null);
            });//prepare grid model

            
            return model;
        }  


        public virtual async Task<TimesheetReportSearchModel> PrepareTimeSheetSearchModelAsync(TimesheetReportSearchModel searchModel)
        {
            await PrepareEmployeeListWithoutAllAsync(searchModel);
            searchModel.SetGridPageSize();
            var month = await MonthEnum.Select.ToSelectListAsync();
            await PrepareProjectListAsync(searchModel);
            var periods = await SearchPeriodEnum.Today.ToSelectListAsync();

            searchModel.PeriodList = periods.Select(store => new SelectListItem
            {
                Value = store.Value,
                Text = store.Text,
                Selected = searchModel.SearchPeriodId.ToString() == store.Value
            }).ToList();
            await PrepareOverEstimationListAsync(searchModel);
            return searchModel;
        }

        public virtual async Task<TimesheetReportListModel> PrepareMonthlyPerformanceReportListModelAsync
     (TimesheetReportSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get timesheet reports
            var timeSheetReports = await _timeSheetsService.GetAllEmployeePerformanceReportForParentTaskAsync(searchModel.SearchEmployeeId, searchModel.From, searchModel.To, searchModel.SelectedProjectIds);


            var model = await new TimesheetReportListModel().PrepareToGridAsync(searchModel, timeSheetReports, () =>
            {
                return timeSheetReports.SelectAwait(async timeSheetReport =>
                {
                    if (timeSheetReport.EmployeeId == 0)
                    {
                        return null;
                    }
                    TimesheetReportModel monthlyReportModel = new TimesheetReportModel();
                    //var monthlyReportModel = timeSheetReport.ToModel<TimesheetReportModel>();
                    var project = await _projectService.GetProjectsByIdAsync(timeSheetReport.ProjectId);
                    if (project == null)
                        return null;

                    monthlyReportModel.ProjectName = project.ProjectTitle;

                    var task = await _projectTaskService.GetProjectTasksWithoutCacheByIdAsync(timeSheetReport.TaskId);

                    if (task == null)
                        return null;
                    monthlyReportModel.TaskId = task.Id;
                    monthlyReportModel.TaskName = task.TaskTitle;

                    monthlyReportModel.EstimatedTime = task.EstimatedTime;
                    monthlyReportModel.BugCount = task.BugCount;
                    monthlyReportModel.QualityComments = task.QualityComments;

                    //monthlyReportModel.SpentHours = await _timeSheetsService.get(timeSheetReport.EmployeeId,timeSheetReport.TaskId);
                    var developmentTime = await _timeSheetsService.GetDevelopmentTimeByTaskId(task.Id);
               
                    monthlyReportModel.SpentTimeFormat = await _timeSheetsService.ConvertSpentTimeAsync(developmentTime.SpentHours,developmentTime.SpentMinutes);

                    // Convert SpentTime and EstimatedTime to total minutes
                    int spentTimeInMinutes = (developmentTime.SpentHours * 60) + developmentTime.SpentMinutes;
                    int estimatedTimeInMinutes = (int)(monthlyReportModel.EstimatedTime * 60);

                    // Calculate ExtraTime in minutes 
                    int extraTimeInMinutes = Math.Max(spentTimeInMinutes - estimatedTimeInMinutes, 0);

                    // Convert ExtraTime to HH:MM format
                    int extraHours = extraTimeInMinutes / 60;
                    int extraMinutes = extraTimeInMinutes % 60;
                    monthlyReportModel.ExtraTime = await _timeSheetsService.ConvertSpentTimeAsync(extraHours, extraMinutes);
                    monthlyReportModel.EstimatedTimeFormat = await _timeSheetsService.ConvertToHHMMFormat(monthlyReportModel.EstimatedTime);
                

                    //monthlyReportModel.DeliveredOnTime = monthlyReportModel.SpentHours <= (monthlyReportModel.EstimatedTime + (monthlyReportModel.EstimatedTime * (monthlyReportModel.AllowedVariations / 100)));
                    monthlyReportModel.DeliveredOnTime = task.DeliveryOnTime;

                    monthlyReportModel.DueDateFormat = task.DueDate.HasValue
          ? await _workflowStatusService.IsTaskOverdue(task.Id)
              ? $"<span style='color: red;'>{task.DueDate.Value:dd-MMMM-yyyy}</span>"
              : task.DueDate.Value.ToString("dd-MMMM-yyyy")
          : "";

                    var status = await _workflowStatusService.GetWorkflowStatusByIdAsync(task.StatusId);
                    if (status != null)
                        monthlyReportModel.StatusName = status.StatusName + "|||" + status.ColorCode;

                    string extraPercentage = estimatedTimeInMinutes > 0
                                     ? $"{(int)Math.Round((extraTimeInMinutes * 100.0) / estimatedTimeInMinutes, 0)}%"
                                     : "--";

                    if (extraTimeInMinutes > 0)
                        monthlyReportModel.ExtraTime = $"{monthlyReportModel.ExtraTime}({extraPercentage})";

                    monthlyReportModel.OverduePercentage = await _commonPluginService.GetOverduePercentageByTimeAsync(monthlyReportModel.SpentTimeFormat, monthlyReportModel.EstimatedTimeFormat);
                    var BugTime = await _projectTaskService.GetBugTimeByTaskIdAsync(task.Id);
                    monthlyReportModel.BugTime = await _timeSheetsService.ConvertSpentTimeAsync(BugTime.Hours, BugTime.Minutes);

                    var qaTime = await _timeSheetsService.GetQATimeByTaskId(task.Id);
                    monthlyReportModel.QaTime = await _timeSheetsService.ConvertSpentTimeAsync(qaTime.SpentHours, qaTime.SpentMinutes);

                    if(task.WorkQuality != null)
                    monthlyReportModel.WorkQuality = task.WorkQuality;
                    if(task.DOTPercentage !=null)
                    monthlyReportModel.DOTPercentage = task.DOTPercentage;
                    monthlyReportModel.HasBugTasks = await _projectTaskService.HasBugTasksAsync(task.Id);


                    return monthlyReportModel;
                }).Where(x => x != null);
            });

            if (searchModel.OverEstimation != 0)
            {
                model.Data = model.Data.Where(r => r.OverduePercentage == searchModel.OverEstimation);
            }

            return model;
        }

        #endregion
    }
}
