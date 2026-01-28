using App.Web.Framework.Models;

namespace App.Web.Areas.Admin.Models.Stores
{
    /// <summary>
    /// Represents a store list model
    /// </summary>
    public partial record StoreListModel : BasePagedListModel<StoreModel>
    {
    }
}