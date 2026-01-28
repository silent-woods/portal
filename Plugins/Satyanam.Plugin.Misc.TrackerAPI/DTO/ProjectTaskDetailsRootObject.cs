using Newtonsoft.Json;
using System.Collections.Generic;

namespace Satyanam.Plugin.Misc.TrackerAPI.DTO;

public partial class ProjectTaskDetailsRootObject
{
    #region Ctor

    public ProjectTaskDetailsRootObject()
    {
        AvailableTaskActivities = new List<TaskActivitiesRootObject>();
    }

    #endregion

    #region Properties

    [JsonProperty("estimation_time")]
    public decimal EstimationTime { get; set; }

    [JsonProperty("spent_hours")]
    public int SpentHours { get; set; }

    [JsonProperty("spent_minutes")]
    public int SpentMinutes { get; set; }

    [JsonProperty("activities")]
    public IList<TaskActivitiesRootObject> AvailableTaskActivities { get; set; }

    #endregion
}
