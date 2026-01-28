using App.Core;
using System;

namespace Satyanam.Nop.Core.Domains
{
    public class FollowUpTask : BaseEntity
    {
        public int TaskId { get; set; }
        public int ReviewerId { get; set; }
        public DateTime? LastFollowupDateTime { get; set; }
        public DateTime? NextFollowupDateTime { get; set; }
        public string LastComment { get; set; }
        public bool IsCompleted { get; set; }
        public int AlertId { get; set; }
        public int ReasonId { get; set; }
        public bool OnTrack { get; set; }
        public string ETAHours { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
    }
}
