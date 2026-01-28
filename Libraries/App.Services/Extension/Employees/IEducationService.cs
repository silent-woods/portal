using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using App.Core;
using App.Core.Domain.Employees;

namespace App.Services.Employees
{
    /// <summary>
    /// Education service interface
    /// </summary>
    public partial interface IEducationService
    {
        /// <summary>
        /// Deletes a Education
        /// </summary>
        /// <param name="education">Education</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task DeleteEducationAsync(Education education);

        /// <summary>
        /// Gets a Education
        /// </summary>
        /// <param name="educationId">education identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the education
        /// </returns>
        Task<Education> GetEducationByIdAsync(int educationId);

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

        Task<IPagedList<Education>> GetAllEducationsAsync(int employeeId,int storeId = 0, int languageId = 0,
            DateTime? dateFrom = null, DateTime? dateTo = null,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, string course = null);

        /// <summary>
        /// Inserts a education
        /// </summary>
        /// <param name="education">Education</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task InsertEducationAsync(Education education);

        /// <summary>
        /// Updates the Education
        /// </summary>
        /// <param name="education">Education</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task UpdateEducationAsync(Education education);

        Task<IList<Education>> GetEducationByIdsAsync(int[] educationIds);
    }
}