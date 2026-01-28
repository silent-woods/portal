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
    /// LeadStatus service
    /// </summary>
    public partial class LeadStatusService : ILeadStatusService
    {
        #region Fields

        private readonly IRepository<LeadStatus> _leadStatusRepository;

        #endregion

        #region Ctor

        public LeadStatusService(
            IRepository<LeadStatus> leadStatusRepository)
        {
            _leadStatusRepository = leadStatusRepository;
        }

        #endregion

        #region Methods

        #region LeadStatus

        /// <summary>
        /// Gets all LeadStatus
        /// </summary>
        /// <param name="name">name</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <param name="leadStatus">Filter by leadStatus name</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the leadStatus
        /// </returns>

        public virtual async Task<IPagedList<LeadStatus>> GetAllLeadStatusAsync(string name, int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false)
        {
            var query = await _leadStatusRepository.GetAllAsync(async query =>
            {
                if (!string.IsNullOrWhiteSpace(name))
                    query = query.Where(c => c.Name.Contains(name));

                return query.OrderBy(c => c.Id);
            });
            //paging
            return new PagedList<LeadStatus>(query.ToList(), pageIndex, pageSize);
        }

        /// <summary>
        /// Gets a LeadStatus
        /// </summary>
        /// <param name="leadSourceId">leadSource identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the leadStatus
        /// </returns>
        public virtual async Task<LeadStatus> GetLeadStatusByIdAsync(int leadStatusId)
        {
            return await _leadStatusRepository.GetByIdAsync(leadStatusId);
        }

        public virtual async Task<IList<LeadStatus>> GetLeadStatusByIdsAsync(int[] leadStatusIds)
        {
            return await _leadStatusRepository.GetByIdsAsync(leadStatusIds);
        }


        /// <summary>
        /// Inserts a LeadStatus
        /// </summary>
        /// <param name="leadStatus">LeadStatus</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task InsertLeadStatusAsync(LeadStatus leadStatus)
        {
            await _leadStatusRepository.InsertAsync(leadStatus);
        }

        /// <summary>
        /// Updates the LeadStatus
        /// </summary>
        /// <param name="LeadStatus">LeadStatus</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task UpdateLeadStatusAsync(LeadStatus leadStatus)
        {
            await _leadStatusRepository.UpdateAsync(leadStatus);
        }

        /// <summary>
        /// Deletes a LeadStatus
        /// </summary>
        /// <param name="leadStatus">LeadStatus</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task DeleteLeadStatusAsync(LeadStatus leadStatus)
        {
            await _leadStatusRepository.DeleteAsync(leadStatus);
        }

        public virtual async Task<IPagedList<LeadStatus>> GetAllLeadStatusByNameAsync(string leadStatusName,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, bool? overridePublished = null)
        {
            var query = await _leadStatusRepository.GetAllAsync(async query =>
            {
                return query.OrderByDescending(c => c.Id);
            });
            //paging
            return new PagedList<LeadStatus>(query.ToList(), pageIndex, pageSize);
        }

        public async Task<LeadStatus> GetOrCreateLeadStatusByNameAsync(string leadStatusName)
        {
            if (string.IsNullOrWhiteSpace(leadStatusName))
                return null;

            var leadStatus = await _leadStatusRepository.Table
                .FirstOrDefaultAsync(i => i.Name.ToLower() == leadStatusName.ToLower());

            if (leadStatus == null)
            {
                leadStatus = new LeadStatus { Name = leadStatusName };
                await _leadStatusRepository.InsertAsync(leadStatus);
            }

            return leadStatus;
        }

        public async Task<string> GetLeadStatusNameByIdAsync(int id)
        {
            var leadStatus = await _leadStatusRepository.GetByIdAsync(id);
            return leadStatus?.Name ?? " "; // Return "Unknown" if no match is found
        }

        #endregion

        #endregion
    }
}