using App.Web.Framework.Models;

namespace App.Web.Areas.Admin.Models.CheckListMappings
{
    /// <summary>
    /// Represents a checklist mapping list model (for DataTables binding)
    /// </summary>
    public partial record CheckListMappingListModel : BasePagedListModel<CheckListMappingModel>
    {
    }
}
