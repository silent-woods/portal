using Newtonsoft.Json;

namespace Satyanam.Plugin.Misc.TrackerAPI.DTO;

public partial class ProcessWorkflowRootObject
{
    #region Properties

    [JsonProperty("text")]
    public string Text { get; set; }

    [JsonProperty("value")]
    public int Value { get; set; }

    #endregion
}
