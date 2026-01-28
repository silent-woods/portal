using Newtonsoft.Json;
using System.Collections.Generic;

namespace Satyanam.Plugin.Misc.TrackerAPI.DTO;

public partial class TaskLogDetailsRootObject
{
	#region Ctor

	public TaskLogDetailsRootObject()
	{
		TaskDetails = new TaskDetailsRootObject();
        CalculateTaskTime = new CalculateTaskTimeRootObject();
        AvailableEmployeeDetails = new List<EmployeeDetailsRootObject>();
        AvailableTaskChangeLogs = new List<TaskChangeLogsRootObject>();
        AvailableTaskComments = new List<TaskCommentsRootObject>();
    }

	#endregion

	#region Properties

	[JsonProperty("task_details")]
	public TaskDetailsRootObject TaskDetails { get; set; }

    [JsonProperty("task_time")]
    public CalculateTaskTimeRootObject CalculateTaskTime { get; set; }

    [JsonProperty("employee_details")]
    public IList<EmployeeDetailsRootObject> AvailableEmployeeDetails { get; set; }

    [JsonProperty("task_change_logs")]
    public IList<TaskChangeLogsRootObject> AvailableTaskChangeLogs { get; set; }

	[JsonProperty("task_comments")]
	public IList<TaskCommentsRootObject> AvailableTaskComments { get; set; }

	#endregion
}
