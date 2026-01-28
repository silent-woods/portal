using App.Core;
using Satyanam.Nop.Core.Domains;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Satyanam.Nop.Core.Services
{
    /// <summary>
    /// Deals service interface
    /// </summary>
    public partial interface IDealsService
    {
        Task<IPagedList<Deals>> GetAllDealsAsync(string name, int amount, int stageid, DateTime? closingdate, int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false);
        Task<Deals> GetDealsByIdAsync(int id);
        Task InsertDealsAsync(Deals deals);
        Task UpdateDealsAsync(Deals deals);
        Task DeleteDealsAsync(Deals deals);
        Task<IList<Deals>> GetDealsByIdsAsync(int[] dealsIds);
        Task<IPagedList<Deals>> GetDealsByContactIdAsync(int contactId, int pageIndex, int pageSize);
        Task<IPagedList<Deals>> GetDealsByCompanyIdAsync(int companyId, int pageIndex, int pageSize);
        Task<int> GetDealIdByContactIdAsync(int contactId);
        Task<int> GetDealIdByCompanyIdAsync(int companyId);
    }
}