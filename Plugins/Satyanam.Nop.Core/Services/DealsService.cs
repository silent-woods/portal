using App.Core;
using App.Data;
using App.Data.Extensions;
using Satyanam.Nop.Core.Domains;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Satyanam.Nop.Core.Services
{
    /// <summary>
    /// Deals service
    /// </summary>
    public partial class DealsService : IDealsService
    {
        #region Fields

        private readonly IRepository<Deals> _dealsRepository;
        #endregion

        #region Ctor

        public DealsService(
             IRepository<Deals> dealsRepository)
        {
            _dealsRepository = dealsRepository;
        }

        #endregion

        #region Methods

        #region Deals

        /// <summary>
        /// Gets all Deals
        /// </summary>
        /// <param name="name">name</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <param name="deals">Filter by deals name</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the deals
        /// </returns>

        public virtual async Task<IPagedList<Deals>> GetAllDealsAsync(string name, int amount, int stageid, DateTime? closingdate, int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false)
        {
            var query = await _dealsRepository.GetAllAsync(async query =>
            {
                if (!string.IsNullOrWhiteSpace(name))
                    query = query.Where(c => c.DealName.Contains(name));

                if (amount != 0)
                    query = query.Where(c => c.Amount == amount);

                if (stageid != 0)
                    query = query.Where(c => c.StageId == stageid);

                if (closingdate != null)
                    query = query.Where(c => c.ClosingDate >= closingdate);

                return query.OrderBy(c => c.Id);
            });
            //paging
            return new PagedList<Deals>(query.ToList(), pageIndex, pageSize);
        }

        /// <summary>
        /// Gets a Deals
        /// </summary>
        /// <param name="dealsId">deals identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the deals
        /// </returns>
        public virtual async Task<Deals> GetDealsByIdAsync(int dealsId)
        {
            return await _dealsRepository.GetByIdAsync(dealsId);
        }

        public virtual async Task<IList<Deals>> GetDealsByIdsAsync(int[] dealsIds)
        {
            return await _dealsRepository.GetByIdsAsync(dealsIds);
        }

        /// <summary>
        /// Inserts a Deals
        /// </summary>
        /// <param name="deals">Deals</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task InsertDealsAsync(Deals deals)
        {
            await _dealsRepository.InsertAsync(deals);
        }

        /// <summary>
        /// Updates the Deals
        /// </summary>
        /// <param name="deals">deals</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task UpdateDealsAsync(Deals deals)
        {
            await _dealsRepository.UpdateAsync(deals);
        }

        /// <summary>
        /// Deletes a Deals
        /// </summary>
        /// <param name="deals">Deals</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task DeleteDealsAsync(Deals deals)
        {
            await _dealsRepository.DeleteAsync(deals);
        }



        public async Task<IPagedList<Deals>> GetDealsByContactIdAsync(int contactId, int pageIndex, int pageSize)
        {
            var query = _dealsRepository.Table.Where(x => x.ContactId == contactId);
            return await query.ToPagedListAsync(pageIndex, pageSize);
        }
        public async Task<IPagedList<Deals>> GetDealsByCompanyIdAsync(int companyId, int pageIndex, int pageSize)
        {
            var query = _dealsRepository.Table.Where(x => x.CompanyId == companyId);
            return await query.ToPagedListAsync(pageIndex, pageSize);
        }

        public async Task<int> GetDealIdByContactIdAsync(int contactId)
        {
            // Find the first deal associated with this contact ID
            var deal = await _dealsRepository.Table
                .Where(d => d.ContactId == contactId)
                .FirstOrDefaultAsync();

            return deal?.Id ?? 0;  // Return the DealId if found, else 0
        }
        public async Task<int> GetDealIdByCompanyIdAsync(int companyId)
        {
            // Find the first deal associated with this contact ID
            var deal = await _dealsRepository.Table
                .Where(d => d.CompanyId == companyId)
                .FirstOrDefaultAsync();

            return deal?.Id ?? 0;  // Return the DealId if found, else 0
        }
        #endregion

        #endregion
    }
}