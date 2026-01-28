using App.Core;
using Satyanam.Nop.Core.Domains;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Satyanam.Nop.Core.Services
{
    /// <summary>
    /// LeadSource service interface
    /// </summary>
    public partial interface ILeadSourceService
    {
        Task<IPagedList<LeadSource>> GetAllLeadSourceAsync(string name, int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false);
        Task<LeadSource> GetLeadSourceByIdAsync(int id);
        Task InsertLeadSourceAsync(LeadSource leadSource);
        Task UpdateLeadSourceAsync(LeadSource leadSource);
        Task DeleteLeadSourceAsync(LeadSource leadSource);
        Task<IList<LeadSource>> GetLeadSourceByIdsAsync(int[] leadSourceIds);
        Task<IPagedList<LeadSource>> GetAllLeadSourceByNameAsync(string leadsourceName,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, bool? overridePublished = null);
        Task<LeadSource> GetOrCreateLeadSourceByNameAsync(string sourceName);
    }
}