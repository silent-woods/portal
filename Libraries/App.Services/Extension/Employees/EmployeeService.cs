using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App.Core;
using App.Core.Caching;
using App.Core.Domain.Common;
using App.Core.Domain.Customers;
using App.Core.Domain.Employees;
using App.Data;
using App.Services.Customers;
using App.Core.Domain.Forums;
using App.Core.Domain.News;
using App.Core.Domain.Polls;
using App.Core.Infrastructure;
using App.Data.Extensions;
using App.Services.Common;
using App.Services.Localization;
using App.Services.TimeSheets;
using App.Services.Holidays;
using App.Services.Leaves;
using App.Services.Helpers;
using App.Services.Designations;
using DocumentFormat.OpenXml.ExtendedProperties;
using App.Core.Domain.Extension.TimeSheets;
using System.Text.RegularExpressions;

namespace App.Services.Employees
{
    /// <summary>
    /// Employee service
    /// </summary>
    public partial class EmployeeService : IEmployeeService
    {
        #region Fields

        private readonly IRepository<Employee> _employeeRepository;
        private readonly ITimeSheetsService _timeSheetsService;
        private readonly IHolidayService _holidayService;
        private readonly ILeaveManagementService _leaveManagementService;
        private readonly IRepository<Customer> _customerRepository;
        private readonly CustomerSettings _customerSettings;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IDesignationService _designationService;
        private readonly TimeSheetSetting _timeSheetSetting;

        #endregion

        #region Ctor

        public EmployeeService(
            IRepository<Employee> employeeRepository,ITimeSheetsService timeSheetsService, IHolidayService holidayService ,ILeaveManagementService leaveManagementService, IRepository<Customer> customerRepository, CustomerSettings customerSettings,
            IDateTimeHelper dateTimeHelper,IDesignationService designationService,
            TimeSheetSetting timeSheetSetting)
        {
            _employeeRepository = employeeRepository;
            _timeSheetsService = timeSheetsService;
            _holidayService = holidayService;
            _leaveManagementService = leaveManagementService;
            _customerRepository = customerRepository;
            _customerSettings = customerSettings;
            _dateTimeHelper = dateTimeHelper;
            _designationService = designationService;
            _timeSheetSetting = timeSheetSetting;
        }

        #endregion
        #region Utilities
        public virtual async Task<Customer> GetCustomerByIdAsync(int customerId)
        {
            return await _customerRepository.GetByIdAsync(customerId,
                cache => cache.PrepareKeyForShortTermCache(NopEntityCacheDefaults<Customer>.ByIdCacheKey, customerId));
        }
        public virtual async Task DeleteCustomerAsync(Customer customer)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            if (customer.IsSystemAccount)
                throw new NopException($"System customer account ({customer.SystemName}) could not be deleted");

            customer.Deleted = true;

            if (_customerSettings.SuffixDeletedCustomers)
            {
                if (!string.IsNullOrEmpty(customer.Email))
                    customer.Email += "-DELETED";
                if (!string.IsNullOrEmpty(customer.Username))
                    customer.Username += "-DELETED";
            }

            await _customerRepository.UpdateAsync(customer, false);
            await _customerRepository.DeleteAsync(customer);
        }
        #endregion
        #region Methods

        #region Employee

        /// <summary>
        /// Deletes a Employee
        /// </summary>
        /// <param name="employee">Employee</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task DeleteEmployeeAsync(Employee employee)
        {
            await _employeeRepository.DeleteAsync(employee);
            var customer = await GetCustomerByIdAsync(employee.Customer_Id);
            if (customer != null)
                await DeleteCustomerAsync(customer);
        }

        /// <summary>
        /// Gets a Employee
        /// </summary>
        /// <param name="employeeId">employee identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the employee
        /// </returns>
        public virtual async Task<Employee> GetEmployeeByIdAsync(int employeeId)
        {
            return await _employeeRepository.GetByIdAsync(employeeId);
        }

        public virtual async Task<IList<Employee>> GetEmployeeByIdsAsync(int[] employeeIds)
        {
            return await _employeeRepository.GetByIdsAsync(employeeIds, cache => default, false);
        }

