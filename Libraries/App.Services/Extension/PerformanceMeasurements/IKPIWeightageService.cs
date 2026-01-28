using System.Collections.Generic;
using System.Threading.Tasks;
using App.Core;
using App.Core.Domain.PerformanceMeasurements;

namespace App.Services.PerformanceMeasurements
{
    /// <summary>
    /// KPIWeightage service interface
    /// </summary>
    public partial interface IKPIWeightageService
    {
        /// <summary>
        /// Gets all KPIMaster
        /// </summary>
        Task<IPagedList<KPIWeightage>> GetAllKPIWeightageAsync(string kPIWeightageName,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, bool? overridePublished = null);

        /// <summary>
        /// Get kpiweightage by id
        /// </summary>
        /// <param name="kPIWeightageId"></param>
        /// <returns></returns>
        Task<KPIWeightage> GetKPIWeightageByIdAsync(int kPIWeightageId);

        /// <summary>
        /// Insert KPIWeightage
        /// </summary>
        /// <param name="kPIWeightage"></param>
        /// <returns></returns>
        Task InsertKPIWeightageAsync(KPIWeightage kPIWeightage);

        /// <summary>
        /// Update KPIWeightage
        /// </summary>
        /// <param name="kPIWeightage"></param>
        /// <returns></returns>
        Task UpdateKPIWeightageAsync(KPIWeightage kPIWeightage);

        /// <summary>
        /// Delete KPIWeightage
        /// </summary>
        /// <param name="kPIWeightage"></param>
        /// <returns></returns>
        Task DeleteKPIWeightageAsync(KPIWeightage kPIWeightage);

        /// <summary>
        /// Get kipweightage by ids
        /// </summary>
        /// <param name="kPIWeightageIds"></param>
        /// <returns></returns>
        Task<IList<KPIWeightage>> GetKPIWeightageByIdsAsync(int[] kPIWeightageIds);
        Task<IList<int>> GetKPIMasterBykpiNameAsync(string kpiName);
    }
}