using App.Core;
using App.Core.Domain.Common;
using System;

namespace Satyanam.Plugin.Misc.AccountManagement.Domain;

public partial class Invoice : BaseEntity, ISoftDeletedEntity
{
    #region Properties

    public int InvoiceNumber { get; set; }

    public string Title { get; set; }

    public int ProjectBillingId { get; set; }

    public int AccountGroupId { get; set; }

    public int StatusId { get; set; }

    public decimal SubTotalAmount { get; set; }

    public decimal TaxAmount { get; set; }

    public decimal DiscountAmount { get; set; }

    public decimal TotalPrimaryAmount { get; set; }

    public decimal TotalPaymentAmount { get; set; }

    public string Notes { get; set; }

    public DateTime InvoiceDate { get; set; }

    public DateTime DueDate { get; set; }

    public int InvoiceFileId { get; set; }

    public int TimeSheetFileId { get; set; }

    public int BankAccountId { get; set; }

    public int MonthId { get; set; }

    public int YearId { get; set; }

    public bool IsActive { get; set; }

    public int DisplayOrder { get; set; }

    public bool Deleted { get; set; }

    public DateTime CreatedOnUtc { get; set; }

    public DateTime UpdatedOnUtc { get; set; }

    #endregion
}
