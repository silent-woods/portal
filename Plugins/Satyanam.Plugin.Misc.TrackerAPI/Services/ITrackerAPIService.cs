using App.Core;
using App.Core.Domain.Activities;
using App.Core.Domain.ActivityEvents;
using App.Core.Domain.EmployeeAttendances;
using App.Core.Domain.Employees;
using App.Core.Domain.Projects;
using App.Core.Domain.ProjectTasks;
using App.Core.Domain.TimeSheets;
using Satyanam.Nop.Core.Domains;
using Satyanam.Plugin.Misc.TrackerAPI.Domain;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Satyanam.Plugin.Misc.TrackerAPI.Services;

public partial interface ITrackerAPIService
{
    #region Time Conversion Methods

    Task<int> ConvertHoursToMinutes(decimal totalHours);

    Task<string> ConvertToHHMMFormat(decimal totalHours);

    #endregion

    #region Tracker API Log Methods

    Task InsertTrackerAPILogAsync(TrackerAPILog trackerAPILog);

    Task<IPagedList<TrackerAPILog>> SearchTrackerAPILogsAsync(DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int pageIndex = 0, 
        int pageSize = int.MaxValue);

    Task DeleteTrackerAPILogsAsync(IList<TrackerAPILog> trackerAPILogs);

    Task<IList<TrackerAPILog>> GetTrackerAPILogByIdsAsync(int[] trackerAPILogIds);

    Task ClearTrackerAPILogAsync();

    #endregion

    #region Employee Methods

    Task<Employee> GetEmployeeByCustomerIdAsync(int customerId = 0);

    Task UpdateEmployeeAsync(Employee employee = null);

    Task<Employee> GetEmployeeByIdAsync(int id);

    Task<IList<Employee>> GetAllEmployeesAsync();

    #endregion

    #region Projects Methods

    Task<IList<Project>> GetProjectsByEmployeeIdAsync(int employeeId = 0);

    Task<Project> GetProjectByIdAsync(int id = 0);

    #endregion

    #region Tasks Methods

    Task InsertProjectTaskAsync(ProjectTask projectTask);

    Task UpdateProjectTaskAsync(ProjectTask projectTask);

    Task<ProjectTask> GetProjectTaskByIdAsync(int taskId = 0);

    Task<ProjectTask> GetTaskTitleByProjectIdAsync(int projectId = 0, string projectTitle = null);

    Task<IList<ProjectTask>> GetProjectTasksByProjectIdAsync(int projectId = 0, int assignedTo = 0, bool showHidden = false);

    Task<bool> CheckIfQaOrNotAsync(int employeeId = 0);

    Task<bool> CheckIfLeaderandManagerOrNotAsync(int employeeId = 0);

    Task<IList<Employee>> GetAllEmployeesByProjectIdAsync(int projectId);

    #endregion

    #region Manual Time Entry Methods

    Task InsertTimeSheetAsync(TimeSheet timeSheet);

    Task UpdateTimeSheetAsync(TimeSheet timeSheet);

    Task<TimeSheet> GetTimeSheetByEmployeeIdAsync(int employeeId = 0);

    #endregion

    #region Activity Methods

    Task InsertActivityAsync(Activity activity);

    Task UpdateActivityAsync(Activity activity);

    Task<Activity> GetActivityByIdAsync(int id);

    Task<IList<Activity>> GetActivitiesByTaskIdAsync(int taskId);

    Task<Activity> GetActivityAsync(int taskId, string activityName);

    Task<Activity> GetLastUpdatedActivityAsync(int employeeId);

    #endregion

    #region Activity Event Methods

    Task InsertActivityEventAsync(ActivityEvent activityEvent);

    Task UpdateActivityEventAsync(ActivityEvent activityEvent);

    Task<ActivityEvent> GetActivityEventByTimeSheetIdAndEmployeeIdAsync(int timeSheetId = 0, int employeeId = 0);

    #endregion

    #region Employee Attendance Methods

    Task InsertEmployeeAttendanceAsync(EmployeeAttendance employeeAttendance);

    Task UpdateEmployeeAttendanceAsync(EmployeeAttendance employeeAttendance);

    Task<EmployeeAttendance> GetTodayEmployeeAttendanceAsync(int employeeId = 0);

    #endregion

    #region QA Estimation Time Methods

    Task<decimal> CalculateQAEstimationTimeAsync(ProjectTask projectTask);

    #endregion

    #region Timesheet Methods

    Task<IList<TimeSheet>> GetTodayTimeSheetByEmployeeIdAsync(int employeeId = 0);

    Task<TimeSheet> GetLastTimeSheetByEmployeeIdAsync(int employeeId = 0, int projectId = 0);

    #endregion

    #region Task Change Log Methods

    Task InsertTaskChangeLogAsync(TaskChangeLog taskChangeLog);

    Task<IList<TaskChangeLog>> GetAllTaskChangeLogsByTaskIdAsync(int taskId = 0);

    #endregion

    #region Task Comments Methods

    Task InsertTaskCommentsAsync(TaskComments taskComments);

    Task<IList<TaskComments>> GetAllTaskCommentsByTaskIdAsync(int taskId = 0);

    #endregion

    #region Task Log Details Methods

    Task<IList<ProjectTask>> GetTaskLogDetailsByProjectIdAsync(int projectId = 0, int taskId = 0, string taskName = null,
        int taskStatusId = 0, int taskTypeId = 0, int processWorkflowId = 0, DateTime? dueDate = null);

    #endregion

    #region Process Workflow Methods

    Task<ProcessWorkflow> GetProcessWorkflowByIdAsync(int id);

    Task<IList<ProcessWorkflow>> GetAllProcessWorkflowsAsync();

    Task<IList<ProcessWorkflow>> GetProcessWorkflowsByIdsAsync(int[] processWorkflowIds);

    Task<WorkflowStatus> GetWorkflowStatusByIdAsync(int id);

    Task<WorkflowStatus> GetWorkflowStatusByProcessWorkflowIdAsync(int processWorkflowId);

    Task<IList<WorkflowStatus>> GetWorkflowStatusesByProcessWorkflowIdAsync(int processWorkflowId);

    Task<IList<ProcessRules>> GetProcessRulesByStatusAndProcessWorkflowIdAsync(int statusId, int processWorkflowId);

    Task<bool> GetProcessRulesByPreviousAndCurrentStatusIdAsync(int previousStatusId, int currentStatusId);

    Task<string> GetProcessRuleCommentByPreviousAndCurrentStatusIdAsync(int previousStatusId, int currentStatusId);

    #endregion

    #region Activity Tracking Methods

    Task InsertActivityTrackingAsync(ActivityTracking activityTracking);

    Task UpdateActivityTrackingAsync(ActivityTracking activityTracking);

    Task<ActivityTracking> GetActivityTrackingByEmployeeIdAsync(int employeeId = 0);

    #endregion
}
