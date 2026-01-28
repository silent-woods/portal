using App.Core;
using App.Services.Messages;
using Satyanam.Nop.Core.Domains;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Satyanam.Nop.Core.Services
{
    /// <summary>
    /// Lead service interface
    /// </summary>
    public partial interface ILeadService
    {
        Task<IPagedList<Lead>> GetAllLeadAsync(string name, string companyName, IList<int> selectedtagsid, string email, string website, int nofoEmployee, int leadStatusId, IList<int> titleid,int emailStatusId, int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, bool? isSyncedToReply = null);
        Task<Lead> GetLeadByIdAsync(int id);
        Task InsertLeadAsync(Lead lead);
        Task InsertLeadTagsAsync(LeadTags leadTags);
		Task UpdateLeadAsync(Lead lead);
        Task DeleteLeadAsync(Lead lead);
        Task DeleteLeadTagsByLeadIdAsync(int leadId, int tagId);
        Task UpdateLeadTagsAsync(LeadTags leadTags);
        Task<LeadTags> GetLeadTagsByLeadIdAsync(int leadId);
        Task<IList<Lead>> GetLeadByIdsAsync(int[] leadIds);
        Task<LeadTags> GetOrCreateLeadTagAsync(int leadId, int tagId);
        Task<IList<Lead>> GetLeadsByTagsAsync(List<int> tagIds);
        Task<IList<LeadTags>> GetLeadTagByLeadIdAsync(int leadId);
        Task AddLeadTokensAsync(IList<Token> tokens, Lead lead);
        Task<Lead> GetLeadByEmailAsync(string email);
        Task DeleteTagsByLeadIdAsync(int leadId);
        Task<IList<Lead>> GetLeadsByIdsAsync(List<int> ids);
        Task<IList<Lead>> GetAllLeadsForReplyIoSyncAsync();
        Task<(IList<Lead> Leads, IList<Contacts> Contacts)> GetLeadsAndContactsByTagsAsync(List<int> tagIds);
    }
}