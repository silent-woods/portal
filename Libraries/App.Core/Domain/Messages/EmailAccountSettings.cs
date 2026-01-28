using App.Core.Configuration;

namespace App.Core.Domain.Messages
{
    /// <summary>
    /// Email account settings
    /// </summary>
    public partial class EmailAccountSettings : ISettings
    {
        /// <summary>
        /// Gets or sets a store default email account identifier
        /// </summary>
        public int DefaultEmailAccountId { get; set; }
    }
}
