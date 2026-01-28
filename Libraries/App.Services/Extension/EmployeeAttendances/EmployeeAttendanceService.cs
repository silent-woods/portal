using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App.Core;
using App.Core.Domain.EmployeeAttendances;
using App.Core.Domain.Employees;
using App.Core.Domain.Extension.EmployeeAttendanceSetting;
using App.Core.Domain.Extension.TimeSheets;
using App.Core.Domain.Leaves;
using App.Core.Domain.PerformanceMeasurements;
using App.Core.Domain.TimeSheets;
using App.Data;
using App.Data.Extensions;
using App.Services.Employees;
using App.Services.Helpers;
using App.Services.Holidays;
using App.Services.TimeSheets;
using DocumentFormat.OpenXml.Drawing;

namespace App.Services.EmployeeAttendances
{
    /// <summary>
    /// EmployeeAttendanceService service
    /// </summary>
    public partial class EmployeeAttendanceService : IEmployeeAttendanceService
    {
        #region Fields

        private readonly IRepository<EmployeeAttendance> _employeeAttendanceRepository;
        private readonly IRepository<KPIMaster> _kPIMasterRepository;
        private readonly IRepository<Employee> _employeeRepository;
        private readonly IRepository<TimeSheet> _timesheetRepository;
        private readonly IHolidayService _holidayService;
        private readonly EmployeeAttendanceSetting _employeeAttendanceSetting;
        private readonly IDateTimeHelper _dateTimeHelper;

        #endregion

        #region Ctor

        public EmployeeAttendanceService(IRepository<EmployeeAttendance> employeeAttendanceRepository,
            IRepository<KPIMaster> kPIMasterRepository,
            IRepository<Employee> employeeRepository,
            IRepository<TimeSheet> timesheetRepository,
            IHolidayService holidayService,
            EmployeeAttendanceSetting employeeAttendanceSetting,
            IDateTimeHelper dateTimeHelper
          
           )
        {
            _employeeAttendanceRepository = employeeAttendanceRepository;
            _kPIMasterRepository = kPIMasterRepository;
            _employeeRepository = employeeRepository;
            _timesheetRepository = timesheetRepository;
            _holidayService = holidayService;
            _employeeAttendanceSetting = employeeAttendanceSetting;
            _dateTimeHelper = dateTimeHelper;
           
        }

        #endregion

        #region Utilities
        protected async Task<IList<TimeSheet>> GetTimeSheetByEmpAndSpentDate(int employeeId, DateTime spentDate)
        {
            // Directly use the spentDate parameter as it's already a DateTime
            return await _timesheetRepository.Table
                .Where(ts => ts.EmployeeId == employeeId && ts.SpentDate.Date == spentDate.Date)
                .ToListAsync();
        }

        protected virtual async Task<TimeSheet> GetTimeSheetByIdAsync(int timeSheetId)
        {
            return await _timesheetRepository.GetByIdAsync(timeSheetId, cache => default);
        }
        protected async Task<decimal> GetDifferenceByFromToAsync(DateTime from, DateTime to)
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


        protected virtual async Task<string> ConvertSpentTimeAsync(int SpentHours, int SpentMinutes)
        {
            return await Task.FromResult($"{SpentHours:D2}:{SpentMinutes:D2}");
        }
        protected virtual async Task<(int SpentHours, int SpentMinutes)> AddSpentTimeAsync(
    int prevSpentHours, int prevSpentMinutes, int spentHours, int spentMinutes)
        {
            return await Task.Run(() =>
            {
                // Convert everything to total minutes
                int totalMinutes = (prevSpentHours * 60 + prevSpentMinutes) + (spentHours * 60 + spentMinutes);

                // Convert back to hours and minutes
                int resultHours = totalMinutes / 60;
                int resultMinutes = totalMinutes % 60;

                // Return as a tuple (SpentHours, SpentMinutes)
                return (resultHours, resultMinutes);
            });
        }

