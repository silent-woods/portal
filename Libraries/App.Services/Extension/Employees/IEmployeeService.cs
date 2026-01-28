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
        /// <summary>
        /// Deletes a Employee
        /// </summary>
        /// <param name="employee">Employee</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task DeleteEmployeeAsync(Employee employee);

        /// <summary>
        /// Gets a employee
        /// </summary>
        /// <param name="employeeId">employee identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the employee
        /// </returns>
        Task<Employee> GetEmployeeByIdAsync(int employeeId);

        /// <summary>
        /// Gets all employees
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

        Task<IPagedList<Employee>> GetAllEmployeesAsync(int storeId = 0, int languageId = 0,
            DateTime? dateFrom = null, DateTime? dateTo = null,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, string employee = null, bool showInActive = false);

        Task<IPagedList<Employee>> GetAllEmployeeNameAsync(string employeeName = null, int languageId = 0,
           DateTime? dateFrom = null, DateTime? dateTo = null,
           int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, string employee = null, bool showInActive = false);
        /// <summary>
        /// Inserts a employee
        /// </summary>
        /// <param name="employee">Employee</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task InsertEmployeeAsync(Employee employee);

        /// <summary>
        /// Updates the Employee
        /// </summary>
        /// <param name="employee">Employee</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task UpdateEmployeeAsync(Employee employee);

        Task<IList<Employee>> GetEmployeeByIdsAsync(int[] employeeIds);

        Task<Employee> GetEmployeeByEmailAsync(string email);

        Task<Employee> GetEmployeeByCustomerIdAsync(int customerId);

        Task<IEnumerable<int>> GetAllEmployeeIdsAsync();

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