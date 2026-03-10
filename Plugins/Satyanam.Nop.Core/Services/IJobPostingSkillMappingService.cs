using Satyanam.Nop.Core.Domains;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Satyanam.Nop.Core.Services
{
    /// <summary>
    /// IJobPostingSkillMapping service interface
    /// </summary>
    public partial interface IJobPostingSkillMappingService
    {
        #region  JobPostingSkillMapping
        /// <summary>
        /// Get job posting skill mapping by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<JobPostingSkillMapping> GetByIdAsync(int id);
        /// <summary>
        /// Get job posting skill mapping by job posting id
        /// </summary>
        /// <param name="jobPostingId"></param>
        /// <returns></returns>
        Task<IList<JobPostingSkillMapping>> GetByJobPostingIdAsync(int jobPostingId);
        /// <summary>
        /// Get job posting skill mapping by skill set id
        /// </summary>
        /// <param name="skillSetId"></param>
        /// <returns></returns>
        Task<IList<JobPostingSkillMapping>> GetBySkillSetIdAsync(int skillSetId);
        /// <summary>
        /// Insert job posting skill mapping
        /// </summary>
        /// <param name="mapping"></param>
        /// <returns></returns>
        Task InsertAsync(JobPostingSkillMapping mapping);
        /// <summary>
        /// Delete job posting skill mapping
        /// </summary>
        /// <param name="mapping"></param>
        /// <returns></returns>
        Task DeleteAsync(JobPostingSkillMapping mapping);
        /// <summary>
        /// Delete job posting skill mapping by job posting id
        /// </summary>
        /// <param name="jobPostingId"></param>
        /// <returns></returns>
        Task DeleteByJobPostingIdAsync(int jobPostingId);
        #endregion
    }
}