        protected virtual async Task<(int SpentHours, int SpentMinutes)> SubtractSpentTimeAsync(
    int prevSpentHours, int prevSpentMinutes, int spentHours, int spentMinutes)
        {
            return await Task.Run(() =>
            {
                // Convert everything to total minutes
                int totalPrevMinutes = (prevSpentHours * 60) + prevSpentMinutes;
                int totalSpentMinutes = (spentHours * 60) + spentMinutes;

                // Subtract minutes (ensure it doesn't go negative)
                int remainingMinutes = totalPrevMinutes - totalSpentMinutes;
                if (remainingMinutes < 0)
                {
                    remainingMinutes = 0; // Prevent negative time
                }

                // Convert back to hours and minutes
                int resultHours = remainingMinutes / 60;
                int resultMinutes = remainingMinutes % 60;

                return (resultHours, resultMinutes);
            });
        }

        protected virtual async Task<decimal> ConvertToTotalHours(int spentHours, int spentMinutes)
        {
            if (spentHours < 0 || spentMinutes < 0 || spentMinutes >= 60)
            {
                throw new ArgumentException("Invalid time input. Minutes should be between 0 and 59.");
            }

            return spentHours + (spentMinutes / 60.0m);
        }

        #endregion
        #region Methods

        /// <summary>
        /// Get all EmployeeAttendance
        /// </summary>
        /// <param name="employeeName"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="showHidden"></param>
        /// <param name="overridePublished"></param>
        /// <returns></returns>
        public virtual async Task<IPagedList<EmployeeAttendance>> GetAllEmployeeAttendanceAsync(string employeeName,DateTime? from, DateTime? to, int statusId,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, bool? overridePublished = null)
        {
            var data2 = GetEmployeeAttendanceByEmployeeNameAsync(employeeName);
            var query = await _employeeAttendanceRepository.GetAllAsync(async query =>
            {
                if (!string.IsNullOrWhiteSpace(employeeName))
                    query = query.Where(c => c.EmployeeId == data2.Result.FirstOrDefault());

                if (from != null)
                    query = query.Where(c => c.CheckIn.Date >= from);

                if (to != null)
                    query = query.Where(c => c.CheckOut.Date <= to);

                if(statusId !=0)
                    query =query.Where(c=>c.StatusId == statusId);


                return query.OrderByDescending(c => c.CheckIn.Date);
            });
            //paging
            return new PagedList<EmployeeAttendance>(query.ToList(), pageIndex, pageSize);
        }


        public virtual async Task<IPagedList<EmployeeAttendance>> GetAllEmployeeAttendanceAsync(int employeeId, DateTime? from, DateTime? to, int statusId,
           int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, bool? overridePublished = null)
        {
            
            var query = await _employeeAttendanceRepository.GetAllAsync(async query =>
            {
                if (employeeId !=0)
                    query = query.Where(c => c.EmployeeId == employeeId);

                if (from != null)
                    query = query.Where(c => c.CheckIn.Date >= from);

                if (to != null)
                    query = query.Where(c => c.CheckOut.Date <= to);

                if (statusId != 0)
                    query = query.Where(c => c.StatusId == statusId);


                return query.OrderByDescending(c => c.CheckIn.Date);
            });
            //paging
            return new PagedList<EmployeeAttendance>(query.ToList(), pageIndex, pageSize);
        }
        public virtual async Task<IList<int>> GetEmployeeAttendanceByEmployeeNameAsync(string employeename)
        {
            var querys = (from t1 in _employeeRepository.Table
                          join t2 in _employeeAttendanceRepository.Table
                          on t1.Id equals t2.EmployeeId
                          where t1.FirstName.Contains(employeename) || t1.LastName.Contains(employeename) || ($"{t1.FirstName} {t1.LastName}").Contains(employeename) || ($"{t1.LastName} {t1.FirstName}").Contains(employeename)
                          select t1.Id).Distinct().ToList();
            return querys;
        }

