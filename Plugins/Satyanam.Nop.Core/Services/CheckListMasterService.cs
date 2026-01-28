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
    /// Checklist master service
    /// </summary>
    public partial class CheckListMasterService : ICheckListMasterService
    {
        #region Fields

        private readonly IRepository<CheckListMaster> _checkListRepository;

        #endregion

        #region Ctor

        public CheckListMasterService(IRepository<CheckListMaster> checkListRepository)
        {
            _checkListRepository = checkListRepository;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets all checklists
        /// </summary>
        public virtual async Task<IPagedList<CheckListMaster>> GetAllCheckListsAsync(
            string title = null,
            bool? isActive = null,
            int pageIndex = 0,
            int pageSize = int.MaxValue)
        {
            var query = _checkListRepository.Table;

            if (!string.IsNullOrWhiteSpace(title))
                query = query.Where(c => c.Title.Contains(title));

            if (isActive.HasValue)
                query = query.Where(c => c.IsActive == isActive.Value);

            query = query.OrderByDescending(c => c.CreatedOn);

            return await Task.FromResult(new PagedList<CheckListMaster>(query.ToList(), pageIndex, pageSize));
        }

        /// <summary>
        /// Gets a checklist by Id
        /// </summary>
        public virtual async Task<CheckListMaster> GetCheckListByIdAsync(int id)
        {
            return await _checkListRepository.GetByIdAsync(id);
        }

        /// <summary>
        /// Gets checklists by multiple ids
        /// </summary>
        public virtual async Task<IList<CheckListMaster>> GetCheckListsByIdsAsync(int[] ids)
        {
            return await _checkListRepository.GetByIdsAsync(ids);
        }

        /// <summary>
        /// Inserts a new checklist
        /// </summary>
        public virtual async Task InsertCheckListAsync(CheckListMaster checkList)
        {
            if (checkList == null)
                throw new ArgumentNullException(nameof(checkList));

            await _checkListRepository.InsertAsync(checkList);
        }

        /// <summary>
        /// Updates a checklist
        /// </summary>
        public virtual async Task UpdateCheckListAsync(CheckListMaster checkList)
        {
            if (checkList == null)
                throw new ArgumentNullException(nameof(checkList));

            await _checkListRepository.UpdateAsync(checkList);
        }

        /// <summary>
        /// Deletes a checklist
        /// </summary>
        public virtual async Task DeleteCheckListAsync(CheckListMaster checkList)
        {
            if (checkList == null)
                throw new ArgumentNullException(nameof(checkList));

            await _checkListRepository.DeleteAsync(checkList);
        }

        public async Task<CheckListMaster> GetCheckListByNameAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            return await _checkListRepository.Table
                .Where(c => c.Title.ToLower() == name.ToLower())
                .FirstOrDefaultAsync();
        }
        #endregion
    }
}
