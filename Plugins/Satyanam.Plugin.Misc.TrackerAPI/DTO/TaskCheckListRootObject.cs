using Newtonsoft.Json;

namespace Satyanam.Plugin.Misc.TrackerAPI.DTO;

public partial class TaskCheckListRootObject
{
    #region Properties

    [JsonProperty("id")]
    public int Id { get; set; }

    #endregion
}