        /// <summary>
        /// Get EmployeeAttendance by id
        /// </summary>
        /// <param name="empId"></param>
        /// <returns></returns>
        public virtual async Task<EmployeeAttendance> GetEmployeeAttendanceByIdAsync(int empId)
        {
            return await _employeeAttendanceRepository.GetByIdAsync(empId, cache => default);
        }

        /// <summary>
        /// Get EmployeeAttendance by ids
        /// </summary>
        /// <param name="empIds"></param>
        /// <returns></returns>
        public virtual async Task<IList<EmployeeAttendance>> GetEmployeeAttendanceByIdsAsync(int[] empIds)
        {
            return await _employeeAttendanceRepository.GetByIdsAsync(empIds, cache => default, false);
        }

        /// <summary>
        /// Insert EmployeeAttendance
        /// </summary>
        /// <param name="employeeAttendance"></param>
        /// <returns></returns>
        public virtual async Task InsertEmployeeAttendanceAsync(EmployeeAttendance employeeAttendance)
        {
            await _employeeAttendanceRepository.InsertAsync(employeeAttendance);
        }

        /// <summary>
        /// Update EmployeeAttendance
        /// </summary>
        /// <param name="employeeAttendance"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual async Task UpdateEmployeeAttendanceAsync(EmployeeAttendance employeeAttendance)
        {
            if (employeeAttendance == null)
                throw new ArgumentNullException(nameof(employeeAttendance));

            await _employeeAttendanceRepository.UpdateAsync(employeeAttendance);
        }

        /// <summary>
        /// delete EmployeeAttendance by record
        /// </summary>
        /// <param name="employeeAttendance"></param>
        /// <returns></returns>
        public virtual async Task DeleteEmployeeAttendanceAsync(EmployeeAttendance employeeAttendance)
        {
            await _employeeAttendanceRepository.DeleteAsync(employeeAttendance, false);
        }


        public virtual async Task<IList<EmployeeAttendance>> GetEmployeeAttendanceByCheckInDateAndEmpIdAsync(DateTime date, int employeeId)
        {
           
            // Fetch all employee attendance records asynchronously
            var attendances = await _employeeAttendanceRepository.GetAllAsync(query =>
            {
                // Filter records based on check-in date and employee ID
                return query.Where(ea => ea.CheckIn.Date == date.Date && ea.EmployeeId == employeeId);
            });

            // Return the filtered results as a list
            return attendances.ToList();
        }

