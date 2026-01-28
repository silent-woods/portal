using App.Web.Framework.Models;

namespace App.Web.Areas.Admin.Models.Directory
{
    /// <summary>
    /// Represents a state and province list model
    /// </summary>
    public partial record StateProvinceListModel : BasePagedListModel<StateProvinceModel>
    {
    }
}