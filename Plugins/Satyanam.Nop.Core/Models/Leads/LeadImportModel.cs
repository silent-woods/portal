using App.Web.Framework.Models;

namespace Satyanam.Nop.Core.Models.Leads
{
    public record LeadImportModel : BaseNopEntityModel
    {
        public LeadImportModel()
        {
        }

        #region Properties
        public string Name { get; set; }
        public string CompanyName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public decimal AnnualRevenue { get; set; }
        public string WebsiteUrl { get; set; }
        public int NoofEmployee { get; set; }
        public string SkypeId { get; set; }
        public string SecondaryEmail { get; set; }
        public string Title { get; set; }
        public string Customer { get; set; }
        public string LeadSource { get; set; }
        public string Industry { get; set; }
        public string LeadStatus { get; set; }
        public string Category { get; set; }
        public string Twitter { get; set; }
        public string LinkedinUrl { get; set; }
        public string Facebookurl { get; set; }
        public string Description { get; set; }
        public string Country { get; set; }
        public string State { get; set; }
        public string City { get; set; }

        #endregion
    }
}