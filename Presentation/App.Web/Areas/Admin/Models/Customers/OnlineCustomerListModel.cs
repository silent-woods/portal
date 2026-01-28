using App.Web.Framework.Models;

namespace App.Web.Areas.Admin.Models.Customers
{
    /// <summary>
    /// Represents an online customer list model
    /// </summary>
    public partial record OnlineCustomerListModel : BasePagedListModel<OnlineCustomerModel>
    {
    }
}