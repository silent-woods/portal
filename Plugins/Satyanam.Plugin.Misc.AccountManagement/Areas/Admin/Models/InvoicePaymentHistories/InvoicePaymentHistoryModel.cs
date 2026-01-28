using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Models.InvoicePaymentHistories;

public partial record InvoicePaymentHistoryModel : BaseNopEntityModel
{
    #region Ctor

    public InvoicePaymentHistoryModel()
    {
        AvailableMonths = new List<SelectListItem>();
        AvailableYears = new List<SelectListItem>();
        AvailablePaymentMethods = new List<SelectListItem>();
    }

    #endregion

    #region Properties

    public int InvoiceId { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.InvoicePaymentHistory.Fields.AmountInPaymentCurrency")]
    public decimal AmountInPaymentCurrency { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.InvoicePaymentHistory.Fields.AmountInINR")]
    public decimal AmountInINR { get; set; }

    public string PaymentMethod { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.InvoicePaymentHistory.Fields.PaymentMethodId")]
    public int PaymentMethodId { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.InvoicePaymentHistory.Fields.IsPartialPayment")]
    public bool IsPartialPayment { get; set; }

    [UIHint("Download")]
    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.InvoicePaymentHistory.Fields.PaymentReceiptId")]
    public int PaymentReceiptId { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.InvoicePaymentHistory.Fields.Month")]
    public int MonthId { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.InvoicePaymentHistory.Fields.Year")]
    public int YearId { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.InvoicePaymentHistory.Fields.Notes")]
    public string Notes { get; set; }

    public IList<SelectListItem> AvailableMonths { get; set; }

    public IList<SelectListItem> AvailableYears { get; set; }

    public IList<SelectListItem> AvailablePaymentMethods { get; set; }

    #endregion
}
