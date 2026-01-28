using App.Core;
using System;

namespace Satyanam.Nop.Core.Domains
{
    /// <summary>
    /// Represents a  UpdateTemplate 
    /// </summary>
    public class UpdateTemplate : BaseEntity
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public int FrequencyId { get; set; }
        public int RepeatEvery { get; set; } 
        public string RepeatType { get; set; } 
        public string SelectedWeekDays { get; set; } 
        public int? OnDay { get; set; }
        public DateTime? DueDate { get; set; }
        public string DueTime { get; set; }
        public int ReminderBeforeMinutes { get; set; }
        public string SubmitterUserIds { get; set; }
        public string ViewerUserIds { get; set; }
        public int CreatedByUserId { get; set; }
        public bool IsFileAttachmentRequired { get; set; }
        public bool IsEditingAllowed { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedOnUTC { get; set; }
        public DateTime? LastReminderSentUtc { get; set; }

    }
}
