using App.Web.Framework.Models;

namespace App.Web.Areas.Admin.Models.Employees

{
    /// <summary>
    /// Represents a Assets list model
    /// </summary>
    public partial record AssetsListModel : BasePagedListModel<AssetsModel>
    {
      
    }
}