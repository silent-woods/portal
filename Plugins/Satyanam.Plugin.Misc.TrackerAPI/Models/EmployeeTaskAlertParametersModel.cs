using Newtonsoft.Json;

namespace Satyanam.Plugin.Misc.TrackerAPI.Models;

public partial class EmployeeTaskAlertParametersModel : TrackerAPIResponseModel
{
    #region Properties

    [JsonProperty("task_id")]
    public int TaskId { get; set; }

    [JsonProperty("alert_id")]
    public int AlertId { get; set; }

    [JsonProperty("is_on_track")]
    public bool IsOnTrack { get; set; }

    [JsonProperty("new_eta")]
    public string NewETA { get; set; }

    [JsonProperty("spent_time")]
    public string SpentTime { get; set; }

    [JsonProperty("reason")]
    public string Reason { get; set; }

    [JsonProperty("comment")]
    public string Comment { get; set; }

    #endregion
}
