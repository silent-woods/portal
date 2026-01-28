using App.Core;
using App.Core.Domain.Employees;
using App.Core.Domain.Extension.ProjectTasks;
using App.Core.Domain.Extension.TimeSheets;
using App.Core.Domain.Leaves;
using App.Core.Domain.PerformanceMeasurements;
using App.Core.Domain.Projects;
using App.Core.Domain.ProjectTasks;
using App.Core.Domain.TimeSheets;
using App.Data.Extensions;
using App.Services;
using App.Services.Employees;
using App.Services.Helpers;
using App.Services.Leaves;
using App.Services.Projects;
using App.Services.ProjectTasks;
using App.Services.TimeSheets;
using App.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using App.Web.Areas.Admin.Models.Extension.MonthlyPerformanceReports;
using App.Web.Areas.Admin.Models.Leavetypes;
using App.Web.Areas.Admin.Models.PerformanceMeasurements;
using App.Web.Areas.Admin.Models.TimeSheets;
using App.Web.Framework.Models.Extensions;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.EMMA;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Satyanam.Nop.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace App.Web.Areas.Admin.Factories.Extension
{
    public partial class MonthlyPerformanceReportModelFactory : IMonthlyPerformanceReportModelFactory
    {
        #region Fields

        private readonly IProjectsService _projectService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IEmployeeService _employeeService;
        private readonly ITimeSheetsService _timeSheetsService;
        private readonly IBaseAdminModelFactory _baseAdminModelFactory;
        private readonly IProjectTaskService _projectTaskService;
        private readonly IWorkflowStatusService _workflowStatusService;
        private readonly ICommonPluginService _commonPluginService;
        private readonly IProcessWorkflowService _processWorkflowService;
        #endregion

        #region Ctor

        public MonthlyPerformanceReportModelFactory(IProjectsService projectsService,
            IDateTimeHelper dateTimeHelper,
            IEmployeeService employeeService,
            ITimeSheetsService timeSheetsService,
            IBaseAdminModelFactory baseAdminModelFactory,
            IProjectTaskService projectTaskService
,
            IWorkflowStatusService workflowStatusService,
            ICommonPluginService commonPluginService,
            IProcessWorkflowService processWorkflowService)
        {
            _projectService = projectsService;
            _dateTimeHelper = dateTimeHelper;
            _employeeService = employeeService;
            _timeSheetsService = timeSheetsService;
            _baseAdminModelFactory = baseAdminModelFactory;
            _projectTaskService = projectTaskService;
            _workflowStatusService = workflowStatusService;
            _commonPluginService = commonPluginService;
            _processWorkflowService = processWorkflowService;
        }

        #endregion
        #region Utilities
        public virtual async Task PrepareEmployeeListAsync(MonthlyPerformanceReportSearchModel model)
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
        }
        public virtual async Task PrepareAllProcessWorkflowListAsync(MonthlyPerformanceReportSearchModel searchModel)
        {


            var workflows = await _processWorkflowService.GetAllProcessWorkflowsAsync("");
            searchModel.AvailableProcessWorkflow.Add(new SelectListItem
            {
                Text = "All",
                Value = null
            });
            foreach (var workflow in workflows)
            {
                searchModel.AvailableProcessWorkflow.Add(new SelectListItem
                {
                    Text = workflow.Name,
                    Value = workflow.Id.ToString()
                });
            }



        }

        public virtual async Task PrepareDeliveryOnTimeFilterListAsync(MonthlyPerformanceReportSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));



            searchModel.AvailableDeliveryOnTime.Add(new SelectListItem
            {
                Text = "All",
                Value = "0"
            });
            searchModel.AvailableDeliveryOnTime.Add(new SelectListItem
            {
                Text = "Yes",
                Value = "1"
            });
            searchModel.AvailableDeliveryOnTime.Add(new SelectListItem
            {
                Text = "No",
                Value = "2"
            });

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

        public virtual async Task PrepareProjectListAsync(MonthlyPerformanceReportSearchModel model)
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

        public virtual async Task PrepareOverEstimationListAsync(MonthlyPerformanceReportSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var overEstimations = await _commonPluginService.GetOverDueEmailPercentage();
            searchModel.AvailableOverEstimations.Add(new SelectListItem
            {
                Text = "All",
                Value = "0"
            });

            foreach(var overEstimation in overEstimations)
            {
                searchModel.AvailableOverEstimations.Add(new SelectListItem
                {
                    Text = overEstimation.ToString()+"%",
                    Value = overEstimation.ToString()
                });

            }
           
        }

        


        #endregion

        #region Methods

        public virtual async Task<MonthlyPerformanceReportSearchModel> PrepareTimeSheetSearchModelAsync(MonthlyPerformanceReportSearchModel searchModel)
        {
            await PrepareEmployeeListAsync(searchModel);
            searchModel.SetGridPageSize();
            var month = await MonthEnum.Select.ToSelectListAsync();
         
            searchModel.Months = month.Select(store => new SelectListItem
            {
                Value = store.Value,
                Text = store.Text,
                Selected = searchModel.MonthId.ToString() == store.Value
            }).ToList();

            var periods = await SearchPeriodEnum.Today.ToSelectListAsync();

            searchModel.PeriodList = periods.Select(store => new SelectListItem
            {
                Value = store.Value,
                Text = store.Text,
                Selected = searchModel.SearchPeriodId.ToString() == store.Value
            }).ToList();
           await  PrepareOverEstimationListAsync(searchModel);
           await PrepareProjectListAsync(searchModel);


            var statusList = new List<SelectListItem>();
           
            await PrepareAllProcessWorkflowListAsync(searchModel);

            await PrepareDeliveryOnTimeFilterListAsync(searchModel);

            statusList.Insert(0, new SelectListItem
            {
                Text = "All",
                Value = null
            });

            searchModel.AvailableStatus = statusList;


            return searchModel;
        }

        public virtual async Task<MonthlyPerformanceReportListModel> PrepareMonthlyPerformanceReportListModelAsync
     (MonthlyPerformanceReportSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));
            
            //get timesheet reports
            var timeSheetReports = await _timeSheetsService.GetAllEmployeePerformanceReportForParentTaskAsync(searchModel.SearchEmployeeId, searchModel.From,searchModel.To,searchModel.SelectedProjectIds);

            var model = await new MonthlyPerformanceReportListModel().PrepareToGridAsync(searchModel, timeSheetReports, () =>
            {
                return timeSheetReports.SelectAwait(async timeSheetReport =>
                {
                    if (timeSheetReport.EmployeeId == 0)
                    {
                        return null;
                    }  
                    var monthlyReportModel = timeSheetReport.ToModel<MonthlyPerformanceReportModel>();
                    var project = await _projectService.GetProjectsByIdAsync(timeSheetReport.ProjectId);
                    if (project == null)
                        return null;

                    monthlyReportModel.ProjectName = project.ProjectTitle;

                    var task = await _projectTaskService.GetProjectTasksWithoutCacheByIdAsync(timeSheetReport.TaskId);

                    if (task == null)
                        return null;

                    monthlyReportModel.TaskName = task.TaskTitle;
                    monthlyReportModel.TaskId = task.Id;
                    var selectedAvailableDaysOption = task.Tasktypeid;
                    monthlyReportModel.EstimatedTime = task.EstimatedTime;
                    monthlyReportModel.BugCount = task.BugCount;
                    monthlyReportModel.QualityComments = task.QualityComments;
                    monthlyReportModel.TaskTypeName = ((TaskTypeEnum)selectedAvailableDaysOption).ToString();

                    //monthlyReportModel.SpentHours = await _timeSheetsService.get(timeSheetReport.EmployeeId,timeSheetReport.TaskId);
                    var developmentTime = await _timeSheetsService.GetDevelopmentTimeByTaskId(task.Id);
                    var qaTime = await _timeSheetsService.GetQATimeByTaskId(task.Id);

                    monthlyReportModel.SpentTimeFormat = await _timeSheetsService.ConvertSpentTimeAsync(developmentTime.SpentHours, developmentTime.SpentMinutes);
                    monthlyReportModel.QaTimeFormat = await _timeSheetsService.ConvertSpentTimeAsync(qaTime.SpentHours, qaTime.SpentMinutes);
                    // Convert SpentTime and EstimatedTime to total minutes
                    int spentTimeInMinutes = (developmentTime.SpentHours * 60) + developmentTime.SpentMinutes;
                    int estimatedTimeInMinutes = (int)(monthlyReportModel.EstimatedTime * 60);

                    // Calculate ExtraTime in minutes 
                    int extraTimeInMinutes = Math.Max(spentTimeInMinutes - estimatedTimeInMinutes, 0);

                    // Convert ExtraTime to HH:MM format
                    int extraHours = extraTimeInMinutes / 60;
                    int extraMinutes = extraTimeInMinutes % 60;
                    monthlyReportModel.ExtraTime = await _timeSheetsService.ConvertSpentTimeAsync(extraHours, extraMinutes);

                    monthlyReportModel.AllowedVariations = timeSheetReport.AllowedVariations;
                  monthlyReportModel.EstimatedTimeFormat = await _timeSheetsService.ConvertToHHMMFormat(monthlyReportModel.EstimatedTime);
                    //monthlyReportModel.DelivereadOnTime = monthlyReportModel.SpentHours <= (monthlyReportModel.EstimatedTime + (monthlyReportModel.EstimatedTime * (monthlyReportModel.AllowedVariations / 100)));
                    monthlyReportModel.DeliveredOnTime = task.DeliveryOnTime;
                    monthlyReportModel.DueDateFormat = task.DueDate.HasValue
    ? await _workflowStatusService.IsTaskOverdue(task.Id)
        ? $"<span style='color: red; font-weight: bold;'>{task.DueDate.Value:dd-MMMM-yyyy}</span>"
        : task.DueDate.Value.ToString("dd-MMMM-yyyy")
    : "";

                    var status = await _workflowStatusService.GetWorkflowStatusByIdAsync(task.StatusId);
                    if (status != null)
                        monthlyReportModel.StatusName = status.StatusName+"|||"+status.ColorCode;

                    string extraPercentage = estimatedTimeInMinutes > 0
                                     ? $"{(int)Math.Round((extraTimeInMinutes * 100.0) / estimatedTimeInMinutes, 0)}%"
                                     : "--";
                                                                                           
                    if (extraTimeInMinutes > 0)
                        monthlyReportModel.ExtraTime = $"{monthlyReportModel.ExtraTime}({extraPercentage})";

                    monthlyReportModel.OverduePercentage = await _commonPluginService.GetOverduePercentageByTimeAsync(monthlyReportModel.SpentTimeFormat, monthlyReportModel.EstimatedTimeFormat);
                    var parentTask = await _projectTaskService.GetProjectTasksByIdAsync(task.ParentTaskId);
                    if (parentTask != null)
                    {
                        monthlyReportModel.ParentTaskName = parentTask.TaskTitle;
                    }
                    var BugTime = await _projectTaskService.GetBugTimeByTaskIdAsync(task.Id);
                    monthlyReportModel.BugTime = await _timeSheetsService.ConvertSpentTimeAsync(BugTime.Hours, BugTime.Minutes);

                    if(task.WorkQuality !=null)
                    monthlyReportModel.WorkQuality = task.WorkQuality;

                    if (task.DOTPercentage != null)
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


        public virtual async Task<MonthlyPerformanceReportListModel> PreparePerformanceSummaryReportListModelAsync(
    MonthlyPerformanceReportSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            // get employees list
            var employees = new List<int>();
            if (searchModel.SearchEmployeeId > 0)
            {
                employees.Add(searchModel.SearchEmployeeId);
            }
            else
            {
                employees = (await _employeeService.GetAllEmployeesAsync())
                    .Select(e => e.Id)
                    .ToList();
            }

            // build all summaries upfront into a list
            var summaries = new List<MonthlyPerformanceReportModel>();

            foreach (var empId in employees)
            {
                var timeSheetReports = await _timeSheetsService.GetAllEmployeePerformanceReportForParentTaskAsync(
                    empId, searchModel.From, searchModel.To, searchModel.SelectedProjectIds);

                if (!timeSheetReports.Any())
                    continue;

                var emp = await _employeeService.GetEmployeeByIdAsync(empId);
                if (emp == null)
                    continue;

                var totalTask = 0;
                var deliveredOnTime = 0;
                var overdueCount = 0;
                decimal totalDot = 0;
                int dotCount = 0;
                decimal totalWorkQuality = 0;
                int workQualityCount = 0;

                var overDueThresholds = await _commonPluginService.GetOverDueEmailPercentage();
                int[] thresholdCounts = new int[overDueThresholds.Count];

                foreach (var report in timeSheetReports)
                {
                    var task = await _projectTaskService.GetProjectTasksWithoutCacheByIdAsync(report.TaskId);
                    if (task == null)
                        continue;

                    var overduePercentage = await _commonPluginService.GetOverduePercentageByTaskIdAsync(task.Id);
                    if (searchModel.OverEstimation != 0 && overduePercentage != searchModel.OverEstimation)
                        continue;

                    totalTask++;
                    if (task.DeliveryOnTime)
                        deliveredOnTime++;
                    if (await _workflowStatusService.IsTaskOverdue(task.Id))
                        overdueCount++;

                    // Count threshold distribution
                    for (int i = 0; i < overDueThresholds.Count; i++)
                    {
                        if (overduePercentage == overDueThresholds[i])
                            thresholdCounts[i]++;
                    }

                    // Work quality
                    if (task.Tasktypeid == 3 && task.ParentTaskId != 0)
                    {
                        var parent = await _projectTaskService.GetProjectTasksWithoutCacheByIdAsync(task.ParentTaskId);
                        if (parent?.WorkQuality != null)
                        {
                            totalWorkQuality += parent.WorkQuality.Value;
                            workQualityCount++;
                        }
                    }
                    else if (task.WorkQuality != null)
                    {
                        totalWorkQuality += task.WorkQuality.Value;
                        workQualityCount++;
                    }

                    // DOT %
                    if (task.DOTPercentage != null)
                    {
                        totalDot += task.DOTPercentage.Value;
                        dotCount++;
                    }
                }

                if (totalTask == 0)
                    continue;

                summaries.Add(new MonthlyPerformanceReportModel
                {
                    EmployeeId = emp.Id,
                    EmployeeName = emp.FirstName + " " + emp.LastName,
                    TotalTask = totalTask,
                    TotalDeleverdOnTime = deliveredOnTime,
                    DOTPercentageString = dotCount > 0
    ? $"{Math.Round((totalDot / dotCount), 2)}%"
    : "0%",

                    WorkQualityString = workQualityCount > 0
    ? $"{Math.Round(totalWorkQuality / workQualityCount, 2)}%"
    : "0%",

                    NoOfOverdueTask = overdueCount,
                    OverEstimationSummary = string.Join(", ",
                        overDueThresholds.Select((t, i) => $"{t}%: {thresholdCounts[i]}"))
                });
            }

            // prepare grid (now summaries is a list)
            var model = await new MonthlyPerformanceReportListModel().PrepareToGridAsync<
                MonthlyPerformanceReportListModel,
                MonthlyPerformanceReportModel,
                MonthlyPerformanceReportModel
            >(
                searchModel,
                summaries.ToPagedList(searchModel),
                () => summaries.ToAsyncEnumerable()
            );

            return model;
        }






        public virtual async Task<TimeSheetModel> PrepareTimeSheetModelAsync(TimeSheetModel model, TimeSheet timeSheet, bool excludeProperties = false)
        {
            if (timeSheet != null)
            {
             
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
        #endregion
    }
}
                              