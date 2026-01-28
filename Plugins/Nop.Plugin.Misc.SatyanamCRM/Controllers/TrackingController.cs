using App.Services.Localization;
using App.Services.Messages;
using App.Services.Security;
using App.Web.Framework.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Satyanam.Nop.Core.Services;
using System.Threading.Tasks;
using System;
using System.Web;

namespace Satyanam.Nop.Plugin.Misc.SatyanamCRM.Controllers
{
    [AllowAnonymous]
    public class TrackingController : BasePluginController
    {
        #region Fields

        private readonly IPermissionService _permissionService;
        private readonly INotificationService _notificationService;
        private readonly ILocalizationService _localizationService;
        private readonly ICampaingsEmailLogsService _campaingsEmailLogsService;
        #endregion

        #region Ctor 

        public TrackingController(IPermissionService permissionService,
                               INotificationService notificationService,
                               ILocalizationService localizationService,
                               ICampaingsEmailLogsService campaingsEmailLogsService)
        {
            _permissionService = permissionService;
            _notificationService = notificationService;
            _localizationService = localizationService;
            _campaingsEmailLogsService = campaingsEmailLogsService;
        }

        #endregion

        #region Utilities


        #endregion

        #region Methods

        [HttpGet("email-tracking/pixel")]
        public async Task<IActionResult> TrackOpen(Guid guid, int cid)
        {
            var referer = Request.Headers["Referer"].ToString();
            if (!string.IsNullOrEmpty(referer) && referer.Contains("/Admin/QueuedEmail/Edit"))
            {
                return new EmptyResult();
            }
            var log = await _campaingsEmailLogsService.GetByTrackingGuidAsync(guid);
            if (log != null)
            {
                log.OpenCount += 1; //  Increment open count on every hit

                if (!log.IsOpened)
                {
                    log.IsOpened = true; //  Set only once (first time)
                    log.OpenedOnUtc = DateTime.UtcNow;
                }

                log.UpdatedOnUtc = DateTime.UtcNow;
                await _campaingsEmailLogsService.UpdateAsync(log);
            }

            // Transparent 1x1 GIF
            var imageBytes = new byte[]
            {
            71,73,70,56,57,97,1,0,1,0,128,0,0,0,0,0,255,255,255,33,249,4,1,
            0,0,1,0,44,0,0,0,0,1,0,1,0,0,2,2,68,1,0,59
            };

            return File(imageBytes, "image/gif");
        }

        [HttpGet]
        [Route("email-tracking/click")]
        public async Task<IActionResult> TrackClick(Guid guid, string url)
        {
            if (!string.IsNullOrEmpty(url))
            {
                var decodedUrl = HttpUtility.UrlDecode(url);

                var log = await _campaingsEmailLogsService.GetByGuidAsync(guid);
                if (log != null)
                {
                    log.IsClicked = true;
                    log.ClickCount += 1;
                    log.UpdatedOnUtc = DateTime.UtcNow;
                    await _campaingsEmailLogsService.UpdateAsync(log);
                }

                return Redirect(decodedUrl);
            }

            return Content("Invalid tracking link.");
        }




        #endregion
    }
}
