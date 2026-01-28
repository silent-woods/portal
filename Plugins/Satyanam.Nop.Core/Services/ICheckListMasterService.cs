using App.Core;
using Satyanam.Nop.Core.Domains;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Satyanam.Nop.Core.Services
{
    /// <summary>
    /// Checklist master service interface
    /// </summary>
    public partial interface ICheckListMasterService
    {
        /// <summary>
        /// Gets all checklists
        /// </summary>
        /// <param name="title">Filter by title</param>
        /// <param name="isActive">Filter by active flag</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Paged list of checklists</returns>
        Task<IPagedList<CheckListMaster>> GetAllCheckListsAsync(
            string title = null,
            bool? isActive = null,
            int pageIndex = 0,
            int pageSize = int.MaxValue);

        /// <summary>
        /// Gets a checklist by Id
        /// </summary>
        Task<CheckListMaster> GetCheckListByIdAsync(int id);

        /// <summary>
        /// Gets checklists by multiple ids
        /// </summary>
        Task<IList<CheckListMaster>> GetCheckListsByIdsAsync(int[] ids);

        /// <summary>
        /// Inserts a new checklist
        /// </summary>
        Task InsertCheckListAsync(CheckListMaster checkList);

        /// <summary>
        /// Updates a checklist
        /// </summary>
        Task UpdateCheckListAsync(CheckListMaster checkList);

        /// <summary>
        /// Deletes a checklist
        /// </summary>
        Task DeleteCheckListAsync(CheckListMaster checkList);

        Task<CheckListMaster> GetCheckListByNameAsync(string name);
    }
}
