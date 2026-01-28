using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Models.InvoiceItems;
using Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Models.InvoicePaymentHistories;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Models.Invoices;

public partial record InvoiceModel : BaseNopEntityModel
{
    #region Ctor

    public InvoiceModel()
    {
        AvailableStatuses = new List<SelectListItem>();
        AvailableMonths = new List<SelectListItem>();
        AvailableYears = new List<SelectListItem>();
        AvailableProjectBillings = new List<SelectListItem>();
        AvailableAccountGroups = new List<SelectListItem>();
        AvailableBankAccounts = new List<SelectListItem>();
        InvoiceItemSearchModel = new InvoiceItemSearchModel();
        InvoicePaymentHistorySearchModel = new InvoicePaymentHistorySearchModel();
        SendEmail = new SendEmailModel();
    }

    #endregion

    #region Properties

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.Invoice.Fields.InvoiceNumber")]
    public int InvoiceNumber { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.Invoice.Fields.Title")]
    public string Title { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.Invoice.Fields.ProjectBillingId")]
    public int ProjectBillingId { get; set; }

    public string ProjectBillingName { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.Invoice.Fields.AccountGroupId")]
    public int AccountGroupId { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.Invoice.Fields.StatusId")]
    public int StatusId { get; set; }

    public string InvoiceStatus { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.Invoice.Fields.BankAccountId")]
    public int BankAccountId { get; set; }

    public string CompanyName { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.Invoice.Fields.SubTotalAmount")]
    public decimal SubTotalAmount { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.Invoice.Fields.TaxAmount")]
    public decimal TaxAmount { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.Invoice.Fields.DiscountAmount")]
    public decimal DiscountAmount { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.Invoice.Fields.TotalPrimaryAmount")]
    public decimal TotalPrimaryAmount { get; set; }

    public string PrimaryCurrency { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.Invoice.Fields.TotalPaymentAmount")]
    public decimal TotalPaymentAmount { get; set; }

    public decimal TotalPaidAmount { get; set; }

    public string PaymentCurrency { get; set; }

    [UIHint("Download")]
    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.Invoice.Fields.InvoiceFileId")]
    public int InvoiceFileId { get; set; }

    [UIHint("Download")]
    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.Invoice.Fields.TimeSheetFileId")]
    public int TimeSheetFileId { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.Invoice.Fields.MonthId")]
    public int MonthId { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.Invoice.Fields.YearId")]
    public int YearId { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.Invoice.Fields.Notes")]
    public string Notes { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.Invoice.Fields.IsActive")]
    public bool IsActive { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.Invoice.Fields.DisplayOrder")]
    public int DisplayOrder { get; set; }

    public DateTime CreatedOn { get; set; }

    public bool InvoiceItems { get; set; }

    public bool ShowPaymentHistories { get; set; }

    public IList<SelectListItem> AvailableStatuses { get; set; }

    public IList<SelectListItem> AvailableMonths { get; set; }

    public IList<SelectListItem> AvailableYears { get; set; }

    public IList<SelectListItem> AvailableProjectBillings { get; set; }

    public IList<SelectListItem> AvailableAccountGroups { get; set; }

    public IList<SelectListItem> AvailableBankAccounts { get; set; }

    public InvoiceItemSearchModel InvoiceItemSearchModel { get; set; }

    public InvoicePaymentHistorySearchModel InvoicePaymentHistorySearchModel { get; set; }

    public SendEmailModel SendEmail { get; set; }

    #endregion

    #region Nested classes

    public partial record SendEmailModel : BaseNopModel
    {
        [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.Invoice.SendEmail.Fields.Name")]
        public string Name { get; set; }

        [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.Invoice.SendEmail.Fields.Email")]
        public string Email { get; set; }

        [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.Invoice.SendEmail.Fields.Subject")]
        public string Subject { get; set; }

        [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.Invoice.SendEmail.Fields.Body")]
        public string Body { get; set; }
    }

    #endregion
}
