using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;

namespace App.Web.Areas.Admin.Models.Stores
{
    /// <summary>
    /// Represents a store search model
    /// </summary>
    public partial record StoreSearchModel : BaseSearchModel
    {
        [NopResourceDisplayName("Admin.Configuration.Stores.Name")]
        public string Name { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Stores.Email")]
        public string Email { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Stores.CompanyPhoneNumber")]
        public int CompanyPhoneNumber { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Stores.CompanyAddress")]
        public string CompanyAddress { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Stores.Website")]
        public string Website { get; set; }
    }
}