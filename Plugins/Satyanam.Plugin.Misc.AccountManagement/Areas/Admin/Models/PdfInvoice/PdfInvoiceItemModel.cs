namespace Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Models.PdfInvoice;

public partial class PdfInvoiceItemModel
{
    #region Properties

    public string Description { get; set; }

    public decimal Hours { get; set; }

    public decimal Rate { get; set; }

    public decimal Amount { get; set; }

    #endregion
}
