using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using App.Core;
using App.Core.Domain.Employees;
using App.Core.Domain.Extension.Leaves;
using App.Core.Domain.Leaves;
using App.Core.Domain.PerformanceMeasurements;
using App.Data;
using App.Data.Extensions;
using App.Services.EmployeeAttendances;
using App.Services.Helpers;
using App.Services.Holidays;
using App.Services.Localization;
using Azure.Storage.Blobs.Models;
using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using Humanizer;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using StackExchange.Redis;
using static LinqToDB.Sql;
namespace App.Services.Leaves
{
    /// <summary>
    /// Leavetype service
    /// </summary>
    public partial class LeaveTransactionLogService : ILeaveTransactionLogService
    {
        #region Fields

        private readonly IRepository<LeaveManagement> _leaveManagementRepository;
        private readonly IRepository<Employee> _employeeRepository;
        private readonly IRepository<LeaveTransactionLog> _leaveTransactionLogRepository;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly LeaveSettings _leaveSettings;
        private readonly ILeaveTypeService _leaveTypeService;
        private readonly ILocalizationService _localizationService;


        #endregion

        #region Ctor

        public LeaveTransactionLogService(IRepository<LeaveManagement> leaveManagementRepository,
            IRepository<Employee> employeeRepository,
            IRepository<LeaveTransactionLog> leaveTransactionLogRepository,
            IDateTimeHelper dateTimeHelper,
            LeaveSettings leaveSettings,
            ILeaveTypeService leaveTypeService,
            ILocalizationService localizationService


           )
        {
            _leaveManagementRepository = leaveManagementRepository;
            _employeeRepository= employeeRepository;
            _leaveTransactionLogRepository = leaveTransactionLogRepository;
            _dateTimeHelper = dateTimeHelper;
            _leaveSettings = leaveSettings;
            _leaveTypeService = leaveTypeService;
            _localizationService = localizationService;

         
        }

        #endregion


        #region Utilities
        private async Task<IList<int>> GetLeaveLogByEmployeeNameAsync(string employeename)
        {
            var querys = (from t1 in _employeeRepository.Table
                          join t2 in _leaveTransactionLogRepository.Table
                          on t1.Id equals t2.EmployeeId
                          where t1.FirstName.Contains(employeename) || t1.LastName.Contains(employeename) || ($"{t1.FirstName} {t1.LastName}").Contains(employeename)
                          select t1.Id).Distinct().ToList();
            return querys;
        }
        private  async Task<IEnumerable<int>> GetAllEmployeeIdsAsync()
        {
            var projects = await GetAllEmployeesAsync();
            return projects.Where(p => p.EmployeeStatusId == 1).Select(p => p.Id);
        }
        private async Task<IPagedList<Employee>> GetAllEmployeesAsync(int storeId = 0, int languageId = 0,
          DateTime? dateFrom = null, DateTime? dateTo = null,
          int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, string employee = null, bool showInActive = false)
        {
            return await _employeeRepository.GetAllPagedAsync(async query =>
            {

                if (!string.IsNullOrEmpty(employee))
                    query = query.Where(b => b.FirstName.Contains(employee.Trim()) || b.LastName.Contains(employee.Trim()) || ($"{b.FirstName} {b.LastName}").Contains(employee.Trim()) || ($"{b.LastName} {b.FirstName}").Contains(employee.Trim()));
                if (showInActive == false)
                {
                    query = query.Where(p => p.EmployeeStatusId == 1);
                }
                return query;
            }, pageIndex, pageSize);
        }
        #endregion

        #region Methods

