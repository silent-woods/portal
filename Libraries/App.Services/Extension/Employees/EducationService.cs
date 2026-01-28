using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App.Core;
using App.Core.Domain.Employees;
using App.Data;

namespace App.Services.Employees
{
    /// <summary>
    /// Education service
    /// </summary>
    public partial class EducationService : IEducationService
    {
        #region Fields

        private readonly IRepository<Education> _educationRepository;       
        //private readonly IStoreContext _storeContext;

        #endregion

        #region Ctor

        public EducationService(
            IRepository<Education> educationRepository)
        {
            _educationRepository = educationRepository;
        }

        #endregion

        #region Methods

        #region Education

        /// <summary>
        /// Deletes a Education
        /// </summary>
        /// <param name="education">Education</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task DeleteEducationAsync(Education education)
        {
            await _educationRepository.DeleteAsync(education);
        }

        /// <summary>
        /// Gets a Education
        /// </summary>
        /// <param name="educationId">education identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the education
        /// </returns>
        public virtual async Task<Education> GetEducationByIdAsync(int educationId)
        {
            return await _educationRepository.GetByIdAsync(educationId, cache => default);
        }

        /// <summary>
        /// Gets all educations
        /// </summary>
        /// <param name="storeId">The store identifier; pass 0 to load all records</param>
        /// <param name="languageId">Language identifier; 0 if you want to get all records</param>
        /// <param name="dateFrom">Filter by created date; null if you want to get all records</param>
        /// <param name="dateTo">Filter by created date; null if you want to get all records</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <param name="education">Filter by education name</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the education
        /// </returns>

        public virtual async Task<IPagedList<Education>> GetAllEducationsAsync(int employeeId,int storeId = 0, int languageId = 0,
            DateTime? dateFrom = null, DateTime? dateTo = null,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, string course = null)
        {
            return await _educationRepository.GetAllPagedAsync(async query =>
            {

                if (!string.IsNullOrEmpty(course))
                    query = query.Where(b => b.Course.Contains(course));
                if (employeeId != 0)
                {
                    query = query.Where(b => b.EmployeeID == employeeId);
                }
                return query;
            }, pageIndex, pageSize);
        }

        /// <summary>
        /// Inserts a education
        /// </summary>
        /// <param name="education">Education</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task InsertEducationAsync(Education education)
        {
            await _educationRepository.InsertAsync(education);
        }

        /// <summary>
        /// Updates the Education
        /// </summary>
        /// <param name="education">Education</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task UpdateEducationAsync(Education education)
        {
            await _educationRepository.UpdateAsync(education);
        }

        public virtual async Task<IList<Education>> GetEducationByIdsAsync(int[] educationIds)
        {
            return await _educationRepository.GetByIdsAsync(educationIds, cache => default, false);
        }

        #endregion

        #endregion
    }
}