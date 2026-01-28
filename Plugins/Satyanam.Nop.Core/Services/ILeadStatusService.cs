using App.Core;
using Satyanam.Nop.Core.Domains;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Satyanam.Nop.Core.Services
{
    /// <summary>
    /// LeadStatus service interface
    /// </summary>
    public partial interface ILeadStatusService
    {
        Task<IPagedList<LeadStatus>> GetAllLeadStatusAsync(string name, int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false);
        Task<LeadStatus> GetLeadStatusByIdAsync(int id);
        Task InsertLeadStatusAsync(LeadStatus leadStatus);
        Task UpdateLeadStatusAsync(LeadStatus leadStatus);
        Task DeleteLeadStatusAsync(LeadStatus leadStatus);
        Task<IList<LeadStatus>> GetLeadStatusByIdsAsync(int[] leadStatusIds);
        Task<IPagedList<LeadStatus>> GetAllLeadStatusByNameAsync(string leadStatusName,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, bool? overridePublished = null);
        Task<LeadStatus> GetOrCreateLeadStatusByNameAsync(string leadStatusName);
        Task<string> GetLeadStatusNameByIdAsync(int id);
    }
}