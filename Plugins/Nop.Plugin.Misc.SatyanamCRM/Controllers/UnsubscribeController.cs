using App.Services.Localization;
using App.Services.Messages;
using App.Services.Security;
using App.Web.Framework.Controllers;
using Microsoft.AspNetCore.Mvc;
using Satyanam.Nop.Core.Services;
using Satyanam.Nop.Plugin.SatyanamCRM.Models.Unsubscribes;
using System;
using System.Threading.Tasks;

namespace Satyanam.Nop.Plugin.Misc.SatyanamCRM.Controllers
{
    public class UnsubscribeController : BasePluginController
    {
        #region Fields

        private readonly IPermissionService _permissionService;
        private readonly ILeadService _leadService;
        private readonly INotificationService _notificationService;
        private readonly ILocalizationService _localizationService;
        private readonly IContactsService _contactsService;
        private readonly ICampaingsEmailLogsService _campaingsEmailLogsService;
        #endregion

        #region Ctor 

        public UnsubscribeController(IPermissionService permissionService,
                               INotificationService notificationService,
                               ILocalizationService localizationService,
                               ILeadService leadService,
                               IContactsService contactsService,
                               ICampaingsEmailLogsService campaingsEmailLogsService)
        {
            _permissionService = permissionService;
            _notificationService = notificationService;
            _localizationService = localizationService;
            _leadService = leadService;
            _contactsService = contactsService;
            _campaingsEmailLogsService = campaingsEmailLogsService;
        }

        #endregion

        #region Utilities


        #endregion

        #region Methods

        [HttpGet]
        [Route("unsubscribe")]
        public async Task<IActionResult> Index(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return Content("Invalid request. Email not found.");
            }

            // Find lead by email
            var lead = await _leadService.GetLeadByEmailAsync(email);
            var contact = await _contactsService.GetContactsByEmailAsync(email);
            if (lead == null && contact == null)
            {
                return Content("Lead not found.");
            }

            if (lead != null)
            {
                lead.EmailOptOut = false;
                await _leadService.UpdateLeadAsync(lead);
            }
            if (contact != null)
            {
                contact.EmailOptOut = false;
                await _contactsService.UpdateContactsAsync(contact);
            }
           
            var logs = await _campaingsEmailLogsService.GetLogsByEmailAsync(email);
            foreach (var log in logs)
            {
                log.IsUnsubscribed = true;
                log.UpdatedOnUtc = DateTime.UtcNow;
                await _campaingsEmailLogsService.UpdateAsync(log);
            }

            var model = new UnsubscribeModel
            {
                Email = email,
                Message = "You have successfully unsubscribed."
            };

            return View("~/Plugins/Misc.SatyanamCRM/Views/Unsubscribe/Index.cshtml", model);
        }

        [HttpGet]
        public async Task<IActionResult> Resubscribe(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                var errorModel = new UnsubscribeModel { Message = "Invalid email address." };
                return View("~/Plugins/Misc.SatyanamCRM/Views/Unsubscribe/Unsubscribes.cshtml", errorModel);
            }

            var lead = await _leadService.GetLeadByEmailAsync(email);
            var contact = await _contactsService.GetContactsByEmailAsync(email);

            if (lead == null && contact == null)
            {
                var errorModel = new UnsubscribeModel { Message = "Email not found in our system." };
                return View("~/Plugins/Misc.SatyanamCRM/Views/Unsubscribe/Unsubscribes.cshtml", errorModel);
            }

            if (lead != null)
            {
                lead.EmailOptOut = true;
                await _leadService.UpdateLeadAsync(lead);
            }

            if (contact != null)
            {
                contact.EmailOptOut = true;
                await _contactsService.UpdateContactsAsync(contact);
            }

            var logs = await _campaingsEmailLogsService.GetLogsByEmailAsync(email);
            foreach (var log in logs)
            {
                log.IsUnsubscribed = false;
                log.UpdatedOnUtc = DateTime.UtcNow;
                await _campaingsEmailLogsService.UpdateAsync(log);
            }

            var model = new UnsubscribeModel
            {
                Email = email,
                Message = "You have successfully resubscribed."
            };
            return View("~/Plugins/Misc.SatyanamCRM/Views/Unsubscribe/Unsubscribes.cshtml", model);
        }

    }
    #endregion
}
