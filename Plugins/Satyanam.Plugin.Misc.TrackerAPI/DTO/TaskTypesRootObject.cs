using Newtonsoft.Json;

namespace Satyanam.Plugin.Misc.TrackerAPI.DTO;

public partial class TaskTypesRootObject
{
    #region Properties

    [JsonProperty("text")]
    public string Text { get; set; }

    [JsonProperty("value")]
    public string Value { get; set; }

    [JsonProperty("selected")]
    public bool Selected { get; set; }

    #endregion
}