        /// <summary>
        /// Gets all Employees
        /// </summary>
        /// <param name="storeId">The store identifier; pass 0 to load all records</param>
        /// <param name="languageId">Language identifier; 0 if you want to get all records</param>
        /// <param name="dateFrom">Filter by created date; null if you want to get all records</param>
        /// <param name="dateTo">Filter by created date; null if you want to get all records</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <param name="employee">Filter by employee name</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the employee
        /// </returns>

        public virtual async Task<IPagedList<Employee>> GetAllEmployeesAsync(int storeId = 0, int languageId = 0,
            DateTime? dateFrom = null, DateTime? dateTo = null,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, string employee = null, bool showInActive = false)
        {
            return await _employeeRepository.GetAllPagedAsync(async query =>
            {

                if (!string.IsNullOrEmpty(employee))
                    query = query.Where(b => b.FirstName.Contains(employee.Trim()) || b.LastName.Contains(employee.Trim()) || ($"{b.FirstName} {b.LastName}").Contains(employee.Trim()) || ($"{b.LastName} {b.FirstName}").Contains(employee.Trim()));
                if(showInActive == false)
                {
                    query = query.Where(p => p.EmployeeStatusId == 1);
                }
                return query;
            }, pageIndex, pageSize);
        }


        public virtual async Task<IEnumerable<int>> GetAllEmployeeIdsAsync()
        {
            var projects = await GetAllEmployeesAsync();
            return projects.Where(p => p.EmployeeStatusId == 1).OrderBy(e=>e.FirstName).Select(p => p.Id);
        }


        public virtual async Task<IPagedList<Employee>> GetAllEmployeeNameAsync(string employeeName=null, int languageId = 0,
            DateTime? dateFrom = null, DateTime? dateTo = null,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, string employee = null, bool showInActive = false)
        {
            return await _employeeRepository.GetAllPagedAsync(async query =>
            {
                if (showInActive == false)
                {
                    query = query.Where(p => p.EmployeeStatusId == 1);
                }
                return query.OrderBy(c => c.Id);
            }, pageIndex, pageSize);
        }

        /// <summary>
        /// Inserts a Employee
        /// </summary>
        /// <param name="employee">Employee</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task InsertEmployeeAsync(Employee employee)
        {
            await _employeeRepository.InsertAsync(employee);
        }

        /// <summary>
        /// Updates the Employee
        /// </summary>
        /// <param name="employee">Employee</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task UpdateEmployeeAsync(Employee employee)
        {
            await _employeeRepository.UpdateAsync(employee);
        }

        /// <summary>
        /// Get employee by email
        /// </summary>
        /// <param name="email">Email</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the employee
        /// </returns>
        public virtual async Task<Employee> GetEmployeeByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return null;

            var query = from c in _employeeRepository.Table
                        orderby c.Id
                        where c.OfficialEmail == email
                        select c;
            var employee = await query.FirstOrDefaultAsync();