        public virtual async Task UpdateEmployeeAttendanceBasedOnTimeSheetAsync(DateTime date ,int EmployeeId , int spentHours, int spentMinutes,DateTime prevDate, int prevEmployeeId)
        {
            var employeeAttendances = await GetEmployeeAttendanceByCheckInDateAndEmpIdAsync(date, EmployeeId);

            var OfficeInTime = _employeeAttendanceSetting.OfficeTime_From.Value;
            var OfficeOutTime = _employeeAttendanceSetting.OfficeTime_To.Value;

            if (employeeAttendances.Count == 0)
            {
                var timesheets = await GetTimeSheetByEmpAndSpentDate(EmployeeId, date);
                int spentHoursTime = 0;
                int spentMinutesTime = 0;

                if (timesheets.Count != 0)
                    foreach (var timesheet in timesheets)
                    {
                        //spentHoursTime += timesheet.SpentHours;
                        (spentHoursTime, spentMinutesTime) = await AddSpentTimeAsync(spentHoursTime, spentMinutesTime, timesheet.SpentHours, timesheet.SpentMinutes);
                    }

                EmployeeAttendance employeeAttendance = new EmployeeAttendance();
                employeeAttendance.CreateOnUtc =await _dateTimeHelper.GetUTCAsync();
                employeeAttendance.UpdateOnUtc = await _dateTimeHelper.GetUTCAsync();
                employeeAttendance.EmployeeId = EmployeeId;
                employeeAttendance.StatusId = 1;
                employeeAttendance.CheckIn = new DateTime(date.Year, date.Month, date.Day, OfficeInTime.Hour, OfficeInTime.Minute, OfficeInTime.Second); // Check-in at 10 AM
                employeeAttendance.CheckOut = new DateTime(date.Year, date.Month, date.Day, OfficeOutTime.Hour, OfficeOutTime.Minute, OfficeOutTime.Second);
                employeeAttendance.SpentHours = spentHoursTime;
                employeeAttendance.SpentMinutes = spentMinutesTime;


                await InsertEmployeeAttendanceAsync(employeeAttendance);

                if (prevDate != date || prevEmployeeId != EmployeeId)
                {
                    var prevEmployeeAttendances = await GetEmployeeAttendanceByCheckInDateAndEmpIdAsync(prevDate, prevEmployeeId);
                    var prevEmployeeAttendance = prevEmployeeAttendances.FirstOrDefault();

                    var prevTimesheets = await GetTimeSheetByEmpAndSpentDate(prevEmployeeId, prevDate);
                    int prevSpentHours = 0;
                    int prevSpentMinutes = 0;

                    if (prevTimesheets.Count != 0)
                        foreach (var timesheet in prevTimesheets)
                        {
                            prevSpentHours += timesheet.SpentHours;
                            prevSpentMinutes += timesheet.SpentMinutes;

                        }

                    prevEmployeeAttendance.UpdateOnUtc = await _dateTimeHelper.GetUTCAsync();

                    prevEmployeeAttendance.SpentHours = prevSpentHours;
                    prevEmployeeAttendance.SpentMinutes = prevSpentMinutes;


                    await UpdateEmployeeAttendanceAsync(prevEmployeeAttendance);

                    if (prevEmployeeAttendance.SpentHours == 0 && prevEmployeeAttendance.SpentMinutes ==0 && prevEmployeeAttendance.StatusId == 1)
                    {
                        await DeleteEmployeeAttendanceAsync(prevEmployeeAttendance);
                    }

                }
            }
            else
            {
                if (prevDate != date || prevEmployeeId != EmployeeId)
                {
                    var prevEmployeeAttendances = await GetEmployeeAttendanceByCheckInDateAndEmpIdAsync(prevDate, prevEmployeeId);
                    var prevEmployeeAttendance = prevEmployeeAttendances.FirstOrDefault();

                    var prevTimesheets = await GetTimeSheetByEmpAndSpentDate(prevEmployeeId, prevDate);
                    int prevSpentHours = 0;
                    int prevSpentMinutes = 0;

                    if (prevTimesheets.Count != 0)
                        foreach (var timesheet in prevTimesheets)
                        {
                            //prevSpentTime += timesheet.SpentHours;
                            (prevSpentHours, prevSpentMinutes) = await AddSpentTimeAsync(prevSpentHours, prevSpentMinutes, timesheet.SpentHours, timesheet.SpentMinutes);

                        }

                    prevEmployeeAttendance.UpdateOnUtc = await _dateTimeHelper.GetUTCAsync();

                    prevEmployeeAttendance.SpentHours = prevSpentHours;
                    prevEmployeeAttendance.SpentMinutes = prevSpentMinutes;


                    await UpdateEmployeeAttendanceAsync(prevEmployeeAttendance);

                    if (prevEmployeeAttendance.SpentHours == 0 && prevEmployeeAttendance.SpentMinutes==0 && prevEmployeeAttendance.StatusId == 1)
                    {
                        await DeleteEmployeeAttendanceAsync(prevEmployeeAttendance);
                    }

                }
                var employeeAttendance = employeeAttendances.FirstOrDefault();

              
                var timesheets = await GetTimeSheetByEmpAndSpentDate(EmployeeId,date);
                int spentHoursTime = 0;
                int spentMinutesTime = 0;

                if (timesheets.Count != 0)
                    foreach(var timesheet in timesheets)
                    {
                        //spentTime += timesheet.SpentHours;
                        (spentHoursTime, spentMinutesTime) = await AddSpentTimeAsync(spentHoursTime, spentMinutesTime, timesheet.SpentHours, timesheet.SpentMinutes);

                    }

                employeeAttendance.UpdateOnUtc = await _dateTimeHelper.GetUTCAsync();

                employeeAttendance.SpentHours = spentHoursTime;
                employeeAttendance.SpentMinutes = spentMinutesTime;



                await UpdateEmployeeAttendanceAsync(employeeAttendance);

            }
        }

