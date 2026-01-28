using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using App.Core;
using App.Core.Domain.Customers;
using App.Services.Localization;
using App.Services.Media;
using App.Web.Framework.Mvc.Filters;

namespace App.Web.Controllers
{
    public partial class DownloadController : BasePublicController
    {
        private readonly CustomerSettings _customerSettings;
        private readonly IDownloadService _downloadService;
        private readonly ILocalizationService _localizationService;
        private readonly IWorkContext _workContext;

        public DownloadController(CustomerSettings customerSettings,
            IDownloadService downloadService,
            ILocalizationService localizationService,
            IWorkContext workContext)
        {
            _customerSettings = customerSettings;
            _downloadService = downloadService;
            _localizationService = localizationService;
            _workContext = workContext;
        }
        
        public virtual async Task<IActionResult> GetFileUpload(Guid downloadId)
        {
            var download = await _downloadService.GetDownloadByGuidAsync(downloadId);
            if (download == null)
                return Content("Download is not available any more.");

            //A warning (SCS0027 - Open Redirect) from the "Security Code Scan" analyzer may appear at this point. 
            //In this case, it is not relevant. Url may not be local.
            if (download.UseDownloadUrl)
                return new RedirectResult(download.DownloadUrl);

            //binary download
            if (download.DownloadBinary == null)
                return Content("Download data is not available any more.");

            //return result
            var fileName = !string.IsNullOrWhiteSpace(download.Filename) ? download.Filename : downloadId.ToString();
            var contentType = !string.IsNullOrWhiteSpace(download.ContentType) ? download.ContentType : MimeTypes.ApplicationOctetStream;
            return new FileContentResult(download.DownloadBinary, contentType) { FileDownloadName = fileName + download.Extension };
        }
    }
}