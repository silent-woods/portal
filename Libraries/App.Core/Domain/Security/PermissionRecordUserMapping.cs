using System;

namespace App.Core.Domain.Security;

public partial class PermissionRecordUserMapping : BaseEntity
{
    #region Properties

    public int PermissionId { get; set; }

    public int UserId { get; set; }

    public bool Add { get; set; }

    public bool Edit { get; set; }

    public bool Delete { get; set; }

    public bool View { get; set; }

    public bool Full { get; set; }

    public DateTime CreatedOnUtc { get; set; }

    public DateTime UpdatedOnUtc { get; set; }

    #endregion
}
