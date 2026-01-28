using App.Core;
using App.Data;
using Satyanam.Nop.Core.Domains;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Satyanam.Nop.Core.Services
{
    /// <summary>
    /// Title service
    /// </summary>
    public partial class CampaingsService : ICampaingsService
    {
        #region Fields

        private readonly IRepository<Campaings> _campaingsRepository;

        #endregion

        #region Ctor

        public CampaingsService(IRepository<Campaings> campaingsRepository)
        {
            _campaingsRepository = campaingsRepository;
        }

        #endregion

        #region Methods

        #region Campaings

        /// <summary>
        /// Gets all Campaings
        /// </summary>
        /// <param name="name">name</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <param name="campaings">Filter by campaings name</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the title
        /// </returns>

        public virtual async Task<IPagedList<Campaings>> GetAllCampaingsAsync(string name, int? statusid = null,
    int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, bool? overridePublished = null)
        {
            var query = _campaingsRepository.Table;

            if (!string.IsNullOrWhiteSpace(name))
                query = query.Where(c => c.Name.Contains(name));

            if (statusid.HasValue && statusid.Value > 0)
                query = query.Where(c => c.StatusId == statusid.Value);

            query = query.OrderByDescending(c => c.Id);

            return await Task.FromResult(new PagedList<Campaings>(query.ToList(), pageIndex, pageSize));
        }


        /// <summary>
        /// Gets a Campaings
        /// </summary>
        /// <param name="campaingsId">campaings identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the campaings
        /// </returns>
        public virtual async Task<Campaings> GetCampaingsByIdAsync(int campaingId)
        {
            return await _campaingsRepository.GetByIdAsync(campaingId);
        }

        public virtual async Task<IList<Campaings>> GetCampaingsByIdsAsync(int[] campaingsIds)
        {
            return await _campaingsRepository.GetByIdsAsync(campaingsIds);
        }


        /// <summary>
        /// Inserts a Campaings
        /// </summary>
        /// <param name="campaings">Campaings</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task InsertCampaingsAsync(Campaings campaings)
        {
            await _campaingsRepository.InsertAsync(campaings);
        }

        /// <summary>
        /// Updates the Campaings
        /// </summary>
        /// <param name="campaings">campaings</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task UpdateCampaingsAsync(Campaings campaings)
        {
            await _campaingsRepository.UpdateAsync(campaings);
        }

        /// <summary>
        /// Deletes a Campaings
        /// </summary>
        /// <param name="campaings">Campaings</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task DeleteCampaingsAsync(Campaings campaings)
        {
            await _campaingsRepository.DeleteAsync(campaings);
        }

        #endregion

        #endregion
    }
}