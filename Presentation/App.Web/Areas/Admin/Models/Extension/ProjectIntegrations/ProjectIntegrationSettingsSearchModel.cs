using App.Web.Framework.Models;

namespace App.Web.Areas.Admin.Models.ProjectIntegrations;

public partial record ProjectIntegrationSettingsSearchModel : BaseSearchModel
{
	#region Properties

	public int ProjectIntegrationId { get; set; }

	public int ProjectIntegrationMappingId { get; set; }

    #endregion
}
