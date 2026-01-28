using System.Collections.Generic;
using System.Threading.Tasks;
using App.Core;
using App.Core.Domain.Employees;

namespace App.Services.Employees
{
    /// <summary>
    /// Experience service interface
    /// </summary>
    public partial interface IExperienceService
    {
        /// <summary>
        /// Deletes a Experience
        /// </summary>
        /// <param name="experience">Experience</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task DeleteExperienceAsync(Experience experience);

        /// <summary>
        /// Gets a Experience
        /// </summary>
        /// <param name="educationId">experience identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the experience
        /// </returns>
        Task<Experience> GetExperienceByIdAsync(int experienceId);

        /// <summary>
        /// Gets all experience
        /// </summary>
        /// <param name="experienceName">The store identifier; pass 0 to load all records</param>
        /// <param name="languageId">Language identifier; 0 if you want to get all records</param>
        /// <param name="dateFrom">Filter by created date; null if you want to get all records</param>
        /// <param name="dateTo">Filter by created date; null if you want to get all records</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <param name="experience">Filter by experience name</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the experience
        /// </returns>

        Task<IPagedList<Experience>> GetAllExperienceAsync(int employeeId,string experienceName,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, bool? overridePublished = null);

        /// <summary>
        /// Inserts a experience
        /// </summary>
        /// <param name="experience">Experience</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task InsertExperienceAsync(Experience experience);

        /// <summary>
        /// Updates the Experience
        /// </summary>
        /// <param name="experience">Experience</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task UpdateExperienceAsync(Experience experience);

        Task<IList<Experience>> GetExperienceByIdsAsync(int[] experienceIds);
    }
}