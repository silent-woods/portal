using System.Collections.Generic;
using System.Threading.Tasks;
using App.Core;
using App.Core.Domain.JobPostings;
using App.Core.Domain.Projects;

namespace App.Services.JobPostings
{
    /// <summary>
    /// Category service interface
    /// </summary>
    public partial interface IJobPostingService
    {
        /// <summary>
        /// Gets all Projects
        /// </summary>
        Task<IPagedList<JobPosting>> GetAllJobPostingAsync(string tittle,int positionid,
             int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, bool? overridePublished = null);

        /// <summary>
        /// Get project by id
        /// </summary>
        /// <param name="projectId"></param>
        /// <returns></returns>
        Task<JobPosting> GetJobPostingByIdAsync(int projectId);

        /// <summary>
        /// Insert project
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        Task InsertJobPostingAsync(JobPosting project);

        /// <summary>
        /// Update project
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        Task UpdateJobPostingAsync(JobPosting project);

        /// <summary>
        /// Delete project
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        Task DeleteJobPostingAsync(JobPosting project);

        /// <summary>
        /// Get project by ids
        /// </summary>
        /// <param name="designationIds"></param>
        /// <returns></returns>
        Task<IList<JobPosting>> GetJobPostingByIdsAsync(int[] projectIds);
    }
}