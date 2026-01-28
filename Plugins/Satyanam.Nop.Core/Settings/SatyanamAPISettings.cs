using App.Core;
using App.Core.Configuration;

namespace Satyanam.Nop.Core.Settings
{
    /// <summary>
    /// Represents a  SatyanamAPISettings
    /// </summary>
    public class SatyanamAPISettings : ISettings
    {
        public string APIKey { get; set; }
        public string APISecret { get; set; }
        public string AllowedDomains { get; set; }
        public string InquiryEmailSendTo { get; set; }
    }
}
