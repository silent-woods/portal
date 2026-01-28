using App.Web.Framework.Models;
using System;

namespace Satyanam.Plugin.Misc.LeadAPI.Areas.Admin.Models.LeadAPILog;

public partial record LeadAPILogModel : BaseNopEntityModel
{
    #region Properties

    public string EmployeeEmail { get; set; }

    public string EndPoint { get; set; }

    public string RequestJson { get; set; }

    public string ResponseJson { get; set; }

    public string ResponseMessage { get; set; }

    public bool Success { get; set; }

    public DateTime CreatedOn { get; set; }

	#endregion
}
