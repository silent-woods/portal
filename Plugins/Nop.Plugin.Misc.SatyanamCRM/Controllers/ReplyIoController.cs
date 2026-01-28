using App.Services.Security;
using App.Web.Framework;
using App.Web.Framework.Controllers;
using Microsoft.AspNetCore.Mvc;
using Satyanam.Nop.Core.Services;
using Satyanam.Nop.Plugin.Misc.SatyanamCRM.Services;
using Satyanam.Nop.Plugin.SatyanamCRM.Models.Leads;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Satyanam.Nop.Plugin.Misc.SatyanamCRM.Controllers
{

    [Area(AreaNames.Admin)]
    public class ReplyIoController : BasePluginController
    {
        #region Fields

        private readonly IPermissionService _permissionService;
        private readonly ILeadService _leadService;
        private readonly IReplyIoService _replyIoService;
        private readonly ITitleService _titleService;
        #endregion

        #region Ctor 

        public ReplyIoController(IPermissionService permissionService,
                                 ILeadService leadService,
                                 IReplyIoService replyIoService,
                                 ITitleService titleService)
        {
            _permissionService = permissionService;
            _leadService = leadService;
            _replyIoService = replyIoService;
            _titleService = titleService;
        }

        #endregion

        #region Utilities


        #endregion

        #region Methods

        [HttpPost]
        public async Task<IActionResult> SyncSelectedLeads([FromBody] List<int> leadIds)
        {
            try
            {
                if (leadIds == null || !leadIds.Any())
                    return Json(new { success = false, message = "No leads selected." });

                var leads = await _leadService.GetLeadsByIdsAsync(leadIds);
                var validLeads = leads
                    .Where(l => l.EmailStatusId == 1 &&
                                !string.IsNullOrWhiteSpace(l.FirstName) &&
                                !string.IsNullOrWhiteSpace(l.LastName) &&
                                !string.IsNullOrWhiteSpace(l.Email))
                    .ToList();

                if (!validLeads.Any())
                    return Json(new { success = false, message = "No valid leads to sync (missing required fields or invalid email)." });

                var allTitles = await _titleService.GetAllTitleAsync("");

                foreach (var lead in validLeads)
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

                    if (!success)
                    {
                        // You could return early or collect errors to display them all at once
                        return Json(new { success = false, message = $"Failed to sync lead: {lead.Email}" });
                    }
                    lead.IsSyncedToReply= true;
                    await _leadService.UpdateLeadAsync(lead);
                }

                return Json(new { success = true, message = "Leads synced to Reply.io successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred: " + ex.Message });
            }
        }

        #endregion
    }
}
