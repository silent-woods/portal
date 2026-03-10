using App.Data;
using Satyanam.Nop.Core.Domains;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Satyanam.Nop.Core.Services
{
    /// <summary>
    /// JobPostingSkillMapping service
    /// </summary>
    public partial class JobPostingSkillMappingService : IJobPostingSkillMappingService
    {
        /// <summary>
        /// JobPostingSkillMapping service
        /// </summary>

        private readonly IRepository<JobPostingSkillMapping> _jobPostingSkillMappingRepository;

        public JobPostingSkillMappingService(IRepository<JobPostingSkillMapping> jobPostingSkillMappingRepository)
        {
            _jobPostingSkillMappingRepository = jobPostingSkillMappingRepository;
        }

        #region JobPostingSkillMapping

        public async Task<JobPostingSkillMapping> GetByIdAsync(int id)
        {
            return await _jobPostingSkillMappingRepository.GetByIdAsync(id);
        }

        public async Task<IList<JobPostingSkillMapping>> GetByJobPostingIdAsync(int jobPostingId)
        {
            var query = await _jobPostingSkillMappingRepository.GetAllAsync(q =>
                q.Where(x => x.JobPostingId == jobPostingId));

            return query.ToList();
        }

        public async Task<IList<JobPostingSkillMapping>> GetBySkillSetIdAsync(int skillSetId)
        {
            var query = await _jobPostingSkillMappingRepository.GetAllAsync(q =>
                q.Where(x => x.SkillSetId == skillSetId));

            return query.ToList();
        }

        public async Task InsertAsync(JobPostingSkillMapping mapping)
        {
            await _jobPostingSkillMappingRepository.InsertAsync(mapping);
        }

        public async Task DeleteAsync(JobPostingSkillMapping mapping)
        {
            await _jobPostingSkillMappingRepository.DeleteAsync(mapping);
        }

        public async Task DeleteByJobPostingIdAsync(int jobPostingId)
        {
            var mappings = await GetByJobPostingIdAsync(jobPostingId);

            foreach (var mapping in mappings)
            {
                await _jobPostingSkillMappingRepository.DeleteAsync(mapping);
            }
        }

        #endregion
    }
}