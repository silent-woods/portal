using App.Core;
using Nop.Core;
using System;

namespace Satyanam.Nop.Core.Domains
{
    /// <summary>
    /// Represents a checklist master
    /// </summary>
    public class CheckListMaster : BaseEntity
    {
        /// <summary>
        /// Gets or sets the checklist title
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the checklist is active
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the date when the checklist was created
        /// </summary>
        public DateTime CreatedOn { get; set; }
    }
}
