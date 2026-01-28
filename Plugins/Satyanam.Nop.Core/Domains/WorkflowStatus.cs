using App.Core;
using System;

namespace Satyanam.Nop.Core.Domains
{
    /// <summary>
    /// Represents a  Categorys
    /// </summary>
    public class WorkflowStatus : BaseEntity
    {
       
        public int ProcessWorkflowId { get; set; }
        public string StatusName { get; set; }

        public bool IsDefaultDeveloperStatus { get; set; }
        public bool IsDefaultQAStatus { get; set; }

        public string ColorCode { get; set; }
        public int DisplayOrder { get; set; }

        
        public DateTime CreatedOn { get; set; }
    }
}
