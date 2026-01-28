using Newtonsoft.Json;

namespace Satyanam.Plugin.Misc.TrackerAPI.DTO;

public partial class TaskAlertConfigurationRootObject
{
    #region Properties

    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("task_alert_type_id")]
    public int TaskAlertTypeId { get; set; }

    [JsonProperty("message")]
    public string Message { get; set; }

    [JsonProperty("percentage")]
    public decimal Percentage { get; set; }

    [JsonProperty("comment_required")]
    public bool CommentRequired { get; set; }

    [JsonProperty("reason_required")]
    public bool ReasonRequired { get; set; }

    [JsonProperty("new_eta")]
    public bool NewETA { get; set; }

    [JsonProperty("enable_ontrack")]
    public bool EnableOnTrack { get; set; }

    [JsonProperty("total_hours")]
    public int TotalHours { get; set; }

    [JsonProperty("total_minutes")]
    public int TotalMinutes { get; set; }

    #endregion
}
