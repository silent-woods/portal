using Newtonsoft.Json;

namespace Satyanam.Plugin.Misc.TrackerAPI.DTO;

public partial class ProjectTasksRootObject
{
    #region Properties

    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("task_title")]
    public string TaskTitle { get; set; }

    [JsonProperty("estimation_time")]
    public decimal EstimationTime { get; set; }

    [JsonProperty("spent_time")]
    public decimal SpentTime { get; set; }

	#endregion
}
