using App.Web.Framework.Models;

namespace App.Web.Areas.Admin.Models.Customers
{
    /// <summary>
    /// Represents a customer attribute value list model
    /// </summary>
    public partial record CustomerAttributeValueListModel : BasePagedListModel<CustomerAttributeValueModel>
    {
    }
}