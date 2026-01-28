using Newtonsoft.Json;

namespace Satyanam.Plugin.Misc.LeadAPI.Models;

public partial class LeadAPIResponseModel
{
    #region Properties

    [JsonProperty("response_message")]
    public string ResponseMessage { get; set; }

    [JsonProperty("success")]
    public bool Success { get; set; }

    #endregion
}
