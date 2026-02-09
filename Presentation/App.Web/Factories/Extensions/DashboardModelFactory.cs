using App.Core.Domain.Employees;
using App.Core.Domain.Extension.TimeSheets;
using App.Core.Domain.Projects;
using App.Core.Domain.ProjectTasks;
using App.Core.Domain.TaskAlerts;
using App.Core.Domain.TimeSheets;
using App.Data;
using App.Data.Extensions;
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
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Mvc.Rendering;
using Satyanam.Nop.Core.Domains;
using Satyanam.Nop.Core.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
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
        private readonly IRepository<ProjectTask> _projectTaskRepository;
        private readonly IRepository<TaskAlertConfiguration> _taskAlertConfigurationRepository;
        private readonly IRepository<TimeSheet> _timeSheetRepository;
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
            ITaskAlertService taskAlertService,
            IRepository<ProjectTask> projectTaskRepository,
            IRepository<TaskAlertConfiguration> taskAlertConfigurationRepository,
            IRepository<TimeSheet> timeSheetRepository)
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
            _projectTaskRepository = projectTaskRepository;
            _taskAlertConfigurationRepository = taskAlertConfigurationRepository;
            _timeSheetRepository = timeSheetRepository;
        }
        #endregion

        #region Utilities
        public virtual async Task PrepareProjectListAsync(PendingDashboardModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            var project = await _projectService.GetProjectListByEmployee(model.CurrentEmployeeId);
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
            var projects = await _projectService.GetProjectListByEmployee(model.CurrentEmployeeId);
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

            var employees = await _projectEmployeeMappingService.GetJuniorsIdsByEmployeeIdAsync(model.CurrentEmployeeId);

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

        public virtual async Task PrepareAlertPercentageDropdownAsync(
            PendingDashboardModel model,
            int selectedPercentage = 0)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            var percentages = await _taskAlertConfigurationRepository.Table
                .Where(x => x.IsActive && !x.Deleted)
                .OrderBy(x => x.DisplayOrder)
                .Select(x => (int)x.Percentage)   
                .Distinct()
                .ToListAsync();

            model.AvailableAlertPercentages = percentages
                .Select(p => new SelectListItem
                {
                    Text = $"{p} %",
                    Value = p.ToString(),
                    Selected = p == selectedPercentage
                }).ToList();

            model.AvailableAlertPercentages.Insert(0,
                new SelectListItem
                {
                    Text = "Select %",
                    Value = ""
                });
        }

        public virtual async Task PrepareProcessWorkflowDropdownAsync(
            PendingDashboardModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));
            var workflows = await _processWorkflowService.GetAllProcessWorkflowsAsync();
            model.AvailableProcessWorkflow = workflows
                .Select(w => new SelectListItem
                {
                    Text = w.Name,
                    Value = w.Id.ToString()
                })
                .ToList();
            model.AvailableProcessWorkflow.Insert(0,
                new SelectListItem
                {
                    Text = "All Workflows",
                    Value = "0",
                    Selected = false
                });
            if (model.AvailableProcessWorkflow.Count > 1)
            {
                model.AvailableProcessWorkflow[1].Selected = true;
            }
        }


        #endregion
        #region Methods

        public async Task<PendingDashboardModel> PrepareFollowUpDashboardModelAsync(string taskName = null, int statusType = 0, int currEmployeeId = 0, int projectId = 0, int employeeId = 0, int page = 1, int pageSize = int.MaxValue, IList<int> managedProjectIds = null, IList<int> visibleProjectIds = null, bool showOnlyNotOnTrack = false, string sourceType = null, DateTime? from = null, DateTime? to = null, int percentageFilter =0,int processWorkflow=0 ,int statusId =0)
        {
            var today = DateTime.UtcNow.Date;
            var allFollowUps = await _followUpTaskService.GetAllFollowUpTasksAsync(taskName: taskName, statusType: statusType, projectId: projectId, employeeId: employeeId, currEmployeeId: currEmployeeId, visibleProjectIds: visibleProjectIds, managedProjectIds: managedProjectIds, showOnlyNotOnTrack: showOnlyNotOnTrack, sourceType: sourceType, from: from, to: to, percentageFilter: percentageFilter, processWorkflow: processWorkflow, statusId: statusId, pageIndex: page - 1, pageSize: pageSize);
            var model = new PendingDashboardModel();

            var istTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
            var taskIds = allFollowUps
                .Select(f => f.TaskId)
                .Distinct()
                .ToList();
            var lastTrackedDict = await _timeSheetRepository.Table
              .Where(t => taskIds.Contains(t.TaskId))
              .GroupBy(t => t.TaskId)
              .Select(g => new
              {
                  TaskId = g.Key,
                  LastTrackedUtc = (DateTime?)g.Max(x =>
                      x.UpdateOnUtc > x.CreateOnUtc
                          ? x.UpdateOnUtc
                          : x.CreateOnUtc)
              })
              .ToDictionaryAsync(x => x.TaskId, x => x.LastTrackedUtc);

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
                lastTrackedDict.TryGetValue(f.TaskId, out var lastTrackedUtc);
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
                    LastFollowupDateTime = f.LastFollowupDateTime != null ? TimeZoneInfo.ConvertTimeFromUtc(f.LastFollowupDateTime.Value, istTimeZone) : null,
                    NextFollowupDateTime = f.NextFollowupDateTime != null ? TimeZoneInfo.ConvertTimeFromUtc(f.NextFollowupDateTime.Value, istTimeZone) : null,
                    LastTrackedOn = lastTrackedUtc !=null ? TimeZoneInfo.ConvertTimeFromUtc(lastTrackedUtc.Value, istTimeZone) : null,
                    LastComment = f.LastComment,
                    AlertId = f.AlertId,
                    AlertType = alertType,
                    TrackReason = reasonText,
                    IsManual = f.AlertId == 0 ? true : false,
                    StatusName = status?.StatusName,
                    StatusColor = status?.ColorCode,
                    CanTakeFollowUp = managedProjectIds != null && managedProjectIds.Contains(task.ProjectId)
                };
                if (statusType != 1)
                {
                    if (!f.NextFollowupDateTime.HasValue)
                        model.Overdue.Add(item);
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
            model.CurrentEmployeeId = currEmployeeId;
            model.PendingCodeReviewCount = await _workflowStatusService.GetPendingCodeReviewCountAsync(currEmployeeId);
            model.PendingReadyToTestCount = await _workflowStatusService.GetPendingReadyToTestCountAsync(currEmployeeId);
            model.PendingOverdueCount = await _commonPluginService.GetDashboardOverdueCountAsync(currEmployeeId);
            model.PageIndex = page;
            model.PageSize = pageSize;
            model.TotalRecords = allFollowUps.TotalCount;
            model.TotalPages = allFollowUps.TotalPages;
            await PrepareProjectListByEmployeeAsync(model);
            await PrepareEmployeeListAsync(model);
            await PrepareSearchPeriodListAsync(model);
            await PrepareAlertPercentageDropdownAsync(model);
            await PrepareProcessWorkflowDropdownAsync(model);
            return model;
        }

        public async Task<PendingDashboardModel> PrepareCodeReviewDashboardModelAsync(int currEmployeeId = 0,int projectId = 0,int employeeId = 0,string taskName = null,int statusId =0)
        {
            var currEmployee = await _employeeService.GetEmployeeByIdAsync(currEmployeeId);
            var model = new PendingDashboardModel
            {
                CurrentEmployeeId = currEmployeeId,
                CurrentEmployeeName = $"{currEmployee?.FirstName} {currEmployee?.LastName}",
                ProjectId = projectId,
                EmployeeId = employeeId,
                TaskName = taskName,
                PendingReadyToTestCount = await _workflowStatusService.GetPendingReadyToTestCountAsync(currEmployeeId),
                PendingOverdueCount = await _commonPluginService.GetDashboardOverdueCountAsync(currEmployeeId),
            };
            await PrepareProjectListByEmployeeAsync(model);
            await PrepareEmployeeListAsync(model);
            var codeReviewStatuses = await _workflowStatusService
      .GetAllWorkflowStatusAsync(statusNames: new List<string>
      {
          "Code Review",
          "Code Review Done"
      });

            var codeReviewStatusIds = codeReviewStatuses
                .Select(s => s.Id)
                .ToList();


            var managedProjectIds =
                await _projectEmployeeMappingService
                    .GetProjectIdsManagedOrCoordinateByEmployeeIdAsync(currEmployeeId)
                ?? new List<int>();
            var query = _projectTaskRepository.Table
                .Where(t => codeReviewStatusIds.Contains(t.StatusId));
            query = query.Where(t =>
                t.AssignedTo == currEmployeeId
                || t.DeveloperId == currEmployeeId
                || managedProjectIds.Contains(t.ProjectId)
            );
            if (projectId > 0)
            {
                query = query.Where(t => t.ProjectId == projectId);
            }
            if (statusId > 0)
                query = query.Where(t => t.StatusId == statusId);
            if (employeeId > 0)
            {
                query = query.Where(t =>
                    t.AssignedTo == employeeId
                );
            }
            if (!string.IsNullOrWhiteSpace(taskName))
            {
                query = query.Where(t => t.TaskTitle.Contains(taskName));
            }
            var tasks = await query.ToListAsync();
            var projectIds = tasks.Select(t => t.ProjectId).Distinct().ToList();
            var employeeIds = tasks.Select(t => t.AssignedTo).Distinct().ToList();
            var developerIds = tasks.Select(t => t.DeveloperId).Distinct().ToList();
            var projects = projectIds.Any()
                ? await _projectService.GetProjectsByIdsAsync(projectIds.ToArray())
                : new List<Project>();
            var employees = employeeIds.Any()
                ? await _employeeService.GetEmployeesByIdsAsync(employeeIds.ToArray())
                : new List<Employee>();
            var developers = developerIds.Any()
                ? await _employeeService.GetEmployeesByIdsAsync(developerIds.ToArray())
                : new List<Employee>();
            var projectDict = projects
                .Where(p => p != null)
                .ToDictionary(p => p.Id, p => p.ProjectTitle);
            var employeeDict = employees
                .Where(e => e != null)
                .ToDictionary(e => e.Id, e => $"{e.FirstName} {e.LastName}");
            var developerDict = developers
                 .Where(d => d != null)
                 .ToDictionary(d => d.Id, d => $"{d.FirstName} {d.LastName}");
            var statusIds = tasks
.Select(t => t.StatusId)
.Distinct()
.ToList();
     var statuses = statusIds.Any()
   ? await _workflowStatusService.GetWorkflowStatusByIdsAsync(statusIds.ToArray())
   : new List<WorkflowStatus>();
            var statusDict = statuses.ToDictionary(
                s => s.Id,
                s => new
                {
                    s.StatusName,
                    s.ColorCode
                });
            var workflows = await _processWorkflowService.GetAllProcessWorkflowsAsync();

            var workflowDict = workflows
                .ToDictionary(w => w.Id, w => w.Name);
            var istTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
            var todayIstDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, istTimeZone).Date;
            model.StatusFilters = codeReviewStatuses
    .Select(s => new StatusFilterModel
    {
        StatusId = s.Id,
        StatusName = s.StatusName,
        ColorCode = s.ColorCode,
        ProcessWorkflowId = s.ProcessWorkflowId,
        ProcessWorkflowName = workflowDict.TryGetValue(
            s.ProcessWorkflowId,
            out var wfName)
                ? wfName
                : string.Empty,

        Count = tasks.Count(t => t.StatusId == s.Id)
    })
    .Where(x => x.Count > 0)  
    .OrderBy(x => x.ProcessWorkflowName)
    .ThenBy(x => x.StatusName)
    .ToList();
            model.CodeReviewTasks = (await Task.WhenAll(
                tasks.Select(async t =>
                {
                    var statusStartDate =
                        await _taskChangeLogService.GetCurrentStatusStartDateAsync(
                            t.Id,
                            t.StatusId);
                    DateTime? statusStartIst = statusStartDate.HasValue ? TimeZoneInfo.ConvertTimeFromUtc(statusStartDate.Value, istTimeZone): (DateTime?)null;

                    int pendingSinceDays = statusStartIst.HasValue
                        ? (todayIstDate - statusStartIst.Value.Date).Days
                        : 0;
                    return new CodeReviewTaskModel
                    {
                        TaskId = t.Id,
                        TaskTitle = t.TaskTitle ?? string.Empty,
                        ProjectName = projectDict.TryGetValue(t.ProjectId, out var pn)
                            ? pn
                            : "—",
                        AssignedTo = employeeDict.TryGetValue(t.AssignedTo, out var en)
                            ? en
                            : "—",
                        DeveloperName = developerDict.TryGetValue(t.DeveloperId, out var dv)
                            ? dv
                            : "—",
                        ProcessWorkflowId = t.ProcessWorkflowId,
                        StatusId = t.StatusId,
                        CodeReviewStartDate = statusStartDate != null
                            ? TimeZoneInfo.ConvertTimeFromUtc(statusStartDate.Value, istTimeZone)
                            : null,
                        DueDate = t.DueDate.HasValue
                            ? t.DueDate.Value.ToString("d-MMM-yyyy", CultureInfo.InvariantCulture)
                            : "—",
                        StatusName = statusDict.TryGetValue(t.StatusId, out var st)
                           ? st.StatusName
                           : "—",
                        StatusColorCode = statusDict.TryGetValue(t.StatusId, out var stColor)
                           ? stColor.ColorCode
                           : "#9e9e9e",
                        PendingSinceDays = pendingSinceDays,
                    };
                })
            )).ToList();
            return model;
        }

        public async Task<PendingDashboardModel> PrepareReadyToTestDashboardModelAsync(int currEmployeeId = 0,int projectId = 0,int employeeId = 0,string taskName = null,int statusId =0)
        {
            var model = new PendingDashboardModel
            {
                CurrentEmployeeId = currEmployeeId,
                ProjectId = projectId,
                EmployeeId = employeeId,
                TaskName = taskName,
                PendingCodeReviewCount = await _workflowStatusService.GetPendingCodeReviewCountAsync(currEmployeeId),
                PendingOverdueCount = await _commonPluginService.GetDashboardOverdueCountAsync(currEmployeeId),
            };

            await PrepareProjectListByEmployeeAsync(model);
            await PrepareEmployeeListAsync(model);

            var readyToTestStatuses = await _workflowStatusService
      .GetAllWorkflowStatusAsync(statusNames: new List<string>
      {
        "Ready to Test",
        "QA",
        "Test Failed",
        "QA on Live",
        "Ready for Live"
      });

            var readyToTestStatusIds = readyToTestStatuses
                .Select(s => s.Id)
                .ToList();

            var managedProjectIds =
                await _projectEmployeeMappingService
                    .GetProjectIdsManagedOrCoordinateByEmployeeIdAsync(currEmployeeId);

            var qaProjectIds =
                await _projectEmployeeMappingService
                    .GetProjectIdsQaByEmployeeIdAsync(currEmployeeId);
            var query = _projectTaskRepository.Table
                  .Where(t => readyToTestStatusIds.Contains(t.StatusId));
            if (employeeId > 0)
            {
                query = query.Where(t =>
                    t.DeveloperId == employeeId
                );
            }
            else
            {
                query = query.Where(t =>
                    managedProjectIds.Contains(t.ProjectId) ||
                    qaProjectIds.Contains(t.ProjectId) ||
                    t.AssignedTo == currEmployeeId ||
                    t.DeveloperId == currEmployeeId
                );
            }

            if (projectId > 0)
                query = query.Where(t => t.ProjectId == projectId);

            if (statusId > 0)
                query = query.Where(t => t.StatusId == statusId);

            if (!string.IsNullOrWhiteSpace(taskName))
                query = query.Where(t => t.TaskTitle.Contains(taskName));

            var tasks = await query.ToListAsync();
            var projectIds = tasks.Select(t => t.ProjectId).Distinct().ToList();
            var employeeIds = tasks
                .SelectMany(t => new[] { t.AssignedTo, t.DeveloperId })
                .Distinct()
                .ToList();

            var projects = projectIds.Any()
                ? await _projectService.GetProjectsByIdsAsync(projectIds.ToArray())
                : new List<Project>();

            var employees = employeeIds.Any()
                ? await _employeeService.GetEmployeesByIdsAsync(employeeIds.ToArray())
                : new List<Employee>();

            var projectDict = projects.ToDictionary(p => p.Id, p => p.ProjectTitle);
            var employeeDict = employees.ToDictionary(
                e => e.Id,
                e => e.FirstName + " " + e.LastName
            );
            var statusIds = tasks
.Select(t => t.StatusId)
.Distinct()
.ToList();
            var workflows = await _processWorkflowService.GetAllProcessWorkflowsAsync();

            var workflowDict = workflows
                .ToDictionary(w => w.Id, w => w.Name);

            var statuses = statusIds.Any()
    ? await _workflowStatusService.GetWorkflowStatusByIdsAsync(statusIds.ToArray())
    : new List<WorkflowStatus>();
            var statusDict = statuses.ToDictionary(
                s => s.Id,
                s => new   
                {
                    s.StatusName,
                    s.ColorCode
                });
            var istTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
            var todayIstDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, istTimeZone).Date;
            model.StatusFilters = readyToTestStatuses
    .Select(s => new StatusFilterModel
    {
        StatusId = s.Id,
        StatusName = s.StatusName,
        ColorCode = s.ColorCode,
        ProcessWorkflowId = s.ProcessWorkflowId,
        ProcessWorkflowName = workflowDict.TryGetValue(
            s.ProcessWorkflowId,
            out var wfName)
                ? wfName
                : string.Empty,

        Count = tasks.Count(t => t.StatusId == s.Id)
    })
    .Where(x => x.Count > 0)  
    .OrderBy(x => x.ProcessWorkflowName)
    .ThenBy(x => x.StatusName)
    .ToList();

            model.ReadyToTestTasks = await Task.WhenAll(
                tasks.Select(async t =>
                {
                    var statusStartDate =
                        await _taskChangeLogService.GetCurrentStatusStartDateAsync(
                            t.Id,
                            t.StatusId
                        );
                    DateTime? statusStartIst = statusStartDate.HasValue
    ? TimeZoneInfo.ConvertTimeFromUtc(statusStartDate.Value, istTimeZone)
    : (DateTime?)null;

                    int pendingSinceDays = statusStartIst.HasValue
                        ? (todayIstDate - statusStartIst.Value.Date).Days
                        : 0;
                    return new ReadyToTestTaskModel
                    {
                        TaskId = t.Id,
                        TaskTitle = t.TaskTitle,
                        ProjectName = projectDict.GetValueOrDefault(t.ProjectId, "—"),
                        AssignedTo = employeeDict.GetValueOrDefault(t.AssignedTo, "—"),
                        DeveloperName = employeeDict.GetValueOrDefault(t.DeveloperId, "—"),
                        StatusId = t.StatusId,
                        ReadyToTestStartDate = statusStartDate != null ? TimeZoneInfo.ConvertTimeFromUtc(statusStartDate.Value, istTimeZone): null,
                        PendingSinceDays = pendingSinceDays,
                        DueDate = t.DueDate?.ToString("d-MMM-yyyy") ?? "—",
                        StatusName = statusDict.TryGetValue(t.StatusId, out var st)
                           ? st.StatusName
                           : "—",
                        StatusColorCode = statusDict.TryGetValue(t.StatusId, out var stColor)
                           ? stColor.ColorCode
                           : "#9e9e9e"
                    };
                })
            );
            return model;
        }

        public async Task<PendingDashboardModel> PrepareOverdueDashboardModelAsync(int currEmployeeId = 0,int projectId = 0,int employeeId = 0,string taskName = null)
        {
            var currEmployee = await _employeeService.GetEmployeeByIdAsync(currEmployeeId);

            var model = new PendingDashboardModel
            {
                CurrentEmployeeId = currEmployeeId,
                CurrentEmployeeName = $"{currEmployee?.FirstName} {currEmployee?.LastName}",
                ProjectId = projectId,
                EmployeeId = employeeId,
                TaskName = taskName,
                PendingCodeReviewCount = await _workflowStatusService.GetPendingCodeReviewCountAsync(currEmployeeId),
                PendingReadyToTestCount = await _workflowStatusService.GetPendingReadyToTestCountAsync(currEmployeeId)
            };

            await PrepareProjectListByEmployeeAsync(model);
            await PrepareEmployeeListAsync(model);

            var overdueTasks = new List<ProjectTask>();
            overdueTasks = (List<ProjectTask>)await _commonPluginService.GetOverdueTasksByCurrentEmployeeForDashboardAsync(currEmployeeId);
            var today = DateTime.UtcNow.Date;

            if (projectId > 0)
                overdueTasks = overdueTasks.Where(t => t.ProjectId == projectId).ToList();

            if (employeeId > 0)
                overdueTasks = overdueTasks
                    .Where(t => t.AssignedTo == employeeId || t.DeveloperId == employeeId)
                    .ToList();

            if (!string.IsNullOrWhiteSpace(taskName))
                overdueTasks = overdueTasks
                    .Where(t => t.TaskTitle.Contains(taskName))
                    .ToList();

            var statusIds = overdueTasks
    .Select(t => t.StatusId)
    .Distinct()
    .ToList();
            var statuses = statusIds.Any()
    ? await _workflowStatusService.GetWorkflowStatusByIdsAsync(statusIds.ToArray())
    : new List<WorkflowStatus>();
            var statusDict = statuses.ToDictionary(
                s => s.Id,
                s => new
                {
                    s.StatusName,
                    s.ColorCode
                });
            var projectIds = overdueTasks.Select(t => t.ProjectId).Distinct().ToList();
            var employeeIdsUsed = overdueTasks.Select(t => t.AssignedTo).Distinct().ToList();
            var developerIds = overdueTasks.Select(t => t.DeveloperId).Distinct().ToList();
            var projects = await _projectService.GetProjectsByIdsAsync(projectIds.ToArray());
            var employees = await _employeeService.GetEmployeesByIdsAsync(employeeIdsUsed.ToArray());
            var developers = await _employeeService.GetEmployeesByIdsAsync(developerIds.ToArray());
            var projectDict = projects.ToDictionary(p => p.Id, p => p.ProjectTitle);
            var employeeDict = employees.ToDictionary(e => e.Id, e => $"{e.FirstName} {e.LastName}");
            var developerDict = developers.ToDictionary(d => d.Id, d => $"{d.FirstName} {d.LastName}");

            var ist = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
            model.OverdueTasks = (await Task.WhenAll(
               overdueTasks.Select(async t =>
               {
                   var deveTime = await _timeSheetsService.GetDevelopmentTimeByTaskId(t.Id);

                   return new OverdueTaskModel
                   {
                       TaskId = t.Id,
                       TaskTitle = t.TaskTitle,

                       ProjectName = projectDict.GetValueOrDefault(t.ProjectId, "—"),
                       AssignedTo = employeeDict.GetValueOrDefault(t.AssignedTo, "—"),
                       DeveloperName = developerDict.GetValueOrDefault(t.DeveloperId, "—"),
                       EstimationTime = await _timeSheetsService
                           .ConvertToHHMMFormat(t.EstimatedTime),
                       DeveloperTime = await _timeSheetsService.ConvertSpentTimeAsync(deveTime.SpentHours,deveTime.SpentMinutes),
                       DueDate = t.DueDate.HasValue
                           ? TimeZoneInfo.ConvertTimeFromUtc(t.DueDate.Value, ist)
                               .ToString("d-MMM-yyyy")
                           : "—",
                       ProcessWorkflowId = t.ProcessWorkflowId,
                       StatusId = t.StatusId,
                       StatusName = statusDict.TryGetValue(t.StatusId, out var st)
                           ? st.StatusName
                           : "—",
                       StatusColorCode = statusDict.TryGetValue(t.StatusId, out var stColor)
                           ? stColor.ColorCode
                           : "#9e9e9e"
                   };
               })
           )).ToList();
            return model;
        }
        #endregion
    }
}


