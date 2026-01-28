using System;

namespace App.Core.Domain.ProjectIntegrations;

public partial class ProjectIntegrationSettings: BaseEntity
{
	#region Properties

	public int ProjectIntegrationMappingId { get; set; }

	public string KeyName { get; set; }

	public string KeyValue { get; set; }

    public DateTime CreatedOnUtc { get; set; }

    public DateTime UpdatedOnUtc { get; set; }

    #endregion

}
