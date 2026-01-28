using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App.Core;
using App.Core.Domain.Employees;
using App.Core.Domain.Extension.Leaves;
using App.Core.Domain.Holidays;
using App.Core.Domain.Leaves;
using App.Data;
using App.Data.Extensions;
using App.Services.EmployeeAttendances;
using App.Services.Helpers;
using App.Services.Holidays;
using Azure.Storage.Blobs.Models;
using DocumentFormat.OpenXml.Office.CustomUI;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using QuestPDF.Helpers;
using StackExchange.Redis;
using static System.Runtime.InteropServices.JavaScript.JSType;


namespace App.Services.Leaves
{
    /// <summary>
    /// Leavetype service
    /// </summary>
    public partial class LeaveManagementService : ILeaveManagementService
    {
        #region Fields

        private readonly IRepository<LeaveManagement> _leaveManagementRepository;
        private readonly IRepository<Employee> _employeeRepository;
        private readonly IRepository<Leave> _leaveRepository;
        private readonly IEmployeeAttendanceService _employeeAttendanceService;
        private readonly IHolidayService _holidayService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ILeaveTypeService _leaveTypeService;
        private readonly ILeaveTransactionLogService _leaveTransactionLogService;
        private readonly LeaveSettings _leaveSettings;

        #endregion

        #region Ctor

        public LeaveManagementService(IRepository<LeaveManagement> leaveManagementRepository,
            IRepository<Employee> employeeRepository,
            IRepository<Leave> leaveRepository,
            IEmployeeAttendanceService employeeAttendanceService,
            IHolidayService holidayService,
            IDateTimeHelper dateTimeHelper,
            ILeaveTypeService leaveTypeService,
            ILeaveTransactionLogService leaveTransactionLogService,
            LeaveSettings leaveSettings

           )
        {
            _leaveManagementRepository = leaveManagementRepository;
            _employeeRepository= employeeRepository;
            _leaveRepository = leaveRepository;
            _employeeAttendanceService = employeeAttendanceService;
            _holidayService = holidayService;
            _dateTimeHelper = dateTimeHelper;
            _leaveTypeService = leaveTypeService;
            _leaveTransactionLogService = leaveTransactionLogService;
            _leaveSettings = leaveSettings;
        }

        #endregion

        #region Utilities
        protected async Task<List<string>> GetEmployeesOnLeave(DateTime date)
        {
            var employeeOnLeaveIds = await _leaveManagementRepository.Table
                .Where(lm => lm.From.Date <= date.Date && lm.To.Date >= date.Date && lm.StatusId == 2 && lm.IsArchived ==false)
                .Select(lm => lm.EmployeeId)
                .ToListAsync();

            if (!employeeOnLeaveIds.Any())
                return new List<string>();

            return await _employeeRepository.Table
                .Where(e => employeeOnLeaveIds.Contains(e.Id))
                .Select(e => e.FirstName + " " + e.LastName)
                .ToListAsync();
        }
        #endregion

        #region Methods

        public virtual async Task<IPagedList<LeaveManagement>> GetAllLeaveManagementAsync(string employeeName, DateTime? From, DateTime? To, int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, bool? overridePublished = null,int leaveType = 0,int status=0 , bool showArchived =false)
            {
            var data = GetLeaveManagementsByEmployeeNameAsync(employeeName);
            var query = await _leaveManagementRepository.GetAllAsync(async query =>
            {
                if (!string.IsNullOrWhiteSpace(employeeName))
                    query = query.Where(c => c.EmployeeId == data.Result.FirstOrDefault());
                if (From.HasValue)
                    query = query.Where(pr => From.Value<= pr.From);
                if (To.HasValue)
                    query = query.Where(pr => To.Value >= pr.To);
                if (showArchived == false)
                    query = query.Where(pr => pr.IsArchived == false);

                if (leaveType > 0)
                    query = query.Where(c => c.LeaveTypeId == leaveType);
                if(status>0)
                    query=query.Where(c=>c.StatusId == status);
         


                return query.OrderByDescending(c => c.CreatedOnUTC);
            });
            //paging
            return new PagedList<LeaveManagement>(query.ToList(), pageIndex, pageSize);
        }

