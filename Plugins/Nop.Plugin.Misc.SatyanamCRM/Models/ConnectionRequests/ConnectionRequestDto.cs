using App.Core;
using System;

namespace Satyanam.Nop.Plugin.SatyanamCRM.Models.ConnectionRequests
{
    public class ConnectionRequestDto : BaseEntity
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string LinkedinUrl { get; set; }
        public string Email { get; set; }
        public string WebsiteUrl { get; set; }
        public int StatusId { get; set; }
        public string StatusName { get; set; } 
        public DateTime CreatedOnUtc { get; set; }
    }

}