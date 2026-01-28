using App.Core;
using App.Core.Domain.Common;
using System;

namespace Satyanam.Plugin.Misc.AccountManagement.Domain;

public partial class ProjectBilling : BaseEntity, ISoftDeletedEntity
{
    #region Properties

    public string BillingName { get; set; }

    public int ProjectId { get; set; }

    public int CompanyId { get; set; }

    public int PaymentTermId { get; set; }

    public int BillingTypeId { get; set; }

    public int BillingRate { get; set; }

    public int PrimaryCurrencyId { get; set; }

    public int PaymentCurrencyId { get; set; }

    public bool IsActive { get; set; }

    public int DisplayOrder { get; set; }

    public bool Deleted { get; set; }

    public DateTime CreatedOnUtc { get; set; }

    public DateTime UpdatedOnUtc { get; set; }

    #endregion
}
