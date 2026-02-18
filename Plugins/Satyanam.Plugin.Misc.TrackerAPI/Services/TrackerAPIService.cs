using App.Core;
using App.Core.Domain.Activities;
using App.Core.Domain.ActivityEvents;
using App.Core.Domain.Designations;
using App.Core.Domain.EmployeeAttendances;
using App.Core.Domain.Employees;
using App.Core.Domain.Extension.ProjectTasks;
using App.Core.Domain.Extension.TimeSheets;
using App.Core.Domain.ProjectEmployeeMappings;
using App.Core.Domain.Projects;
using App.Core.Domain.ProjectTasks;
using App.Core.Domain.TimeSheets;
using App.Data;
using App.Data.Extensions;
using App.Services.Designations;
using App.Services.ProjectTasks;
using Satyanam.Nop.Core.Domains;
using Satyanam.Plugin.Misc.TrackerAPI.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Satyanam.Plugin.Misc.TrackerAPI.Services;

public partial class TrackerAPIService : ITrackerAPIService
{
    #region Fields

    protected readonly IDesignationService _designationService;
    protected readonly IProjectTaskService _projectTaskService;
    protected readonly IRepository<Activity> _activityRepository;
    protected readonly IRepository<ActivityEvent> _activityEventRepository;
    protected readonly IRepository<ActivityTracking> _activityTrackingRepository;
    protected readonly IRepository<Designation> _designationRepository;
    protected readonly IRepository<EmployeeAttendance> _employeeAttendanceRepository;
    protected readonly IRepository<Employee> _employeeRepository;
    protected readonly IRepository<ProcessRules> _processRulesRepository;
    protected readonly IRepository<ProcessWorkflow> _processWorkflowRepository;
    protected readonly IRepository<ProjectEmployeeMapping> _projectEmployeeMappingRepository;
    protected readonly IRepository<Project> _projectRepository;
    protected readonly IRepository<ProjectTask> _projectTaskRepository;
    protected readonly IRepository<TaskChangeLog> _taskChangeLogRepository;
    protected readonly IRepository<TaskComments> _taskCommentsRepository;
    protected readonly IRepository<TimeSheet> _timeSheetRepository;
    protected readonly IRepository<TrackerAPILog> _trackerAPILogRepository;
    protected readonly IRepository<WorkflowStatus> _workFlowStatusRepository;
    protected readonly MonthlyReportSetting _monthlyReportSettings;

    #endregion

    #region Ctor

    public TrackerAPIService(IDesignationService designationService,
        IProjectTaskService projectTaskService,
        IRepository<Activity> activityRepository,
        IRepository<ActivityEvent> activityEventRepository,
        IRepository<ActivityTracking> activityTrackingRepository,
        IRepository<Designation> designationRepository,
        IRepository<EmployeeAttendance> employeeAttendanceRepository,
        IRepository<Employee> employeeRepository,
        IRepository<ProcessRules> processRulesRepository,
        IRepository<ProcessWorkflow> processWorkflowRepository,
        IRepository<ProjectEmployeeMapping> projectEmployeeMappingRepository,
        IRepository<Project> projectRepository,
        IRepository<ProjectTask> projectTaskRepository,
        IRepository<TaskChangeLog> taskChangeLogRepository,
        IRepository<TaskComments> taskCommentsRepository,
        IRepository<TimeSheet> timeSheetRepository,
        IRepository<TrackerAPILog> trackerAPILogRepository,
        IRepository<WorkflowStatus> workFlowStatusRepository,
        MonthlyReportSetting monthlyReportSettings)
    {
        _designationService = designationService;
        _projectTaskService = projectTaskService;
        _activityRepository = activityRepository;
        _activityEventRepository = activityEventRepository;
        _activityTrackingRepository = activityTrackingRepository;
        _designationRepository = designationRepository;
        _employeeAttendanceRepository = employeeAttendanceRepository;
        _employeeRepository = employeeRepository;
        _processRulesRepository = processRulesRepository;
        _processWorkflowRepository = processWorkflowRepository;
        _projectEmployeeMappingRepository = projectEmployeeMappingRepository;
        _projectRepository = projectRepository;
        _projectTaskRepository = projectTaskRepository;
        _taskChangeLogRepository = taskChangeLogRepository;
        _taskCommentsRepository = taskCommentsRepository;
        _timeSheetRepository = timeSheetRepository;
        _trackerAPILogRepository = trackerAPILogRepository;
        _workFlowStatusRepository = workFlowStatusRepository;
        _monthlyReportSettings = monthlyReportSettings;
    }

