using App.Core;
using Satyanam.Nop.Core.Domains;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Satyanam.Nop.Core.Services
{
    /// <summary>
    /// Company service interface
    /// </summary>
    public partial interface ICompanyService
    {
        Task<IPagedList<Company>> GetAllCompanyAsync(string name, string website, int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false);
        Task<Company> GetCompanyByIdAsync(int id);
        Task InsertCompanyAsync(Company company);
        Task UpdateCompanyAsync(Company company);
        Task DeleteCompanyAsync(Company company);
        Task<IList<Company>> GetCompanyByIdsAsync(int[] companyIds);
        Task<IPagedList<Company>> GetDealsByCompanyIdAsync(int companyId, int pageIndex, int pageSize);
        Task<Company> GetCompanyByNameAsync(string companyName);

    }
}