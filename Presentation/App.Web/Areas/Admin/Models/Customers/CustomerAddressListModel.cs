using App.Web.Areas.Admin.Models.Common;
using App.Web.Framework.Models;

namespace App.Web.Areas.Admin.Models.Customers
{
    /// <summary>
    /// Represents a customer address list model
    /// </summary>
    public partial record CustomerAddressListModel : BasePagedListModel<AddressModel>
    {
    }
}