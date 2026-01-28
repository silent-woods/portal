using App.Core;
using App.Data;
using App.Data.Extensions;
using Satyanam.Nop.Core.Domains;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Satyanam.Nop.Core.Services
{
    /// <summary>
    /// LeadSource service
    /// </summary>
    public partial class LeadSourceService : ILeadSourceService
    {
        #region Fields

        private readonly IRepository<LeadSource> _leadSourceRepository;

        #endregion

        #region Ctor

        public LeadSourceService(
            IRepository<LeadSource> leadSourceRepository)
        {
            _leadSourceRepository = leadSourceRepository;
        }

        #endregion

        #region Methods

        #region LeadSource

        /// <summary>
        /// Gets all LeadSource
        /// </summary>
        /// <param name="name">name</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <param name="leadSource">Filter by leadSource name</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the leadSource
        /// </returns>

        public virtual async Task<IPagedList<LeadSource>> GetAllLeadSourceAsync(string name, int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false)
        {
            var query = await _leadSourceRepository.GetAllAsync(async query =>
            {
                if (!string.IsNullOrWhiteSpace(name))
                    query = query.Where(c => c.Name.Contains(name));

                return query.OrderBy(c => c.Id);
            });
            //paging
            return new PagedList<LeadSource>(query.ToList(), pageIndex, pageSize);
        }

        /// <summary>
        /// Gets a LeadSource
        /// </summary>
        /// <param name="leadSourceId">LeadSource identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the leadSource
        /// </returns>
        public virtual async Task<LeadSource> GetLeadSourceByIdAsync(int leadSourceId)
        {
            return await _leadSourceRepository.GetByIdAsync(leadSourceId);
        }

        public virtual async Task<IList<LeadSource>> GetLeadSourceByIdsAsync(int[] leadSourceIds)
        {
            return await _leadSourceRepository.GetByIdsAsync(leadSourceIds);
        }


        /// <summary>
        /// Inserts a LeadSource
        /// </summary>
        /// <param name="leadSource">LeadSource</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task InsertLeadSourceAsync(LeadSource leadSource)
        {
            await _leadSourceRepository.InsertAsync(leadSource);
        }

        /// <summary>
        /// Updates the LeadSource
        /// </summary>
        /// <param name="leadSource">LeadSource</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task UpdateLeadSourceAsync(LeadSource leadSource)
        {
            await _leadSourceRepository.UpdateAsync(leadSource);
        }

        /// <summary>
        /// Deletes a LeadSource
        /// </summary>
        /// <param name="leadSource">LeadSource</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task DeleteLeadSourceAsync(LeadSource leadSource)
        {
            await _leadSourceRepository.DeleteAsync(leadSource);
        }
        public virtual async Task<IPagedList<LeadSource>> GetAllLeadSourceByNameAsync(string leadsourceName,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, bool? overridePublished = null)
        {
            var query = await _leadSourceRepository.GetAllAsync(async query =>
            {
                return query.OrderByDescending(c => c.Id);
            });
            //paging
            return new PagedList<LeadSource>(query.ToList(), pageIndex, pageSize);
        }

        public async Task<LeadSource> GetOrCreateLeadSourceByNameAsync(string sourceName)
        {
            if (string.IsNullOrWhiteSpace(sourceName))
                return null;

            var leadSource = await _leadSourceRepository.Table
                .FirstOrDefaultAsync(ls => ls.Name.ToLower() == sourceName.ToLower());

            if (leadSource == null)
            {
                leadSource = new LeadSource { Name = sourceName };
                await _leadSourceRepository.InsertAsync(leadSource);
            }

            return leadSource;
        }
        #endregion

        #endregion
    }
}