        public virtual async Task<IPagedList<LeaveTransactionLog>> GetAllLeaveTransactionLogAsync(string employeeName,int leaveId ,DateTime? From, DateTime? To,int leaveTypeId, int statusId, string comment, int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, bool? overridePublished = null , bool showArchived =false)
            {
            var data = GetLeaveLogByEmployeeNameAsync(employeeName);

            var query = await _leaveTransactionLogRepository.GetAllAsync(async query =>
            {
                if(!string.IsNullOrWhiteSpace(employeeName))
                    query = query.Where(c => c.EmployeeId == data.Result.FirstOrDefault());
                if (leaveTypeId !=0)
                    query = query.Where(c => c.ApprovedId == leaveTypeId);

                if (From.HasValue)
                    query = query.Where(pr => From.Value.Date<= pr.CreatedOnUTC.Date);
                if (To.HasValue)
                    query = query.Where(pr => To.Value.Date >= pr.CreatedOnUTC.Date);
                if(comment != null)
                    query = query.Where(pr=>pr.Comment.Contains(comment));

                if(leaveId != 0)
                    query = query.Where(pr=>pr.LeaveId == leaveId);
            
                
                if(statusId != 0)
                    query=query.Where(c=>c.StatusId == statusId);
         


                return query.OrderByDescending(c => c.CreatedOnUTC);
            });
            //paging
            return new PagedList<LeaveTransactionLog>(query.ToList(), pageIndex, pageSize);
        }

        public virtual async Task<LeaveTransactionLog> GetLeaveTransactionLogByIdAsync(int leavetransactionlogId)
        {
            return await _leaveTransactionLogRepository.GetByIdAsync(leavetransactionlogId);
        }

        public virtual async Task AddLeaveTransactionLogAsync(int EmployeeId, int LeaveId, int StatusId , int LeaveTypeId , decimal LeaveBalance, decimal BalanceChange , string Comments ,bool isReset = false, DateTime? balanceMonthYear = null)
        {

            // If balanceMonthYear is not provided, assume the current month
            balanceMonthYear ??= new DateTime((await _dateTimeHelper.GetIndianTimeAsync()).Year,
                                              (await _dateTimeHelper.GetIndianTimeAsync()).Month, 1);

            LeaveTransactionLog leaveTransactionLog = new LeaveTransactionLog();
            leaveTransactionLog.EmployeeId = EmployeeId;
            leaveTransactionLog.LeaveId = LeaveId;
            leaveTransactionLog.StatusId = StatusId;
            //leaveTransactionLog.LeaveBalance = LeaveBalance;
            leaveTransactionLog.BalanceChange=BalanceChange;
            leaveTransactionLog.Comment = Comments;
            leaveTransactionLog.ApprovedId = LeaveTypeId;
            leaveTransactionLog.BalanceMonthYear = balanceMonthYear.Value;
            leaveTransactionLog.CreatedOnUTC = await _dateTimeHelper.GetUTCAsync(); 


            var leaveLog =   await GetLeaveBalanceByLog(EmployeeId, LeaveTypeId);
            

            if (leaveLog != null)
            {
               decimal leaveBalance = leaveLog.LeaveBalance;
                leaveBalance += BalanceChange;
                leaveTransactionLog.LeaveBalance += leaveBalance;


                if (isReset && BalanceChange == 0)
                {
                    leaveTransactionLog.LeaveBalance = 0;
                }
                else if (isReset && BalanceChange != 0)
                {
                    leaveTransactionLog.LeaveBalance = BalanceChange;
                }

               await _leaveTransactionLogRepository.InsertAsync(leaveTransactionLog);
            }
            else
            {
                LeaveTransactionLog newLeaveTransactionLog = new LeaveTransactionLog();
                newLeaveTransactionLog.EmployeeId = EmployeeId;
                newLeaveTransactionLog.LeaveId = LeaveId;
                newLeaveTransactionLog.StatusId = StatusId;
                newLeaveTransactionLog.LeaveBalance = BalanceChange;
                newLeaveTransactionLog.BalanceChange = BalanceChange;
                newLeaveTransactionLog.Comment = Comments;
                newLeaveTransactionLog.ApprovedId = LeaveTypeId;
                newLeaveTransactionLog.BalanceMonthYear = balanceMonthYear.Value;

                newLeaveTransactionLog.CreatedOnUTC = await _dateTimeHelper.GetUTCAsync();

                if (isReset && BalanceChange ==0 )
                { 
                    newLeaveTransactionLog.LeaveBalance = 0;
                }
                else if(isReset &&  BalanceChange != 0)
                {
                    newLeaveTransactionLog.LeaveBalance = BalanceChange;
                }

              await  _leaveTransactionLogRepository.InsertAsync(newLeaveTransactionLog);
            }

        }

