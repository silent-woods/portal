using Satyanam.Nop.Core.Domains;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Satyanam.Nop.Core.Services
{
    /// <summary>
    /// Task checklist entry service interface
    /// </summary>
    public partial interface ITaskCheckListEntryService
    {
        /// <summary>
        /// Gets all checklist entries for a task
        /// </summary>
        Task<IList<TaskCheckListEntry>> GetAllEntriesByTaskIdAsync(int taskId);

        /// <summary>
        /// Gets all checklist entries for a task by status
        /// </summary>
        Task<IList<TaskCheckListEntry>> GetEntriesByTaskAndStatusAsync(int taskId, int statusId);

        /// <summary>
        /// Gets a checklist entry by id
        /// </summary>
        Task<TaskCheckListEntry> GetEntryByIdAsync(int id);

        /// <summary>
        /// Inserts a new checklist entry
        /// </summary>
        Task InsertEntryAsync(TaskCheckListEntry entry);

        /// <summary>
        /// Updates an existing checklist entry
        /// </summary>
        Task UpdateEntryAsync(TaskCheckListEntry entry);

        /// <summary>
        /// Deletes a checklist entry
        /// </summary>
        Task DeleteEntryAsync(TaskCheckListEntry entry);
    }
}
