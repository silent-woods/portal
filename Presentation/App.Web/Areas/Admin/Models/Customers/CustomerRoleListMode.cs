using App.Web.Framework.Models;

namespace App.Web.Areas.Admin.Models.Customers
{
    /// <summary>
    /// Represents a customer role list model
    /// </summary>
    public partial record CustomerRoleListModel : BasePagedListModel<CustomerRoleModel>
    {
    }
}