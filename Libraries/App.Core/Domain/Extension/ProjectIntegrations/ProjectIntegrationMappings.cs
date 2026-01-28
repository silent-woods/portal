using System;

namespace App.Core.Domain.ProjectIntegrations;

public partial class ProjectIntegrationMappings : BaseEntity
{
	#region Properties

	public int IntegrationId { get; set; }

	public int ProjectId { get; set; }

    public DateTime CreatedOnUtc { get; set; }

    public DateTime UpdatedOnUtc { get; set; }

    #endregion
}
