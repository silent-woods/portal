using App.Web.Framework.Models;
using System;

namespace App.Web.Areas.Admin.Models.ProjectIntegrations;

public partial record ProjectIntegrationMappingsModel : BaseNopEntityModel
{
	#region Properties

	public string ProjectName { get; set; }

	public DateTime CreatedOn { get; set; }

	#endregion
}