        public virtual async Task<LeaveTransactionLog> GetLeaveBalanceByLog(int employeeId, int leaveTypeId)
        {

            var query = await _leaveTransactionLogRepository.GetAllAsync(async query =>
            {
                if (employeeId != 0)
                    query = query.Where(c => c.EmployeeId == employeeId);
             
                if (leaveTypeId != 0)
                    query = query.Where(pr => pr.ApprovedId == leaveTypeId);


                return query.OrderByDescending(c => c.CreatedOnUTC);
            });

            return query.FirstOrDefault();
        }

        public virtual async Task<LeaveTransactionLog> GetLeaveBalanceByLogForPreviousMonth(int employeeId, int leaveTypeId, int month, int year)
        {

            // Calculate the previous month
            int prevMonth = month == 1 ? 12 : month - 1;
            int prevYear = month == 1 ? year - 1 : year;
            DateTime prevMonthYear = new DateTime(prevYear, prevMonth, 1);
            DateTime prevMonthStart = new DateTime(prevYear, prevMonth, 1);
            DateTime prevMonthEnd = prevMonthStart.AddMonths(1).AddSeconds(-1); // Last second of previous month

            // Try to get January's balance first when in February
            if (month == 1)
            {
                DateTime janMonthYear = new DateTime(year, 1, 1);
                var janLog = await _leaveTransactionLogRepository.GetAllAsync(async query =>
                {
                    if (employeeId != 0)
                        query = query.Where(c => c.EmployeeId == employeeId);

                    if (leaveTypeId != 0)
                        query = query.Where(pr => pr.ApprovedId == leaveTypeId);

                    query = query.Where(c => c.BalanceMonthYear == janMonthYear);

                    return query.OrderByDescending(c => c.CreatedOnUTC);
                });

                var janLeaveLog = janLog.FirstOrDefault();
                if (janLeaveLog != null)
                    return janLeaveLog; // If January is available, return it
            }


            var query = await _leaveTransactionLogRepository.GetAllAsync(async query =>
            {
                if (employeeId != 0)
                    query = query.Where(c => c.EmployeeId == employeeId);

                if (leaveTypeId != 0)
                    query = query.Where(pr => pr.ApprovedId == leaveTypeId);

                // Filter for logs created in the previous month
                query = query.Where(c => c.BalanceMonthYear == prevMonthYear);

                return query.OrderByDescending(c => c.CreatedOnUTC);
            });

            return query.FirstOrDefault(); // Return the latest log from the previous month
        }

        public virtual async Task<LeaveTransactionLog> GetLeaveBalanceByLogForCurrentMonth(int employeeId, int leaveTypeId, int month, int year)
        {
            // Calculate the previous month
 
            DateTime monthYear = new DateTime(year, month, 1);
            
            var query = await _leaveTransactionLogRepository.GetAllAsync(async query =>
            {
                if (employeeId != 0)
                    query = query.Where(c => c.EmployeeId == employeeId);

                if (leaveTypeId != 0)
                    query = query.Where(pr => pr.ApprovedId == leaveTypeId);

                query = query.Where(c => c.BalanceMonthYear == monthYear);

                return query.OrderByDescending(c => c.CreatedOnUTC);
            });

            return query.FirstOrDefault(); // Return the latest log from the previous month
        }

        //public virtual async Task<decimal> GetAddedLeaveBalanceForCurrentMonth(int employeeId, int leaveTypeId, int month, int year)
        //{
        //    // Get previous month's leave balance log
        //    var prevMonthLog = await GetLeaveBalanceByLogForPreviousMonth(employeeId, leaveTypeId, month, year);

        //    if (_leaveSettings.SeletedLeaveTypeId == leaveTypeId)
        //    {
        //        if (prevMonthLog == null)
        //        {

        //            decimal leaveBalance = 0;

        //            leaveBalance += 1;//add monthly leave

        //            return leaveBalance < 0 ? 0 : leaveBalance;
        //        }
        //        else
        //        {

        //            decimal leaveBalance = prevMonthLog.LeaveBalance;

        //            leaveBalance += 1;//add monthly leave
        //            return leaveBalance < 0 ? 0 : leaveBalance;

        //        }
        //    }
        //    else
        //    {
        //        if (prevMonthLog == null)
        //        {
        //            // Set leave balance for next month
        //            DateTime nextMonthStart = new DateTime(year, month, 1);

