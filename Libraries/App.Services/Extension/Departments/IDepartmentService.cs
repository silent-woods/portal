using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using App.Core;
using App.Core.Domain.Departments;
using App.Core.Domain.Designations;

namespace App.Services.Departments
{
    public partial interface IDepartmentService
    {
        Task DeleteDepartmentAsync(Department department);
        Task<Department> GetDepartmentByIdAsync(int departmentId);
        Task<IPagedList<Department>> GetAllDepartmentsAsync(int storeId = 0, int languageId = 0,
            DateTime? dateFrom = null, DateTime? dateTo = null,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, string name = null);
        Task<IPagedList<Department>> GetAllDepartmentByNameAsync(string departmentName = null,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, bool? overridePublished = null);
        Task InsertDepartmentAsync(Department department);
        Task UpdateDepartmentAsync(Department department);
        Task<IList<Department>> GetDepartmentByIdsAsync(int[] departmentIds);
    }
}