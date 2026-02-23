namespace Satyanam.Plugin.Misc.TrackerAPI;

public static partial class TrackerAPIDefaults
{
    #region Static System Names for Configuration

    public const string APIKeyHeader = "X-APIkey";

    public const string APISecretKeyHeader = "X-APISecretkey";

    #endregion

    #region Static System Names for Header and Log

    public const string EmployeeIdHeader = "EmployeeId";

    public const string TrackerLog = "Tracker Log : ";

    #endregion

    #region Static System Names for Employee

    public const string EmployeeLoginEndPoint = "EmployeeLogin";

    public const string GetEmployeeDetailsEndPoint = "GetEmployeeDetails";

    public const string GetEmployeeProfileEndPoint = "EmployeeProfile";

    #endregion

    #region Static System Names for Projects

    public const string GetDayTimeEndPoint = "GetDayTime";

    public const string GetProjectsEndPoint = "GetProjects";

    #endregion

    #region Static System Names for Tasks

    public const string CreateTaskEndPoint = "CreateTask";

    public const string GetTasksEndPoint = "GetTasks";

    public const string GetActivityTasksEndPoint = "ActivityTasks";

    public const string SearchTasksEndPoint = "SearchTasks";

    public const string SearchTaskDetailsEndPoint = "SearchTaskDetails";

    public const string GetTaskDetailsEndPoint = "GetTaskDetails";

    public const string UpdateTaskDetailsEndPoint = "UpdateTaskDetails";

    public const string GetTaskTypesAndProcessWorkflowsEndPoint = "GetTaskTypesAndProcessWorkflows";

    public const string GetTaskAlertProjectTaskEndPoint = "GetTaskAlertProjectTask";

    #endregion

    #region Static System Names for Time Management

    public const string ManualTimeEntryEndPoint = "ManualTimeEntry";

    public const string StartTrackEndPoint = "StartTrack";

    public const string ActivityEventEndPoint = "ActivityEvent";

    public const string StopTrackEndPoint = "StopTrack";

    #endregion

    #region Static System Names for Overdue Task & Swtich Task & Task Follow Up & Task Reasons

    public const string GetTaskAlertReasonsEndPoint = "TaskAlertReasons";

    public const string SendTaskAlertEmailEndPoint = "SendTaskAlertEmail";

    #endregion

    #region Static System Names for Task Statuses

    public const string GetTaskStatusesEndPoint = "GetTaskStatuses";

    public const string TaskStatusChangeEndPoint = "TaskStatusChange";

    #endregion

    #region Static System Names for Auto Tag Comment

    public const string EmployeeAutoTagEndPoint = "AutoTag";

    #endregion

    #region Static System Names for Process Workflow Statuses

    public const string Hold = "Hold";

    public const string HoldComment = "This task is hold due to started.";

    #endregion
}
