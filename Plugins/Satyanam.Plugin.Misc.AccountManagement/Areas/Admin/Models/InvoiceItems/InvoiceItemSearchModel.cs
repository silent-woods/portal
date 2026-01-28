using App.Web.Framework.Models;

namespace Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Models.InvoiceItems;

public partial record InvoiceItemSearchModel : BaseSearchModel
{
	#region Properties

	public int InvoiceId { get; set; }

	#endregion
}
