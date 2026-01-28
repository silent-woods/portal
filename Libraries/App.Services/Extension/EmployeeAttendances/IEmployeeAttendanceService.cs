using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using App.Core;
using App.Core.Domain.EmployeeAttendances;
using App.Core.Domain.Extension.TimeSheets;
using App.Core.Domain.Leaves;
using App.Core.Domain.TimeSheets;

namespace App.Services.EmployeeAttendances
{
    /// <summary>
    /// EmployeeAttendance service interface
    /// </summary>
    public partial interface IEmployeeAttendanceService
    {
        /// <summary>
        /// Gets all EmployeeAttendance
        /// </summary>
        Task<IPagedList<EmployeeAttendance>> GetAllEmployeeAttendanceAsync(string employeeName, DateTime? from, DateTime? to, int statusId,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, bool? overridePublished = null);

        Task<IPagedList<EmployeeAttendance>> GetAllEmployeeAttendanceAsync(int employeeId, DateTime? from, DateTime? to, int statusId,
           int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, bool? overridePublished = null);

        /// <summary>
        /// Get EmployeeAttendance by id
        /// </summary>
        /// <param name="empId"></param>
        /// <returns></returns>
        Task<EmployeeAttendance> GetEmployeeAttendanceByIdAsync(int empId);

        /// <summary>
        /// Insert EmployeeAttendance
        /// </summary>
        /// <param name="employeeAttendance"></param>
        /// <returns></returns>
        Task InsertEmployeeAttendanceAsync(EmployeeAttendance employeeAttendance);

        /// <summary>
        /// Update EmployeeAttendance
        /// </summary>
        /// <param name="teamPerformance"></param>
        /// <returns></returns>
        Task UpdateEmployeeAttendanceAsync(EmployeeAttendance employeeAttendance);

        /// <summary>
        /// Delete EmployeeAttendance
        /// </summary>
        /// <param name="kPIWeightage"></param>
        /// <returns></returns>
        Task DeleteEmployeeAttendanceAsync(EmployeeAttendance employeeAttendance);

        /// <summary>
        /// Get EmployeeAttendance by ids
        /// </summary>
        /// <param name="empIds"></param>
        /// <returns></returns>
        Task<IList<EmployeeAttendance>> GetEmployeeAttendanceByIdsAsync(int[] empIds);
        Task<IList<int>> GetEmployeeAttendanceByEmployeeNameAsync(string employeename);

        Task<IList<EmployeeAttendance>> GetEmployeeAttendanceByCheckInDateAndEmpIdAsync(DateTime date, int employeeId);

        Task UpdateEmployeeAttendanceBasedOnTimeSheetAsync(DateTime date, int EmployeeId, int spentHours, int spentMinutes, DateTime prevDate, int prevEmployeeId);

        Task DeleteEmployeeAttendanceBasedOnTimeSheetAsync(TimeSheet timeSheet);

        Task UpdateEmployeeAttendanceBasedOnLeave(int EmployeeId, DateTime from, DateTime to, decimal NoOfDays, int statusId, DateTime? prevFrom = null, DateTime? prevTo = null);

        Task DeleteEmployeeAttendanceBasedOnLeaveAsync(LeaveManagement leaveManagement);

        Task<IList<TimeSheetReport>> GetAttendanceReportListAsync(int employeeId, DateTime from, DateTime to, int showById);

    }
}