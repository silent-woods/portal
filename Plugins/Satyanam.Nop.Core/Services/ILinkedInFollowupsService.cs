using App.Core;
using Satyanam.Nop.Core.Domains;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Satyanam.Nop.Core.Services
{
    /// <summary>
    /// LinkedInFollowups service interface
    /// </summary>
    public partial interface ILinkedInFollowupsService
    {
        Task<IPagedList<LinkedInFollowups>> GetAllLinkedInFollowupsAsync(string firstname, string lastname,
         string email, string linkedinUrl, string website, DateTime? lastMessageDate = null, DateTime? nextFollowUpDate = null,
         int? statusId = null, int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, bool? isSyncedToReply = null);
        Task<LinkedInFollowups> GetLinkedInFollowupsByIdAsync(int id);
        Task<IList<LinkedInFollowups>> GetLinkedInFollowupsByIdsAsync(int[] linkedInFollowUpsIds);
        Task InsertLinkedInFollowupsAsync(LinkedInFollowups linkedInFollowups);
        Task UpdateLinkedInFollowupsAsync(LinkedInFollowups linkedInFollowups);
        Task DeleteLinkedInFollowupsAsync(LinkedInFollowups linkedInFollowups);
        Task<LinkedInFollowups> GetLinkedInFollowupByLinkedinUrlOrEmailAsync(string linkedinUrl, string email);

    }
}