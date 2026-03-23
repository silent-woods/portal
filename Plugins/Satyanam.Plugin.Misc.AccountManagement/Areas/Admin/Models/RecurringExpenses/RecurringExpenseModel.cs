using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Models.RecurringExpenses;

public partial record RecurringExpenseModel : BaseNopEntityModel
{
    #region Ctor
    public RecurringExpenseModel()
    {
        AvailableCategories = new List<SelectListItem>();
        AvailableFrequencies = new List<SelectListItem>();
        AvailableCurrencies = new List<SelectListItem>();
        AvailableAccountGroups = new List<SelectListItem>();
        AvailablePaymentMethods = new List<SelectListItem>();
    }
    #endregion

    #region Properties

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.RecurringExpense.Fields.ExpenseCategory")]
    [Range(1, int.MaxValue, ErrorMessage = "Please select a category.")]
    public int ExpenseCategoryId { get; set; }
    public string ExpenseCategoryName { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.RecurringExpense.Fields.Title")]
    [Required(ErrorMessage = "Title is required.")]
    [StringLength(500)]
    public string Title { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.RecurringExpense.Fields.Description")]
    public string Description { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.RecurringExpense.Fields.Amount")]
    [Range(typeof(decimal), "0.01", "99999999.99", ErrorMessage = "Amount must be greater than 0.")]
    public decimal Amount { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.RecurringExpense.Fields.CurrencyCode")]
    public string CurrencyCode { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.RecurringExpense.Fields.Frequency")]
    public int FrequencyId { get; set; }

    public string FrequencyName { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.RecurringExpense.Fields.RecurrenceDay")]
    public int RecurrenceDay { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.RecurringExpense.Fields.RecurrenceMonth")]
    public int? RecurrenceMonth { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.RecurringExpense.Fields.StartDate")]
    [UIHint("Date")]
    public DateTime StartDate { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.RecurringExpense.Fields.EndDate")]
    [UIHint("DateNullable")]
    public DateTime? EndDate { get; set; }

    [UIHint("DateNullable")]
    public DateTime? LastGeneratedOnUtc { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.RecurringExpense.Fields.NextGenerate")]
    [UIHint("DateNullable")]
    public DateTime? NextGenerateOnUtc { get; set; }
    public string NextGenerateStr { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.RecurringExpense.Fields.IsActive")]
    public bool IsActive { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.RecurringExpense.Fields.AccountGroupId")]
    [Range(1, int.MaxValue, ErrorMessage = "Please select an account group.")]
    public int AccountGroupId { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.RecurringExpense.Fields.PaymentMethodId")]
    [Range(1, int.MaxValue, ErrorMessage = "Please select a payment method.")]
    public int PaymentMethodId { get; set; }
    public IList<SelectListItem> AvailableCategories { get; set; }
    public IList<SelectListItem> AvailableFrequencies { get; set; }
    public IList<SelectListItem> AvailableCurrencies { get; set; }
    public IList<SelectListItem> AvailableAccountGroups { get; set; }
    public IList<SelectListItem> AvailablePaymentMethods { get; set; }
    #endregion
}
