using App.Data;
using Satyanam.Nop.Core.Domains;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Satyanam.Nop.Core.Services
{
    /// <summary>
    /// TechnologySkillMapping service
    /// </summary>
    public partial class TechnologySkillMappingService : ITechnologySkillMappingService
    {
        private readonly IRepository<TechnologySkillMapping> _mappingRepository;

        public TechnologySkillMappingService(IRepository<TechnologySkillMapping> mappingRepository)
        {
            _mappingRepository = mappingRepository;
        }

        #region TechnologySkillMapping
        public async Task<IList<TechnologySkillMapping>> GetBySkillSetIdAsync(int skillSetId)
        {
            var query = await _mappingRepository.GetAllAsync(q =>
                q.Where(x => x.SkillSetId == skillSetId));

            return query.ToList();
        }

        public async Task<IList<TechnologySkillMapping>> GetByTechnologyIdAsync(int technologyId)
        {
            var query = await _mappingRepository.GetAllAsync(q =>
                q.Where(x => x.TechnologyId == technologyId));

            return query.ToList();
        }

        public async Task InsertAsync(TechnologySkillMapping mapping)
        {
            await _mappingRepository.InsertAsync(mapping);
        }

        public async Task DeleteAsync(TechnologySkillMapping mapping)
        {
            await _mappingRepository.DeleteAsync(mapping);
        }

        public async Task DeleteBySkillSetIdAsync(int skillSetId)
        {
            var mappings = await GetBySkillSetIdAsync(skillSetId);

            foreach (var mapping in mappings)
            {
                await _mappingRepository.DeleteAsync(mapping);
            }
        }

        #endregion
    }
}