        public virtual async Task UpdateEmployeeAttendanceBasedOnLeave(int EmployeeId, DateTime from, DateTime to, decimal NoOfDays, int statusId, DateTime? prevFrom = null, DateTime? prevTo = null)
        {
            var allHolidays = await _holidayService.GetAllHolidaysAsync();
            //decimal diffInDays = (to - from).Days + 1;
            decimal diffInDays = await GetDifferenceByFromToAsync(from, to);
            bool isHalfDay = (diffInDays - (decimal)0.5) == NoOfDays;

            var OfficeInTime = _employeeAttendanceSetting.OfficeTime_From.Value;
            var OfficeOutTime = _employeeAttendanceSetting.OfficeTime_To.Value;


            // Case: Remove attendance records outside the new range if editing
            if (prevFrom.HasValue && prevTo.HasValue)
            {
                for (DateTime date = prevFrom.Value; date <= prevTo.Value; date = date.AddDays(1))
                {
                    // Check if the date is outside the new range
                    if (date < from || date > to)
                    {
                        var employeeAttendances = await GetEmployeeAttendanceByCheckInDateAndEmpIdAsync(date, EmployeeId);
                        foreach (var attendance in employeeAttendances)
                        {
                            await DeleteEmployeeAttendanceAsync(attendance);
                        }
                    }
                }
            }

            // Iterate from fromDate to toDate
            for (DateTime date = from; date <= to; date = date.AddDays(1))
            {

                // Skip weekends and holidays
                if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday || allHolidays.Any(h => h.Date == date))
                {
                    continue;
                }

                var employeeAttendances = await GetEmployeeAttendanceByCheckInDateAndEmpIdAsync(date, EmployeeId);

                if (employeeAttendances.Count == 0)
                {
                    EmployeeAttendance employeeAttendance = new EmployeeAttendance();
                    employeeAttendance.CreateOnUtc = await _dateTimeHelper.GetUTCAsync();
                    employeeAttendance.UpdateOnUtc = await _dateTimeHelper.GetUTCAsync();
                    employeeAttendance.EmployeeId = EmployeeId;
                    // If it's the last day and the difference in days matches the NoOfDays condition, set StatusId to 4
                    if (isHalfDay && date == to)
                    {
                        employeeAttendance.StatusId = 2;
                    }
                    else
                    {
                        employeeAttendance.StatusId = 3;
                    }
                    employeeAttendance.CheckIn = new DateTime(date.Year, date.Month, date.Day, OfficeInTime.Hour, OfficeInTime.Minute, OfficeInTime.Second); // Check-in at 10 AM
                    employeeAttendance.CheckOut = new DateTime(date.Year, date.Month, date.Day, OfficeOutTime.Hour, OfficeOutTime.Minute, OfficeOutTime.Second);
                    employeeAttendance.SpentHours = 0;

                    await InsertEmployeeAttendanceAsync(employeeAttendance);

                    if (statusId != 2)
                    {
                        var timesheets = await GetTimeSheetByEmpAndSpentDate(EmployeeId, date);
                        int spentHours = 0;
                        int spentMinutes = 0;

                        if (timesheets.Count != 0)
                            foreach (var timesheet in timesheets)
                            {
                                //spentTime += timesheet.SpentHours;
                                (spentHours, spentMinutes) = await AddSpentTimeAsync(spentHours, spentMinutes, timesheet.SpentHours, timesheet.SpentMinutes);
                            }
                        employeeAttendance.SpentHours = spentHours;
                        employeeAttendance.SpentMinutes = spentMinutes;


                        if (employeeAttendance.SpentHours == 0 && employeeAttendance.SpentMinutes==0)
                        {
                            await DeleteEmployeeAttendanceAsync(employeeAttendance);
                        }
                        else
                        {
                            employeeAttendance.StatusId = 1;
                        }
                    }

                }
              
                else
                {

                    var employeeAttendance = employeeAttendances.FirstOrDefault();

                    //var timesheets = await GetTimeSheetByEmpAndSpentDate(EmployeeId, date);
                    //decimal spentTime = 0;
                    //if (timesheets.Count != 0)
                    //    foreach (var timesheet in timesheets)
                    //    {
                    //        spentTime += timesheet.SpentHours;
                    //    }

                    employeeAttendance.UpdateOnUtc = await _dateTimeHelper.GetUTCAsync();
                    // If it's the last day and the difference in days matches the NoOfDays condition, set StatusId to 4
                    if (isHalfDay && date == to)
                    {
                        employeeAttendance.StatusId = 2;
                    }
                    else
                    {
                        employeeAttendance.StatusId = 3;
                    }

                    if (employeeAttendance.StatusId == 2)
                    {

                        var timesheets = await GetTimeSheetByEmpAndSpentDate(EmployeeId, date);
                        int spentHours = 0;
                        int spentMinutes = 0;

                        if (timesheets.Count != 0)
                            foreach (var timesheet in timesheets)
                            {
                                //spentTime += timesheet.SpentHours;
                                (spentHours, spentMinutes) = await AddSpentTimeAsync(spentHours, spentMinutes, timesheet.SpentHours, timesheet.SpentMinutes);
                            }
                        employeeAttendance.SpentHours = spentHours;
                        employeeAttendance.SpentMinutes = spentMinutes;

                    }
                    else
                    {
                        employeeAttendance.SpentHours = 0;
                        employeeAttendance.SpentMinutes = 0;

                    }

                    if (statusId != 2)
                    {
                        var timesheets = await GetTimeSheetByEmpAndSpentDate(EmployeeId, date);
                        int spentHours = 0;
                        int spentMinutes = 0;

                        if (timesheets.Count != 0)
                            foreach (var timesheet in timesheets)
                            {
                                //spentTime += timesheet.SpentHours;
                                (spentHours, spentMinutes) = await AddSpentTimeAsync(spentHours, spentMinutes, timesheet.SpentHours, timesheet.SpentMinutes);
                            }
                        employeeAttendance.SpentHours = spentHours;
                        employeeAttendance.SpentMinutes = spentMinutes;


                        if (employeeAttendance.SpentHours == 0 && employeeAttendance.SpentMinutes ==0)
                        {
                            await DeleteEmployeeAttendanceAsync(employeeAttendance);
                        }
                        else
                        {
                            employeeAttendance.StatusId = 1;
                        }
                    }
                    
                    await UpdateEmployeeAttendanceAsync(employeeAttendance);

                }

               
            }
        }

