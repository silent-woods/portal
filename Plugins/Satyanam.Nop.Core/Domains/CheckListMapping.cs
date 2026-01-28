using App.Core;
using Nop.Core;
using System;

namespace Satyanam.Nop.Core.Domains
{
    /// <summary>
    /// Represents a checklist mapping with task category and status
    /// </summary>
    public class CheckListMapping : BaseEntity
    {
        /// <summary>
        /// Gets or sets the task category Id
        /// </summary>
        public int TaskCategoryId { get; set; }

        /// <summary>
        /// Gets or sets the status Id (linked to workflow status or similar)
        /// </summary>
        public int StatusId { get; set; }

        /// <summary>
        /// Gets or sets the checklist Id
        /// </summary>
        public int CheckListId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this checklist item is mandatory
        /// </summary>
        public bool IsMandatory { get; set; }

        /// <summary>
        /// Gets or sets the order of the checklist
        /// </summary>
        public int OrderBy { get; set; }
    }
}
