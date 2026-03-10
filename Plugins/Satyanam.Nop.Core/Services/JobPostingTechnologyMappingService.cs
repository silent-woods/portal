using App.Data;
using Satyanam.Nop.Core.Domains;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Satyanam.Nop.Core.Services
{
    /// <summary>
    /// JobPostingTechnologyMapping  service
    /// </summary>
    public partial class JobPostingTechnologyMappingService : IJobPostingTechnologyMappingService
    {
        /// <summary>
        /// JobPostingTechnologyMapping  service 
        /// </summary>

        private readonly IRepository<JobPostingTechnologyMapping> _repository;

        public JobPostingTechnologyMappingService(IRepository<JobPostingTechnologyMapping> repository)
        {
            _repository = repository;
        }

        #region JobPostingTechnologyMapping
        public async Task<IList<JobPostingTechnologyMapping>> GetByJobPostingIdAsync(int jobPostingId)
        {
            var query = await _repository.GetAllAsync(q =>
                q.Where(x => x.JobPostingId == jobPostingId));

            return query.ToList();
        }
        public async Task InsertAsync(JobPostingTechnologyMapping entity)
        {
            await _repository.InsertAsync(entity);
        }
        public async Task DeleteByJobPostingIdAsync(int jobPostingId)
        {
            var mappings = await GetByJobPostingIdAsync(jobPostingId);

            foreach (var mapping in mappings)
                await _repository.DeleteAsync(mapping);
        }
        #endregion
    }
}