using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using App.Core;
using App.Core.Domain.Employees;
using App.Core.Domain.Leaves;
using StackExchange.Redis;

namespace App.Services.Leaves
{
    public partial interface ILeaveManagementService
    {
        /// <summary>
        /// Gets all LeaveManagement
        /// </summary>
        Task<IPagedList<LeaveManagement>> GetAllLeaveManagementAsync(string employeeName, DateTime? From, DateTime? To, int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, bool? overridePublished = null, int leaveType = 0, int status = 0, bool showArchived = false);
        /// <summary>
        /// Gets LeaveManagement by Id
        /// </summary>
        /// <param name="leavemanagementId"></param>
        /// <returns></returns>
        Task<LeaveManagement> GetLeaveManagementByIdAsync(int leavemanagementId);
        /// <summary>
        /// Insert LeaveManagement
        /// </summary>
        /// <param name="leaveManagement"></param>
        /// <returns></returns>
        Task InsertLeaveManagementAsync(LeaveManagement leaveManagement);
        /// <summary>
        /// Update LeaveManagement
        /// </summary>
        /// <param name="leaveManagement"></param>
        /// <returns></returns>
        Task UpdateLeaveManagementAsync(LeaveManagement leaveManagement);
        /// <summary>
        /// Delete LeaveManagement
        /// </summary>
        /// <param name="leaveManagement"></param>
        /// <returns></returns>
        Task DeleteLeaveManagementAsync(LeaveManagement leaveManagement);
        /// <summary>
        /// Get LeaveManagements by ids
        /// </summary>
        /// <param name="leavemanagementIds"></param>
        /// <returns></returns>
        Task<IList<LeaveManagement>> GetLeaveManagementsByIdsAsync(int[] leavemanagementIds);
        Task<IList<int>> GetLeaveManagementsByEmployeeNameAsync(string employeename);




        Task<IPagedList<LeaveManagement>> GetLeaveManagementsAsync(int currentemp,
            int leaveTypeId,
            DateTime? fromDate,
            DateTime? toDate,
            int statusId,
            int pageIndex = 0,
            int pageSize = int.MaxValue,
            bool showArchived = false);

        Task<int> GetEmployeeLeaveQuotaAsync(int employeeId);

        Task<LeaveManagement> GetLeaveStatusByNameAsync(int statusName);

        Task<IList<LeaveManagement>> GetLeavesAsync(int employeeId, int year);

        Task<IEnumerable<LeaveManagement>> GetApprovedLeavesForCurrentYearAndEmployeeAsync(int employeeId);

        Task<IEnumerable<LeaveManagement>> GetApprovedLeavesForByMonthYearAsync(int employeeId, int monthNumber, int year);
        Task<IEnumerable<LeaveManagement>> GetApprovedTakenLeavesForCurrentYearAndEmployeeAsync(int employeeId, int leaveTypeId, bool showArchived = false);

        Task<decimal> GetDifferenceByFromToAsync(DateTime from, DateTime to);

        Task<bool> IsLeaveAlreadyTaken(int employeeId, DateTime from, DateTime to, int? excludeLeaveId = null);

        Task ExecuteLeaveBalanceCalculation();

        Task<List<(DateTime Date, string EmployeeList)>> GetEmployeeOnLeaveList();
    }
}




