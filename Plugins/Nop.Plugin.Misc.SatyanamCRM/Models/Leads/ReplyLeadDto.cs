using Newtonsoft.Json;

namespace Satyanam.Nop.Plugin.SatyanamCRM.Models.Leads
{
    /// <summary>
    /// Represents a  ReplyLeadDto
    /// </summary>
    public class ReplyLeadDto
    {
        [JsonProperty("firstName")]
        public string FirstName { get; set; }

        [JsonProperty("lastName")]
        public string LastName { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("company")]
        public string Company { get; set; }

        [JsonProperty("domain")]
        public string Domain { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("phone")]
        public string Phone { get; set; }
    }

}
