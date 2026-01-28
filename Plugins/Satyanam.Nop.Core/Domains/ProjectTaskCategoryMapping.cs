using App.Core;
using Nop.Core;
using System;

namespace Satyanam.Nop.Core.Domains
{
    /// <summary>
    /// Represents a task category
    /// </summary>
    public class ProjectTaskCategoryMapping : BaseEntity
    {
        /// <summary>
        /// Gets or sets the project identifier
        /// </summary>
        public int ProjectId { get; set; }

        /// <summary>
        /// Gets or sets the task category identifier
        /// </summary>
        public int TaskCategoryId { get; set; }

        /// <summary>
        /// Gets or sets the display order
        /// </summary>
        public int OrderBy { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the mapping is active
        /// </summary>
        public bool IsActive { get; set; }

    }
}