    #endregion

    #region Time Conversion Methods

    public Task<int> ConvertHoursToMinutes(decimal totalHours)
    {
        if (totalHours < 0)
            throw new ArgumentException("Total hours cannot be negative.");

        int totalMinutes = (int)Math.Round(totalHours * 60);

        return Task.FromResult(totalMinutes);
    }

    public virtual Task<string> ConvertToHHMMFormat(decimal totalHours)
    {
        if (totalHours < 0)
            throw new ArgumentException("Total hours cannot be negative.");

        int hours = (int)totalHours;
        int minutes = (int)Math.Round((totalHours - hours) * 60);

        string formatted = $"{hours}.{minutes:D2}";

        return Task.FromResult(formatted);
    }

    #endregion

    #region Tracker API Log Methods

    public virtual async Task InsertTrackerAPILogAsync(TrackerAPILog trackerAPILog)
    {
        ArgumentNullException.ThrowIfNull(nameof(trackerAPILog));

        await _trackerAPILogRepository.InsertAsync(trackerAPILog);
    }

    public virtual async Task<IPagedList<TrackerAPILog>> SearchTrackerAPILogsAsync(DateTime? createdFromUtc = null, DateTime? createdToUtc = null,
        int pageIndex = 0, int pageSize = int.MaxValue)
    {
        var trackerAPILogs = from tal in _trackerAPILogRepository.Table
                             select tal;

        if (createdFromUtc.HasValue)
            trackerAPILogs = trackerAPILogs.Where(tal => createdFromUtc.Value <= tal.CreatedOnUtc);

        if (createdToUtc.HasValue)
            trackerAPILogs = trackerAPILogs.Where(tal => createdToUtc.Value >= tal.CreatedOnUtc);

        trackerAPILogs = trackerAPILogs.OrderByDescending(tal => tal.Id);

        return await trackerAPILogs.ToPagedListAsync(pageIndex, pageSize);
    }

    public virtual async Task DeleteTrackerAPILogsAsync(IList<TrackerAPILog> trackerAPILogs)
    {
        await _trackerAPILogRepository.DeleteAsync(trackerAPILogs);
    }

    public virtual async Task<IList<TrackerAPILog>> GetTrackerAPILogByIdsAsync(int[] trackerAPILogIds)
    {
        return await _trackerAPILogRepository.GetByIdsAsync(trackerAPILogIds);
    }

    public virtual async Task ClearTrackerAPILogAsync()
    {
        await _trackerAPILogRepository.TruncateAsync();
    }

    #endregion

    #region Employee Methods

    public virtual async Task<Employee> GetEmployeeByCustomerIdAsync(int customerId = 0)
    {
        ArgumentNullException.ThrowIfNull(nameof(customerId));

        return await _employeeRepository.Table.Where(e => e.Customer_Id == customerId).FirstOrDefaultAsync();
    }

    public virtual async Task UpdateEmployeeAsync(Employee employee = null)
    {
        ArgumentNullException.ThrowIfNull(nameof(employee));

        await _employeeRepository.UpdateAsync(employee);
    }

    public virtual async Task<Employee> GetEmployeeByIdAsync(int id = 0)
    {
        ArgumentNullException.ThrowIfNull(nameof(id));

        return await _employeeRepository.GetByIdAsync(id);
    }

    public virtual async Task<IList<Employee>> GetAllEmployeesAsync()
    {
        var employees = from e in _employeeRepository.Table
                        select e;

        return await employees.ToListAsync();
    }

    #endregion

    #region Projects Methods

    public virtual async Task<IList<Project>> GetProjectsByEmployeeIdAsync(int employeeId = 0)
    {
        ArgumentNullException.ThrowIfNull(nameof(employeeId));

        var projects = from p in _projectRepository.Table
                       join pem in _projectEmployeeMappingRepository.Table on p.Id equals pem.ProjectId
                       where pem.EmployeeId == employeeId && pem.IsActive && !p.IsDeleted
                       select p;

        projects = projects.OrderByDescending(p => p.Id);

        return await projects.ToListAsync();
    }

