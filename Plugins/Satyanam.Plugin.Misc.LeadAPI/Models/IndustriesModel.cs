using Newtonsoft.Json;

namespace Satyanam.Plugin.Misc.LeadAPI.Models;

public partial class IndustriesModel
{
    #region Properties

    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    #endregion
}
