using App.Core;
using System;

namespace Satyanam.Nop.Core.Domains
{
    /// <summary>
    /// Represents a  Categorys
    /// </summary>
    public class ProcessWorkflow : BaseEntity
    {
       
        public string Name { get; set; }

        public bool IsActive { get; set; }

        public string Description { get; set; }

        public int DisplayOrder { get; set; }

        public DateTime CreatedOn { get; set; }
    }
}