        public virtual async Task DeleteEmployeeAttendanceBasedOnTimeSheetAsync(TimeSheet timeSheet)
        {
            

            var employeeAttendances = await GetEmployeeAttendanceByCheckInDateAndEmpIdAsync(timeSheet.SpentDate, timeSheet.EmployeeId);

            if (employeeAttendances.Count != 0)
            {
               var employeeAttendance = employeeAttendances.FirstOrDefault();
                //employeeAttendance.SpentHours -= timeSheet.SpentHours;
                (employeeAttendance.SpentHours, employeeAttendance.SpentMinutes) = await SubtractSpentTimeAsync(employeeAttendance.SpentHours, employeeAttendance.SpentMinutes, timeSheet.SpentHours, timeSheet.SpentMinutes);

                if (employeeAttendance.SpentHours == 0 &&  employeeAttendance.SpentMinutes==0 && employeeAttendance.StatusId ==1)
                {
                    await DeleteEmployeeAttendanceAsync(employeeAttendance);
                }
                else
                {
                    await UpdateEmployeeAttendanceAsync(employeeAttendance);
                }
             
            }
           
        }

        public virtual async Task DeleteEmployeeAttendanceBasedOnLeaveAsync(LeaveManagement leaveManagement)
        {   
            var to = leaveManagement.To;
            var from = leaveManagement.From;
          
          
            // Iterate from fromDate to toDate
            for (DateTime date = from; date <= to; date = date.AddDays(1))
            {

                var employeeAttendances = await GetEmployeeAttendanceByCheckInDateAndEmpIdAsync(date, leaveManagement.EmployeeId);

                if (employeeAttendances.Count != 0)
                {
                    var employeeAttendance = employeeAttendances.FirstOrDefault();

                    var timesheets = await GetTimeSheetByEmpAndSpentDate(employeeAttendance.EmployeeId, date);
                    int spentHours = 0;
                    int spentMinutes = 0;

                    if (timesheets.Count != 0)
                        foreach (var timesheet in timesheets)
                        {
                            //spentTime += timesheet.SpentHours;
                            (spentHours, spentMinutes) = await AddSpentTimeAsync(spentHours, spentMinutes, timesheet.SpentHours, timesheet.SpentMinutes);
                        }


                    employeeAttendance.SpentHours = spentHours;
                    employeeAttendance.SpentMinutes = spentMinutes;


                    if (employeeAttendance.SpentHours == 0 && employeeAttendance.SpentMinutes ==0)
                    {
                        await DeleteEmployeeAttendanceAsync(employeeAttendance);
                    }
                    else
                    {
                        employeeAttendance.StatusId = 1;
                        await UpdateEmployeeAttendanceAsync(employeeAttendance);
                    }

                }
            }
        }