    public virtual async Task<Project> GetProjectByIdAsync(int id = 0)
    {
        ArgumentNullException.ThrowIfNull(nameof(id));

        return await _projectRepository.GetByIdAsync(id);
    }

    #endregion

    #region Tasks Methods

    public virtual async Task InsertProjectTaskAsync(ProjectTask projectTask)
    {
        ArgumentNullException.ThrowIfNull(nameof(projectTask));

        await _projectTaskRepository.InsertAsync(projectTask);
    }

    public virtual async Task UpdateProjectTaskAsync(ProjectTask projectTask)
    {
        ArgumentNullException.ThrowIfNull(nameof(projectTask));

        var oldTask = await GetProjectTaskByIdAsync(projectTask.Id);

        var oldTaskCopy = new ProjectTask
        {
            Id = oldTask.Id,
            TaskTitle = oldTask.TaskTitle,
            EstimatedTime = oldTask.EstimatedTime,
            ProjectId = oldTask.ProjectId,
            Description = oldTask.Description,
            StatusId = oldTask.StatusId,
            AssignedTo = oldTask.AssignedTo,
            DueDate = oldTask.DueDate,
            Tasktypeid = oldTask.Tasktypeid,
            ProcessWorkflowId = oldTask.ProcessWorkflowId,
            ParentTaskId = oldTask.ParentTaskId,
            IsSync = oldTask.IsSync,
        };

        await _projectTaskRepository.UpdateAsync(projectTask);
        await _projectTaskService.UpdateParentTaskWorkQualityAsync(oldTaskCopy, projectTask);
    }

    public virtual async Task<ProjectTask> GetProjectTaskByIdAsync(int taskId = 0)
    {
        ArgumentNullException.ThrowIfNull(nameof(taskId));

        return await _projectTaskRepository.GetByIdAsync(taskId);
    }

    public virtual async Task<ProjectTask> GetTaskTitleByProjectIdAsync(int projectId = 0, string projectTitle = null)
    {
        ArgumentNullException.ThrowIfNull(nameof(projectId));

        var projectTask = from pt in _projectTaskRepository.Table
                          where pt.ProjectId == projectId && pt.TaskTitle == projectTitle
                          select pt;

        return await projectTask.FirstOrDefaultAsync();
    }

    public virtual async Task<IList<ProjectTask>> GetProjectTasksByProjectIdAsync(int projectId = 0, int assignedTo = 0,
        bool showHidden = false)
    {
        ArgumentNullException.ThrowIfNull(nameof(projectId));

        var projectTasks = from pt in _projectTaskRepository.Table
                           where pt.ProjectId == projectId && !pt.IsDeleted
                           select pt;

        if (assignedTo > 0)
            projectTasks = projectTasks.Where(pt => pt.AssignedTo == assignedTo || pt.DeveloperId == assignedTo);

        if (showHidden)
            projectTasks = projectTasks.Where(pt => pt.Tasktypeid != (int)TaskTypeEnum.Bug);

        if (!showHidden)
            projectTasks = projectTasks.Where(pt => pt.Tasktypeid != (int)TaskTypeEnum.UserStory);

        projectTasks = projectTasks.OrderByDescending(pt => pt.Id);

        return await projectTasks.ToListAsync();
    }

    public virtual async Task<bool> CheckIfQaOrNotAsync(int employeeId = 0)
    {
        var roleId = await _designationService.GetQARoleId();

        var qaEmployeeId = from e in _employeeRepository.Table
                           where e.EmployeeStatusId == 1 && e.DesignationId == roleId && e.Id == employeeId
                           select e;

        var employee = qaEmployeeId.FirstOrDefault();

        if (employee != null)
            return true;

        return false;
    }

    public virtual async Task<bool> CheckIfLeaderandManagerOrNotAsync(int employeeId = 0)
    {
        var roleId = await GetRoleIdForProjectLeaderAndProjectManager();

        var leaderOrManagerEmployeeId = from e in _employeeRepository.Table
                                        where e.EmployeeStatusId == 1 && e.DesignationId == roleId && e.Id == employeeId
                                        select e;

        var employee = leaderOrManagerEmployeeId.FirstOrDefault();

        if (employee != null)
            return true;

        return false;
    }

