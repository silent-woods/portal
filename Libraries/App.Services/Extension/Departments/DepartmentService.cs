using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App.Core;
using App.Core.Domain.Departments;
using App.Core.Domain.Designations;
using App.Data;

namespace App.Services.Departments
{
    public partial class DepartmentService : IDepartmentService
    {
        #region Fields

        private readonly IRepository<Department> _departmentRepository;       

        #endregion

        #region Ctor

        public DepartmentService(
            IRepository<Department> departmentRepository)
        {            
            _departmentRepository = departmentRepository;
        }

        #endregion

        #region Methods

        #region Department
        public virtual async Task DeleteDepartmentAsync(Department department)
        {
            await _departmentRepository.DeleteAsync(department);
        }
        public virtual async Task<Department> GetDepartmentByIdAsync(int departmentId)
        {
            return await _departmentRepository.GetByIdAsync(departmentId, cache => default);
        }
        public virtual async Task<IList<Department>> GetDepartmentByIdsAsync(int[] departmentIds)
        {
            return await _departmentRepository.GetByIdsAsync(departmentIds, cache => default, false);
        }
        public virtual async Task<IPagedList<Department>> GetAllDepartmentsAsync(int storeId = 0, int languageId = 0,
            DateTime? dateFrom = null, DateTime? dateTo = null,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, string name = null)
        {
            return await _departmentRepository.GetAllPagedAsync(async query =>
            {

                if (!string.IsNullOrEmpty(name))
                    query = query.Where(b => b.Name.Contains(name));

                return query.OrderByDescending(c => c.CreatedOnUtc);
            }, pageIndex, pageSize);
        }
        public virtual async Task<IPagedList<Department>> GetAllDepartmentByNameAsync(string departmentName=null,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, bool? overridePublished = null)
        {
            var query = await _departmentRepository.GetAllAsync(async query =>
            {
                return query.OrderByDescending(c => c.Id);
            });
            return new PagedList<Department>(query.ToList(), pageIndex, pageSize);
        }
        public virtual async Task InsertDepartmentAsync(Department department)
        {
            await _departmentRepository.InsertAsync(department);
        }
        public virtual async Task UpdateDepartmentAsync(Department department)
        {
            await _departmentRepository.UpdateAsync(department);
        }
        #endregion
        #endregion
    }
}