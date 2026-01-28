using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;

namespace App.Web.Areas.Admin.Models.Affiliates
{
    /// <summary>
    /// Represents an affiliated customer model
    /// </summary>
    public partial record AffiliatedCustomerModel : BaseNopEntityModel
    {
        #region Properties

        [NopResourceDisplayName("Admin.Affiliates.Customers.Name")]
        public string Name { get; set; }

        #endregion
    }
}