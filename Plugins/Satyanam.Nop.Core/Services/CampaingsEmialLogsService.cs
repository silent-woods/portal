using App.Data;
using Satyanam.Nop.Core.Domains;
using System.Threading.Tasks;
using System;
using LinqToDB;
using System.Linq;
using System.Collections.Generic;

namespace Satyanam.Nop.Core.Services
{
    /// <summary>
    /// CampaingsEmialLogs service
    /// </summary>
    public partial class CampaingsEmialLogsService : ICampaingsEmailLogsService
    {
        #region Fields

        private readonly IRepository<CampaingsEmailLogs> _campaingsEmailLogsRepository;

        #endregion

        #region Ctor

        public CampaingsEmialLogsService(IRepository<CampaingsEmailLogs> campaingsEmailLogsRepository)
        {
            _campaingsEmailLogsRepository = campaingsEmailLogsRepository;
        }

        #endregion

        #region Methods

        #region CampaingsEmialLogs

        public async Task InsertAsync(CampaingsEmailLogs entity)
        {
            await _campaingsEmailLogsRepository.InsertAsync(entity);
        }

        public async Task UpdateAsync(CampaingsEmailLogs entity)
        {
            await _campaingsEmailLogsRepository.UpdateAsync(entity);
        }

        public async Task<CampaingsEmailLogs> GetByTrackingGuidAsync(Guid guid)
        {
            return await _campaingsEmailLogsRepository.Table.FirstOrDefaultAsync(x => x.TrackingGuid == guid);
        }

        public async Task<int> GetTotalOpensByCampaignIdAsync(int campaingId)
        {
            return await _campaingsEmailLogsRepository.Table.CountAsync(x => x.CampaingId == campaingId && x.IsOpened);
        }
        public async Task<int> GetOpenedEmailCountAsync(int campaingId)
        {
            return await _campaingsEmailLogsRepository.Table
                .CountAsync(x => x.CampaingId == campaingId && x.IsOpened);
        }

        public async Task<int> GetTotalEmailCountAsync(int campaingId)
        {
            return await _campaingsEmailLogsRepository.Table
                .CountAsync(x => x.CampaingId == campaingId);
        }
        public async Task<int> GetClickedEmailCountAsync(int campaignId)
        {
            return await _campaingsEmailLogsRepository.Table
                .Where(x => x.CampaingId == campaignId && x.IsClicked)
                .CountAsync();
        }
        public async Task<CampaingsEmailLogs> GetByGuidAsync(Guid trackingGuid)
        {
            if (trackingGuid == Guid.Empty)
                return null;

            return await _campaingsEmailLogsRepository.Table
                .FirstOrDefaultAsync(x => x.TrackingGuid == trackingGuid);
        }
        public async Task<IList<CampaingsEmailLogs>> GetLogsByEmailAsync(string email)
        {
            return await _campaingsEmailLogsRepository.Table
                .Where(x => x.Email == email)
                .ToListAsync();
        }
        public async Task<int> GetUnsubscribedCountAsync(int campaignId)
        {
            return await _campaingsEmailLogsRepository.Table
                .CountAsync(x => x.CampaingId == campaignId && x.IsUnsubscribed);
        }

        public async Task<IList<CampaingsEmailLogs>> GetByCampaignIdAsync(int campaingId)
        {
            return await _campaingsEmailLogsRepository.Table
                .Where(log => log.CampaingId == campaingId)
                .ToListAsync();
        }
        #endregion

        #endregion
    }
}