    public virtual async Task<int> GetRoleIdForProjectLeaderAndProjectManager()
    {
        int roleId = 0;
        var designations = await _designationService.GetAllDesignationAsync(string.Empty);
        foreach (var designation in designations)
        {
            if (designation.Name.ToLower().Trim() == "manager" || designation.Name.ToLower().Trim() == "project manager" ||
                designation.Name.ToLower().Trim() == "team manager" || designation.Name.ToLower().Trim() == "team leader" ||
                designation.Name.ToLower().Trim() == "project leader")
                roleId = designation.Id;
        }

        return roleId;
    }

    public virtual async Task<IList<Employee>> GetAllEmployeesByProjectIdAsync(int projectId)
    {
        var employees = from e in _employeeRepository.Table
                        join pem in _projectEmployeeMappingRepository.Table on e.Id equals pem.EmployeeId
                        join p in _projectRepository.Table on pem.ProjectId equals p.Id
                        where pem.ProjectId == projectId
                        select e;

        return await employees.ToListAsync();
    }

    #endregion

    #region Manual Time Entry Methods

    public virtual async Task InsertTimeSheetAsync(TimeSheet timeSheet)
    {
        ArgumentNullException.ThrowIfNull(nameof(timeSheet));

        await _timeSheetRepository.InsertAsync(timeSheet);
    }

    public virtual async Task UpdateTimeSheetAsync(TimeSheet timeSheet)
    {
        ArgumentNullException.ThrowIfNull(nameof(timeSheet));

        await _timeSheetRepository.UpdateAsync(timeSheet);
    }

    public virtual async Task<TimeSheet> GetTimeSheetByEmployeeIdAsync(int employeeId = 0)
    {
        ArgumentNullException.ThrowIfNull(nameof(employeeId));

        var timeSheets = from ts in _timeSheetRepository.Table
                         where ts.EmployeeId == employeeId && ts.StartTime != null
                         orderby ts.Id descending
                         select ts;

        return await timeSheets.FirstOrDefaultAsync();
    }

    #endregion

    #region Activity Methods

    public virtual async Task InsertActivityAsync(Activity activity)
    {
        ArgumentNullException.ThrowIfNull(nameof(activity));

        await _activityRepository.InsertAsync(activity);
    }

    public virtual async Task UpdateActivityAsync(Activity activity)
    {
        ArgumentNullException.ThrowIfNull(nameof(activity));

        await _activityRepository.UpdateAsync(activity);
    }

    public virtual async Task<Activity> GetActivityByIdAsync(int id)
    {
        ArgumentNullException.ThrowIfNull(nameof(id));

        return await _activityRepository.GetByIdAsync(id);
    }

    public virtual async Task<IList<Activity>> GetActivitiesByTaskIdAsync(int taskId)
    {
        ArgumentNullException.ThrowIfNull(nameof(taskId));

        var activities = from a in _activityRepository.Table
                         where a.TaskId == taskId
                         select a;

        return await activities.ToListAsync();
    }

    public virtual async Task<Activity> GetActivityAsync(int taskId, string activityName)
    {
        ArgumentNullException.ThrowIfNull(nameof(taskId));

        ArgumentNullException.ThrowIfNull(nameof(activityName));

        return await _activityRepository.Table.Where(a => a.TaskId == taskId && a.ActivityName == activityName).FirstOrDefaultAsync();
    }

    public virtual async Task<Activity> GetLastUpdatedActivityAsync(int employeeId)
    {
        ArgumentNullException.ThrowIfNull(nameof(employeeId));

        var activity = from a in _activityRepository.Table
                       orderby a.UpdateOnUtc descending
                       select a;

        return await activity.FirstOrDefaultAsync();
    }

    #endregion

    #region Activity Event Methods

    public virtual async Task InsertActivityEventAsync(ActivityEvent activityEvent)
    {
        ArgumentNullException.ThrowIfNull(nameof(activityEvent));

        await _activityEventRepository.InsertAsync(activityEvent);
    }

    public virtual async Task UpdateActivityEventAsync(ActivityEvent activityEvent)
    {
        ArgumentNullException.ThrowIfNull(nameof(activityEvent));

        await _activityEventRepository.UpdateAsync(activityEvent);
    }

