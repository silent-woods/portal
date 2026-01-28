using App.Core;
using Microsoft.VisualBasic;
using System;

namespace Satyanam.Nop.Core.Domains
{
    /// <summary>
    /// Represents a  Campaings
    /// </summary>
    public class Campaings : BaseEntity
    {
        public string Name { get; set; }
        public int EmailAccountId { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public int StatusId { get; set; }
        public string TagsId { get; set; }
        public DateTime? ScheduledDate { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public DateTime UpdatedOnUtc { get; set; }

    }
}