        //            decimal leaveBalance = 0;
        //            var leaveType = await _leaveTypeService.GetLeaveTypeByIdAsync(leaveTypeId);
        //            if (leaveType != null)
        //                leaveBalance += leaveType.Total_Allowed;//add monthly leave

        //            return leaveBalance < 0 ? 0 : leaveBalance;
        //        }
        //        else
        //        {

        //            decimal leaveBalance = prevMonthLog.LeaveBalance;

        //            return leaveBalance < 0 ? 0 : leaveBalance;

        //        }
        //    }
        //    return 0;
        //}

        //public virtual async Task<decimal> GetAddedLeaveBalanceForCurrentMonth(int employeeId, int leaveTypeId, int month, int year)
        //{
        //    // Get the previous month's leave balance log
        //    var prevMonthLog = await GetLeaveBalanceByLogForPreviousMonth(employeeId, leaveTypeId, month, year);
        //    decimal leaveBalance = prevMonthLog?.LeaveBalance ?? 0; // Use previous balance, default to 0

        //    if (_leaveSettings.SeletedLeaveTypeId == leaveTypeId)
        //    {
        //        // Ensure "Monthly Leave Added" is applied
        //        if (prevMonthLog == null)
        //        {
        //            leaveBalance += 1; // First-time addition
        //        }
        //        else if (!prevMonthLog.Comment.Contains("Monthly Leave Added"))
        //        {
        //            leaveBalance += 1; // Ensure it's added if missing
        //        }
        //    }
        //    else
        //    {
        //        // If this is not the selected leave type, check if monthly leave was missing before
        //        if (prevMonthLog == null)
        //        {
        //            var leaveType = await _leaveTypeService.GetLeaveTypeByIdAsync(leaveTypeId);
        //            leaveBalance += leaveType?.Total_Allowed ?? 0; // Add standard leave
        //        }
        //        else if (!prevMonthLog.Comment.Contains("Monthly Leave Added"))
        //        {
        //            var leaveType = await _leaveTypeService.GetLeaveTypeByIdAsync(leaveTypeId);
        //            leaveBalance += leaveType?.Total_Allowed ?? 0; // Fix missing monthly leave
        //        }
        //    }

        //    return leaveBalance < 0 ? 0 : leaveBalance; // Ensure balance is never negative
        //}
        public virtual async Task<decimal> GetAddedLeaveBalanceForCurrentMonth(int employeeId, int leaveTypeId, int month, int year)
        {
            // Get previous month's leave balance log
            var prevMonthLog = await GetLeaveBalanceByLogForPreviousMonth(employeeId, leaveTypeId, month, year);
            var currMonth = await GetLeaveBalanceByLogForCurrentMonth(employeeId, leaveTypeId, month, year);

            if (prevMonthLog !=null)
            if (month == 1 && prevMonthLog.ApprovedId == _leaveSettings.SeletedLeaveTypeId)
                prevMonthLog.LeaveBalance = 0;

            decimal leaveBalance = prevMonthLog?.LeaveBalance ?? 0;

            // Handle negative balance first
            if (leaveBalance < 0)
            {
                leaveBalance = 0; // Reset negative balance
            }

            // Add Monthly Leave
            if (_leaveSettings.SeletedLeaveTypeId == leaveTypeId)
            {
                leaveBalance += 1;
                
                //else
                //{
                //    leaveBalance += 1;
                //}
            }
            else
            {
                //var leaveType = await _leaveTypeService.GetLeaveTypeByIdAsync(leaveTypeId);
                //if (leaveType != null)
                //{
                //    leaveBalance += leaveType.Total_Allowed;
                //}
            }
            if (currMonth != null)
            {
                var diffinbalance = currMonth.ManualBalanceChange;
                leaveBalance += diffinbalance;
            }

            return leaveBalance;
        }


