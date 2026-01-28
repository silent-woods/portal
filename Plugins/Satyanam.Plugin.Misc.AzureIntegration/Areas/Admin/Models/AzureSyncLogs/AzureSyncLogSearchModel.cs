using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;
using System;
using System.ComponentModel.DataAnnotations;

namespace Satyanam.Plugin.Misc.AzureIntegration.Areas.Admin.Models.AzureSyncLogs;

public partial record AzureSyncLogSearchModel : BaseSearchModel
{
    #region Properties

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AzureIntegration.Admin.AzureSyncLog.Fields.SearchStartDate")]
    [UIHint("DateNullable")]
    public DateTime? SearchStartDate { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AzureIntegration.Admin.AzureSyncLog.Fields.SearchEndDate")]
    [UIHint("DateNullable")]
    public DateTime? SearchEndDate { get; set; }

    #endregion
}
