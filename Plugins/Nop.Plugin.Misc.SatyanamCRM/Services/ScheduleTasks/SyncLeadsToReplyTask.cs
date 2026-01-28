using App.Services.Logging;
using Satyanam.Nop.Core.Services;
using Satyanam.Nop.Plugin.SatyanamCRM.Models.Leads;
using System.Linq;
using System;
using System.Threading.Tasks;
using App.Services.ScheduleTasks;

namespace Satyanam.Nop.Plugin.Misc.SatyanamCRM.Services.ScheduleTasks
{
    
    public partial class SyncLeadsToReplyTask : IScheduleTask
    {
        #region Fields
        private readonly ILeadService _leadService;
        private readonly ITitleService _titleService;
        private readonly IReplyIoService _replyIoService;
        private readonly ILogger _logger;


        #endregion

        #region Ctor

        public SyncLeadsToReplyTask(ILeadService leadService,
            ITitleService titleService,
            IReplyIoService replyIoService,
            ILogger logger)
        {
            _leadService = leadService;
            _titleService = titleService;
            _replyIoService = replyIoService;
            _logger = logger;
        }

        #endregion

        #region Methods

        #region 
        public async Task ExecuteAsync()
        {
            try
            {
                var leads = await _leadService.GetAllLeadsForReplyIoSyncAsync();
                if (!leads.Any())
                    return;

                var allTitles = await _titleService.GetAllTitleAsync("");

                foreach (var lead in leads)
                {
                    var titleName = allTitles.FirstOrDefault(t => t.Id == lead.TitleId)?.Name ?? "";

                    var dto = new ReplyLeadDto
                    {
                        FirstName = lead.FirstName,
                        LastName = lead.LastName,
                        Email = lead.Email,
                        Company = lead.CompanyName,
                        Domain = lead.WebsiteUrl,
                        Title = titleName,
                        Phone = lead.Phone
                    };

                    var success = await _replyIoService.CreateOrUpdateLeadAsync(dto);

                    if (success)
                    {
                        lead.IsSyncedToReply = true;
                        await _leadService.UpdateLeadAsync(lead);
                    }
                    else
                    {
                        await _logger.ErrorAsync($"Reply.io sync failed for lead: {lead.Email}", null);
                    }
                }
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync("Error in SyncLeadsToReplyTask", ex);
            }
        }
    

    #endregion

    #endregion
}
}