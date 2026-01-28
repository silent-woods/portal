using App.Web.Framework.Models;

namespace App.Web.Areas.Admin.Models.Common
{
    /// <summary>
    /// Represents an address attribute list model
    /// </summary>
    public partial record AddressAttributeListModel : BasePagedListModel<AddressAttributeModel>
    {
    }
}