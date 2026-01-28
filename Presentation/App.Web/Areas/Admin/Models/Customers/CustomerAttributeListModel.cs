using App.Web.Framework.Models;

namespace App.Web.Areas.Admin.Models.Customers
{
    /// <summary>
    /// Represents a customer attribute list model
    /// </summary>
    public partial record CustomerAttributeListModel : BasePagedListModel<CustomerAttributeModel>
    {
    }
}