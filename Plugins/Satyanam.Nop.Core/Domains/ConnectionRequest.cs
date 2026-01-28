using App.Core;
using System;

namespace Satyanam.Nop.Core.Domains
{
    /// <summary>
    /// Represents a  ConnectionRequest
    /// </summary>
    public class ConnectionRequest : BaseEntity
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string LinkedinUrl { get; set; }
        public string Email { get; set; }
        public string WebsiteUrl { get; set; }
        public int StatusId { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public DateTime UpdatedOnUtc { get; set; }
    }
}