        public virtual async Task<LeaveManagement> GetLeaveManagementByIdAsync(int leavemanagementId)
        {
            return await _leaveManagementRepository.GetByIdAsync(leavemanagementId);
        }

        public virtual async Task<IList<LeaveManagement>> GetLeaveManagementsByIdsAsync(int[] leaveManagementIds)
        {
            return await _leaveManagementRepository.GetByIdsAsync(leaveManagementIds);
        }

        public virtual async Task<IList<int>> GetLeaveManagementsByEmployeeNameAsync(string employeename)
        {
            var querys = (from t1 in _employeeRepository.Table
                         join t2 in _leaveManagementRepository.Table
                         on t1.Id equals t2.EmployeeId
                         where t1.FirstName.Contains(employeename) || t1.LastName.Contains(employeename) || ($"{t1.FirstName} {t1.LastName}").Contains(employeename)
                         select t1.Id).Distinct().ToList();
            return querys;
        }
        public virtual async Task InsertLeaveManagementAsync(LeaveManagement leaveManagement)
        {
            await _leaveManagementRepository.InsertAsync(leaveManagement);

        }

        public virtual async Task UpdateLeaveManagementAsync(LeaveManagement leaveManagement)
        {
            if (leaveManagement == null)
                throw new ArgumentNullException(nameof(leaveManagement));

            await _leaveManagementRepository.UpdateAsync(leaveManagement);
        }

        public virtual async Task DeleteLeaveManagementAsync(LeaveManagement leaveManagement)
        {
            if(leaveManagement != null)
            {
                await _employeeAttendanceService.DeleteEmployeeAttendanceBasedOnLeaveAsync(leaveManagement);
            }

            await _leaveManagementRepository.DeleteAsync(leaveManagement, false);


        }


        public async Task<IPagedList<LeaveManagement>> GetLeaveManagementsAsync(int currentemp,
       int leaveTypeId,
       DateTime? fromDate,
       DateTime? toDate,
       int statusId,
       int pageIndex = 0,
       int pageSize = int.MaxValue,
       bool showArchived=false)
        {
            var query = _leaveManagementRepository.Table;
            if(currentemp != -1)
            query = query.Where(lm => lm.EmployeeId == currentemp);

            if (leaveTypeId > 0)
                query = query.Where(lm => lm.LeaveTypeId == leaveTypeId);

            if (statusId > 0)
                query = query.Where(lm => lm.StatusId == statusId);
            if(showArchived == false)
                query = query.Where(lm=> lm.IsArchived == false);


            if (fromDate.HasValue && toDate.HasValue)
            {
                query = query.Where(lm => lm.From <= toDate.Value.Date && lm.To >= fromDate.Value.Date);
            }
            else if (fromDate.HasValue)
            {
                query = query.Where(lm => lm.From >= fromDate.Value.Date);
            }
            else if (toDate.HasValue)
            {
                query = query.Where(lm => lm.To <= toDate.Value.Date);
            }

            query = query.OrderBy(lm => lm.From); 

            return await query.ToPagedListAsync(pageIndex, pageSize);
        }


        public async Task<IList<LeaveManagement>> GetLeavesAsync(int employeeId,int year)
        {
            var leaves = await _leaveManagementRepository.Table
                .Where(lm => lm.EmployeeId == employeeId && lm.From.Year == year)
                .ToListAsync();
            return leaves;
        }
        public async Task<int> GetEmployeeLeaveQuotaAsync(int employeeId)
        {
            // Assuming there's a LeaveQuota table that holds the quota for each employee
            var leaveQuota = await _leaveRepository.Table
                .Where(lq => lq.Id == employeeId)
                .Select(lq => lq.Total_Allowed)
                .FirstOrDefaultAsync();

            return (int)leaveQuota;
        }
        public async Task<LeaveManagement> GetLeaveStatusByNameAsync(int statusName)
        {
           
            return await _leaveManagementRepository.Table.FirstOrDefaultAsync(s => s.StatusId == statusName);
        }

