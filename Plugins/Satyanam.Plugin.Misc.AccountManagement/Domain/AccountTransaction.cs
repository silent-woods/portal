using App.Core;
using App.Core.Domain.Common;
using System;

namespace Satyanam.Plugin.Misc.AccountManagement.Domain;

public partial class AccountTransaction : BaseEntity, ISoftDeletedEntity
{
    #region Properties

    public int InvoicePaymentHistoryId { get; set; }

    public int EmployeeId { get; set; }

    public int TransactionTypeId { get; set; }

    public int InvoiceId { get; set; }

    public int AccountGroupId { get; set; }

    public decimal Amount { get; set; }

    public string Currency { get; set; }

    public int PaymentMethodId { get; set; }

    public string ReferenceNo { get; set; }

    public string Notes { get; set; }

    public int MonthId { get; set; }

    public int YearId { get; set; }

    public int CreatedBy { get; set; }

    public bool Deleted { get; set; }

    public DateTime CreatedOnUtc { get; set; }

    public DateTime UpdatedOnUtc { get; set; }

    #endregion
}
