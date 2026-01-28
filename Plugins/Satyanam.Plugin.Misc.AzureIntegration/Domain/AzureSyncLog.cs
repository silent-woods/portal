using App.Core;
using System;

namespace Satyanam.Plugin.Misc.AzureIntegration.Domain;

public partial class AzureSyncLog : BaseEntity
{
	#region Properties

	public int TaskId { get; set; }

	public string APIEndPoint { get; set; }

	public string Exception { get; set; }

	public DateTime CreatedOnUtc { get; set; }

	#endregion
}