        public virtual async Task<decimal> GetAddedLeaveBalanceForCurrentMonthForReport(int employeeId, int leaveTypeId, int month, int year)
        {
            // Get previous month's leave balance log
            var prevMonthLog = await GetLeaveBalanceByLogForPreviousMonth(employeeId, leaveTypeId, month, year);

          
            if (_leaveSettings.SeletedLeaveTypeId == leaveTypeId)
            {
                if (prevMonthLog == null)
                {

                    decimal leaveBalance = 0;

                    leaveBalance += 1;//add monthly leave

                    return leaveBalance;
                }
                else
                {

                    decimal leaveBalance = prevMonthLog.LeaveBalance;

                    //leaveBalance += 1;//add monthly leave
                    return leaveBalance;

                }
            }
            else
            {
                if (prevMonthLog == null)
                {
                    // Set leave balance for next month
                    DateTime nextMonthStart = new DateTime(year, month, 1);

                    decimal leaveBalance = 0;
                    var leaveType = await _leaveTypeService.GetLeaveTypeByIdAsync(leaveTypeId);
                    if (leaveType != null)
                        leaveBalance += leaveType.Total_Allowed;//add monthly leave

                    return leaveBalance;
                }
                else
                {

                    decimal leaveBalance = prevMonthLog.LeaveBalance;

                    return leaveBalance;

                }
            }
            return 0;
        }

        //public virtual async Task<decimal> GetAddedLeaveBalanceForCurrentMonthForReport(int employeeId, int leaveTypeId, int month, int year)
        //{
        //    // Get the last leave balance log for the previous month
        //    var prevMonthLog = await GetLeaveBalanceByLogForPreviousMonth(employeeId, leaveTypeId, month, year);

        //    decimal leaveBalance = prevMonthLog?.LeaveBalance ?? 0; // Use last recorded balance if available

        //    var leaveType = await _leaveTypeService.GetLeaveTypeByIdAsync(leaveTypeId);
        //    if (leaveType == null)
        //        return leaveBalance; // Return existing balance if leave type is invalid

        //    if (_leaveSettings.SeletedLeaveTypeId == leaveTypeId)
        //    {
        //        // Add monthly leave only if the previous log exists
        //        if (prevMonthLog == null)
        //        {
        //            leaveBalance += 1; // Add default monthly leave
        //        }
        //    }
        //    else
        //    {
        //        // If leave type is different, use its allowed leave per month
        //        if (prevMonthLog == null)
        //        {
        //            leaveBalance += leaveType.Total_Allowed; // Add allowed monthly leave for this type
        //        }
        //    }

        //    return leaveBalance < 0 ? 0 : leaveBalance; // Ensure balance is not negative
        //}


        //public virtual async Task<decimal> GetAddedLeaveBalanceForCurrentMonth(int employeeId, int leaveTypeId, int month, int year)
        //{
        //    // Convert month and year to BalanceMonthYear format
        //    DateTime balanceMonthYear = new DateTime(year, month, 1);

        //    // Check if leave balance is already recorded for this month
        //    var existingLogForCurrentMonth = await GetLeaveBalanceByLogForCurrentMonth(employeeId, leaveTypeId, month, year);
        //    if (existingLogForCurrentMonth != null)
        //    {
        //        // If an entry exists for this month, return its balance (to prevent duplicate increments)
        //        return existingLogForCurrentMonth.LeaveBalance;
        //    }

        //    // Get previous month's leave balance
        //    var prevMonthLog = await GetLeaveBalanceByLogForPreviousMonth(employeeId, leaveTypeId, month, year);
        //    decimal leaveBalance = prevMonthLog?.LeaveBalance ?? 0; // If no previous log, start from 0

        //    // Only add leave if no log exists for this month
        //    var leaveType = await _leaveTypeService.GetLeaveTypeByIdAsync(leaveTypeId);
        //    if (leaveType != null)
        //    {
        //        leaveBalance += (leaveType.Id == _leaveSettings.SeletedLeaveTypeId) ? 1 : leaveType.Total_Allowed;
        //    }

        //    return leaveBalance < 0 ? 0 : leaveBalance; // Ensure balance never goes negative
        //}

        public virtual async Task<IList<LeaveTransactionLog>> GetLeaveTrnasactionLogsByIdsAsync(int[] leaveTransactionIds)
        {
            return await _leaveTransactionLogRepository.GetByIdsAsync(leaveTransactionIds);
        }

