using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App.Core;
using App.Core.Domain.Employees;
using App.Data;

namespace App.Services.Employees
{
    /// <summary>
    /// Assets service
    /// </summary>
    public partial class AssetsService : IAssetsService
    {
        #region Fields

        private readonly IRepository<Assets> _assestsRepository;

        #endregion

        #region Ctor

        public AssetsService(
            IRepository<Assets> assestsRepository)
        {
            _assestsRepository = assestsRepository;
        }

        #endregion

        #region Methods

        #region Assets

        /// <summary>
        /// Deletes a Assets
        /// </summary>
        /// <param name="assets">Assets</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task DeleteAssetsAsync(Assets assets)
        {
            await _assestsRepository.DeleteAsync(assets);
        }

        /// <summary>
        /// Gets a Assets
        /// </summary>
        /// <param name="assestsId">assests identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the assests
        /// </returns>
        public virtual async Task<Assets> GetAssetsByIdAsync(int assestsId)
        {
            return await _assestsRepository.GetByIdAsync(assestsId, cache => default);
        }

        /// <summary>
        /// Gets all assests
        /// </summary>
        /// <param name="assestsName">The store identifier; pass 0 to load all records</param>
        /// <param name="languageId">Language identifier; 0 if you want to get all records</param>
        /// <param name="dateFrom">Filter by created date; null if you want to get all records</param>
        /// <param name="dateTo">Filter by created date; null if you want to get all records</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <param name="assests">Filter by assests name</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the assests
        /// </returns>

        public virtual async Task<IPagedList<Assets>> GetAllAssetsAsync(int employeeId,string assestsName,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, bool? overridePublished = null)
        {
            var query = await _assestsRepository.GetAllAsync(async query =>
            {
                if (employeeId != 0)
                {
                    query = query.Where(b => b.EmployeeID == employeeId);
                }
                return query.OrderByDescending(c => c.Id);
            });
            //paging
            return new PagedList<Assets>(query.ToList(), pageIndex, pageSize);
        }

        /// <summary>
        /// Inserts a assests
        /// </summary>
        /// <param name="assests">Assets</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task InsertAssetsAsync(Assets assets)
        {
            await _assestsRepository.InsertAsync(assets);
        }

        /// <summary>
        /// Updates the Assets
        /// </summary>
        /// <param name="assests">Assets</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task UpdateAssetsAsync(Assets assets)
        {
            await _assestsRepository.UpdateAsync(assets);
        }

        public virtual async Task<IList<Assets>> GetAssetsByIdsAsync(int[] assestsIds)
        {
            return await _assestsRepository.GetByIdsAsync(assestsIds, cache => default, false);
        }

        #endregion

        #endregion
    }
}