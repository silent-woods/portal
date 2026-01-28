using App.Core;
using App.Core.Configuration;

namespace Satyanam.Nop.Core.Settings
{
    /// <summary>
    /// Represents a  CampaignEmailSettings
    /// </summary>
    public class CampaignEmailSettings : ISettings
    {
        public string LinkedInUrl { get; set; }
        public string WebsiteUrl { get; set; }
        public string FooterText { get; set; }
    }
}
