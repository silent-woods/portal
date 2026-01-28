using App.Core;
using Nop.Core;
using System;

namespace Satyanam.Nop.Core.Domains
{
    /// <summary>
    /// Represents a task checklist entry
    /// </summary>
    public class TaskCheckListEntry : BaseEntity
    {
        /// <summary>
        /// Gets or sets the task identifier
        /// </summary>
        public int TaskId { get; set; }

        /// <summary>
        /// Gets or sets the status identifier
        /// </summary>
        public int StatusId { get; set; }

        /// <summary>
        /// Gets or sets the checklist JSON (stores dynamic checklist items and their state)
        /// </summary>
        public string CheckListJson { get; set; }

        /// <summary>
        /// Gets or sets the employee who checked the checklist
        /// </summary>
        public int CheckedBy { get; set; }

        /// <summary>
        /// Gets or sets the date the entry was created
        /// </summary>
        public DateTime CreatedOn { get; set; }
    }

    public class TaskChecklistItemDto
    {
        public int CheckListId { get; set; }
        public bool IsChecked { get; set; }
    }

}
