using Newtonsoft.Json;

namespace Satyanam.Plugin.Misc.TrackerAPI.Models;

public partial class TrackerAPIResponseModel
{
    #region Properties

    [JsonProperty("response_message")]
    public string ResponseMessage { get; set; }

    [JsonProperty("success")]
    public bool Success { get; set; }

    [JsonProperty("employee_id")]
    public int EmployeeId { get; set; }

    [JsonProperty("project_id")]
    public int ProjectId { get; set; }

    [JsonProperty("task_id")]
    public int TaskId { get; set; }

    #endregion
}
