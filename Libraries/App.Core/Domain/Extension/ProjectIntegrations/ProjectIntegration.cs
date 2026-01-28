using System;

namespace App.Core.Domain.ProjectIntegrations;

public partial class ProjectIntegration : BaseEntity
{
	#region Properties

	public string IntegrationName { get; set; }

	public string SystemName { get; set; }

	public bool IsActive { get; set; }

	public int DisplayOrder { get; set; }

	public DateTime CreatedOnUtc { get; set; }

	public DateTime UpdatedOnUtc { get; set; }

    #endregion
}
