using App.Core;
using System;

namespace Satyanam.Nop.Core.Domains
{
	/// <summary>
	/// Represents a  Company
	/// </summary>
	public class Company : BaseEntity
    {
        public int ContactId { get; set; }
        public string CompanyName { get; set; }
        public string WebsiteUrl { get; set; }
        public int ParentAccountID { get; set; }
        public int AccountNumber { get; set; }
        public int AccountTypeId { get; set; }
        public int IndustryId { get; set; }
        public int CustomerId { get; set; }
		public string Phone { get; set; }
        public int OwnerShipId { get; set; }
        public int NoofEmployee { get; set; }
        public int AnnualRevenue { get; set; }
        public int BillingAddressId { get; set; }
        public int ShipingAddressId { get; set; }
        public string Description { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public DateTime UpdatedOnUtc { get; set; }


    }
}
