using Newtonsoft.Json;
using System;

namespace Satyanam.Plugin.Misc.TrackerAPI.Models;

public partial class EmployeeManualTimeEntryParametersModel : TrackerAPIResponseModel
{
    #region Properties

    [JsonProperty("activity_description")]
    public string ActivityDescription { get; set; }

    [JsonProperty("billable")]
    public bool Billable { get; set; }

    [JsonProperty("spent_date")]
    public DateTime SpentDate { get; set; }

    [JsonProperty("spent_hours")]
    public int SpentHours { get; set; }

    [JsonProperty("spent_minutes")]
    public int SpentMinutes { get; set; }

    #endregion
}
