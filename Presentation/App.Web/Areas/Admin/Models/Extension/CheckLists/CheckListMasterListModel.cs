using App.Web.Framework.Models;

namespace App.Web.Areas.Admin.Models.Extension.CheckLists
{
    /// <summary>
    /// Represents a CheckList Master list model for DataTables
    /// </summary>
    public partial record CheckListMasterListModel : BasePagedListModel<CheckListMasterModel>
    {
    }
}
