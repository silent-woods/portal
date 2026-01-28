using App.Core.Domain.Extension.TimeSheets;
using App.Core.Domain.TaskAlerts;
using App.Services.Customers;
using App.Services.Employees;
using App.Services.Helpers;
using App.Services.Leaves;
using App.Services.ProjectEmployeeMappings;
using App.Services.Projects;
using App.Services.ProjectTasks;
using App.Services.TaskAlerts;
using App.Services.TimeSheets;
using App.Web.Areas.Admin.Models.Extension.TaskComments;
using App.Web.Areas.Admin.Models.TaskAlerts;
using App.Web.Models.Dashboard;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Mvc.Rendering;
using Satyanam.Nop.Core.Domains;
using Satyanam.Nop.Core.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace App.Web.Factories.Extensions
{
    public partial class DashboardModelFactory : IDashboardModelFactory
    {
        #region Fields
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IEmployeeService _employeeService;
        private readonly IProjectTaskService _projectTaskService;
        private readonly IProjectsService _projectService;
        private readonly ITimeSheetsService _timeSheetsService;
        private readonly IProjectEmployeeMappingService _projectEmployeeMappingService;
        private readonly ITaskCommentsService _taskCommentsService;
        private readonly ITaskChangeLogService _taskChangeLogService;
        private readonly IProcessWorkflowService _processWorkflowService;
        private readonly IProcessRulesService _processRulesService;
        private readonly IWorkflowStatusService _workflowStatusService;
        private readonly ICommonPluginService _commonPluginService;
        private readonly IProjectTaskCategoryMappingService _projectTaskCategoryMappingService;
        private readonly ITaskCategoryService _taskCategoryService;
        private readonly IFollowUpTaskService _followUpTaskService;
        private readonly ICustomerService _customerService;
        private readonly ITaskAlertService _taskAlertService;
        #endregion

        #region Ctor
        public DashboardModelFactory(
            IDateTimeHelper dateTimeHelper,
            IEmployeeService employeeService,
            IProjectsService projectService,
            IProjectTaskService projectTaskService,
            ITimeSheetsService timeSheetsService,
            IProjectEmployeeMappingService projectEmployeeMappingService,
            ITaskCommentsService taskCommentsService,
            ITaskChangeLogService taskChangeLogService,
            IProcessWorkflowService processWorkflowService,
            IProcessRulesService processRulesService,
            IWorkflowStatusService workflowStatusService,
            ICommonPluginService commonPluginService,
            IProjectTaskCategoryMappingService projectTaskCategoryMappingService,
            ITaskCategoryService taskCategoryService,
            IFollowUpTaskService followUpTaskService,
            ICustomerService customerService,
            ITaskAlertService taskAlertService)
        {
            _dateTimeHelper = dateTimeHelper;
            _employeeService = employeeService;
            _projectTaskService = projectTaskService;
            _projectService = projectService;
            _timeSheetsService = timeSheetsService;
            _projectEmployeeMappingService = projectEmployeeMappingService;
            _taskCommentsService = taskCommentsService;
            _taskChangeLogService = taskChangeLogService;
            _processWorkflowService = processWorkflowService;
            _processRulesService = processRulesService;
            _workflowStatusService = workflowStatusService;
            _commonPluginService = commonPluginService;
            _projectTaskCategoryMappingService = projectTaskCategoryMappingService;
            _taskCategoryService = taskCategoryService;
            _followUpTaskService = followUpTaskService;
            _customerService = customerService;
            _taskAlertService = taskAlertService;
        }
        #endregion

        #region Utilities
        public virtual async Task PrepareProjectListAsync(PendingDashboardModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            var project = await _projectService.GetProjectListByEmployee(model.CuurentEmployeeId);
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
        public virtual async Task PrepareProjectListByEmployeeAsync(PendingDashboardModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));
            var projects = await _projectService.GetProjectListByEmployee(model.CuurentEmployeeId);
            foreach (var p in projects)
            {
                if (p.StatusId != 4)
                {
                    model.AvailableProjects.Add(new SelectListItem
                    {
                        Text = p.ProjectTitle,
                        Value = p.Id.ToString()
                    });
                }
            }
        }
        public virtual async Task PrepareEmployeeListAsync(PendingDashboardModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            var employees = await _projectEmployeeMappingService.GetJuniorsIdsByEmployeeIdAsync(model.CuurentEmployeeId);

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
        public async Task PrepareSearchPeriodListAsync(PendingDashboardModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            model.AvailableSearchPeriods.Clear();
            model.AvailableSearchPeriods.Add(new SelectListItem
            {
                Value = "",
                Text = "Select Next Review Date",
            });
            foreach (SearchPeriodEnum period in Enum.GetValues(typeof(SearchPeriodEnum)))
            {
                model.AvailableSearchPeriods.Add(new SelectListItem
                {
                    Value = ((int)period).ToString(),
                    Text = period.ToString(),
                });
            }
        }

        #endregion
        #region Methods

        public async Task<PendingDashboardModel> PrepareFollowUpDashboardModelAsync(string taskName = null, int statusType = 0, int currEmployeeId = 0, int projectId = 0, int employeeId = 0, int page = 1,int pageSize= int.MaxValue, IList<int> managedProjectIds = null,IList<int> visibleProjectIds=null, bool showOnlyNotOnTrack = false, string sourceType = null,DateTime? from = null,DateTime? to = null)
        {
            var today = DateTime.UtcNow.Date;
            var allFollowUps = await _followUpTaskService.GetAllFollowUpTasksAsync(taskName: taskName, statusType: statusType, projectId: projectId, employeeId: employeeId, currEmployeeId:currEmployeeId, visibleProjectIds: visibleProjectIds,managedProjectIds:managedProjectIds, showOnlyNotOnTrack: showOnlyNotOnTrack, sourceType: sourceType, from:from , to:to,pageIndex: page - 1, pageSize: pageSize);
            var model = new PendingDashboardModel();

            var istTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");

            foreach (var f in allFollowUps)
            {
                var task = await _projectTaskService.GetProjectTasksWithoutCacheByIdAsync(f.TaskId);
                if (task == null)
                    continue;
                var employee = await _employeeService.GetEmployeeByIdAsync(task.AssignedTo);
                var reviewer = await _customerService.GetCustomerByIdAsync(f.ReviewerId);
                var project = await _projectService.GetProjectsByIdAsync(task.ProjectId);
                var developemetTime = await _timeSheetsService.GetDevelopmentTimeByTaskId(task.Id);
                var status = await _workflowStatusService.GetWorkflowStatusByIdAsync(task.StatusId);

                string alertType = string.Empty;
                string reasonText = string.Empty;
                if (f.AlertId > 0)
                {
                    var alert = await _taskAlertService
                        .GetTaskAlertConfigurationByIdAsync(f.AlertId);
                    if (alert != null)
                    {
                        alertType = alert.Percentage > 0
                        ? $"{Math.Round(alert.Percentage)}%"
                        : "";

                    }

                    var reason = (!f.OnTrack && f.ReasonId > 0)
           ? await _taskAlertService.GetTaskAlertReasonByIdAsync(f.ReasonId)
           : null;
                    reasonText = f.OnTrack
                       ? "Yes"
                       : reason?.Name != null
                           ? $@"No 
            <span class='info-icon'
                  title='{System.Net.WebUtility.HtmlEncode(reason.Name)}'>
                ℹ️
            </span>"
                           : "No";

                }
                var item = new FollowUpTaskModel
                {
                    Id = f.Id,
                    TaskId = f.TaskId,
                    TaskName = task?.TaskTitle ?? "",
                    EmployeeId = task?.AssignedTo ?? 0,
                    EmployeeName = employee?.FirstName + " " + employee?.LastName,
                    ReviewerId = f.ReviewerId,
                    EstimationTime = await _timeSheetsService.ConvertToHHMMFormat(task.EstimatedTime),
                    DevelopementTime = await _timeSheetsService.ConvertSpentTimeAsync(developemetTime.SpentHours, developemetTime.SpentMinutes),
                    ReviewerName = reviewer?.FirstName + " " + reviewer?.LastName,
                    ProjectName = project?.ProjectTitle,
                    LastFollowupDateTime = f.LastFollowupDateTime != null ?  TimeZoneInfo.ConvertTimeFromUtc(f.LastFollowupDateTime.Value, istTimeZone):null,
                    NextFollowupDateTime = f.NextFollowupDateTime != null ? TimeZoneInfo.ConvertTimeFromUtc(f.NextFollowupDateTime.Value, istTimeZone) : null,
                    LastComment = f.LastComment,
                    AlertId = f.AlertId,
                    AlertType = alertType,
                    TrackReason = reasonText,
                    IsManual = f.AlertId ==0 ? true:false,
                    StatusName = status?.StatusName,
                    StatusColor = status?.ColorCode,
                    CanTakeFollowUp =
            managedProjectIds != null &&
            managedProjectIds.Contains(task.ProjectId)
                };
                if (statusType != 1)
                {
                    if (f.LastFollowupDateTime == null)
                        model.NewTasks.Add(item);
                    else if (!f.NextFollowupDateTime.HasValue)
                        model.Upcoming.Add(item);
                    else if (f.NextFollowupDateTime.Value.Date < today)
                        model.Overdue.Add(item);
                    else if (f.NextFollowupDateTime.Value.Date == today)
                        model.DueToday.Add(item);
                    else
                        model.Upcoming.Add(item);
                }
                else
                {
                    model.CompletedList.Add(item);
                }
            }
            model.CuurentEmployeeId = currEmployeeId;
            model.PageIndex = page;
            model.PageSize = pageSize;
            model.TotalRecords = allFollowUps.TotalCount;
            model.TotalPages = allFollowUps.TotalPages;
            await PrepareProjectListByEmployeeAsync(model);
            await PrepareEmployeeListAsync(model);
            await PrepareSearchPeriodListAsync(model);
            return model;
        }
        #endregion
    }
}


