using App.Core;
using App.Data;
using Satyanam.Nop.Core.Domains;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Satyanam.Nop.Core.Services
{
    /// <summary>
    /// SkillSet service
    /// </summary>
    public partial class SkillSetService : ISkillSetService
    {
        private readonly IRepository<SkillSet> _skillSetRepository;

        public SkillSetService(IRepository<SkillSet> skillSetRepository)
        {
            _skillSetRepository = skillSetRepository;
        }

        #region SkillSet

        public async Task<SkillSet> GetSkillByIdAsync(int id)
        {
            return await _skillSetRepository.GetByIdAsync(id);
        }
        public async Task<IList<SkillSet>> GetSkillByIdsAsync(int[] skillIds)
        {
            return await _skillSetRepository.GetByIdsAsync(skillIds);
        }
        public async Task<IPagedList<SkillSet>> GetAllSkillsAsync(string name = "",bool? published = null,int pageIndex = 0,int pageSize = int.MaxValue)
        {
            var query = await _skillSetRepository.GetAllAsync(q =>
            {
                if (!string.IsNullOrWhiteSpace(name))
                    q = q.Where(x => x.Name.Contains(name));

                if (published.HasValue)
                    q = q.Where(x => x.Published == published.Value);

                return q.OrderBy(x => x.DisplayOrder)
                        .ThenBy(x => x.Name);
            });

            return new PagedList<SkillSet>(query, pageIndex, pageSize);
        }
        public async Task InsertSkillAsync(SkillSet skill)
        {
            await _skillSetRepository.InsertAsync(skill);
        }

        public async Task UpdateSkillAsync(SkillSet skill)
        {
            await _skillSetRepository.UpdateAsync(skill);
        }
        public async Task DeleteSkillAsync(SkillSet skill)
        {
            await _skillSetRepository.DeleteAsync(skill);
        }
        #endregion
    }
}