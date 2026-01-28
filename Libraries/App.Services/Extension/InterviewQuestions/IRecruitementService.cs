using App.Core;
using Nop.Core.Domain.Catalog;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nop.Services.Recruitements
{
    /// <summary>
    /// Represents service shipping by weight service
    /// </summary>
    public interface IRecruitementService
    {
        /// <summary>
        /// Get all shipping by weight records
        /// </summary>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the list of the shipping by weight record
        /// </returns>
        //Task<IPagedList<Questions>> GetAllAsync(int pageIndex = 0, int pageSize = int.MaxValue);

        Task<IPagedList<Questions>> GetPagedReqlistAsync(string Category, int pageIndex = 0, int pageSize = int.MaxValue, DateTime? createdToUtc = null, DateTime? createdFromUtc = null);

        Task UpdaterecruitementAsync(Questions questions);

        Task<Questions> GetrecruitementByIdAsync(int recid);

        Task<IList<Questions>> GetrecruitementIdsAsync(int[] questionsIds);

        Task DeleterecruitementAsync(IList<Questions> questions);

        Task InsertrecAsync(Questions questions);

        Task UpdateAsync(Questions rts);

        Task DeleteAsync(Questions rts);

        Task<IList<Questions>> GetAllCategoriesByParentCategoryIdAsync(int parentCategoryId,
           bool showHidden = false);
    }
}
