using App.Web.Framework.Models;
using System;

namespace Satyanam.Plugin.Misc.AzureIntegration.Areas.Admin.Models.AzureSyncLogs;

public partial record AzureSyncLogModel : BaseNopEntityModel
{
    #region Properties

    public string TaskName { get; set; }

    public string APIEndPoint { get; set; }

    public string Exception { get; set; }

    public DateTime CreatedOn { get; set; }

    #endregion
}
