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
    /// KPIMaster service
    /// </summary>
    public partial class KPIMasterService : IKPIMasterService
    {
        #region Fields

        private readonly IRepository<KPIMaster> _kPIMasterRepository;

        #endregion

        #region Ctor

        public KPIMasterService(IRepository<KPIMaster> kPIMasterRepository
           )
        {
            _kPIMasterRepository = kPIMasterRepository;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Get all KPIMaster
        /// </summary>
        /// <param name="kpiName"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="showHidden"></param>
        /// <param name="overridePublished"></param>
        /// <returns></returns>
        public virtual async Task<IPagedList<KPIMaster>> GetAllKPIMasterAsync(string kpiName,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, bool? overridePublished = null)
        {
            var query = await _kPIMasterRepository.GetAllAsync(async query =>
            {
                if (!string.IsNullOrWhiteSpace(kpiName))
                    query = query.Where(c => c.Name.Contains(kpiName));
               
                return query.OrderBy(c => c.DisplayOrder).ThenBy(c => c.CreateOnUtc);

            });
            //paging
            return new PagedList<KPIMaster>(query.ToList(), pageIndex, pageSize);
        }

        /// <summary>
        /// Get kpimaster by id
        /// </summary>
        /// <param name="kPIMasterId"></param>
        /// <returns></returns>
        public virtual async Task<KPIMaster> GetKPIMasterByIdAsync(int kPIMasterId)
        {
            return await _kPIMasterRepository.GetByIdAsync(kPIMasterId, cache => default);
        }

        /// <summary>
        /// Get kpimaster by ids
        /// </summary>
        /// <param name="kPIMsterIds"></param>
        /// <returns></returns>
        public virtual async Task<IList<KPIMaster>> GetKPIMasterByIdsAsync(int[] kPIMasterIds)
        {
            return await _kPIMasterRepository.GetByIdsAsync(kPIMasterIds, cache => default, false);
        }

        /// <summary>
        /// Insert kpimaster
        /// </summary>
        /// <param name="kPIMaster"></param>
        /// <returns></returns>
        public virtual async Task InsertKPIMasterAsync(KPIMaster kPIMaster)
        {
            await _kPIMasterRepository.InsertAsync(kPIMaster);
        }

        /// <summary>
        /// Update kpimaster
        /// </summary>
        /// <param name="kPIMaster"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual async Task UpdateKPIMasterAsync(KPIMaster kPIMaster)
        {
            if (kPIMaster == null)
                throw new ArgumentNullException(nameof(kPIMaster));

            await _kPIMasterRepository.UpdateAsync(kPIMaster);
        }

        /// <summary>
        /// delete kpimaster by record
        /// </summary>
        /// <param name="kPIMaster"></param>
        /// <returns></returns>
        public virtual async Task DeleteKPIMasterAsync(KPIMaster kPIMaster)
        {
            await _kPIMasterRepository.DeleteAsync(kPIMaster, false);
        }
        #endregion
    }
}