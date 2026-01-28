using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;

namespace Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Models.PaymentTerms;

public partial record PaymentTermModel : BaseNopEntityModel
{
    #region Properties

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.PaymentTerm.Fields.Name")]
    public string Name { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.PaymentTerm.Fields.NumberOfDays")]
    public int NumberOfDays { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.PaymentTerm.Fields.IsActive")]
    public bool IsActive { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.PaymentTerm.Fields.DisplayOrder")]
    public int DisplayOrder { get; set; }

    #endregion
}
