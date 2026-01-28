using Newtonsoft.Json;

namespace Satyanam.Plugin.Misc.TrackerAPI.Models;

public partial class EmployeeAutoTagParametersModel : TrackerAPIResponseModel
{
    #region Properties

    [JsonProperty("task_id")]
    public int TaskId { get; set; }

    [JsonProperty("status_id")]
    public int StatusId { get; set; }

    [JsonProperty("status")]
    public string Status { get; set; }

    #endregion
}
