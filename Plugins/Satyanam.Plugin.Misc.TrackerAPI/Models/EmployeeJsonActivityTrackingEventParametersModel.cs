using System;

namespace Satyanam.Plugin.Misc.TrackerAPI.Models;

public partial class EmployeeJsonActivityTrackingEventParametersModel
{
    #region Properties

    public int StatusId { get; set; }

    public int Duration { get; set; }

    public DateTime CreatedOnUtc { get; set; }

    #endregion
}
