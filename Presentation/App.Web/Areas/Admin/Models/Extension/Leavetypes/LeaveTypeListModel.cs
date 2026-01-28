using App.Web.Framework.Models;

namespace App.Web.Areas.Admin.Models.Leavetypes
{
    /// <summary>
    /// Represents a LeaveType list model
    /// </summary>
    public partial record LeaveTypeListModel : BasePagedListModel<LeaveTypeModel>
    {
    }
}