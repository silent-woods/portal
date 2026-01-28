using App.Core.Domain.Common;
using System;

namespace App.Core.Domain.TaskAlerts;

public partial class TaskAlertConfiguration : BaseEntity, ISoftDeletedEntity
{
    #region Properties

    public int TaskAlertTypeId { get; set; }

    public string Message { get; set; }

    public decimal Percentage { get; set; }

    public bool EnableComment { get; set; }

    public bool CommentRequired { get; set; }

    public bool EnableReason { get; set; }

    public bool ReasonRequired { get; set; }

    public bool EnableCoordinatorMail { get; set; }

    public bool EnableLeaderMail { get; set; }

    public bool EnableManagerMail { get; set; }

    public bool EnableDeveloperMail { get; set; }

    public bool NewETA { get; set; }

    public bool EnableOnTrack { get; set; }

    public bool IsActive { get; set; }

    public int DisplayOrder { get; set; }

    public bool Deleted { get; set; }

    public DateTime CreatedOnUtc { get; set; }

    public DateTime UpdatedOnUtc { get; set; }

    #endregion
}
