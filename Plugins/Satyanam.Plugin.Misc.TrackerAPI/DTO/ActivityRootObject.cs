using Newtonsoft.Json;

namespace Satyanam.Plugin.Misc.TrackerAPI.DTO;

public partial class ActivityRootObject
{
    #region Properties

    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("activity_name")]
    public string ActivityName { get; set; }

    #endregion
}
