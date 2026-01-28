using App.Web.Framework.Models;

namespace App.Web.Areas.Admin.Models.Common
{
    /// <summary>
    /// Represents an address attribute value list model
    /// </summary>
    public partial record AddressAttributeValueListModel : BasePagedListModel<AddressAttributeValueModel>
    {
    }
}