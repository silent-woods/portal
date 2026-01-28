using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App.Core;
using App.Core.Domain.Designations;
using App.Core.Domain.Employees;
using App.Data;

namespace App.Services.Employees
{
    /// <summary>
    /// Education service
    /// </summary>
    public partial class ExperienceService : IExperienceService
    {
        #region Fields

        private readonly IRepository<Experience> _experienceRepository;

        #endregion

        #region Ctor

        public ExperienceService(
            IRepository<Experience> experienceRepository)
        {
            _experienceRepository = experienceRepository;
        }

        #endregion

        #region Methods

        #region Education

        /// <summary>
        /// Deletes a Experience
        /// </summary>
        /// <param name="experience">Experience</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task DeleteExperienceAsync(Experience experience)
        {
            await _experienceRepository.DeleteAsync(experience);
        }

        /// <summary>
        /// Gets a Experience
        /// </summary>
        /// <param name="experienceId">experience identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the experience
        /// </returns>
        public virtual async Task<Experience> GetExperienceByIdAsync(int experienceId)
        {
            return await _experienceRepository.GetByIdAsync(experienceId, cache => default);
        }

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
        /// <param name="experience">Filter by education name</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the experience
        /// </returns>

        public virtual async Task<IPagedList<Experience>> GetAllExperienceAsync(int employeeId,string experienceName,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, bool? overridePublished = null)
        {
            return await _experienceRepository.GetAllPagedAsync(async query =>
            {
                if (employeeId != 0)
                {
                    query = query.Where(b => b.EmployeeID == employeeId);
                }
                return query.OrderByDescending(c => c.Id);
            }, pageIndex, pageSize);
        }

        /// <summary>
        /// Inserts a experience
        /// </summary>
        /// <param name="experience">Experience</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task InsertExperienceAsync(Experience experience)
        {
            await _experienceRepository.InsertAsync(experience);
        }

        /// <summary>
        /// Updates the Experience
        /// </summary>
        /// <param name="experience">Experience</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task UpdateExperienceAsync(Experience experience)
        {
            await _experienceRepository.UpdateAsync(experience);
        }

        public virtual async Task<IList<Experience>> GetExperienceByIdsAsync(int[] experienceIds)
        {
            return await _experienceRepository.GetByIdsAsync(experienceIds, cache => default, false);
        }

        #endregion

        #endregion
    }
}