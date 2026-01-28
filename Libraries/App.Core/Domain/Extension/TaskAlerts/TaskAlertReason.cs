using App.Core.Domain.Common;
using System;

namespace App.Core.Domain.TaskAlerts;

public partial class TaskAlertReason : BaseEntity, ISoftDeletedEntity
{
	#region Properties

	public string Name { get; set; }

    public bool IsActive { get; set; }

    public int DisplayOrder { get; set; }

    public bool Deleted { get; set; }

    public DateTime CreatedOnUtc { get; set; }

    public DateTime UpdatedOnUtc { get; set; }

    #endregion
}
