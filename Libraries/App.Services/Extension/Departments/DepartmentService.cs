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
    /// <summary>
    /// Customer service
    /// </summary>
    public partial class DepartmentService : IDepartmentService
    {
        #region Fields

        private readonly IRepository<Department> _departmentRepository;       
        //private readonly IStoreContext _storeContext;

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

        /// <summary>
        /// Deletes a Department
        /// </summary>
        /// <param name="department">Department</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task DeleteDepartmentAsync(Department department)
        {
            await _departmentRepository.DeleteAsync(department);
        }

        /// <summary>
        /// Gets a Department
        /// </summary>
        /// <param name="departmentId">department identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the department
        /// </returns>
        public virtual async Task<Department> GetDepartmentByIdAsync(int departmentId)
        {
            return await _departmentRepository.GetByIdAsync(departmentId, cache => default);
        }

        public virtual async Task<IList<Department>> GetDepartmentByIdsAsync(int[] departmentIds)
        {
            return await _departmentRepository.GetByIdsAsync(departmentIds, cache => default, false);
        }

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
        public virtual async Task<IPagedList<Department>> GetAllDepartmentByNameAsync(string departmentName,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, bool? overridePublished = null)
        {
            var query = await _departmentRepository.GetAllAsync(async query =>
            {
                return query.OrderByDescending(c => c.Id);
            });
            //paging
            return new PagedList<Department>(query.ToList(), pageIndex, pageSize);
        }
        /// <summary>
        /// Inserts a department
        /// </summary>
        /// <param name="department">Department</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task InsertDepartmentAsync(Department department)
        {
            await _departmentRepository.InsertAsync(department);
        }

        /// <summary>
        /// Updates the department
        /// </summary>
        /// <param name="department">Department</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task UpdateDepartmentAsync(Department department)
        {
            await _departmentRepository.UpdateAsync(department);
        }

        #endregion

        #endregion
    }
}