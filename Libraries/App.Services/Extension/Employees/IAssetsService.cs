using System.Collections.Generic;
using System.Threading.Tasks;
using App.Core;
using App.Core.Domain.Employees;

namespace App.Services.Employees
{
    /// <summary>
    /// Assets service interface
    /// </summary>
    public partial interface IAssetsService
    {
        /// <summary>
        /// Deletes a Assets
        /// </summary>
        /// <param name="assets">Assets</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task DeleteAssetsAsync(Assets assets);

        /// <summary>
        /// Gets a Assets
        /// </summary>
        /// <param name="assestsId">assests identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the assests
        /// </returns>
        Task<Assets> GetAssetsByIdAsync(int assestsId);

        /// <summary>
        /// Gets all Assets
        /// </summary>
        /// <param name="assetsName">The store identifier; pass 0 to load all records</param>
        /// <param name="languageId">Language identifier; 0 if you want to get all records</param>
        /// <param name="dateFrom">Filter by created date; null if you want to get all records</param>
        /// <param name="dateTo">Filter by created date; null if you want to get all records</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <param name="assestsName">Filter by assests name</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the assests
        /// </returns>

        Task<IPagedList<Assets>> GetAllAssetsAsync(int employeeId,string assestsName,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, bool? overridePublished = null);

        /// <summary>
        /// Inserts a assests
        /// </summary>
        /// <param name="experience">Assets</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task InsertAssetsAsync(Assets assets);

        /// <summary>
        /// Updates the Assets
        /// </summary>
        /// <param name="assets">Assets</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task UpdateAssetsAsync(Assets assets);

        Task<IList<Assets>> GetAssetsByIdsAsync(int[] assestsIds);
    }
}