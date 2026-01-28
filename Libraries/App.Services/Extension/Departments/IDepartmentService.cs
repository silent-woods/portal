using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using App.Core;
using App.Core.Domain.Departments;
using App.Core.Domain.Designations;

namespace App.Services.Departments
{
    /// <summary>
    /// Department service interface
    /// </summary>
    public partial interface IDepartmentService
    {
        /// <summary>
        /// Deletes a Department
        /// </summary>
        /// <param name="department">Department</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task DeleteDepartmentAsync(Department department);

        /// <summary>
        /// Gets a Department
        /// </summary>
        /// <param name="departmentId">department identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the department
        /// </returns>
        Task<Department> GetDepartmentByIdAsync(int departmentId);

        /// <summary>
        /// Gets all Departments
        /// </summary>
        /// <param name="storeId">The store identifier; pass 0 to load all records</param>
        /// <param name="languageId">Language identifier; 0 if you want to get all records</param>
        /// <param name="dateFrom">Filter by created date; null if you want to get all records</param>
        /// <param name="dateTo">Filter by created date; null if you want to get all records</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <param name="name">Filter by department name</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the department
        /// </returns>

        Task<IPagedList<Department>> GetAllDepartmentsAsync(int storeId = 0, int languageId = 0,
            DateTime? dateFrom = null, DateTime? dateTo = null,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, string name = null);

        Task<IPagedList<Department>> GetAllDepartmentByNameAsync(string departmentName,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, bool? overridePublished = null);
        /// <summary>
        /// Inserts a department
        /// </summary>
        /// <param name="department">Department</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task InsertDepartmentAsync(Department department);

        /// <summary>
        /// Updates the department
        /// </summary>
        /// <param name="department">Department</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task UpdateDepartmentAsync(Department department);

        Task<IList<Department>> GetDepartmentByIdsAsync(int[] departmentIds);
    }
}