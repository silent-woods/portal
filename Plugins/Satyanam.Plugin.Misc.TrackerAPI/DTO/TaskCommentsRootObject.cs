using Newtonsoft.Json;
using System;

namespace Satyanam.Plugin.Misc.TrackerAPI.DTO;

public partial class TaskCommentsRootObject
{
    #region Properties

    [JsonProperty("employee_name")]
    public string EmployeeName { get; set; }

    [JsonProperty("comments")]
    public string Comments { get; set; }

    [JsonProperty("created_on")]
    public DateTime CreatedOn { get; set; }

    #endregion
}
