using App.Core;
using System;

namespace Satyanam.Nop.Plugin.SatyanamCRM.Models.LinkedInFollowups
{
    public class LinkedInFollowupsDto : BaseEntity
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string LinkedinUrl { get; set; }
        public string Email { get; set; }
        public string WebsiteUrl { get; set; }
        public string LastMessDate { get; set; }
        public int FollowUp { get; set; }
        public string NextFollowupsDate { get; set; }
        public int DaysUntilNext { get; set; }
        public int RemainingFollowUps { get; set; }
        public string AutoStatus { get; set; }
        public int StatusId { get; set; }
        public string StatusName { get; set; } 
        public string Notes { get; set; }
        public DateTime CreatedOnUtc { get; set; }
    }

}