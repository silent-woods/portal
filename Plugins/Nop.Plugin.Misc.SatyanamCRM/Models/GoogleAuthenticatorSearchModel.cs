using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;

namespace Satyanam.Nop.Plugin.SatyanamCRM.Models
{
    /// <summary>
    /// Represents GoogleAuthenticator search model
    /// </summary>
    public record GoogleAuthenticatorSearchModel : BaseSearchModel
    {
        [NopResourceDisplayName("Admin.Customers.Customers.List.SearchEmail")]
        public string SearchEmail { get; set; }

        public bool HideSearchBlock { get; set; }
    }
}
