using App.Core;
using App.Core.Domain.Customers;
using System;

namespace Satyanam.Nop.Core.Domains
{
    /// <summary>
    /// Represents a  Deals
    /// </summary>
    public class Deals : BaseEntity
    {
        /// <summary>
        /// Gets or sets a Name for Deals
        /// </summary>
        public int CustomerId { get; set; }
        public string DealName { get; set; }
        public int Amount { get; set; }
        public int CompanyId { get; set; }
        public int StageId { get; set; }
        public int TypeId { get; set; }
        public decimal Probability { get; set; }
        public string NextStep { get; set; }
        public decimal ExpectedRevenue { get; set; }
        public int LeadSourceId { get; set; }
        public DateTime? ClosingDate { get; set; }
        public string Description { get; set; }
        public int ContactId { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public DateTime UpdatedOnUtc { get; set; }
    }
}
