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
    /// Industry service
    /// </summary>
    public partial class IndustryService : IIndustryService
    {
        #region Fields

        private readonly IRepository<Industry> _industryRepository;

        #endregion

        #region Ctor

        public IndustryService(
            IRepository<Industry> industryRepository)
        {
            _industryRepository = industryRepository;
        }

        #endregion

        #region Methods

        #region Industry

        /// <summary>
        /// Gets all Industry
        /// </summary>
        /// <param name="name">name</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <param name="industry">Filter by industry name</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the industry
        /// </returns>

        public virtual async Task<IPagedList<Industry>> GetAllIndustryAsync(string name, int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false)
        {
            var query = await _industryRepository.GetAllAsync(async query =>
            {
                if (!string.IsNullOrWhiteSpace(name))
                    query = query.Where(c => c.Name.Contains(name));

                return query.OrderBy(c => c.Id);
            });
            //paging
            return new PagedList<Industry>(query.ToList(), pageIndex, pageSize);
        }

        /// <summary>
        /// Gets a Industry
        /// </summary>
        /// <param name="industryId">industry identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the industry
        /// </returns>
        public virtual async Task<Industry> GetIndustryByIdAsync(int industryId)
        {
            return await _industryRepository.GetByIdAsync(industryId);
        }

        public virtual async Task<IList<Industry>> GetIndustryByIdsAsync(int[] industryIds)
        {
            return await _industryRepository.GetByIdsAsync(industryIds);
        }


        /// <summary>
        /// Inserts a Industry
        /// </summary>
        /// <param name="industry">Industry</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task InsertIndustryAsync(Industry industry)
        {
            await _industryRepository.InsertAsync(industry);
        }

        /// <summary>
        /// Updates the Industry
        /// </summary>
        /// <param name="industry">industry</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task UpdateIndustryAsync(Industry industry)
        {
            await _industryRepository.UpdateAsync(industry);
        }

        /// <summary>
        /// Deletes a Industry
        /// </summary>
        /// <param name="industry">industry</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task DeleteIndustryAsync(Industry industry)
        {
            await _industryRepository.DeleteAsync(industry);
        }

        public virtual async Task<IPagedList<Industry>> GetAllIndustryByNameAsync(string industryName,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, bool? overridePublished = null)
        {
            var query = await _industryRepository.GetAllAsync(async query =>
            {
                return query.OrderByDescending(c => c.Id);
            });
            //paging
            return new PagedList<Industry>(query.ToList(), pageIndex, pageSize);
        }
        public async Task<Industry> GetOrCreateIndustryByNameAsync(string industryName)
        {
            if (string.IsNullOrWhiteSpace(industryName))
                return null;

            var industry = await _industryRepository.Table
                .FirstOrDefaultAsync(i => i.Name.ToLower() == industryName.ToLower());

            if (industry == null)
            {
                industry = new Industry { Name = industryName };
                await _industryRepository.InsertAsync(industry);
            }

            return industry;
        }
        #endregion

        #endregion
    }
}