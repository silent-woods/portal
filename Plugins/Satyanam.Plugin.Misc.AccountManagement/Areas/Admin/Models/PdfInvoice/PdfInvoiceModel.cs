using System;
using System.Collections.Generic;

namespace Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Models.PdfInvoice;

public partial class PdfInvoiceModel
{
    #region Ctor

    public PdfInvoiceModel()
    {
        Items = new List<PdfInvoiceItemModel>();
    }

    #endregion

    #region Properties

    public byte[] LogoImage { get; set; }

    public int InvoiceId { get; set; }

    public DateTime InvoiceDate { get; set; }

    public DateTime DueDate { get; set; }

    public string BillTo { get; set; }

    public string CompanyName { get; set; }

    public string CompanyAddress { get; set; }

    public string CompanyWebsite { get; set; }

    public IList<PdfInvoiceItemModel> Items { get; set; }

    public decimal SubTotal { get; set; }

    public decimal Discount { get; set; }

    public decimal Tax { get; set; }

    public decimal Total { get; set; }

    public string PrimaryCurrency { get; set; }

    public string PaymentCurrency { get; set; }

    public decimal TotalPrimaryAmount { get; set; }

    public decimal TotalPaymentAmount { get; set; }

    public string Notes { get; set; }

    public string BankDetails { get; set; }

    #endregion
}
