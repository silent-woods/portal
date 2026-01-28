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
    /// Checklist mapping service
    /// </summary>
    public partial class CheckListMappingService : ICheckListMappingService
    {
        #region Fields

        private readonly IRepository<CheckListMapping> _checkListMappingRepository;
        private readonly IRepository<CheckListMaster> _checkListMasterRepository;

        #endregion

        #region Ctor

        public CheckListMappingService(IRepository<CheckListMapping> checkListMappingRepository, IRepository<CheckListMaster> checkListMasterRepository)
        {
            _checkListMappingRepository = checkListMappingRepository;
            _checkListMasterRepository = checkListMasterRepository;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets all checklist mappings
        /// </summary>
        public virtual async Task<IPagedList<CheckListMapping>> GetAllCheckListMappingsAsync(
            int? taskCategoryId = null,
            int? statusId = null,
            int? checkListId = null,
            int pageIndex = 0,
            int pageSize = int.MaxValue)
        {
            var query = _checkListMappingRepository.Table;

            if (taskCategoryId > 0)
                query = query.Where(m => m.TaskCategoryId == taskCategoryId.Value);

            if (statusId > 0)
                query = query.Where(m => m.StatusId == statusId.Value);

            if (checkListId > 0 )
                query = query.Where(m => m.CheckListId == checkListId.Value);

            query = query.OrderBy(m => m.OrderBy);

            return await Task.FromResult(new PagedList<CheckListMapping>(query.ToList(), pageIndex, pageSize));
        }

        /// <summary>
        /// Gets checklist mapping by Id
        /// </summary>
        public virtual async Task<CheckListMapping> GetCheckListMappingByIdAsync(int id)
        {
            return await _checkListMappingRepository.GetByIdAsync(id);
        }

        /// <summary>
        /// Gets checklist mappings by multiple ids
        /// </summary>
        public virtual async Task<IList<CheckListMapping>> GetCheckListMappingsByIdsAsync(int[] ids)
        {
            return await _checkListMappingRepository.GetByIdsAsync(ids);
        }

        /// <summary>
        /// Inserts a new checklist mapping
        /// </summary>
        public virtual async Task InsertCheckListMappingAsync(CheckListMapping mapping)
        {
            if (mapping == null)
                throw new ArgumentNullException(nameof(mapping));

            await _checkListMappingRepository.InsertAsync(mapping);
        }

        /// <summary>
        /// Updates a checklist mapping
        /// </summary>
        public virtual async Task UpdateCheckListMappingAsync(CheckListMapping mapping)
        {
            if (mapping == null)
                throw new ArgumentNullException(nameof(mapping));

            await _checkListMappingRepository.UpdateAsync(mapping);
        }

        /// <summary>
        /// Deletes a checklist mapping
        /// </summary>
        public virtual async Task DeleteCheckListMappingAsync(CheckListMapping mapping)
        {
            if (mapping == null)
                throw new ArgumentNullException(nameof(mapping));

            await _checkListMappingRepository.DeleteAsync(mapping);
        }

        public async Task<bool> IsMappingExistAsync(int taskCategoryId, int statusId, int checkListId, int excludeId = 0)
        {
            if (taskCategoryId <= 0 || statusId <= 0 || checkListId <= 0)
                return false;

            return await _checkListMappingRepository.Table
                .AnyAsync(x => x.TaskCategoryId == taskCategoryId
                               && x.StatusId == statusId
                               && x.CheckListId == checkListId
                               && (excludeId == 0 || x.Id != excludeId));
        }


        public async Task<IList<CheckListMapping>> GetCheckListsByCategoryAndStatusAsync(int taskCategoryId, int statusId)
        {
            var query = from mapping in _checkListMappingRepository.Table
                        join master in _checkListMasterRepository.Table on mapping.CheckListId equals master.Id
                        where mapping.TaskCategoryId == taskCategoryId
                              && mapping.StatusId == statusId
                              && master.IsActive
                        orderby mapping.OrderBy
                        select mapping;

            return await query.ToListAsync();
        }


        #endregion
    }
}
