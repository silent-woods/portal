using Satyanam.Nop.Core.Domains;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Satyanam.Nop.Core.Services
{
    /// <summary>
    /// IJobPostingTechnologyMapping service interface
    /// </summary>
    public partial interface IJobPostingTechnologyMappingService
    {
        #region  JobPostingTechnologyMapping
        Task<IList<JobPostingTechnologyMapping>> GetByJobPostingIdAsync(int jobPostingId);
        Task InsertAsync(JobPostingTechnologyMapping entity);
        Task DeleteByJobPostingIdAsync(int jobPostingId);

        #endregion
    }
}