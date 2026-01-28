using App.Core;
using System.Collections.Generic;

namespace Satyanam.Nop.Core.Domains
{
    /// <summary>
    /// Represents a  LeadDto
    /// </summary>
    public class LeadDto : BaseEntity
    {
        public LeadDto()
        {
            LeadModels = new List<LeadModel>();
        }
        public string Name { get; set; }
        public string CompanyName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public int AnnualRevenue { get; set; }
        public string WebsiteUrl { get; set; }
        public int NoofEmployee { get; set; }
        public string SkypeId { get; set; }
        public string SecondaryEmail { get; set; }
        public string TitleId { get; set; }
        public string CustomerId { get; set; }
        public string LeadSourceId { get; set; }
        public string IndustryId { get; set; }
        public string LeadStatusId { get; set; }
        public string AddressId { get; set; }
        public string CategoryId { get; set; }
        public bool EmailOptOut { get; set; }
        public string Twitter { get; set; }
        public string LinkedinUrl { get; set; }
        public string Facebookurl { get; set; }
        public string Description { get; set; }
        public string Country { get; set; }
        public string State { get; set; }
        public string City { get; set; }
        public string ZipCode { get; set; }

        public List<LeadModel> LeadModels { get; set; }
    }
}
