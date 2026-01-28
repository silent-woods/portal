using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Models.AccountTransactions;

public partial record AccountTransactionModel : BaseNopEntityModel
{
    #region Ctor

    public AccountTransactionModel()
    {
        AvailableTransactionTypes = new List<SelectListItem>();
        AvailableInvoices = new List<SelectListItem>();
        AvailableAccountGroups = new List<SelectListItem>();
        AvailablePaymentMethods = new List<SelectListItem>();
        AvailableMonths = new List<SelectListItem>();
        AvailableYears = new List<SelectListItem>();
    }

    #endregion

    #region Properties

    public string TransactionType { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.AccountTransaction.Fields.TransactionTypeId")]
	public int TransactionTypeId { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.AccountTransaction.Fields.InvoiceId")]
    public int InvoiceId { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.AccountTransaction.Fields.AccountGroupId")]
    public int AccountGroupId { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.AccountTransaction.Fields.Amount")]
    public decimal Amount { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.AccountTransaction.Fields.Currency")]
    public string Currency { get; set; }

    public string PaymentMethod { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.AccountTransaction.Fields.PaymentMethodId")]
    public int PaymentMethodId { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.AccountTransaction.Fields.ReferenceNo")]
    public string ReferenceNo { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.AccountTransaction.Fields.Notes")]
    public string Notes { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.AccountTransaction.Fields.MonthId")]
    public int MonthId { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.AccountTransaction.Fields.YearId")]
    public int YearId { get; set; }

    public IList<SelectListItem> AvailableTransactionTypes { get; set; }

    public IList<SelectListItem> AvailableInvoices { get; set; }

    public IList<SelectListItem> AvailableAccountGroups { get; set; }

    public IList<SelectListItem> AvailablePaymentMethods { get; set; }

    public IList<SelectListItem> AvailableMonths { get; set; }

    public IList<SelectListItem> AvailableYears { get; set; }

    #endregion
}
