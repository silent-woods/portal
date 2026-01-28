using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App.Core;
using App.Core.Domain.Affiliates;
using App.Core.Domain.JobPostings;
using App.Data;
using App.Services.JobPostings;

namespace App.Services.JobPostings
{
    /// <summary>
    /// Projects service
    /// </summary>
    public partial class JobPostingService : IJobPostingService
    {
        #region Fields

        private readonly IRepository<JobPosting> _projectRepository;

        #endregion

        #region Ctor

        public JobPostingService(IRepository<JobPosting> projectRepository
           )
        {
            _projectRepository = projectRepository;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Get all project
        /// </summary>
        /// <param name="projectName"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="showHidden"></param>
        /// <param name="overridePublished"></param>
        /// <returns></returns>
        public virtual async Task<IPagedList<JobPosting>> GetAllJobPostingAsync(string tittle,int positionid,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, bool? overridePublished = null)
        {
            var query = await _projectRepository.GetAllAsync(async query =>
            {
                if (!string.IsNullOrWhiteSpace(tittle))
                    query = query.Where(c => c.Title.Contains(tittle));
                if (positionid > 0)
                    query = query.Where(c => positionid == c.PositionId);

                return query.OrderByDescending(c => c.CreatedOn);
            });
            //paging
            return new PagedList<JobPosting>(query.ToList(), pageIndex, pageSize);
        }

        /// <summary>
        /// Get project by id
        /// </summary>
        /// <param name="projectId"></param>
        /// <returns></returns>
        public virtual async Task<JobPosting> GetJobPostingByIdAsync(int projectId)
        {
            return await _projectRepository.GetByIdAsync(projectId, cache => default);
        }

        /// <summary>
        /// Get project by ids
        /// </summary>
        /// <param name="projectIds"></param>
        /// <returns></returns>
        public virtual async Task<IList<JobPosting>> GetJobPostingByIdsAsync(int[] projectIds)
        {
            return await _projectRepository.GetByIdsAsync(projectIds, cache => default, false);
        }

        /// <summary>
        /// Insert project
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        public virtual async Task InsertJobPostingAsync(JobPosting project)
        {
            await _projectRepository.InsertAsync(project);
        }

        /// <summary>
        /// Update project
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual async Task UpdateJobPostingAsync(JobPosting project)
        {
            if (project == null)
                throw new ArgumentNullException(nameof(project));

            await _projectRepository.UpdateAsync(project);
        }

        /// <summary>
        /// delete project by record
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        public virtual async Task DeleteJobPostingAsync(JobPosting project)
        {
            await _projectRepository.DeleteAsync(project, false);
        }
        #endregion
    }
}