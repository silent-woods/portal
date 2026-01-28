using App.Web.Framework.Models;

namespace App.Web.Areas.Admin.Models.Logging
{
    /// <summary>
    /// Represents an activity log list model
    /// </summary>
    public partial record ActivityLogListModel : BasePagedListModel<ActivityLogModel>
    {
    }
}