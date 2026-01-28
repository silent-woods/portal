using App.Web.Framework.Models;

namespace App.Web.Areas.Admin.Models.Logging
{
    /// <summary>
    /// Represents a log list model
    /// </summary>
    public partial record LogListModel : BasePagedListModel<LogModel>
    {
    }
}