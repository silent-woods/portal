using System.Collections.Generic;
using System.Threading.Tasks;
using App.Core;
using App.Core.Domain.PerformanceMeasurements;

namespace App.Services.PerformanceMeasurements
{
    /// <summary>
    /// KPIMaster service interface
    /// </summary>
    public partial interface IKPIMasterService
    {
        /// <summary>
        /// Gets all KPIMaster
        /// </summary>
        Task<IPagedList<KPIMaster>> GetAllKPIMasterAsync(string kpiName,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, bool? overridePublished = null);

        /// <summary>
        /// Get kpimaster by id
        /// </summary>
        /// <param name="kpimasterId"></param>
        /// <returns></returns>
        Task<KPIMaster> GetKPIMasterByIdAsync(int kpimasterId);

        /// <summary>
        /// Insert KPIMaster
        /// </summary>
        /// <param name="kPIMaster"></param>
        /// <returns></returns>
        Task InsertKPIMasterAsync(KPIMaster kPIMaster);

        /// <summary>
        /// Update KPIMaster
        /// </summary>
        /// <param name="kPIMaster"></param>
        /// <returns></returns>
        Task UpdateKPIMasterAsync(KPIMaster kPIMaster);

        /// <summary>
        /// Delete kPIMaster
        /// </summary>
        /// <param name="kPIMaster"></param>
        /// <returns></returns>
        Task DeleteKPIMasterAsync(KPIMaster kPIMaster);

        /// <summary>
        /// Get kpimaster by ids
        /// </summary>
        /// <param name="kPIMasterIds"></param>
        /// <returns></returns>
        Task<IList<KPIMaster>> GetKPIMasterByIdsAsync(int[] kPIMasterIds);
    }
}