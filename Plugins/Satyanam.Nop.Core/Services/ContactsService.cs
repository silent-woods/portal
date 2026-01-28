using App.Core;
using App.Core.Events;
using App.Data;
using App.Data.Extensions;
using App.Services.Messages;
using LinqToDB;
using Satyanam.Nop.Core.Domains;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Satyanam.Nop.Core.Services
{
    /// <summary>
    /// Contacts service
    /// </summary>
    public partial class ContactsService : IContactsService
    {
        #region Fields

        private readonly IRepository<Contacts> _contactsRepository;
        private readonly IRepository<ContactsTags> _contactsTagsRepository;
        private readonly IRepository<Company> _companyRepository;
        private readonly IRepository<Title> _titleRepository;
        private readonly IEventPublisher _eventPublisher;
        #endregion

        #region Ctor

        public ContactsService(
            IRepository<Contacts> contactsRepository, IRepository<ContactsTags> contactsTagsRepository, IRepository<Company> companyRepository, IRepository<Title> titleRepository, IEventPublisher eventPublisher)
        {
            _contactsRepository = contactsRepository;
            _contactsTagsRepository = contactsTagsRepository;
            _companyRepository = companyRepository;
            _titleRepository = titleRepository;
            _eventPublisher = eventPublisher;
        }

        #endregion

        #region Methods

        #region Contacts

        /// <summary>
        /// Gets all Contacts
        /// </summary>
        /// <param name="name">name</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <param name="contacts">Filter by contacts name</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the contacts
        /// </returns>

        public virtual async Task<IPagedList<Contacts>> GetAllContactsAsync(string name, string companyName, string website, int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false)
        {
            var query = await _contactsRepository.GetAllAsync(async query =>
            {
                if (!string.IsNullOrWhiteSpace(name))
                    query = query.Where(c => c.FirstName.Contains(name) || c.LastName.Contains(name));
                if (!string.IsNullOrWhiteSpace(companyName))
                    query = query.Where(c => c.CompanyName.Contains(companyName));
                if (!string.IsNullOrWhiteSpace(website))
                    query = query.Where(c => c.WebsiteUrl.Contains(website));
                return query.OrderBy(c => c.Id);
            });
            //paging
            return new PagedList<Contacts>(query.ToList(), pageIndex, pageSize);
        }

        /// <summary>
        /// Gets a Contacts
        /// </summary>
        /// <param name="contactsId">contacts identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the contact
        /// </returns>
        public virtual async Task<Contacts> GetContactsByIdAsync(int contactsId)
        {
            return await _contactsRepository.GetByIdAsync(contactsId);
        }

        public virtual async Task<IList<Contacts>> GetContactsByIdsAsync(int[] contactsIds)
        {
            return await _contactsRepository.GetByIdsAsync(contactsIds);
        }


        /// <summary>
        /// Inserts a Contacts
        /// </summary>
        /// <param name="contacts">Contacts</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task InsertContactsAsync(Contacts contacts)
        {
            await _contactsRepository.InsertAsync(contacts);
        }

        /// <summary>
        /// Updates the Contacts
        /// </summary>
        /// <param name="contacts">contacts</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task UpdateContactsAsync(Contacts contacts)
        {
            await _contactsRepository.UpdateAsync(contacts);
        }

        /// <summary>
        /// Deletes a Contacts
        /// </summary>
        /// <param name="contacts">Contacts</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task DeleteContactsAsync(Contacts contacts)
        {
            await _contactsRepository.DeleteAsync(contacts);
        }

        public async Task<IList<ContactsTags>> GetContactsTagByContactsIdAsync(int contactsId)
        {
            return await _contactsTagsRepository.Table
                .Where(x => x.ContactsId == contactsId)
                .ToListAsync();
        }
        public virtual async Task InsertContactsTagsAsync(ContactsTags contactsTags)
        {
            await _contactsTagsRepository.InsertAsync(contactsTags);
        }
        public async Task DeleteContactsTagsByLeadIdAsync(int contactsId, int tagId)
        {
            var contactsTag = await _contactsTagsRepository.Table
        .FirstOrDefaultAsync(lt => lt.ContactsId == contactsId && lt.TagsId == tagId);

            if (contactsTag != null)
            {
                await _contactsTagsRepository.DeleteAsync(contactsTag);
            }
        }

        public async Task DeleteTagsByContactsIdAsync(int contactsId)
        {
            var contactsTags = await _contactsTagsRepository.Table
                .Where(lt => lt.ContactsId == contactsId)
                .ToListAsync();

            if (contactsTags.Any())
            {
                await _contactsTagsRepository.DeleteAsync(contactsTags);
            }
        }
        public async Task<IPagedList<Contacts>> GetDealsByContactsIdAsync(int contactsId, int pageIndex, int pageSize)
        {
            var query = _contactsRepository.Table.Where(x => x.Id == contactsId);
            return await query.ToPagedListAsync(pageIndex, pageSize);
        }

        public async Task<IList<Contacts>> GetContactsByCompanyIdAsync(int companyId)
        {
            if (companyId <= 0)
                return new List<Contacts>();

            var company = await _companyRepository.GetByIdAsync(companyId);
            if (company == null || string.IsNullOrEmpty(company.CompanyName))
                return new List<Contacts>();

            var query = from contact in _contactsRepository.Table
                        where contact.CompanyId == company.Id
                        select contact;

            return await query.ToListAsync();
        }
        public async Task<IPagedList<Contacts>> GetContactByCompanyIdAsync(int companyId, int pageIndex, int pageSize)
        {
            var query = _contactsRepository.Table.Where(x => x.CompanyId == companyId);
            return await query.ToPagedListAsync(pageIndex, pageSize);
        }

        public virtual async Task AddContactTokensAsync(IList<Token> tokens, Contacts contacts)
        {
            if (contacts == null)
                throw new ArgumentNullException(nameof(contacts));

            tokens.Add(new Token("FirstName", contacts.FirstName ?? ""));
            tokens.Add(new Token("LastName", contacts.LastName ?? ""));
            tokens.Add(new Token("WebsiteUrl", contacts.WebsiteUrl ?? ""));
            // Event notification (if needed)
            await _eventPublisher.EntityTokensAddedAsync(contacts, tokens);
        }

        public async Task<Contacts> GetContactsByEmailAsync(string email)
        {
            if (string.IsNullOrEmpty(email))
                return null;

            var contacts = await _contactsRepository.Table
                .FirstOrDefaultAsync(l => l.Email == email);

            return contacts;
        }
        #endregion

        #endregion
    }
}