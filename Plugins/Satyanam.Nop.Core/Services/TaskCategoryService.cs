using App.Core;
using App.Data;
using App.Data.Extensions;
using Satyanam.Nop.Core.Domains; 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Satyanam.Nop.Core.Services
{
    /// <summary>
    /// TaskCategory service
    /// </summary>
    public partial class TaskCategoryService : ITaskCategoryService
    {
        #region Fields

        private readonly IRepository<TaskCategory> _taskCategoryRepository;
        private readonly IRepository<ProjectTaskCategoryMapping> _projectTaskCategoryMappingRepository;
        private readonly IRepository<CheckListMapping> _checkListMappingRepository;
        #endregion

        #region Ctor

        public TaskCategoryService(IRepository<TaskCategory> taskCategoryRepository, IRepository<ProjectTaskCategoryMapping> projectTaskCategoryMappingRepository, IRepository<CheckListMapping> checkListMappingRepository)
        {
            _taskCategoryRepository = taskCategoryRepository;
            _projectTaskCategoryMappingRepository = projectTaskCategoryMappingRepository;
            _checkListMappingRepository = checkListMappingRepository;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets all task categories
        /// </summary>
        public virtual async Task<IPagedList<TaskCategory>> GetAllTaskCategoriesAsync(
            string categoryName = null,
            bool isActive = false,
            int pageIndex = 0,
            int pageSize = int.MaxValue)
        {
            var query = _taskCategoryRepository.Table;

            if (!string.IsNullOrWhiteSpace(categoryName))
                query = query.Where(tc => tc.CategoryName.Contains(categoryName));

            if (isActive)
                query = query.Where(tc => tc.IsActive);

            query = query.OrderByDescending(tc => tc.CreatedOn);

            return await Task.FromResult(new PagedList<TaskCategory>(query.ToList(), pageIndex, pageSize));
        }

        /// <summary>
        /// Gets a task category by Id
        /// </summary>
        public virtual async Task<TaskCategory> GetTaskCategoryByIdAsync(int id)
        {
            return await _taskCategoryRepository.GetByIdAsync(id);
        }

        /// <summary>
        /// Gets task categories by multiple ids
        /// </summary>
        public virtual async Task<IList<TaskCategory>> GetTaskCategoriesByIdsAsync(int[] ids)
        {
            return await _taskCategoryRepository.GetByIdsAsync(ids);
        }

        /// <summary>
        /// Inserts a new task category
        /// </summary>
        public virtual async Task InsertTaskCategoryAsync(TaskCategory taskCategory)
        {
            if (taskCategory == null)
                throw new ArgumentNullException(nameof(taskCategory));
            await _taskCategoryRepository.InsertAsync(taskCategory);
        }

        /// <summary>
        /// Updates a task category
        /// </summary>
        public virtual async Task UpdateTaskCategoryAsync(TaskCategory taskCategory)
        {
            if (taskCategory == null)
                throw new ArgumentNullException(nameof(taskCategory));

            await _taskCategoryRepository.UpdateAsync(taskCategory);
        }

        /// <summary>
        /// Deletes a task category
        /// </summary>
        public virtual async Task DeleteTaskCategoryAsync(TaskCategory taskCategory)
        {
            if (taskCategory == null)
                throw new ArgumentNullException(nameof(taskCategory));

            var projectTaskCategoryMappings = await _projectTaskCategoryMappingRepository.Table
                .Where(m => m.TaskCategoryId == taskCategory.Id)
                .ToListAsync();

            if (projectTaskCategoryMappings.Any())
                await _projectTaskCategoryMappingRepository.DeleteAsync(projectTaskCategoryMappings);

            var checkListMappings = await _checkListMappingRepository.Table
                .Where(m => m.TaskCategoryId == taskCategory.Id)
                .ToListAsync();

            if (checkListMappings.Any())
                await _checkListMappingRepository.DeleteAsync(checkListMappings);

            await _taskCategoryRepository.DeleteAsync(taskCategory);
        }


        #endregion
    }
}
