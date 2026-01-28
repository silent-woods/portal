using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;
using System;
using System.ComponentModel.DataAnnotations;

namespace Satyanam.Plugin.Misc.LeadAPI.Areas.Admin.Models.LeadAPILog;

public partial record LeadAPILogSearchModel : BaseSearchModel
{
    #region Properties

    [NopResourceDisplayName("Satyanam.Plugin.Misc.LeadAPI.Admin.LeadAPILog.Fields.SearchStartDate")]
    [UIHint("DateNullable")]
    public DateTime? SearchStartDate { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.LeadAPI.Admin.LeadAPILog.Fields.SearchEndDate")]
    [UIHint("DateNullable")]
    public DateTime? SearchEndDate { get; set; }

    #endregion
}
