using App.Core;
using System;

namespace Satyanam.Nop.Core.Domains
{
    /// <summary>
    /// Represents a  Categorys
    /// </summary>
    public class ProcessRules : BaseEntity
    {
       
        public int ProcessWorkflowId { get; set; }
        public int  FromStateId { get; set; }

        public int ToStateId { get; set; }

        public bool IsCommentRequired { get; set; }

        public string CommentTemplate { get; set; }

        public bool IsActive { get; set; }
        
        public DateTime CreatedOn { get; set; }
    }
}
