using App.Core;
using System;

namespace Satyanam.Nop.Core.Domains
{
    /// <summary>
    /// Represents a  LinkedInFollowups
    /// </summary>
    public class LinkedInFollowups : BaseEntity
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string LinkedinUrl { get; set; }
        public string Email { get; set; }
        public string WebsiteUrl { get; set; }
        public DateTime? LastMessageDate { get; set; }
        public int FollowUp { get; set; }
        public DateTime? NextFollowUpDate { get; set; }
        public int DaysUntilNext { get; set; }
        public int RemainingFollowUps { get; set; }
        public string AutoStatus { get; set; }
        public int StatusId { get; set; }
        public string Notes { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public DateTime UpdatedOnUtc { get; set; }
    }
}
