using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;

namespace Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Models.InvoiceItems;

public partial record InvoiceItemModel : BaseNopEntityModel
{
    #region Properties

    public int InvoiceId { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.InvoiceItem.Fields.Description")]
    public string Description { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.InvoiceItem.Fields.Hours")]
    public decimal Hours { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.InvoiceItem.Fields.Rate")]
    public int Rate { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.InvoiceItem.Fields.Amount")]
    public decimal Amount { get; set; }

    #endregion
}
