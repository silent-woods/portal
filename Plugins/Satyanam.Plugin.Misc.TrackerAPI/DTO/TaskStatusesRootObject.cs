using Newtonsoft.Json;

namespace Satyanam.Plugin.Misc.TrackerAPI.DTO;

public partial class TaskStatusesRootObject
{
    #region Properties

    [JsonProperty("text")]
    public string Text { get; set; }

    [JsonProperty("value")]
    public string Value { get; set; }

    [JsonProperty("color_code")]
    public string ColorCode { get; set; }

    [JsonProperty("selected")]
    public bool Selected { get; set; }

    [JsonProperty("display_order")]
    public int DisplayOrder { get; set; }

    #endregion
}
