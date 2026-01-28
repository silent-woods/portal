using App.Web.Framework.Models;

namespace App.Web.Areas.Admin.Models.LeaveManagement
{
    /// <summary>
    /// Represents a LeaveManagement List Model
    /// </summary>
    public partial record LeaveManagementListModel : BasePagedListModel<LeaveManagementModel>
    {
    }
}