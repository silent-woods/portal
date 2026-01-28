using App.Core;
using System;

namespace Satyanam.Nop.Core.Domains
{
    /// <summary>
    /// Represents a  UpdateSubmissionComment  
    /// </summary>
    public class UpdateSubmissionComment : BaseEntity
    {
        public int UpdateSubmissionId { get; set; }
        public int? ParentCommentId { get; set; }
        public int CommentedByCustomerId { get; set; }
        public string CommentText { get; set; }
        public DateTime CreatedOnUtc { get; set; }

    }
}
