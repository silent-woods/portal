using Newtonsoft.Json;

namespace Satyanam.Plugin.Misc.TrackerAPI.DTO;

public partial class TaskStatusChecklistsRootObject
{
    #region Properties

    [JsonProperty("text")]
    public string Text { get; set; }

    [JsonProperty("value")]
    public int Value { get; set; }

    [JsonProperty("selected")]
    public bool Selected { get; set; }

    #endregion
}
