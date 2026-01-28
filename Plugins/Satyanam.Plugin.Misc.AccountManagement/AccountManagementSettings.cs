using App.Core.Configuration;

namespace Satyanam.Plugin.Misc.AccountManagement;

public partial class AccountManagementSettings : ISettings
{
	#region Properties

	public bool EnablePlugin { get; set; }

    public int InvoiceNumber { get; set; }

    public int InvoiceLogoId { get; set; }

	#endregion
}
