using App.Data;
using Satyanam.Nop.Core.Domains;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Satyanam.Nop.Core.Services
{
    /// <summary>
    /// Task checklist entry service
    /// </summary>
    public partial class TaskCheckListEntryService : ITaskCheckListEntryService
    {
        #region Fields

        private readonly IRepository<TaskCheckListEntry> _taskCheckListEntryRepository;

        #endregion

        #region Ctor

        public TaskCheckListEntryService(IRepository<TaskCheckListEntry> taskCheckListEntryRepository)
        {
            _taskCheckListEntryRepository = taskCheckListEntryRepository;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets all checklist entries for a task
        /// </summary>
        public virtual async Task<IList<TaskCheckListEntry>> GetAllEntriesByTaskIdAsync(int taskId)
        {
            if (taskId <= 0)
                return new List<TaskCheckListEntry>();

            var query = _taskCheckListEntryRepository.Table
                .Where(e => e.TaskId == taskId)
                .OrderByDescending(e => e.CreatedOn);

            return await Task.FromResult(query.ToList());
        }

        /// <summary>
        /// Gets all checklist entries for a task by status
        /// </summary>
        public virtual async Task<IList<TaskCheckListEntry>> GetEntriesByTaskAndStatusAsync(int taskId, int statusId)
        {
            if (taskId <= 0 || statusId <= 0)
                return new List<TaskCheckListEntry>();

            var query = _taskCheckListEntryRepository.Table
                .Where(e => e.TaskId == taskId && e.StatusId == statusId)
                .OrderByDescending(e => e.CreatedOn);

            return await Task.FromResult(query.ToList());
        }

        /// <summary>
        /// Gets a checklist entry by id
        /// </summary>
        public virtual async Task<TaskCheckListEntry> GetEntryByIdAsync(int id)
        {
            if (id <= 0)
                return null;

            return await _taskCheckListEntryRepository.GetByIdAsync(id);
        }

        /// <summary>
        /// Inserts a new checklist entry
        /// </summary>
        public virtual async Task InsertEntryAsync(TaskCheckListEntry entry)
        {
            if (entry == null)
                throw new ArgumentNullException(nameof(entry));

            await _taskCheckListEntryRepository.InsertAsync(entry);
        }

        /// <summary>
        /// Updates an existing checklist entry
        /// </summary>
        public virtual async Task UpdateEntryAsync(TaskCheckListEntry entry)
        {
            if (entry == null)
                throw new ArgumentNullException(nameof(entry));

            await _taskCheckListEntryRepository.UpdateAsync(entry);
        }

        /// <summary>
        /// Deletes a checklist entry
        /// </summary>
        public virtual async Task DeleteEntryAsync(TaskCheckListEntry entry)
        {
            if (entry == null)
                throw new ArgumentNullException(nameof(entry));

            await _taskCheckListEntryRepository.DeleteAsync(entry);
        }

        #endregion
    }
}
