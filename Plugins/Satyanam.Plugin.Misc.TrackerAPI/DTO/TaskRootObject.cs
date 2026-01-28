using Newtonsoft.Json;
using System.Collections.Generic;

namespace Satyanam.Plugin.Misc.TrackerAPI.DTO;

public partial class TaskRootObject
{
    #region Ctor

    public TaskRootObject()
    {
        AvailableActivities = new List<ActivityRootObject>();
    }

    #endregion

    #region Properties

    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("task_title")]
    public string TaskTitle { get; set; }

    [JsonProperty("estimation_time")]
    public decimal EstimationTime { get; set; }

    [JsonProperty("spent_hours")]
    public int SpentHours { get; set; }

    [JsonProperty("spent_minutes")]
    public int SpentMinutes { get; set; }

    [JsonProperty("activities")]
    public IList<ActivityRootObject> AvailableActivities { get; set; }

    #endregion
}
