using App.Core;
using Satyanam.Nop.Core.Domains;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Satyanam.Nop.Core.Services
{
    /// <summary>
    /// Checklist mapping service interface
    /// </summary>
    public partial interface ICheckListMappingService
    {
        /// <summary>
        /// Gets all checklist mappings
        /// </summary>
        Task<IPagedList<CheckListMapping>> GetAllCheckListMappingsAsync(
            int? taskCategoryId = null,
            int? statusId = null,
            int? checkListId = null,
            int pageIndex = 0,
            int pageSize = int.MaxValue);

        /// <summary>
        /// Gets checklist mapping by Id
        /// </summary>
        Task<CheckListMapping> GetCheckListMappingByIdAsync(int id);

        /// <summary>
        /// Gets checklist mappings by multiple ids
        /// </summary>
        Task<IList<CheckListMapping>> GetCheckListMappingsByIdsAsync(int[] ids);

        /// <summary>
        /// Inserts a new checklist mapping
        /// </summary>
        Task InsertCheckListMappingAsync(CheckListMapping mapping);

        /// <summary>
        /// Updates a checklist mapping
        /// </summary>
        Task UpdateCheckListMappingAsync(CheckListMapping mapping);

        /// <summary>
        /// Deletes a checklist mapping
        /// </summary>
        Task DeleteCheckListMappingAsync(CheckListMapping mapping);

        Task<bool> IsMappingExistAsync(int taskCategoryId, int statusId, int checkListId, int excludeId = 0);

        Task<IList<CheckListMapping>> GetCheckListsByCategoryAndStatusAsync(int taskCategoryId, int statusId);
    }
}
