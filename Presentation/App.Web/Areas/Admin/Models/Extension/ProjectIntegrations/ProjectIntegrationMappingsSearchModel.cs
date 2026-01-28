using App.Web.Framework.Models;

namespace App.Web.Areas.Admin.Models.ProjectIntegrations;

public partial record ProjectIntegrationMappingsSearchModel : BaseSearchModel
{
	#region Properties

	public int ProjectIntegrationId { get; set; }

	#endregion
}
