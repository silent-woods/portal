using App.Core;
using System;

namespace Satyanam.Nop.Core.Domains
{
    /// <summary>
    /// Represents a  CampaingsEmailLogs
    /// </summary>
    public class CampaingsEmailLogs : BaseEntity
    {
        public int CampaingId { get; set; }
        public string Email { get; set; }
        public Guid TrackingGuid { get; set; }
        public bool IsOpened { get; set; }
        public int OpenCount { get; set; }
        public DateTime? OpenedOnUtc { get; set; }
        public bool IsClicked { get; set; }
        public int ClickCount { get; set; }
        public bool IsUnsubscribed { get; set; }
        public int QueuedEmailId { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public DateTime UpdatedOnUtc { get; set; }

    }
}