        //public virtual async Task<IList<int>> GetLeaveTransactionLogByEmployeeNameAsync(string employeename)
        //{
        //    var querys = (from t1 in _employeeRepository.Table
        //                 join t2 in _leaveManagementRepository.Table
        //                 on t1.Id equals t2.EmployeeId
        //                 where t1.FirstName.Contains(employeename) || t1.LastName.Contains(employeename) || ($"{t1.FirstName} {t1.LastName}").Contains(employeename)
        //                 select t1.Id).Distinct().ToList();
        //    return querys;
        //}
        public virtual async Task InsertLeaveTransactionLogAsync(LeaveTransactionLog leaveTransationLog)
        {
            await _leaveTransactionLogRepository.InsertAsync(leaveTransationLog);
        }

        public virtual async Task UpdateLeaveTransactionLogAsync(LeaveTransactionLog leaveTransationLog)
        {
            if (leaveTransationLog == null)
                throw new ArgumentNullException(nameof(leaveTransationLog));

            await _leaveTransactionLogRepository.UpdateAsync(leaveTransationLog);
        }

        public virtual async Task DeleteLeaveTransationLogAsync(LeaveTransactionLog leaveTransationLog)
        {
  
            await _leaveTransactionLogRepository.DeleteAsync(leaveTransationLog, false);

        }

        public virtual async Task AddMonthlyLeave(int monthId, int year)
        {
            var employeeIds = await GetAllEmployeeIdsAsync();
            var selectedMonthOption = monthId;
            var monthName = ((MonthEnum)selectedMonthOption).ToString();

            // Selected leave type
            var leaveType = 0;
            if (_leaveSettings.SeletedLeaveTypeId != 0)
                leaveType = _leaveSettings.SeletedLeaveTypeId;

            // Get all leave types
            var leaveTypes = await _leaveTypeService.GetAllLeaveTypeAsync("");

            if (employeeIds != null)
            {
                foreach (var employeeId in employeeIds)
                {
                    foreach (var type in leaveTypes)
                    {
                        //var leaveLog = await _leaveTransactionLogService.GetLeaveBalanceByLog(employeeId, type.Id);
                        var leaveLog = await GetLeaveBalanceByLog(employeeId, type.Id);
                        //var currLog = await GetLeaveBalanceByLogForCurrentMonth(employeeId, type.Id, monthId, year);
	
						
                            if (leaveLog != null)
                            {
                                // Deduct negative balance for all leave types
                                if (leaveLog.LeaveBalance < 0)
                                {
                                    var monthYear = new DateTime(year, monthId, 1);
                                    await AddLeaveTransactionLogAsync(
                                        employeeId,0, 0, type.Id, 0,
                                                                -leaveLog.LeaveBalance,
                                        await _localizationService.GetResourceAsync("Admin.LeaveManagement.LeaveTransactionLogComment.DeductedReset"), false, monthYear);
                                }

                            }


                            // Add monthly leave only for the selected leave type
                            if (type.Id == leaveType)
                            {
                                var currTime = await _dateTimeHelper.GetIndianTimeAsync();
                                var monthYear = new DateTime(year, monthId, 1);
                                if (monthYear.Month == 1)
                                {
                                    await AddLeaveTransactionLogAsync(
                                    employeeId,
                                    0, 0, leaveType, 0,
                                    1,
                                   $"{await _localizationService.GetResourceAsync("admin.leavemanagement.leavetransactionlogcomment.monthlyadded")} - {monthName}", true, monthYear);
                                }
                                else
                                {
                                    await AddLeaveTransactionLogAsync(
                                    employeeId,
                                    0, 0, leaveType, 0,
                                    1,
                                        $"{await _localizationService.GetResourceAsync("admin.leavemanagement.leavetransactionlogcomment.monthlyadded")} - {monthName}", false, monthYear);
                                }
                            }
                            else
                            {
                                //add sick leave record
                                var lastsickleave = await GetLeaveBalanceByLogForPreviousMonth(employeeId, type.Id, monthId, year);
                                if (lastsickleave != null)
                                {
                                    var currTime = await _dateTimeHelper.GetIndianTimeAsync();
                                    var monthYear = new DateTime(year, monthId, 1);
                                    await AddLeaveTransactionLogAsync(
                                    employeeId,
                                    0, 0, type.Id, 0,
                                    0,
                                           $"{await _localizationService.GetResourceAsync("admin.leavemanagement.leavetransactionlogcomment.monthlyadded")} - {monthName} sick leave", false, monthYear);
                                }
                            }

                        
                    }
                }
            }
        }
   

        #endregion
    }
}