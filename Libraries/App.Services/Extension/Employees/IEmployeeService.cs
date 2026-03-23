using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using App.Core;
using App.Core.Domain.Customers;
using App.Core.Domain.Employees;
namespace App.Services.Employees
{
    /// <summary>
    /// Employee service interface
    /// </summary>
    public partial interface IEmployeeService
    {
        Task DeleteEmployeeAsync(Employee employee);
        Task<Employee> GetEmployeeByIdAsync(int employeeId);
        Task<IPagedList<Employee>> GetAllEmployeesAsync(int storeId = 0, int languageId = 0,
            DateTime? dateFrom = null, DateTime? dateTo = null,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, string employee = null, bool showInActive = false, bool showVendors = false);
        Task<IPagedList<Employee>> GetAllEmployeeNameAsync(string employeeName = null, int languageId = 0,
           DateTime? dateFrom = null, DateTime? dateTo = null,
           int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, string employee = null, bool showInActive = false, bool showVendors = false);
        Task InsertEmployeeAsync(Employee employee);
        Task UpdateEmployeeAsync(Employee employee);
        Task<IList<Employee>> GetEmployeeByIdsAsync(int[] employeeIds);
        Task<Employee> GetEmployeeByEmailAsync(string email);
        Task<Employee> GetEmployeeByCustomerIdAsync(int customerId);
        Task<IEnumerable<int>> GetAllEmployeeIdsAsync(bool showVendors = false);
        Task<IList<Employee>> GetEmployeesForTimesheetReminder();
        Task<IList<Employee>> GetEmployeesForTimesheetReminder2();
        Task<Employee> GetEmployeeByEmployeeName(string fullName);
        Task<IList<Employee>> GetEmployeesByMention(string comments);
        Task<IList<Employee>> GetAllManagerEmployees();
        Task<IList<Employee>> GetAllHREmployees();
        Task<IList<Employee>> GetEmployeesByIdsAsync(int[] ids);
        Task<IList<Employee>> GetEmployeesByDesignationIdsAsync(int[] designationIds);
    }
}