using App.Core;
using App.Core.Domain.Common;
using System;

namespace Satyanam.Plugin.Misc.AccountManagement.Domain;

public partial class InvoiceItem : BaseEntity, ISoftDeletedEntity
{
    #region Properties

    public int InvoiceId { get; set; }

    public string Description { get; set; }

    public decimal Hours { get; set; }

    public int Rate { get; set; }

    public decimal Amount { get; set; }

    public bool Deleted { get; set; }

    public DateTime CreatedOnUtc { get; set; }

    public DateTime UpdatedOnUtc { get; set; }

    #endregion
}
