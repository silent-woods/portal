using App.Web.Framework.Models;

namespace App.Web.Areas.Admin.Models.ProjectIntegrations;

public partial record ProjectIntegrationSettingsModel : BaseNopEntityModel
{
	#region Properties

	public string SettingsName { get; set; }

	public string Value { get; set; }

	#endregion
}
