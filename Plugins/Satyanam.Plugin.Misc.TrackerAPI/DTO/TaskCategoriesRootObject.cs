using Newtonsoft.Json;

namespace Satyanam.Plugin.Misc.TrackerAPI.DTO;

public partial class TaskCategoriesRootObject
{
    #region Properties

    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("category_name")]
    public string CategoryName { get; set; }

    #endregion
}