    public virtual async Task<ActivityEvent> GetActivityEventByTimeSheetIdAndEmployeeIdAsync(int timeSheetId = 0, int employeeId = 0)
    {
        ArgumentNullException.ThrowIfNull(nameof(timeSheetId));

        ArgumentNullException.ThrowIfNull(nameof(employeeId));

        var timeSheets = from ae in _activityEventRepository.Table
                         join ts in _timeSheetRepository.Table on ae.TimesheetId equals ts.Id
                         where ae.TimesheetId == timeSheetId && ae.EmployeeId == employeeId
                         orderby ts.Id descending
                         select ae;

        return await timeSheets.FirstOrDefaultAsync();
    }

    #endregion

    #region Employee Attendance Methods

    public virtual async Task InsertEmployeeAttendanceAsync(EmployeeAttendance employeeAttendance)
    {
        ArgumentNullException.ThrowIfNull(nameof(employeeAttendance));

        await _employeeAttendanceRepository.InsertAsync(employeeAttendance);
    }

    public virtual async Task UpdateEmployeeAttendanceAsync(EmployeeAttendance employeeAttendance)
    {
        ArgumentNullException.ThrowIfNull(nameof(employeeAttendance));

        await _employeeAttendanceRepository.UpdateAsync(employeeAttendance);
    }

    public virtual async Task<EmployeeAttendance> GetTodayEmployeeAttendanceAsync(int employeeId = 0)
    {
        var employeeAttendance = from ea in _employeeAttendanceRepository.Table
                                 where ea.CheckIn.Date == DateTime.UtcNow.Date && ea.EmployeeId == employeeId
                                 select ea;

        return await employeeAttendance.FirstOrDefaultAsync();
    }

    #endregion

    #region QA Estimation Time Methods

    public virtual async Task<decimal> CalculateQAEstimationTimeAsync(ProjectTask projectTask)
    {
        decimal qaEstimationHours = ((projectTask.EstimatedTime * 60 * _monthlyReportSettings.AllowedQABillableHours) / 100) / 60;

        return qaEstimationHours;
    }

    #endregion

    #region Timesheet Methods

    public virtual async Task<IList<TimeSheet>> GetTodayTimeSheetByEmployeeIdAsync(int employeeId = 0)
    {
        ArgumentNullException.ThrowIfNull(nameof(employeeId));

        var timeSheets = from ts in _timeSheetRepository.Table
                         where ts.UpdateOnUtc.Date == DateTime.UtcNow.Date && ts.EndTime != null && ts.EmployeeId == employeeId
                         select ts;

        return await timeSheets.ToListAsync();
    }
    public virtual async Task<TimeSheet> GetLastTimeSheetByEmployeeIdAsync(int employeeId = 0, int projectId = 0)
    {
        ArgumentNullException.ThrowIfNull(nameof(employeeId));

        var timeSheet = from ts in _timeSheetRepository.Table
                        where ts.EmployeeId == employeeId
                        select ts;

        if (projectId > 0)
            timeSheet = timeSheet.Where(ts => ts.ProjectId == projectId);

        timeSheet = timeSheet.OrderByDescending(ts => ts.Id);

        return await timeSheet.FirstOrDefaultAsync();
    }

    #endregion

    #region Task Change Log Methods

    public virtual async Task InsertTaskChangeLogAsync(TaskChangeLog taskChangeLog)
    {
        ArgumentException.ThrowIfNullOrEmpty(nameof(taskChangeLog));

        await _taskChangeLogRepository.InsertAsync(taskChangeLog);
    }

    public virtual async Task<IList<TaskChangeLog>> GetAllTaskChangeLogsByTaskIdAsync(int taskId = 0)
    {
        ArgumentNullException.ThrowIfNull(nameof(taskId));

        var taskChangeLogs = from tcl in _taskChangeLogRepository.Table
                             join e in _employeeRepository.Table on tcl.EmployeeId equals e.Id
                             where tcl.TaskId == taskId
                             select tcl;

        taskChangeLogs = taskChangeLogs.OrderByDescending(tcl => tcl.Id);

        return await taskChangeLogs.ToListAsync();
    }

    #endregion

