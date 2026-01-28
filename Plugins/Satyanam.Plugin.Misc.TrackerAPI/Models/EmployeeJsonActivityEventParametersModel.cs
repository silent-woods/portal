using System;

namespace Satyanam.Plugin.Misc.TrackerAPI.Models;

public partial class EmployeeJsonActivityEventParametersModel
{
    #region Properties

    public int KeyboardHits { get; set; }

    public int MouseHits { get; set; }

    public string ScreenshotUrl { get; set; }

    public int StatusId { get; set; }

    public DateTime CreateOnUtc { get; set; }

    #endregion
}
