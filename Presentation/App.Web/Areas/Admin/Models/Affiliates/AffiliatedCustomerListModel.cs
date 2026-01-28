using App.Web.Framework.Models;

namespace App.Web.Areas.Admin.Models.Affiliates
{
    /// <summary>
    /// Represents an affiliated customer list model
    /// </summary>
    public partial record AffiliatedCustomerListModel : BasePagedListModel<AffiliatedCustomerModel>
    {
    }
}