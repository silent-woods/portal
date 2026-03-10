using App.Core;
using Satyanam.Nop.Core.Domains;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Satyanam.Nop.Core.Services
{
    /// <summary>
    /// ITechnology service interface
    /// </summary>
    public partial interface ITechnologyService
    {
        #region Technology

        /// <summary>
        /// Get technology by id
        /// </summary>
        Task<Technology> GetTechnologyByIdAsync(int id);
        Task<IList<Technology>> GetTechnologyByIdsAsync(int[] technologyIds);
        /// <summary>
        /// Get all technology (paged)
        /// </summary>
        Task<IPagedList<Technology>> GetAllTechnologyAsync(string name = "", bool? published=null, int pageIndex = 0, int pageSize = int.MaxValue);
        /// <summary>
        /// Insert technology
        /// </summary>
        /// <param name="technology"></param>
        /// <returns></returns>
        /// <summary>
        /// Get all published technologies
        /// </summary>
        Task<IList<Technology>> GetAllPublishedTechnologiesAsync();
        Task InsertTechnologyAsync(Technology technology);
        /// <summary>
        /// Update technology
        /// </summary>
        /// <param name="technology"></param>
        /// <returns></returns>
        Task UpdateTechnologyAsync(Technology technology);
        /// <summary>
        /// Delete technology 
        /// </summary>
        /// <param name="technology"></param>
        /// <returns></returns>
        Task DeleteTechnologyAsync(Technology technology);
        #endregion
    }
}