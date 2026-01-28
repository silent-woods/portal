using App.Core;
using App.Data;
using App.Data.Extensions;
using Satyanam.Nop.Core.Domains;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Satyanam.Nop.Core.Services
{
    /// <summary>
    /// LinkedInFollowups service
    /// </summary>
    public partial class LinkedInFollowupsService : ILinkedInFollowupsService
    {
        #region Fields

        private readonly IRepository<LinkedInFollowups> _linkedInFollowupsRepository;

        #endregion

        #region Ctor

        public LinkedInFollowupsService(IRepository<LinkedInFollowups> linkedInFollowupsRepository)
        {
            _linkedInFollowupsRepository = linkedInFollowupsRepository;
        }

        #endregion

        #region Methods

        #region LinkedInFollowups

        /// <summary>
        /// Gets all LinkedInFollowups
        /// </summary>
        /// <param name="name">name</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <param name="linkedInFollowups">Filter by linkedInFollowups name</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the linkedInFollowups
        /// </returns>

        public virtual async Task<IPagedList<LinkedInFollowups>> GetAllLinkedInFollowupsAsync(string firstname, string lastname,
             string email, string linkedinUrl, string website, DateTime? lastMessageDate = null, DateTime? nextFollowUpDate = null,
    int? statusId = null, int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, bool? isSyncedToReply = null)
        {
            var query = await _linkedInFollowupsRepository.GetAllAsync(async query =>
            {
                if (!string.IsNullOrWhiteSpace(firstname))
                    query = query.Where(c => c.FirstName.Contains(firstname));

                if (!string.IsNullOrWhiteSpace(firstname))
                    query = query.Where(c => c.LastName.Contains(lastname));

                if (!string.IsNullOrWhiteSpace(email))
                    query = query.Where(c => c.Email.Contains(email));

                if (!string.IsNullOrWhiteSpace(website))
                    query = query.Where(c => c.WebsiteUrl.Contains(website));

                if (!string.IsNullOrWhiteSpace(linkedinUrl))
                    query = query.Where(c => c.LinkedinUrl.Contains(linkedinUrl));

                if (lastMessageDate.HasValue)
                    query = query.Where(c => c.LastMessageDate.HasValue &&
                                     c.LastMessageDate.Value.Date == lastMessageDate.Value.Date);

                if (nextFollowUpDate.HasValue)
                    query = query.Where(c => c.NextFollowUpDate.HasValue &&
                                     c.NextFollowUpDate.Value.Date == nextFollowUpDate.Value.Date);

                if (statusId.HasValue)
                    query = query.Where(c => c.StatusId == statusId.Value);

                return query.OrderBy(c => c.Id);
            });
            //paging
            return new PagedList<LinkedInFollowups>(query.ToList(), pageIndex, pageSize);
        }

        /// <summary>
        /// Gets a LinkedInFollowups
        /// </summary>
        /// <param name="linkedInFollowupsId">linkedInFollowups identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the linkedInFollowups
        /// </returns>
        public virtual async Task<LinkedInFollowups> GetLinkedInFollowupsByIdAsync(int linkedInFollowupsId)
        {
            return await _linkedInFollowupsRepository.GetByIdAsync(linkedInFollowupsId);
        }

        public virtual async Task<IList<LinkedInFollowups>> GetLinkedInFollowupsByIdsAsync(int[] linkedInFollowUpsIds)
        {
            return await _linkedInFollowupsRepository.GetByIdsAsync(linkedInFollowUpsIds);
        }

        /// <summary>
        /// Inserts a linkedInFollowups
        /// </summary>
        /// <param name="linkedInFollowups">LinkedInFollowups</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task InsertLinkedInFollowupsAsync(LinkedInFollowups linkedInFollowups)
        {
            await _linkedInFollowupsRepository.InsertAsync(linkedInFollowups);
        }

        /// <summary>
        /// Updates the LinkedInFollowups
        /// </summary>
        /// <param name="linkedInFollowups">LinkedInFollowups</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task UpdateLinkedInFollowupsAsync(LinkedInFollowups linkedInFollowups)
        {
            await _linkedInFollowupsRepository.UpdateAsync(linkedInFollowups);
        }

        /// <summary>
        /// Deletes a LinkedInFollowups
        /// </summary>
        /// <param name="linkedInFollowups">LinkedInFollowups</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task DeleteLinkedInFollowupsAsync(LinkedInFollowups linkedInFollowups)
        {
            await _linkedInFollowupsRepository.DeleteAsync(linkedInFollowups);
        }

        public async Task<LinkedInFollowups> GetLinkedInFollowupByLinkedinUrlOrEmailAsync(string linkedinUrl, string email)
        {
            var query = from f in _linkedInFollowupsRepository.Table
                        where (!string.IsNullOrEmpty(linkedinUrl) && f.LinkedinUrl == linkedinUrl)
                           || (!string.IsNullOrEmpty(email) && f.Email == email)
                        select f;

            return await query.FirstOrDefaultAsync();
        }

        #endregion

        #endregion
    }
}