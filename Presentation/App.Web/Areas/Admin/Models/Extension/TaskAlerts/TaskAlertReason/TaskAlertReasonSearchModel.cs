using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;

namespace App.Web.Areas.Admin.Models.TaskAlerts.TaskAlertReason;

public partial record TaskAlertReasonSearchModel : BaseSearchModel
{
	#region Properties

	[NopResourceDisplayName("Admin.TaskAlert.TaskAlertReason.Fields.SearchName")]
	public string SearchName { get; set; }

	#endregion
}
