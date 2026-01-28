using App.Web.Framework.Models;

namespace App.Web.Areas.Admin.Models.EmployeeAttendances
{
    /// <summary>
    /// Represents a EmployeeAttendance List Model
    /// </summary>
    public partial record EmployeeAttendanceListModel : BasePagedListModel<EmployeeAttendanceModel>
    {
    }
}