            return employee;
        }

        //get customer id
        public virtual async Task<Employee> GetEmployeeByCustomerIdAsync(int customerId)
        {
            if (customerId <= 0)
                return null;

            var query = from e in _employeeRepository.Table
                        where e.Customer_Id == customerId
                        select e;
            return await query.FirstOrDefaultAsync();
        }

        public virtual async Task<IList<Employee>> GetEmployeesForTimesheetReminder()
        {
            var allEmployees = await GetAllEmployeesAsync();
            var currentDate = await _dateTimeHelper.GetIndianTimeAsync();
            var date = currentDate.Date;

          var employeeList = new List<Employee>();
            string excludeDepartment = _timeSheetSetting.DepartmentIds;
            var excludeList = excludeDepartment.Split(",");
            if (allEmployees != null)
            foreach(var employee in allEmployees)
                {
                    if (excludeList.Contains(employee.DepartmentId.ToString()))
                    {
                        continue; 
                    }
                    var timesheet = await _timeSheetsService.GetTimeSheetByEmpAndSpentDate(employee.Id, date);
                if(timesheet.Count==0)
                {
                        var allHolidays = await _holidayService.GetAllHolidaysAsync();
                        
                        if (date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday && !allHolidays.Any(h => h.Date == date) && !(await _leaveManagementService.GetLeaveManagementsAsync(employee.Id, 0, date, date, 2))
                           .Any(leave => leave.StatusId == 2))
                        {
                            employeeList.Add(employee);
                        }
                }
            }
            return employeeList;

        }

        public virtual async Task<IList<Employee>> GetEmployeesForTimesheetReminder2()
        {
            var allEmployees = await GetAllEmployeesAsync();
            var currentDate = await _dateTimeHelper.GetIndianTimeAsync();

            var date = currentDate.Date;

            int managerId = await _designationService.GetRoleIdProjectManager();
            var employeeList = new List<Employee>();
            string excludeDepartment = _timeSheetSetting.DepartmentIds;
            var excludeList = excludeDepartment.Split(",");
            var allHolidays = await _holidayService.GetAllHolidaysAsync();
            if (allEmployees != null)
                foreach (var employee in allEmployees)
                {
                    if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday ||excludeList.Contains(employee.DepartmentId.ToString()) ||allHolidays.Any(h => h.Date == date ) || (await _leaveManagementService.GetLeaveManagementsAsync(employee.Id, 0, date, date, 2)).Any(leave => leave.StatusId == 2))
                    {
                        continue; 
                    }                   
                        employeeList.Add(employee);
                    
                }
            return employeeList;

        }
        public virtual async Task<Employee> GetEmployeeByEmployeeName(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                return null;

            var nameParts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (nameParts.Length < 2)
                return null;

            var firstName = nameParts[0];
            var lastName = nameParts[1];

            var allEmployees = await GetAllEmployeeNameAsync("", showInActive: false);

            return allEmployees.FirstOrDefault(e =>
                e.FirstName.Equals(firstName, StringComparison.OrdinalIgnoreCase) &&
                e.LastName.Equals(lastName, StringComparison.OrdinalIgnoreCase));
        }

        public virtual async Task<IList<Employee>> GetEmployeesByMention(string comments)
        {
            var result = new List<Employee>();

            if (string.IsNullOrWhiteSpace(comments))
                return result;

            var mentionMatches = Regex.Matches(comments, @"@<([^>]+)>");

            var mentionedNames = mentionMatches
       .Select(m => Regex.Replace(m.Groups[1].Value.Trim(), @"\s+", " ")) 
       .Where(name => !string.IsNullOrWhiteSpace(name))
       .Distinct(StringComparer.OrdinalIgnoreCase)
       .ToList();
            foreach (var name in mentionedNames)
            {
                var employee = await GetEmployeeByEmployeeName(name);
                if (employee != null)
                {
                    result.Add(employee);
                }
            }

            return result;
        }

        public virtual async Task<IList<Employee>> GetAllManagerEmployees()
        {
            var managerDesignationId = await _designationService.GetRoleIdProjectManager();

            var query = await _employeeRepository.Table
                .Where(e => e.DesignationId == managerDesignationId && e.EmployeeStatusId == 1)
                .ToListAsync();

            return query;
        }

        public virtual async Task<IList<Employee>> GetAllHREmployees()
        {
            var hrDesignationId = await _designationService.GetHrRoleId();

            var query = await _employeeRepository.Table
                .Where(e => e.DesignationId == hrDesignationId && e.EmployeeStatusId == 1)
                .ToListAsync();

            return query;
        }

        public async Task<IList<Employee>> GetEmployeesByIdsAsync(int[] ids)
        {
            if (ids == null || ids.Length == 0)
                return new List<Employee>();

            var query = _employeeRepository.Table.Where(e => ids.Contains(e.Id));
            return await query.ToListAsync();
        }



        public async Task<IList<Employee>> GetEmployeesByDesignationIdsAsync(int[] designationIds)
        {
            if (designationIds == null || designationIds.Length == 0)
                return new List<Employee>();

            return await _employeeRepository.Table
                .Where(e => designationIds.Contains(e.DesignationId))
                .ToListAsync();
        }


        #endregion
        #endregion
    }
}