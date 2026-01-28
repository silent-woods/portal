using Satyanam.Nop.Core.Domains;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Satyanam.Nop.Core.Services
{
    /// <summary>
    /// CampaingsEmailLogs service interface
    /// </summary>
    public partial interface ICampaingsEmailLogsService
    {
        Task InsertAsync(CampaingsEmailLogs entity);
        Task UpdateAsync(CampaingsEmailLogs entity);
        Task<CampaingsEmailLogs> GetByTrackingGuidAsync(Guid guid);
        Task<int> GetTotalOpensByCampaignIdAsync(int campaignId);
        Task<int> GetOpenedEmailCountAsync(int campaignId);
        Task<int> GetTotalEmailCountAsync(int campaignId);
        Task<CampaingsEmailLogs> GetByGuidAsync(Guid trackingGuid);
        Task<int> GetClickedEmailCountAsync(int campaignId);
        Task<IList<CampaingsEmailLogs>> GetLogsByEmailAsync(string email);
        Task<int> GetUnsubscribedCountAsync(int campaignId);
        Task<IList<CampaingsEmailLogs>> GetByCampaignIdAsync(int campaingId);
    }
}