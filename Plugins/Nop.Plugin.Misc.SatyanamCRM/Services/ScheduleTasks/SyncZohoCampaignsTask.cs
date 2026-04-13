using App.Services.Localization;
using App.Services.Logging;
using App.Services.ScheduleTasks;
using Satyanam.Nop.Core.Services;
using System;
using System.Threading.Tasks;

namespace Satyanam.Nop.Plugin.Misc.SatyanamCRM.Services.ScheduleTasks
{
    public partial class SyncZohoCampaignsTask : IScheduleTask
    {
        #region Fields

        private readonly IZohoCampaignService _zohoCampaignService;
        private readonly ILogger              _logger;
        private readonly ILocalizationService _localizationService;
        #endregion

        #region Ctor

        public SyncZohoCampaignsTask(IZohoCampaignService zohoCampaignService, ILogger logger, ILocalizationService localizationService)
        {
            _zohoCampaignService = zohoCampaignService;
            _logger = logger;
            _localizationService = localizationService;
        }

        #endregion

        #region Methods

        public async Task ExecuteAsync()
        {
            try
            {
                await _zohoCampaignService.SyncAsync();
            }
            catch (Exception ex)
            {
         await _logger.ErrorAsync(
    await _localizationService.GetResourceAsync("Plugins.SatyanamCRM.ZohoCampaign.SyncTask.Error"),ex);
            }
        }

        #endregion
    }
}
