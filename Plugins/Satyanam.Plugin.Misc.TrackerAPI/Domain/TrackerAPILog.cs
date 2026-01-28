using App.Core;
using System;

namespace Satyanam.Plugin.Misc.TrackerAPI.Domain;

public partial class TrackerAPILog : BaseEntity
{
	#region Properties

    public int? EmployeeId { get; set; }

	public string EndPoint { get; set; }

	public string RequestJson { get; set; }

    public string ResponseJson { get; set; }

    public string ResponseMessage { get; set; }

    public bool Success { get; set; }

    public DateTime CreatedOnUtc { get; set; }

	#endregion
}