    #region Task Comments Methods

    public virtual async Task InsertTaskCommentsAsync(TaskComments taskComments)
    {
        ArgumentException.ThrowIfNullOrEmpty(nameof(taskComments));

        await _taskCommentsRepository.InsertAsync(taskComments);
    }

    public virtual async Task<IList<TaskComments>> GetAllTaskCommentsByTaskIdAsync(int taskId = 0)
    {
        ArgumentNullException.ThrowIfNull(nameof(taskId));

        var taskComments = from tc in _taskCommentsRepository.Table
                           join e in _employeeRepository.Table on tc.EmployeeId equals e.Id
                           where tc.TaskId == taskId
                           select tc;

        taskComments = taskComments.OrderByDescending(tcl => tcl.Id);

        return await taskComments.ToListAsync();
    }

    #endregion

    #region Task Log Details Methods

    public virtual async Task<IList<ProjectTask>> GetTaskLogDetailsByProjectIdAsync(int projectId = 0, int taskId = 0,
        string taskName = null, int taskStatusId = 0, int taskTypeId = 0, int processWorkflowId = 0, DateTime? dueDate = null)
    {
        var workflowStatus = _workFlowStatusRepository.Table.Where(ws => ws.IsDefaultDeveloperStatus && ws.DisplayOrder == 0)
                            .OrderBy(ws => ws.Id)
                            .FirstOrDefault();

        var projectTaskDetails = from pt in _projectTaskRepository.Table
                                 where pt.ProjectId == projectId
                                 select pt;

        if (taskId > 0)
            projectTaskDetails = projectTaskDetails.Where(pt => pt.Id == taskId);

        if (!string.IsNullOrWhiteSpace(taskName))
            projectTaskDetails = projectTaskDetails.Where(pt => pt.TaskTitle.Contains(taskName));

        if (taskStatusId > 0 && dueDate == null)
            projectTaskDetails = projectTaskDetails.Where(pt => pt.StatusId == taskStatusId);

        if (taskTypeId > 0)
            projectTaskDetails = projectTaskDetails.Where(pt => pt.Tasktypeid == taskTypeId);

        if (processWorkflowId > 0 && dueDate == null)
            projectTaskDetails = projectTaskDetails.Where(pt => pt.ProcessWorkflowId == processWorkflowId);

        if (dueDate != null && processWorkflowId == 0 && taskStatusId == 0)
        {
            projectTaskDetails = from pt in projectTaskDetails
                                 join ws in _workFlowStatusRepository.Table on pt.StatusId equals ws.Id
                                 join pw in _processWorkflowRepository.Table on pt.ProcessWorkflowId equals pw.Id
                                 where (ws.IsDefaultDeveloperStatus || ws.DisplayOrder == 0 || ws.DisplayOrder == 1) &&
                                 pt.DueDate.Value.Date <= dueDate.Value.Date
                                 select pt;
        }
        else if (dueDate != null && processWorkflowId > 0 && taskStatusId == 0)
        {
            projectTaskDetails = from pt in projectTaskDetails
                                 join ws in _workFlowStatusRepository.Table on pt.StatusId equals ws.Id
                                 join pw in _processWorkflowRepository.Table on pt.ProcessWorkflowId equals pw.Id
                                 where (ws.IsDefaultDeveloperStatus || ws.DisplayOrder == 0 || ws.DisplayOrder == 1) &&
                                 pt.ProcessWorkflowId == processWorkflowId
                                 select pt;
        }
        else if (dueDate != null && processWorkflowId > 0 && taskStatusId > 0)
        {
            projectTaskDetails = from ptd in projectTaskDetails
                                 join pc in _processWorkflowRepository.Table on ptd.ProcessWorkflowId equals pc.Id
                                 join ws in _workFlowStatusRepository.Table on ptd.StatusId equals ws.Id
                                 where ptd.ProcessWorkflowId == processWorkflowId && ptd.DueDate.Value.Date <= dueDate.Value.Date &&
                                 ptd.StatusId == taskStatusId
                                 select ptd;
        }

        projectTaskDetails = projectTaskDetails.OrderBy(pt => pt.DueDate);

        return await projectTaskDetails.ToListAsync();
    }

    #endregion

    #region Process Workflow Methods