        public async Task<IEnumerable<LeaveManagement>> GetApprovedLeavesForCurrentYearAndEmployeeAsync(int employeeId)
        {
            var date = await _dateTimeHelper.GetUTCAsync();

            var currentYear = date.Year;
            var startDate = new DateTime(currentYear, 1, 1).AddDays(-1);
            var endDate = new DateTime(currentYear, 12, 31).AddDays(1);

            var query = _leaveManagementRepository.Table
                .Where(lm => lm.EmployeeId == employeeId)
                .Where(lm => lm.From >= startDate && lm.To <= endDate)
                .Where(lm => lm.StatusId == (int)StatusEnum.Approved);

            return await query.ToListAsync();
        }

        public async Task<IEnumerable<LeaveManagement>> GetApprovedLeavesForByMonthYearAsync(int employeeId, int monthNumber,int year)
        {
          
 
            // Get first and last day of the previous month
            var startDate = new DateTime(year, monthNumber, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1); // Last day of previous month

            var query = _leaveManagementRepository.Table
                .Where(lm => lm.EmployeeId == employeeId && lm.IsArchived ==false)
                .Where(lm => lm.From >= startDate && lm.To <= endDate)
                .Where(lm => lm.StatusId == (int)StatusEnum.Approved);

            return await query.ToListAsync();
        }


        public async Task<IEnumerable<LeaveManagement>> GetApprovedTakenLeavesForCurrentYearAndEmployeeAsync(int employeeId, int leaveTypeId,bool showArchived = false)
        {
            var date = await _dateTimeHelper.GetIndianTimeAsync();
            var currentYear = date.Year;
            var query = _leaveManagementRepository.Table
                .Where(l => l.EmployeeId == employeeId && l.LeaveTypeId == leaveTypeId  && l.StatusId == (int)StatusEnum.Approved);
            if (!showArchived)
                query = query.Where(lm => lm.IsArchived == false);

            return await query.ToListAsync();
        }

        public async Task<decimal> GetDifferenceByFromToAsync(DateTime from, DateTime to)
        {
            var allHolidays = await _holidayService.GetAllHolidaysAsync(); // Assuming this returns a list of holiday dates
            decimal totalDays = 0;

            // Iterate from 'from' date to 'to' date
            for (var date = from.Date; date <= to.Date; date = date.AddDays(1))
            {
                // Check if it's a weekend or a holiday
                bool isWeekend = date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday;
                bool isHoliday = allHolidays.Any(h => h.Date == date);

                // Only count the day if it’s neither a weekend nor a holiday
                if (!isWeekend && !isHoliday)
                {
                    totalDays++;
                }
            }

            return totalDays;
        }


        public async Task<bool> IsLeaveAlreadyTaken(int employeeId, DateTime from, DateTime to, int? excludeLeaveId = null)
        {
            // Fetch the leaves for the specified employee within the given range
            var leaves = await GetLeaveManagementsAsync(employeeId, 0, from, to, 0);

            foreach (var leave in leaves)
            {
                // Exclude the record being edited 
                if (excludeLeaveId.HasValue && leave.Id == excludeLeaveId.Value)
                    continue;

                // Check for overlapping leave dates
                if ((from >= leave.From && from <= leave.To) ||
                    (to >= leave.From && to <= leave.To) ||
                    (from <= leave.From && to >= leave.To))
                {
                    return true;
                }
            }

            return false;
        }

