using Newtonsoft.Json;

namespace Satyanam.Plugin.Misc.TrackerAPI.DTO;

public partial class TaskLogsRootObject
{
	#region Properties

	[JsonProperty("task_id")]
	public int TaskId { get; set; }

    [JsonProperty("employee_name")]
    public string EmployeeName { get; set; }

    [JsonProperty("project_name")]
    public string ProjectName { get; set; }

    [JsonProperty("task_name")]
    public string TaskName { get; set; }

    [JsonProperty("task_type")]
    public string TaskType { get; set; }

    [JsonProperty("estimation_time")]
    public string EstimationTime { get; set; }

    [JsonProperty("spent_hours")]
    public int SpentHours { get; set; }

    [JsonProperty("spent_minutes")]
    public int SpentMinutes { get; set; }

    [JsonProperty("status")]
    public string Status { get; set; }

    [JsonProperty("color_code")]
    public string ColorCode { get; set; }

	#endregion
}
