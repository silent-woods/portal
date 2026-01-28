using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;

namespace Satyanam.Plugin.Misc.LeadAPI.Areas.Admin.Models.Configuration;

public partial record ConfigurationModel : BaseNopEntityModel
{
    #region Properties

    [NopResourceDisplayName("Satyanam.Plugin.Misc.LeadAPI.Admin.Configuration.Fields.APIKey")]
    public string APIKey { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.LeadAPI.Admin.Configuration.Fields.APISecretKey")]
    public string APISecretKey { get; set; }

    #endregion
}