        public virtual async Task ExecuteLeaveBalanceCalculation()
        {
            var allEmployees = await _employeeRepository.GetAllAsync(query =>
                query.Where(p => p.EmployeeStatusId == 1));

            var currTime = await _dateTimeHelper.GetIndianTimeAsync();

            if (_leaveSettings.LeaveTestDate != DateTime.MinValue)
                currTime = _leaveSettings.LeaveTestDate;

            int prevMonthNumber = currTime.Month == 1 ? 12 : currTime.Month - 1;
            int prevYear = currTime.Month == 1 ? currTime.Year - 1 : currTime.Year;
            DateTime previousMonthStart = new DateTime(prevYear, prevMonthNumber, 1);

            var allLeaveTypes = await _leaveTypeService.GetAllLeaveTypeAsync("");

            foreach (var employee in allEmployees)
            {
                var approvedLeaves = await GetApprovedLeavesForByMonthYearAsync(employee.Id, prevMonthNumber, prevYear);

                foreach (var leaveType in allLeaveTypes)
                {
                    decimal totalBalanceToDeduct = 0;

                    if (approvedLeaves != null)
                    {
                        totalBalanceToDeduct = approvedLeaves
                            .Where(leave => leave.LeaveTypeId == leaveType.Id)
                            .Sum(leave => leave.NoOfDays);
                    }

                    decimal balanceChange = -totalBalanceToDeduct;

                    // Retrieve the last leave balance log
                    //var lastLog = await _leaveTransactionLogService.GetLeaveBalanceByLog(employee.Id, leaveType.Id);
                    var lastLog = await _leaveTransactionLogService.GetLeaveBalanceByLogForPreviousMonth(employee.Id, leaveType.Id, currTime.Month,currTime.Year);

                    
                    decimal previousBalance =await _leaveTransactionLogService.GetAddedLeaveBalanceForCurrentMonth(employee.Id,leaveType.Id,prevMonthNumber,prevYear); // Use the last recorded balance

                    // Calculate new balance based on previous balance and current deductions
                    decimal newLeaveBalance = previousBalance + balanceChange;

                    if (lastLog != null && lastLog.BalanceMonthYear >= previousMonthStart)
                    {
                        //if (!lastLog.Comment.Contains("Monthly Leave Added"))
                        //{
                            // Update existing log for the previous month
                            if (lastLog.LeaveBalance != newLeaveBalance)
                            {
                                lastLog.LeaveBalance = newLeaveBalance;
                                lastLog.BalanceChange = balanceChange;
                                lastLog.Comment = $"Updated Leave Balance Deduction from {previousBalance} to {newLeaveBalance}";

                                await _leaveTransactionLogService.UpdateLeaveTransactionLogAsync(lastLog);
                            }
                        //}
                        //else
                        //{
                        //    decimal reAddMonthlyLeave = 1;
                        //    if (balanceChange != 0)
                        //        await _leaveTransactionLogService.AddLeaveTransactionLogAsync(
                        //      employee.Id,
                        //      0,
                        //      0,
                        //      leaveType.Id,
                        //      newLeaveBalance+ reAddMonthlyLeave, // Use calculated balance including previous log with +1 monthly added
                        //      balanceChange+ reAddMonthlyLeave,
                        //      "Leave Balance Deduction", false, lastLog.BalanceMonthYear
                        //  );
                        //}
                    }
                    else
                    {
                        // If no log exists or last log is older, create a new one
                        if (balanceChange != 0) // Only create if deduction is necessary
                        {
                            await _leaveTransactionLogService.AddLeaveTransactionLogAsync(
                                employee.Id,
                                0,
                                0,
                                leaveType.Id,
                                newLeaveBalance, 
                                balanceChange,
                                "Leave Balance Deduction"
                            );
                        }
                    }
                }
            }
        }


        public virtual async Task<List<(DateTime Date, string EmployeeList)>> GetEmployeeOnLeaveList()
        {
            var todayDate = await _dateTimeHelper.GetIndianTimeAsync();
            var allHolidays = await _holidayService.GetAllHolidaysAsync();

            // 1. Get employees on leave today
            var todayLeaveEmployees = await GetEmployeesOnLeave(todayDate);

            // 2. Find the next working day
            DateTime nextDate = todayDate.AddDays(1);
            while (nextDate.DayOfWeek == DayOfWeek.Saturday ||
                   nextDate.DayOfWeek == DayOfWeek.Sunday ||
                   allHolidays.Any(h => h.Date == nextDate.Date))
            {
                nextDate = nextDate.AddDays(1);
            }

            // 3. Get employees on leave on the next working day
            var nextDayLeaveEmployees = await GetEmployeesOnLeave(nextDate);

            // 4. Format and return the results
            return new List<(DateTime Date, string EmployeeList)>
    {
        (todayDate.Date, todayLeaveEmployees.Any() ? string.Join(",  ", todayLeaveEmployees) : ""),
        (nextDate.Date, nextDayLeaveEmployees.Any() ? string.Join(",  ", nextDayLeaveEmployees) : "")
    };
        }

        #endregion
    }
}