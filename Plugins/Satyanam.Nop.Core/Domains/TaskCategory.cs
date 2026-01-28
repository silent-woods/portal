using App.Core;
using Nop.Core;
using System;

namespace Satyanam.Nop.Core.Domains
{
    /// <summary>
    /// Represents a task category
    /// </summary>
    public class TaskCategory : BaseEntity
    {
        public string CategoryName { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
