using Satyanam.Nop.Core.Domains;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Satyanam.Nop.Core.Services
{
    public partial interface IZohoCampaignService
    {
        Task<string> GetValidAccessTokenAsync();
        Task SyncAsync();
        Task<IList<ZohoCampaignStat>> GetStoredStatsAsync(int limit = 10);
        Task<IList<ZohoCampaignLocationStat>> GetLocationStatsByKeyAsync(string campaignKey);
        Task SyncRecipientDataAsync(string campaignKey);
        Task SyncRecipientDataAsync(string campaignKey, string accessToken);
        Task<IList<ZohoCampaignDailyStat>> GetDailyStatsByKeyAsync(string campaignKey, int pastDays = 30, DateTime? from = null);
        Task<IList<ZohoCampaignDailyStat>> GetCombinedDailyStatsAsync(DateTime from, DateTime to, IList<string> campaignKeys = null);
        Task<IDictionary<string, (int Opens, int Clicks, int Bounces)>> GetPeriodStatsByCampaignAsync(DateTime from, DateTime to);
        Task<IList<ZohoCampaignRecipient>> GetRecipientsByLeadIdAsync(int leadId);
        Task<IList<ZohoCampaignRecipient>> GetRecipientsByContactIdAsync(int contactId);
    }
}
