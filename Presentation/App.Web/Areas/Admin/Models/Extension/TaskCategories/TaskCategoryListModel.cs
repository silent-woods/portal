using App.Web.Framework.Models;

namespace App.Web.Areas.Admin.Models.Extension.TaskCategories
{
    /// <summary>
    /// Represents a Task Category list model for DataTables
    /// </summary>
    public partial record TaskCategoryListModel : BasePagedListModel<TaskCategoryModel>
    {
    }
}
