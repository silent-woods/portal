using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Models.Expenses;

public partial record ExpenseModel : BaseNopEntityModel
{
    #region Ctor
    public ExpenseModel()
    {
        AvailableCategories = new List<SelectListItem>();
        AvailableCurrencies = new List<SelectListItem>();
        AvailableAccountGroups = new List<SelectListItem>();
        AvailablePaymentMethods = new List<SelectListItem>();
    }
    #endregion

    #region Properties

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.Expense.Fields.ExpenseCategory")]
    [Range(1, int.MaxValue, ErrorMessage = "Please select a category.")]
    public int ExpenseCategoryId { get; set; }
    public string ExpenseCategoryName { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.Expense.Fields.Title")]
    [Required(ErrorMessage = "Title is required.")]
    [StringLength(500)]
    public string Title { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.Expense.Fields.Description")]
    public string Description { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.Expense.Fields.Amount")]
    [Range(typeof(decimal), "0.01", "99999999.99", ErrorMessage = "Amount must be greater than 0.")]
    public decimal Amount { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.Expense.Fields.CurrencyCode")]
    public string CurrencyCode { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.Expense.Fields.ExpenseDate")]
    [UIHint("Date")]
    [Required(ErrorMessage = "Expense date is required.")]
    public DateTime ExpenseDate { get; set; }
    public string ExpenseDateStr { get; set; }
    public int SubmittedByEmployeeId { get; set; }
    public string SubmittedByEmployeeName { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.Expense.Fields.Receipt")]
    public int ReceiptDownloadId { get; set; }
    public string ReceiptFileName { get; set; }
    public Guid ReceiptDownloadGuid { get; set; }
    public int EmployeeSalaryRecordId { get; set; }
    public int RecurringExpenseId { get; set; }
    public bool IsLinkedToSalary => EmployeeSalaryRecordId > 0;
    public bool IsGeneratedFromRecurring => RecurringExpenseId > 0;
    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.Expense.Fields.AccountGroupId")]
    [Range(1, int.MaxValue, ErrorMessage = "Please select an account group.")]
    public int AccountGroupId { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.Expense.Fields.PaymentMethodId")]
    [Range(1, int.MaxValue, ErrorMessage = "Please select a payment method.")]
    public int PaymentMethodId { get; set; }
    public IList<SelectListItem> AvailableCategories { get; set; }
    public IList<SelectListItem> AvailableCurrencies { get; set; }
    public IList<SelectListItem> AvailableAccountGroups { get; set; }
    public IList<SelectListItem> AvailablePaymentMethods { get; set; }
    #endregion
}
