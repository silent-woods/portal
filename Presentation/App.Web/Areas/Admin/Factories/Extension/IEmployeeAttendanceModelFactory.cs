using App.Core.Domain.EmployeeAttendances;
using App.Web.Areas.Admin.Models.EmployeeAttendances;
using System.Threading.Tasks;

namespace App.Web.Areas.Admin.Factories
{
    /// <summary>
    /// Represents the EmployeeAttendance model factory
    /// </summary>
    public partial interface IEmployeeAttendanceModelFactory
    {
        Task<EmployeeAttendanceSearchModel> PrepareEmployeeAttendanceSearchModelAsync(EmployeeAttendanceSearchModel searchModel);
        Task<EmployeeAttendanceListModel> PrepareEmployeeAttendanceListModelAsync(EmployeeAttendanceSearchModel searchModel);
        Task<EmployeeAttendanceModel> PrepareEmployeeAttendanceModelAsync(EmployeeAttendanceModel model, EmployeeAttendance employeeAttendance, bool excludeProperties = false);
    }
}