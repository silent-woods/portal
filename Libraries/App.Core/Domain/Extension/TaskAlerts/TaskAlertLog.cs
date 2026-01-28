using App.Core.Domain.Common;
using System;

namespace App.Core.Domain.TaskAlerts;

public partial class TaskAlertLog : BaseEntity, ISoftDeletedEntity
{
    #region Properties

    public int EmployeeId { get; set; }

    public int TaskId { get; set; }

    public int AlertId { get; set; }

    public decimal Variation { get; set; }

    public bool MailSent { get; set; }

    public int ReasonId { get; set; }

    public string Comment { get; set; }

    public bool OnTrack { get; set; }

    public string ETAHours { get; set; }

    public int FollowUpTaskId { get; set; }

    public int ReviewerId { get; set; }

    public DateTime? NextFollowupDateTime { get; set; }

    public bool Deleted { get; set; }

    public DateTime CreatedOnUtc { get; set; }

    public DateTime UpdatedOnUtc { get; set; }

    #endregion
}
