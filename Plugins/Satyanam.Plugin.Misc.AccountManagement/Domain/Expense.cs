using App.Core;
using App.Core.Domain.Common;
using System;

namespace Satyanam.Plugin.Misc.AccountManagement.Domain;

public partial class Expense : BaseEntity, ISoftDeletedEntity
{
    #region Properties
    public int ExpenseCategoryId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; }
    public DateTime ExpenseDate { get; set; }
    public int SubmittedByEmployeeId { get; set; }
    public int ReceiptDownloadId { get; set; }
    public int EmployeeSalaryRecordId { get; set; }
    public int RecurringExpenseId { get; set; }
    public int AccountTransactionId { get; set; }
    public int PaymentMethodId { get; set; }
    public bool Deleted { get; set; }
    public DateTime CreatedOnUtc { get; set; }
    public DateTime UpdatedOnUtc { get; set; }
    #endregion
}
