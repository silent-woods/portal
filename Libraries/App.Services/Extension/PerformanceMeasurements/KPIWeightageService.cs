using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App.Core;
using App.Core.Domain.PerformanceMeasurements;
using App.Data;

namespace App.Services.PerformanceMeasurements
{
    /// <summary>
    /// KPIWeightage service
    /// </summary>
    public partial class KPIWeightageService : IKPIWeightageService
    {
        #region Fields

        private readonly IRepository<KPIWeightage> _kPIWeightageRepository;
        private readonly IRepository<KPIMaster> _kPIMasterRepository;

        #endregion

        #region Ctor

        public KPIWeightageService(IRepository<KPIWeightage> kPIWeightageRepository,
            IRepository<KPIMaster> kPIMasterRepository
           )
        {
            _kPIWeightageRepository = kPIWeightageRepository;
            _kPIMasterRepository = kPIMasterRepository;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Get all KPIWeightage
        /// </summary>
        /// <param name="kPIWeightageName"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="showHidden"></param>
        /// <param name="overridePublished"></param>
        /// <returns></returns>
        public virtual async Task<IPagedList<KPIWeightage>> GetAllKPIWeightageAsync(string kPIWeightageName,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, bool? overridePublished = null)
        {
            var data = GetKPIMasterBykpiNameAsync(kPIWeightageName);
            var query = await _kPIWeightageRepository.GetAllAsync(async query =>
            {
                if (!string.IsNullOrWhiteSpace(kPIWeightageName))
                  query = query.Where(c => c.KPIMasterId == data.Result.FirstOrDefault());

                return query.OrderByDescending(c => c.CreateOnUtc);
            });
            //paging
            return new PagedList<KPIWeightage>(query.ToList(), pageIndex, pageSize);
        }
        public virtual async Task<IList<int>> GetKPIMasterBykpiNameAsync(string kpiName)
        {
            var querys = (from t1 in _kPIMasterRepository.Table
                          join t2 in _kPIWeightageRepository.Table
                          on t1.Id equals t2.KPIMasterId
                          where t1.Name.Contains(kpiName) 
                          select t1.Id).Distinct().ToList();
            return querys;
        }
        /// <summary>
        /// Get kpiweightage by id
        /// </summary>
        /// <param name="kPIWeightageId"></param>
        /// <returns></returns>
        public virtual async Task<KPIWeightage> GetKPIWeightageByIdAsync(int kPIWeightageId)
        {
            return await _kPIWeightageRepository.GetByIdAsync(kPIWeightageId, cache => default);
        }

        /// <summary>
        /// Get kpiweightage by ids
        /// </summary>
        /// <param name="kPIMsterIds"></param>
        /// <returns></returns>
        public virtual async Task<IList<KPIWeightage>> GetKPIWeightageByIdsAsync(int[] kPIWeightageIds)
        {
            return await _kPIWeightageRepository.GetByIdsAsync(kPIWeightageIds, cache => default, false);
        }

        /// <summary>
        /// Insert kPIWeightage
        /// </summary>
        /// <param name="kPIWeightage"></param>
        /// <returns></returns>
        public virtual async Task InsertKPIWeightageAsync(KPIWeightage kPIWeightage)
        {
            await _kPIWeightageRepository.InsertAsync(kPIWeightage);
        }

        /// <summary>
        /// Update kPIWeightage
        /// </summary>
        /// <param name="kPIWeightage"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual async Task UpdateKPIWeightageAsync(KPIWeightage kPIWeightage)
        {
            if (kPIWeightage == null)
                throw new ArgumentNullException(nameof(kPIWeightage));

            await _kPIWeightageRepository.UpdateAsync(kPIWeightage);
        }

        /// <summary>
        /// delete kpiweightage by record
        /// </summary>
        /// <param name="kPIWeightage"></param>
        /// <returns></returns>
        public virtual async Task DeleteKPIWeightageAsync(KPIWeightage kPIWeightage)
        {
            await _kPIWeightageRepository.DeleteAsync(kPIWeightage, false);
        }
        #endregion
    }
}