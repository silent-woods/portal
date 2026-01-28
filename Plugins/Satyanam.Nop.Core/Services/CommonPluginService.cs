using App.Core;
using App.Core.Domain.Extension.Alerts;
using App.Core.Domain.Extension.TimeSheets;
using App.Core.Domain.ProjectTasks;
using App.Core.Domain.TimeSheets;
using App.Data;
using App.Data.Extensions;
using App.Services.Messages;
using App.Services.ProjectTasks;
using App.Services.TimeSheets;
using Humanizer;
using Satyanam.Nop.Core.Domains;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static LinqToDB.Sql;

namespace Satyanam.Nop.Core.Services
{
    /// <summary>
    /// Title service
    /// </summary>
    public partial class CommonPluginService : ICommonPluginService
    {
        #region Fields

        private readonly IRepository<ProcessRules> _processRulesRepository;
        private readonly IRepository<WorkflowStatus> _workflowStatusRepository;
        private readonly IRepository<ProjectTask> _projectTaskRepository;
        private readonly ITimeSheetsService _timeSheetsService;
        private readonly IProjectTaskService _projectTaskService;
        private readonly IWorkflowStatusService _workflowStatusService;
        private readonly EmailSettings _emailSettings;
        private readonly MonthlyReportSetting _monthlyReportSetting;
        private readonly IRepository<TimeSheet> _timeSheetRepository;

        #endregion

        #region Ctor

        public CommonPluginService(IRepository<ProcessRules> processRulesRepository, IRepository<WorkflowStatus> workflowStatusRepository, IRepository<ProjectTask> projectTaskRepository, ITimeSheetsService timeSheetsService, IProjectTaskService projectTaskService, IWorkflowStatusService workflowStatusService, EmailSettings emailSettings, MonthlyReportSetting monthlyReportSetting, IRepository<TimeSheet> timeSheetRepository)
        {
            _processRulesRepository = processRulesRepository;
            _workflowStatusRepository = workflowStatusRepository;
            _projectTaskRepository = projectTaskRepository;
            _timeSheetsService = timeSheetsService;
            _projectTaskService = projectTaskService;
            _workflowStatusService = workflowStatusService;
            _emailSettings = emailSettings;
            _monthlyReportSetting = monthlyReportSetting;
            _timeSheetRepository = timeSheetRepository;
        }

        #endregion

