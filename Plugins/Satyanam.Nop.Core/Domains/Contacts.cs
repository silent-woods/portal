using App.Core;
using System;

namespace Satyanam.Nop.Core.Domains
{
    /// <summary>
    /// Represents a  Contacts
    /// </summary>
    public class Contacts : BaseEntity
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string CompanyName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public int AnnualRevenue { get; set; }
        public string WebsiteUrl { get; set; }
        public int NoofEmployee { get; set; }
        public string SkypeId { get; set; }
        public string SecondaryEmail { get; set; }
        public int TitleId { get; set; }
        public int CustomerId { get; set; }
        public int ContactsSourceId { get; set; }
        public int IndustryId { get; set; }
        public int CompanyId { get; set; }
        public bool EmailOptOut { get; set; }
        public string Twitter { get; set; }
        public string LinkedinUrl { get; set; }
        public string Facebookurl { get; set; }
        public int AddressId { get; set; }
        public int EmailStatusId { get; set; }
        public string Description { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public DateTime UpdatedOnUtc { get; set; }


    }
}
