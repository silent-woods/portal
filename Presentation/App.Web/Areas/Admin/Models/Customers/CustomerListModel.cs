using App.Web.Framework.Models;

namespace App.Web.Areas.Admin.Models.Customers
{
    /// <summary>
    /// Represents a customer list model
    /// </summary>
    public partial record CustomerListModel : BasePagedListModel<CustomerModel>
    {
    }
}