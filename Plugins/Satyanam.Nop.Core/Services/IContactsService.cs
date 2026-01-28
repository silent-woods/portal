using App.Core;
using App.Services.Messages;
using Satyanam.Nop.Core.Domains;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Satyanam.Nop.Core.Services
{
    /// <summary>
    /// Contacts service interface
    /// </summary>
    public partial interface IContactsService
    {
        Task<IPagedList<Contacts>> GetAllContactsAsync(string name, string companyName, string website, int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false);
        Task<Contacts> GetContactsByIdAsync(int id);
        Task InsertContactsAsync(Contacts contacts);
        Task UpdateContactsAsync(Contacts contacts);
        Task DeleteContactsAsync(Contacts contacts);
        Task<IList<Contacts>> GetContactsByIdsAsync(int[] contactsIds);
        Task<IList<ContactsTags>> GetContactsTagByContactsIdAsync(int contactsId);
        Task InsertContactsTagsAsync(ContactsTags contactsTags);
        Task DeleteContactsTagsByLeadIdAsync(int contactsId, int tagId);
        Task DeleteTagsByContactsIdAsync(int contactsId);
        Task<IPagedList<Contacts>> GetDealsByContactsIdAsync(int contactsId, int pageIndex, int pageSize);
        Task<IList<Contacts>> GetContactsByCompanyIdAsync(int companyId);
        Task<IPagedList<Contacts>> GetContactByCompanyIdAsync(int companyId, int pageIndex, int pageSize);
        Task AddContactTokensAsync(IList<Token> tokens, Contacts contacts);
        Task<Contacts> GetContactsByEmailAsync(string email);
    }
}