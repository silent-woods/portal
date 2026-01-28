
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using App.Core;
using App.Core.Domain.Employees;
using App.Core.Domain.Leaves;
using StackExchange.Redis;

namespace App.Services.Leaves
{
    public partial interface ILeaveTransactionLogService
    {
        /// <summary>
        /// Gets all LeaveManagement
        /// </summary>
        Task<IPagedList<LeaveTransactionLog>> GetAllLeaveTransactionLogAsync(string employeeName, int leaveId, DateTime? From, DateTime? To, int leaveTypeId, int statusId, string comment, int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, bool? overridePublished = null, bool showArchived = false);
        /// <summary>
        /// Gets LeaveManagement by Id
        /// </summary>
        /// <param name="leavemanagementId"></param>
        /// <returns></returns>
        Task<LeaveTransactionLog> GetLeaveTransactionLogByIdAsync(int leavetransactionlogId);
        /// <summary>
        /// Insert LeaveManagement
        /// </summary>
        /// <param name="leaveManagement"></param>
        /// <returns></returns>
        Task InsertLeaveTransactionLogAsync(LeaveTransactionLog leaveTransationLog);
        /// <summary>
        /// Update LeaveManagement
        /// </summary>
        /// <param name="leaveManagement"></param>
        /// <returns></returns>
        Task UpdateLeaveTransactionLogAsync(LeaveTransactionLog leaveTransationLog);
        /// <summary>
        /// Delete LeaveManagement
        /// </summary>
        /// <param name="leaveManagement"></param>
        /// <returns></returns>
        Task DeleteLeaveTransationLogAsync(LeaveTransactionLog leaveTransationLog);
        /// <summary>
        /// Get LeaveManagements by ids
        /// </summary>
        /// <param name="leavemanagementIds"></param>
        /// <returns></returns>
        /// 
        Task<IList<LeaveTransactionLog>> GetLeaveTrnasactionLogsByIdsAsync(int[] leaveTransactionIds);


        Task AddLeaveTransactionLogAsync(int EmployeeId, int LeaveId, int StatusId, int LeaveTypeId, decimal LeaveBalance, decimal BalanceChange, string Comments, bool isReset = false, DateTime? balanceMonthYear = null);

        Task<LeaveTransactionLog> GetLeaveBalanceByLog(int employeeId, int leaveTypeId);
        //Task<IList<int>> GetLeaveManagementsByEmployeeNameAsync(string employeename);

        Task<LeaveTransactionLog> GetLeaveBalanceByLogForPreviousMonth(int employeeId, int leaveTypeId, int month, int year);
        Task<decimal> GetAddedLeaveBalanceForCurrentMonth(int employeeId, int leaveTypeId, int month, int year);

        Task<LeaveTransactionLog> GetLeaveBalanceByLogForCurrentMonth(int employeeId, int leaveTypeId, int month, int year);
        Task<decimal> GetAddedLeaveBalanceForCurrentMonthForReport(int employeeId, int leaveTypeId, int month, int year);

        Task AddMonthlyLeave(int monthId, int year);
    }
}