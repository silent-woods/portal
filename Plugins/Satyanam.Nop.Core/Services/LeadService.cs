using App.Core;
using App.Core.Events;
using App.Data;
using App.Data.Extensions;
using App.Services.Messages;
using Satyanam.Nop.Core.Domains;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Satyanam.Nop.Core.Services
{
    /// <summary>
    /// Lead service
    /// </summary>
    public partial class LeadService : ILeadService
    {
        #region Fields

        private readonly IRepository<Lead> _leadRepository;
        private readonly IRepository<LeadTags> _leadTagsRepository;
        private readonly IEventPublisher _eventPublisher;
        private readonly IRepository<Contacts> _contactRepository;
        private readonly IRepository<ContactsTags> _contactTagsRepository;

        #endregion

        #region Ctor

        public LeadService(
            IRepository<Lead> leadRepository, IRepository<LeadTags> leadTagsRepository, IEventPublisher eventPublisher, IRepository<Contacts> contactRepository, IRepository<ContactsTags> contactTagsRepository)
        {
            _leadRepository = leadRepository;
            _leadTagsRepository = leadTagsRepository;
            _eventPublisher = eventPublisher;
            _contactRepository = contactRepository;
            _contactTagsRepository = contactTagsRepository;
        }

        #endregion

        #region Methods

        #region Lead

        /// <summary>
        /// Gets all Lead
        /// </summary>
        /// <param name="name">name</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <param name="lead">Filter by lead name</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the lead
        /// </returns>

        public virtual async Task<IPagedList<Lead>> GetAllLeadAsync(string name, string companyName, IList<int> selectedtagsid, string email, string website, int nofoEmployee, int leadStatusId, IList<int> titleid, int emailStatusId, int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, bool? isSyncedToReply = null)
        {
            var query = await _leadRepository.GetAllAsync(async query =>
            {
                if (!string.IsNullOrWhiteSpace(name))
                    query = query.Where(c => c.FirstName.Contains(name) || c.LastName.Contains(name) || (c.FirstName + " " + c.LastName).Contains(name));
                if (!string.IsNullOrWhiteSpace(companyName))
                    query = query.Where(c => c.CompanyName.Contains(companyName));

                if (selectedtagsid != null && selectedtagsid.Any())

                    query = from l in query
                            join lt in _leadTagsRepository.Table on l.Id equals lt.LeadId
                            where selectedtagsid.Contains(lt.TagsId)
                            select l;

                if (!string.IsNullOrWhiteSpace(email))
                    query = query.Where(c => c.Email.Contains(email));

                if (!string.IsNullOrWhiteSpace(website))
                    query = query.Where(c => c.WebsiteUrl.Contains(website));

                if (nofoEmployee != 0)
                    query = query.Where(c => c.NoofEmployee == nofoEmployee);

                if (leadStatusId != 0)
                    query = query.Where(c => c.LeadStatusId == leadStatusId);

                if (titleid != null && titleid.Any())
                    query = query.Where(l => titleid.Contains(l.TitleId));

                if (emailStatusId != 0)
                    query = query.Where(c => c.EmailStatusId == emailStatusId);

                if (isSyncedToReply.HasValue)
                    query = query.Where(c => c.IsSyncedToReply == isSyncedToReply.Value);

                return query.OrderBy(c => c.Id);
            });
            //paging
            return new PagedList<Lead>(query.ToList(), pageIndex, pageSize);
        }

        /// <summary>
        /// Gets a Lead
        /// </summary>
        /// <param name="leadId">lead identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the lead
        /// </returns>
        public virtual async Task<Lead> GetLeadByIdAsync(int leadId)
        {
            return await _leadRepository.GetByIdAsync(leadId);
        }

        public virtual async Task<IList<Lead>> GetLeadByIdsAsync(int[] leadIds)
        {
            return await _leadRepository.GetByIdsAsync(leadIds);
        }
        public async Task<LeadTags> GetLeadTagsByLeadIdAsync(int leadId)
        {
            var leadTag = await _leadTagsRepository.Table
        .FirstOrDefaultAsync(lt => lt.LeadId == leadId);

            if (leadTag == null)
            {
                // If not found, try to get by TagsId
                leadTag = await _leadTagsRepository.Table
                    .FirstOrDefaultAsync(lt => lt.TagsId == leadId);
            }

            return leadTag;
        }
        public async Task<IList<LeadTags>> GetLeadTagByLeadIdAsync(int leadId)
        {
            return await _leadTagsRepository.Table
                .Where(x => x.LeadId == leadId)
                .ToListAsync();
        }

        public async Task<(IList<Lead> Leads, IList<Contacts> Contacts)> GetLeadsAndContactsByTagsAsync(List<int> tagIds)
        {
            if (tagIds == null || !tagIds.Any())
                return (new List<Lead>(), new List<Contacts>());

            // Get leads
            var leads = await (from lt in _leadTagsRepository.Table
                               join l in _leadRepository.Table on lt.LeadId equals l.Id
                               where tagIds.Contains(lt.TagsId)
                                     && l.EmailOptOut == true
                                     && l.EmailStatusId == 1
                               select l)
                              .Distinct()
                              .ToListAsync();

            // Get contacts
            var contacts = await (from ct in _contactTagsRepository.Table
                                  join c in _contactRepository.Table on ct.ContactsId equals c.Id
                                  where tagIds.Contains(ct.TagsId)
                                        && c.EmailOptOut == true
                                        && c.EmailStatusId == 1
                                  select c)
                                 .Distinct()
                                 .ToListAsync();

            return (leads, contacts);
        }

        public async Task<IList<Lead>> GetLeadsByTagsAsync(List<int> tagIds)
        {
            if (tagIds == null || !tagIds.Any())
                return new List<Lead>();

            // Fetch leads with matching tags and exclude those who opted out
            var leads = await (from lt in _leadTagsRepository.Table
                               join l in _leadRepository.Table on lt.LeadId equals l.Id
                               where tagIds.Contains(lt.TagsId) && l.EmailOptOut == true && l.EmailStatusId==1
                               select l)
                              .Distinct()
                              .ToListAsync();


            return leads;
        }


        /// <summary>
        /// Inserts a Lead
        /// </summary>
        /// <param name="lead">Lead</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task InsertLeadAsync(Lead lead)
        {
            await _leadRepository.InsertAsync(lead);
        }

        public virtual async Task InsertLeadTagsAsync(LeadTags leadTags)
        {
            await _leadTagsRepository.InsertAsync(leadTags);
        }

        /// <summary>
        /// Updates the Lead
        /// </summary>
        /// <param name="lead">Lead</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task UpdateLeadAsync(Lead lead)
        {
            await _leadRepository.UpdateAsync(lead);
        }
        public virtual async Task UpdateLeadTagsAsync(LeadTags leadTags)
        {
            await _leadTagsRepository.UpdateAsync(leadTags);
        }
        /// <summary>
        /// Deletes a Lead
        /// </summary>
        /// <param name="lead">Lead</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task DeleteLeadAsync(Lead lead)
        {
            await _leadRepository.DeleteAsync(lead);
        }
        public async Task DeleteTagsByLeadIdAsync(int leadId)
        {
            var leadTags = await _leadTagsRepository.Table
                .Where(lt => lt.LeadId == leadId)
                .ToListAsync();

            if (leadTags.Any())
            {
                await _leadTagsRepository.DeleteAsync(leadTags);
            }
        }

        public async Task DeleteLeadTagsByLeadIdAsync(int leadId, int tagId)
        {
            var leadTag = await _leadTagsRepository.Table
        .FirstOrDefaultAsync(lt => lt.LeadId == leadId && lt.TagsId == tagId);

            if (leadTag != null)
            {
                await _leadTagsRepository.DeleteAsync(leadTag);
            }
        }
        public async Task<LeadTags> GetOrCreateLeadTagAsync(int leadId, int tagId)
        {
            if (leadId <= 0 || tagId <= 0)
                return null;

            // Check if the lead-tag relationship already exists
            var leadTag = await _leadTagsRepository.Table
                .FirstOrDefaultAsync(lt => lt.LeadId == leadId && lt.TagsId == tagId);

            if (leadTag == null)
            {
                // If not found, create a new LeadTags entry
                leadTag = new LeadTags { LeadId = leadId, TagsId = tagId };
                await _leadTagsRepository.InsertAsync(leadTag);
            }

            return leadTag;
        }


        public virtual async Task AddLeadTokensAsync(IList<Token> tokens, Lead lead)
        {
            if (lead == null)
                throw new ArgumentNullException(nameof(lead));

            tokens.Add(new Token("FirstName", lead.FirstName ?? ""));
            tokens.Add(new Token("LastName", lead.LastName ?? ""));
            tokens.Add(new Token("WebsiteUrl", lead.WebsiteUrl ?? ""));
            // Event notification (if needed)
            await _eventPublisher.EntityTokensAddedAsync(lead, tokens);
        }

        public async Task<Lead> GetLeadByEmailAsync(string email)
        {
            if (string.IsNullOrEmpty(email))
                return null;

            var lead = await _leadRepository.Table
                .FirstOrDefaultAsync(l => l.Email == email);

            return lead;
        }
        public async Task<IList<Lead>> GetLeadsByIdsAsync(List<int> ids)
        {
            return await _leadRepository.Table
                .Where(l => ids.Contains(l.Id))
                .ToListAsync();
        }

        public async Task<IList<Lead>> GetAllLeadsForReplyIoSyncAsync()
        {
            return (await _leadRepository.GetAllAsync(query =>
            {
                query = query.Where(x =>
                    !x.IsSyncedToReply &&
                    x.EmailStatusId == 1 &&
                    !string.IsNullOrEmpty(x.FirstName) &&
                    !string.IsNullOrEmpty(x.LastName) &&
                    !string.IsNullOrEmpty(x.Email));
                return query;
            })).ToList();
        }

        #endregion

        #endregion
    }
}