    public virtual async Task<ProcessWorkflow> GetProcessWorkflowByIdAsync(int id)
    {
        return await _processWorkflowRepository.GetByIdAsync(id);
    }

    public virtual async Task<IList<ProcessWorkflow>> GetAllProcessWorkflowsAsync()
    {
        var processWorkflows = from pw in _processWorkflowRepository.Table
                               orderby pw.DisplayOrder
                               select pw;

        return await processWorkflows.ToListAsync();
    }

    public virtual async Task<IList<ProcessWorkflow>> GetProcessWorkflowsByIdsAsync(int[] processWorkflowIds)
    {
        var workflows = await _processWorkflowRepository.GetByIdsAsync(processWorkflowIds);

        var processWorkflows = workflows.Where(w => w.IsActive).OrderBy(w => w.DisplayOrder).ToList();

        return processWorkflows;
    }

    public virtual async Task<WorkflowStatus> GetWorkflowStatusByIdAsync(int id)
    {
        return await _workFlowStatusRepository.GetByIdAsync(id);
    }

    public virtual async Task<WorkflowStatus> GetWorkflowStatusByProcessWorkflowIdAsync(int processWorkflowId)
    {
        var query = from ws in _workFlowStatusRepository.Table
                    where ws.ProcessWorkflowId == processWorkflowId
                    orderby ws.DisplayOrder
                    select ws;

        return await query.FirstOrDefaultAsync();
    }

    public virtual async Task<IList<WorkflowStatus>> GetWorkflowStatusesByProcessWorkflowIdAsync(int processWorkflowId)
    {
        var query = from ws in _workFlowStatusRepository.Table
                    where ws.ProcessWorkflowId == processWorkflowId
                    orderby ws.DisplayOrder
                    select ws;

        return await query.ToListAsync();
    }

    public virtual async Task<IList<ProcessRules>> GetProcessRulesByStatusAndProcessWorkflowIdAsync(int statusId, int processWorkflowId)
    {
        var query = from pr in _processRulesRepository.Table
                    where pr.FromStateId == statusId && pr.ProcessWorkflowId == processWorkflowId && pr.IsActive
                    select pr;

        return await query.ToListAsync();
    }

    public virtual async Task<bool> GetProcessRulesByPreviousAndCurrentStatusIdAsync(int previousStatusId, int currentStatusId)
    {
        var query = from pr in _processRulesRepository.Table
                    where pr.FromStateId == previousStatusId && pr.ToStateId == currentStatusId && pr.IsActive
                    select pr;

        var processRule = await query.FirstOrDefaultAsync();

        if (processRule == null)
            return false;

        return processRule.IsCommentRequired;
    }

    public virtual async Task<string> GetProcessRuleCommentByPreviousAndCurrentStatusIdAsync(int previousStatusId, int currentStatusId)
    {
        var query = from pr in _processRulesRepository.Table
                    where pr.FromStateId == previousStatusId && pr.ToStateId == currentStatusId && pr.IsActive
                    select pr;

        var processRule = await query.FirstOrDefaultAsync();

        if (processRule == null)
            return string.Empty;

        return processRule.CommentTemplate;
    }

    #endregion

    #region Activity Tracking Methods

    public virtual async Task InsertActivityTrackingAsync(ActivityTracking activityTracking)
    {
        ArgumentException.ThrowIfNullOrEmpty(nameof(activityTracking));

        await _activityTrackingRepository.InsertAsync(activityTracking);
    }

    public virtual async Task UpdateActivityTrackingAsync(ActivityTracking activityTracking)
    {
        ArgumentException.ThrowIfNullOrEmpty(nameof(activityTracking));

        await _activityTrackingRepository.UpdateAsync(activityTracking);
    }

    public virtual async Task<ActivityTracking> GetActivityTrackingByEmployeeIdAsync(int employeeId = 0)
    {
        ArgumentException.ThrowIfNullOrEmpty(nameof(employeeId));

        var activityTracking = from at in _activityTrackingRepository.Table
                               where at.EmployeeId == employeeId && at.StartTime.Date == DateTime.UtcNow.Date
                               select at;

        activityTracking = activityTracking.OrderByDescending(at => at.Id);

        return await activityTracking.FirstOrDefaultAsync();
    }

    #endregion
}