        public virtual async Task<IList<TimeSheetReport>> GetAttendanceReportListAsync(int employeeId, DateTime from, DateTime to, int showById)
        {
            var query = await _employeeAttendanceRepository.GetAllAsync(async query =>
            {
                query = query.Where(c => c.EmployeeId == employeeId && c.CheckIn.Date >= from.Date && c.CheckIn.Date <= to.Date);
               

                return query.OrderBy(c => c.CheckIn.Date);
            });

            query = query.ToList();

            // Check if we want to group by day, week, or month
            IList<TimeSheetReport> timeSheetReports = query
                        .GroupBy(c => c.CheckIn.Date)
                        .Select(g => new TimeSheetReport
                        {
                            EmployeeId = g.First().EmployeeId,
                            SpentDate = g.Key,
                            SpentHours = g.Sum(c => c.SpentHours),
                            //TaskId = g.First().TaskId,
                            CreateOnUtc = g.First().CreateOnUtc,
                            UpdateOnUtc = g.First().UpdateOnUtc
                        })
                        .ToList();



            // Fill missing dates if grouped by day
            //if (showById == 1)
            //{

            for (var date = from; date <= to; date = date.AddDays(1))
            {
                if (!timeSheetReports.Any(r => r.SpentDate == date))
                {
                    timeSheetReports.Add(new TimeSheetReport
                    {
                        EmployeeId = employeeId,
                        SpentDate = date,
                        SpentHours = 0,
                        CreateOnUtc = await _dateTimeHelper.GetUTCAsync(),
                        UpdateOnUtc = await _dateTimeHelper.GetUTCAsync()
                });
                }
                //}
            }

            timeSheetReports = timeSheetReports.OrderBy(c => c.SpentDate).ToList();

            // Calculate the total spent time
            if (timeSheetReports.FirstOrDefault() != null)
                timeSheetReports.FirstOrDefault().TotalSpentHours = timeSheetReports.Sum(x => x.SpentHours);

            // Paging or other logic can be added here if needed

            return timeSheetReports;
        }
        #endregion
    }
}