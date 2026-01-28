using App.Core;
using App.Services.Media;
using App.Web.Controllers;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Satyanam.Plugin.Misc.TrackerAPI.Controllers;

public partial class TrackerController : BasePublicController
{
    #region Fields

    protected readonly IDownloadService _downloadService;
    protected readonly TrackerAPISettings _trackerAPISettings;

    #endregion

    #region Ctor

    public TrackerController(IDownloadService downloadService, 
        TrackerAPISettings trackerAPISettings)
    {
        _downloadService = downloadService;
        _trackerAPISettings = trackerAPISettings;
    }

    #endregion

    #region Methods

    public virtual async Task<IActionResult> DownloadTracker()
	{
        var download = await _downloadService.GetDownloadByIdAsync(_trackerAPISettings.UploadTimeTrackerId);
        if (download == null)
            return Content("Sample download is not available any more.");

        if (download.UseDownloadUrl)
            return new RedirectResult(download.DownloadUrl);

        if (download.DownloadBinary == null)
            return Content("Download data is not available any more.");

        var fileName = !string.IsNullOrWhiteSpace(download.Filename) ? download.Filename : string.Empty;
        var contentType = !string.IsNullOrWhiteSpace(download.ContentType) ? download.ContentType : MimeTypes.ApplicationOctetStream;
        return new FileContentResult(download.DownloadBinary, contentType) { FileDownloadName = fileName + download.Extension };
    }

    #endregion
}
