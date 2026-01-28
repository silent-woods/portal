using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;
using System;
using System.ComponentModel.DataAnnotations;

namespace Satyanam.Plugin.Misc.TrackerAPI.Areas.Admin.Models.TrackerAPILog;

public partial record TrackerAPILogSearchModel : BaseSearchModel
{
    #region Properties

    [NopResourceDisplayName("Satyanam.Plugin.Misc.TrackerAPI.Admin.TrackerAPILog.Fields.SearchStartDate")]
    [UIHint("DateNullable")]
    public DateTime? SearchStartDate { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.TrackerAPI.Admin.TrackerAPILog.Fields.SearchEndDate")]
    [UIHint("DateNullable")]
    public DateTime? SearchEndDate { get; set; }

    #endregion
}
