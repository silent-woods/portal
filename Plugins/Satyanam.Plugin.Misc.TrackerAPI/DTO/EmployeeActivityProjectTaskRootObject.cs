using Newtonsoft.Json;

namespace Satyanam.Plugin.Misc.TrackerAPI.DTO;

public partial class EmployeeActivityProjectTaskRootObject
{
    #region Properties

    [JsonProperty("task_id")]
    public int TaskId { get; set; }

    [JsonProperty("activity_id")]
    public int ActivityId { get; set; }

    [JsonProperty("task_name")]
    public string TaskName { get; set; }

    [JsonProperty("activity_name")]
	public string ActivityName { get; set; }

    [JsonProperty("estimation_time")]
    public decimal EstimationTime { get; set; }

    [JsonProperty("spent_hours")]
    public int SpentHours { get; set; }

    [JsonProperty("spent_minutes")]
    public int SpentMinutes { get; set; }

    [JsonProperty("billable")]
    public bool Billable { get; set; }

    #endregion
}
