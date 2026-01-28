using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Models.Configuration;

public partial record ConfigurationModel : BaseNopEntityModel
{
    #region Properties

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.Configuration.Fields.EnablePlugin")]
    public bool EnablePlugin { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.Configuration.Fields.InvoiceNumber")]
    public int InvoiceNumber { get; set; }

    [UIHint("Picture")]
    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.Configuration.Fields.InvoiceLogoId")]
    public int InvoiceLogoId { get; set; }

    #endregion
}
