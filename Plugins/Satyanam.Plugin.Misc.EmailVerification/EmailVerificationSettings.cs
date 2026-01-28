using App.Core.Configuration;

namespace Satyanam.Plugin.Misc.EmailVerification
{
    /// <summary>
    /// Represents a plugin settings
    /// </summary>
    public class EmailVerificationSettings : ISettings
    {
        /// <summary>
        /// Gets or sets the API key
        /// </summary>
        public string ApiKey { get; set; }
        /// <summary>
        /// Gets or sets the ApiUrl
        /// </summary>
        public string ApiUrl { get; set; }

        public bool DisconnectOnUninstall { get; set; }
        public bool ContactUspages { get; set; }
        public bool Registartionpage { get; set; }
        public bool EBookspage { get; set; }

    }
}