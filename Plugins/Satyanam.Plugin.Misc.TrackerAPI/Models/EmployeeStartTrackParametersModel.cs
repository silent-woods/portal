using Newtonsoft.Json;

namespace Satyanam.Plugin.Misc.TrackerAPI.Models;

public partial class EmployeeStartTrackParametersModel : TrackerAPIResponseModel
{
    #region Properties

    [JsonProperty("activity_description")]
    public string ActivityDescription { get; set; }

    [JsonProperty("billable")]
    public bool Billable { get; set; }

    #endregion
}
