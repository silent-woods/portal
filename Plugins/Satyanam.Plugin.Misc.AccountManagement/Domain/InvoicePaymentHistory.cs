using App.Core;
using App.Core.Domain.Common;
using System;

namespace Satyanam.Plugin.Misc.AccountManagement.Domain;

public partial class InvoicePaymentHistory : BaseEntity, ISoftDeletedEntity
{
    #region Properties

    public int InvoiceId { get; set; }

    public decimal AmountInPaymentCurrency { get; set; }

    public decimal AmountInINR { get; set; }

    public int PaymentMethodId { get; set; }

    public bool IsPartialPayment { get; set; }

    public string Notes { get; set; }

    public int PaymentReceiptId { get; set; }

    public int MonthId { get; set; }

    public int YearId { get; set; }

    public bool Deleted { get; set; }

    public DateTime CreatedOnUtc { get; set; }

    public DateTime UpdatedOnUtc { get; set; }

    #endregion
}
