using Newtonsoft.Json;

namespace Satyanam.Plugin.Misc.TrackerAPI.Models;

public partial class AddTaskCommentsParametersModel : TrackerAPIResponseModel
{
    #region Properties

    [JsonProperty("task_id")]
    public int TaskId { get; set; }

    [JsonProperty("comments")]
    public string Comments { get; set; }

    #endregion
}