        #region Methods
        public virtual async Task<IPagedList<ProjectTask>> GetAllProjectTasksAsync(
        int taskId,
        int taskTypeId,
        IList<int> employeeIds,
        IList<int> projectIds,
        string taskName,
        DateTime? from,
        DateTime? to,
        DateTime? dueDate,
        int SelectedStatusId,
        int processWorkflowId,
        int pageIndex = 0,
        int pageSize = int.MaxValue,
        bool showHidden = false,
        bool? overridePublished = null,
        int filterDeliveryOnTime = 0,
        int searchParentTaskId = 0)
        {
            var statusIdsToFilter = new List<int>();
            if (dueDate.HasValue && SelectedStatusId == 0)
            {
                var workflowStatuses = await _workflowStatusRepository.GetAllAsync(query => query);
                if (processWorkflowId != 0)
                {
                    var statuses = workflowStatuses.Where(s => s.ProcessWorkflowId == processWorkflowId).ToList();
                    var newStatusId = statuses.OrderBy(s => s.DisplayOrder).FirstOrDefault()?.Id;
                    var activeStatusId = statuses.FirstOrDefault(s => s.IsDefaultDeveloperStatus)?.Id;
                    if (newStatusId.HasValue)
                        statusIdsToFilter.Add(newStatusId.Value);
                    if (activeStatusId.HasValue)
                        statusIdsToFilter.Add(activeStatusId.Value);
                }
                else
                {
                    var grouped = workflowStatuses.GroupBy(s => s.ProcessWorkflowId);
                    foreach (var group in grouped)
                    {
                        var newStatusId = group.OrderBy(s => s.DisplayOrder).FirstOrDefault()?.Id;
                        var activeStatusId = group.FirstOrDefault(s => s.IsDefaultDeveloperStatus)?.Id;
                        if (newStatusId.HasValue)
                            statusIdsToFilter.Add(newStatusId.Value);
                        if (activeStatusId.HasValue)
                            statusIdsToFilter.Add(activeStatusId.Value);
                    }
                }
            }

            var query = await _projectTaskRepository.GetAllAsync(async query =>
            {
                if (!showHidden)
                    query = query.Where(c => !c.IsDeleted);
                if (taskId != 0)
                    query = query.Where(c => c.Id == taskId);
                if (taskTypeId != 0)
                    query = query.Where(c => c.Tasktypeid == taskTypeId);
                if (searchParentTaskId != 0)
                {
                    query = query
                        .Where(c => c.ParentTaskId == searchParentTaskId || c.Id == searchParentTaskId)
                        .OrderByDescending(c => c.Id == searchParentTaskId); 
                }
                if (employeeIds?.Any() == true)
                    query = query.Where(c => employeeIds.Contains(c.AssignedTo));
                if (projectIds?.Any() == true)
                    query = query.Where(c => projectIds.Contains(c.ProjectId));
                if (!string.IsNullOrWhiteSpace(taskName))
                    query = query.Where(c => c.TaskTitle.Contains(taskName));
                if (SelectedStatusId != 0)
                {
                    query = query.Where(pr => pr.StatusId == SelectedStatusId);
                }
                else if (dueDate.HasValue && statusIdsToFilter.Any())
                {
                    query = query.Where(pr =>
                        statusIdsToFilter.Contains(pr.StatusId) &&
                        pr.DueDate.HasValue &&
                        pr.DueDate.Value <= dueDate.Value);
                }
                if (processWorkflowId != 0)
                    query = query.Where(pr => pr.ProcessWorkflowId == processWorkflowId);
                if (filterDeliveryOnTime == 1)
                    query = query.Where(pr => pr.DeliveryOnTime == true);
                else if (filterDeliveryOnTime == 2)
                    query = query.Where(pr => pr.DeliveryOnTime == false);
                if (from.HasValue)
                    query = query.Where(pr => pr.CreatedOnUtc >= from.Value);
                if (to.HasValue)
                    query = query.Where(pr => pr.CreatedOnUtc <= to.Value);
                if (dueDate.HasValue)
                {
                    query = query.Where(pr => pr.DueDate.HasValue && pr.DueDate.Value <= dueDate.Value);
                    return searchParentTaskId != 0
         ? query.OrderByDescending(c => c.Id == searchParentTaskId).ThenByDescending(c => c.DueDate)
         : query.OrderByDescending(c => c.DueDate);
                }
                return searchParentTaskId != 0
    ? query.OrderByDescending(c => c.Id == searchParentTaskId).ThenByDescending(c => c.CreatedOnUtc)
    : query.OrderByDescending(c => c.CreatedOnUtc);
            });
            return new PagedList<ProjectTask>(query.ToList(), pageIndex, pageSize);
        }

