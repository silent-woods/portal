using Newtonsoft.Json;

namespace Satyanam.Plugin.Misc.LeadAPI.Models;

public partial class LinkedinMessagingModel
{
    #region Properties

    [JsonProperty("message")]
    public string Message { get; set; }

    [JsonProperty("receiverName")]
    public string ReceiverName { get; set; }

    [JsonProperty("receiverProfile")]
    public string ReceiverProfile { get; set; }

    [JsonProperty("conversationId")]
    public string ConversationId { get; set; }

    [JsonProperty("direction")]
    public string Direction { get; set; }

    #endregion
}
