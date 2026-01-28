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
    /// Company service
    /// </summary>
    public partial class CompanyService : ICompanyService
    {
        #region Fields

        private readonly IRepository<Company> _companyRepository;

        #endregion

        #region Ctor

        public CompanyService(
            IRepository<Company> companyRepository)
        {
            _companyRepository = companyRepository;
        }

        #endregion

        #region Methods

        #region Company

        /// <summary>
        /// Gets all Company
        /// </summary>
        /// <param name="name">name</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <param name="company">Filter by company name</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the company
        /// </returns>

        public virtual async Task<IPagedList<Company>> GetAllCompanyAsync(string name,string website, int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false)
        {
            var query = await _companyRepository.GetAllAsync(async query =>
            {
                if (!string.IsNullOrWhiteSpace(name))
                    query = query.Where(c => c.CompanyName.Contains(name));
                if (!string.IsNullOrWhiteSpace(website))
                    query = query.Where(c => c.WebsiteUrl.Contains(website));
                return query.OrderBy(c => c.Id);
            });
            //paging
            return new PagedList<Company>(query.ToList(), pageIndex, pageSize);
        }

        /// <summary>
        /// Gets a Company
        /// </summary>
        /// <param name="companyId">company identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the company
        /// </returns>
        public virtual async Task<Company> GetCompanyByIdAsync(int companyId)
        {
            return await _companyRepository.GetByIdAsync(companyId);
        }

        public virtual async Task<IList<Company>> GetCompanyByIdsAsync(int[] companyIds)
        {
            return await _companyRepository.GetByIdsAsync(companyIds);
        }


        /// <summary>
        /// Inserts a Company
        /// </summary>
        /// <param name="company">Company</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task InsertCompanyAsync(Company company)
        {
            await _companyRepository.InsertAsync(company);
        }

        /// <summary>
        /// Updates the Company
        /// </summary>
        /// <param name="company">company</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task UpdateCompanyAsync(Company company)
        {
            await _companyRepository.UpdateAsync(company);
        }

        /// <summary>
        /// Deletes a Company
        /// </summary>
        /// <param name="company">company</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task DeleteCompanyAsync(Company company)
        {
            await _companyRepository.DeleteAsync(company);
        }

        public async Task<IPagedList<Company>> GetDealsByCompanyIdAsync(int companyId, int pageIndex, int pageSize)
        {
            var query = _companyRepository.Table.Where(x => x.Id == companyId);
            return await query.ToPagedListAsync(pageIndex, pageSize);
        }

        public async Task<Company> GetCompanyByNameAsync(string companyName)
        {
            return await _companyRepository.Table
                .FirstOrDefaultAsync(c => c.CompanyName == companyName);
        }
        #endregion

        #endregion
    }
}