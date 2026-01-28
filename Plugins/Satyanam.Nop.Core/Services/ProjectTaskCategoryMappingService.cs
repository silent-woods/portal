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
    /// ProjectTaskCategoryMapping service
    /// </summary>
    public partial class ProjectTaskCategoryMappingService : IProjectTaskCategoryMappingService
    {
        #region Fields

        private readonly IRepository<ProjectTaskCategoryMapping> _mappingRepository;

        #endregion

        #region Ctor

        public ProjectTaskCategoryMappingService(IRepository<ProjectTaskCategoryMapping> mappingRepository)
        {
            _mappingRepository = mappingRepository;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets all mappings
        /// </summary>
        public virtual async Task<IPagedList<ProjectTaskCategoryMapping>> GetAllMappingsAsync(
            int projectId = 0,
            int taskCategoryId = 0,
            bool isActive = false,
            int pageIndex = 0,
            int pageSize = int.MaxValue)
        {
            var query = _mappingRepository.Table;

            if (projectId>0)
                query = query.Where(m => m.ProjectId == projectId);

            if (taskCategoryId>0)
                query = query.Where(m => m.TaskCategoryId == taskCategoryId);

            if (isActive)
                query = query.Where(m => m.IsActive);

            query = query.OrderBy(m => m.OrderBy);

            return await Task.FromResult(new PagedList<ProjectTaskCategoryMapping>(query.ToList(), pageIndex, pageSize));
        }

        /// <summary>
        /// Gets mapping by Id
        /// </summary>
        public virtual async Task<ProjectTaskCategoryMapping> GetMappingByIdAsync(int id)
        {
            return await _mappingRepository.GetByIdAsync(id);
        }

        /// <summary>
        /// Gets mappings by multiple ids
        /// </summary>
        public virtual async Task<IList<ProjectTaskCategoryMapping>> GetMappingsByIdsAsync(int[] ids)
        {
            return await _mappingRepository.GetByIdsAsync(ids);
        }

        /// <summary>
        /// Inserts a new mapping
        /// </summary>
        public virtual async Task InsertMappingAsync(ProjectTaskCategoryMapping mapping)
        {
            if (mapping == null)
                throw new ArgumentNullException(nameof(mapping));

            await _mappingRepository.InsertAsync(mapping);
        }

        /// <summary>
        /// Updates a mapping
        /// </summary>
        public virtual async Task UpdateMappingAsync(ProjectTaskCategoryMapping mapping)
        {
            if (mapping == null)
                throw new ArgumentNullException(nameof(mapping));

            await _mappingRepository.UpdateAsync(mapping);
        }

        /// <summary>
        /// Deletes a mapping
        /// </summary>
        public virtual async Task DeleteMappingAsync(ProjectTaskCategoryMapping mapping)
        {
            if (mapping == null)
                throw new ArgumentNullException(nameof(mapping));

            await _mappingRepository.DeleteAsync(mapping);
        }


        public virtual async Task<bool> IsCategoryExistAsync(int projectTaskId, int categoryId)
        {
            if (projectTaskId <= 0 || categoryId <= 0)
                return false;

            return await _mappingRepository.Table
                .AnyAsync(x => x.ProjectId == projectTaskId && x.TaskCategoryId == categoryId);
        }

        public async Task<IList<ProjectTaskCategoryMapping>> GetAllMappingsByProjectIdAsync(int projectId)
        {
            if (projectId <= 0)
                return new List<ProjectTaskCategoryMapping>();

            return await _mappingRepository.Table
                .Where(m => m.ProjectId == projectId)
                .OrderBy(m => m.OrderBy)
                .ToListAsync();
        }

        #endregion
    }
}