        public virtual async Task<MonthlyTimeSheetReport> GetEmployeePerformanceSummaryAsync(int searchEmployeeId, DateTime? From, DateTime? To, string ProjectIds)
        {
            var projectList = new List<int>();
            if (ProjectIds != "0" && ProjectIds != null)
            {
                projectList = ProjectIds.Split(',').Select(int.Parse).ToList();
            }
            var timeSheetReports = await _timeSheetsService.GetAllEmployeePerformanceReportAsync(searchEmployeeId, From, To, projectList);
            var totalTask = timeSheetReports.Count;
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
            var overDueThresholds = await GetOverDueEmailPercentage();

            foreach (var report in timeSheetReports)
            {
                var task = await _projectTaskService.GetProjectTasksWithoutCacheByIdAsync(report.TaskId);
                if (task != null)
                {
                    var spentTime = await _timeSheetsService.GetSpeantTimeByEmployeeAndTaskAsync(report.EmployeeId, report.TaskId);
                    var estimatedHours = task.EstimatedTime;
                    var allowedVariations = report.AllowedVariations;
                    if (task.DeliveryOnTime)
                    {
                        totalDeliveredOnTime++;
                    }
                    if(await _workflowStatusService.IsTaskOverdue(task.Id))
                    {
                        overDueCount++;
                    }
                    decimal overDuePercentage = await GetOverduePercentageByTaskIdAsync(task.Id);
                    if (overDuePercentage == overDueThresholds[0])
                        firstOverDueThresholdCount++;
                    else if (overDuePercentage == overDueThresholds[1])
                        secondOverDueThresholdCount++;
                    else if (overDuePercentage == overDueThresholds[2])
                        thirdOverDueThresholdCount++;
                    if(task.Tasktypeid ==3 && task.ParentTaskId != 0)
                    {
                        var parentTask = await _projectTaskService.GetProjectTasksWithoutCacheByIdAsync(task.ParentTaskId);
                        if (parentTask !=null && parentTask.WorkQuality != null)
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
            decimal avgWorkQuality = workQualityCount != 0
     ? Math.Round(totalWorkQuality / workQualityCount, 2)
     : 0;

            double avgDot = dotCount != 0
                ? Math.Round((double)(totalDot / dotCount), 2)
                : 0;

            MonthlyTimeSheetReport summary = new MonthlyTimeSheetReport
            {
                TotalTask = totalTask,
                TotalDeliveredOnTime = totalDeliveredOnTime,
                ResultPercentage = avgDot,
                TotalEstimatedHours = totalEstimatedHours,
                TotalSpentHours = totalSpentHours,
                ExtraTime = extraTime,
                OverDueCount =overDueCount,
                FirstOverDueThreshold = overDueThresholds[0],
                SecondOverDueThreshold = overDueThresholds[1],
                ThirdOverDueThreshold = overDueThresholds[2],
                FirstOverDueThresholdCount = firstOverDueThresholdCount,
                SecondOverDueThresholdCount = secondOverDueThresholdCount,
                ThirdOverDueThresholdCount = thirdOverDueThresholdCount,
                AvgWorkQuality = avgWorkQuality,
            };
            return summary;
        }

        public virtual async Task<IList<decimal>> GetOverDueEmailPercentage()
        {
            IList<decimal> overDuePercentage = new List<decimal> { _emailSettings.FirstMailVariation,_emailSettings.SecondMailVariation,_emailSettings.ThirdMailVariation};
            return overDuePercentage;
        }

        public virtual async Task<decimal> GetOverduePercentageByTimeAsync(string bigTime, string smallTime)
        {
            decimal bigHours = await _timeSheetsService.ConvertHHMMToDecimal(bigTime);
            decimal smallHours = await _timeSheetsService.ConvertHHMMToDecimal(smallTime);
            if (bigHours <= 0 || smallHours <= 0)
                return 0;
            decimal overagePercentage = ((bigHours / smallHours) * 100) - 100;
            var thresholds = await GetOverDueEmailPercentage(); 
            if (thresholds == null || thresholds.Count < 3)
                return 0; 
            if (overagePercentage > thresholds[0] && overagePercentage <= thresholds[1])
            {
                return thresholds[0]; 
            }
            else if (overagePercentage > thresholds[1] && overagePercentage <= thresholds[2])
            {
                return thresholds[1]; 
            }
            else if (overagePercentage > thresholds[2])
            {
                return thresholds[2];
            }
            return 0; 
        }
        public virtual async Task<decimal> GetOverduePercentageByTaskIdAsync(int taskId)
        {
            var task = await _projectTaskService.GetProjectTasksWithoutCacheByIdAsync(taskId);
            if (task == null)
                return 0;
            var developmentTime = await _timeSheetsService.GetDevelopmentTimeByTaskId(taskId);
            string bigTime = await _timeSheetsService.ConvertSpentTimeAsync(developmentTime.SpentHours, developmentTime.SpentMinutes);
            string smallTime = await _timeSheetsService.ConvertToHHMMFormat(task.EstimatedTime);
            var overDue = await GetOverduePercentageByTimeAsync(bigTime, smallTime);
            return overDue;
        }

        public async Task<IList<ProjectTask>> GetOverdueTasksByEmployeeIdAsync(int employeeId)
        {
            var today = DateTime.UtcNow.Date;
            var allStatuses = await _workflowStatusRepository.Table
                .Where(s => s.ProcessWorkflowId != 0)
                .Select(s => new
                {
                    s.Id,
                    s.ProcessWorkflowId,
                    s.DisplayOrder,
                    s.IsDefaultDeveloperStatus,
                    s.StatusName
                })
                .ToListAsync();
            var allowedStatusPairs = allStatuses
                .GroupBy(s => s.ProcessWorkflowId)
                .SelectMany(g =>
                {
                    var newStatusId = g.OrderBy(s => s.DisplayOrder).FirstOrDefault()?.Id;
                    var activeStatusId = g.FirstOrDefault(s => s.IsDefaultDeveloperStatus)?.Id;
                    var codeReviewStatusId = g.FirstOrDefault(s =>
           string.Equals(
               (s.StatusName ?? "").Trim(),
               "Code Review",
               StringComparison.OrdinalIgnoreCase
           )
       )?.Id;
                    var readyToTestStatusId = g.FirstOrDefault(s =>
           string.Equals(
               (s.StatusName ?? "").Trim(),
               "Ready To Test",
               StringComparison.OrdinalIgnoreCase
           )
       )?.Id;
                    var qaOnLiveStatusId = g.FirstOrDefault(s =>
           string.Equals(
               (s.StatusName ?? "").Trim(),
               "QA on Live",
               StringComparison.OrdinalIgnoreCase
           )
       )?.Id;
                    var readyForLiveStatusId = g.FirstOrDefault(s =>
         string.Equals(
             (s.StatusName ?? "").Trim(),
             "Ready for Live",
             StringComparison.OrdinalIgnoreCase
         )
     )?.Id;
                    var reviewDoneStatusId = g.FirstOrDefault(s =>
       string.Equals(
           (s.StatusName ?? "").Trim(),
           "Code Review Done",
           StringComparison.OrdinalIgnoreCase
       )
   )?.Id;
                    var testFailedStatusId = g.FirstOrDefault(s =>
       string.Equals(
           (s.StatusName ?? "").Trim(),
           "Test Failed",
           StringComparison.OrdinalIgnoreCase
       )
   )?.Id;
                    var pairs = new List<(int WorkflowId, int StatusId)>();
                    var statusList = new List<int?>
                           {
                            newStatusId,
                            activeStatusId,
                            codeReviewStatusId,
                            readyToTestStatusId,
                            qaOnLiveStatusId,
                            readyForLiveStatusId,
                            reviewDoneStatusId,
                            testFailedStatusId
                            };

                    foreach (var status in statusList.Where(s => s.HasValue).Select(s => s.Value).Distinct())
                    {
                        pairs.Add((g.Key, status));
                    }

                    return pairs;
                })
                .ToHashSet();
            var baseTasks = await _projectTaskRepository.Table
                .Where(t =>
                    !t.IsDeleted &&
                    t.AssignedTo == employeeId &&
                    t.DueDate.HasValue &&
                    t.ProcessWorkflowId != 0 &&
                    t.StatusId != 0 &&                   
                    t.DueDate.Value.Date <= today)
                .ToListAsync();
            var overdueTasks = baseTasks
                .Where(t => allowedStatusPairs.Contains((t.ProcessWorkflowId, t.StatusId)))
                .ToList();
            return overdueTasks;
        }

        public virtual async Task<string> GetLearningTimeOfEmployeeByRange(int employeeId, DateTime from, DateTime to)
        {
            var learningProjectId = _monthlyReportSetting.LearningProjectId;
            var learningEntries = await _timeSheetRepository.Table
                .Where(ts =>
                    ts.EmployeeId == employeeId &&
                    ts.ProjectId == learningProjectId &&
                    ts.SpentDate >= from &&
                    ts.SpentDate <= to)
                .ToListAsync();
            var totalMinutes = learningEntries.Sum(e => (e.SpentHours * 60) + e.SpentMinutes);
            var hours = totalMinutes / 60;
            var minutes = totalMinutes % 60;
            var time = await _timeSheetsService.ConvertSpentTimeAsync(hours,minutes);
            return time;
        }

        public virtual async Task<int> GetManaualTimeOfEmployeeByRange(int employeeId, DateTime from, DateTime to)
        {           
            var learningEntries = await _timeSheetRepository.Table
                .Where(ts =>
                    ts.EmployeeId == employeeId &&
                    ts.IsManualEntry == true &&
                    ts.SpentDate >= from &&
                    ts.SpentDate <= to)
                .ToListAsync();
            var totalMinutes = learningEntries.Sum(e => (e.SpentHours * 60) + e.SpentMinutes);
            return totalMinutes;
        }
        #endregion
    }
}