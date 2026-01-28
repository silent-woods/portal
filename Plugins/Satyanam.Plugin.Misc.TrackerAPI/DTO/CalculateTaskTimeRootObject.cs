using Newtonsoft.Json;

namespace Satyanam.Plugin.Misc.TrackerAPI.DTO;

public partial class CalculateTaskTimeRootObject
{
    #region Properties

    [JsonProperty("billable_development_time")]
    public string BillableDevelopmentTime { get; set; }

    [JsonProperty("not_billable_development_time")]
    public string NotBillableDevelopmentTime { get; set; }

    [JsonProperty("billable_qa_time")]
    public string BillableQATime { get; set; }

    [JsonProperty("not_billable_qa_time")]
    public string NotBillableQATime { get; set; }

    [JsonProperty("total_development_time")]
    public string TotalDevelopmentTime { get; set; }

    [JsonProperty("total_qa_time")]
    public string TotalQATime { get; set; }

    [JsonProperty("total_billable_time")]
    public string TotalBillableTime { get; set; }

    [JsonProperty("total_not_billable_time")]
    public string TotalNotBillableTime { get; set; }

    [JsonProperty("total_spent_time")]
    public string TotalSpentTime { get; set; }

    #endregion
}
