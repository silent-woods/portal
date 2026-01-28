using App.Core;
using App.Core.Domain.Common;
using System;

namespace Satyanam.Plugin.Misc.AccountManagement.Domain;

public partial class PaymentTerm : BaseEntity, ISoftDeletedEntity
{
    #region Properties

    public string Name { get; set; }

    public int NumberOfDays { get; set; }

    public bool IsActive { get; set; }

    public int DisplayOrder { get; set; }

    public bool Deleted { get; set; }

    public DateTime CreatedOnUtc { get; set; }

    public DateTime UpdatedOnUtc { get; set; }

    #endregion
}
