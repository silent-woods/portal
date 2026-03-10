using App.Core;
using App.Data;
using Satyanam.Nop.Core.Domains;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Satyanam.Nop.Core.Services
{
    /// <summary>
    /// Technology service
    /// </summary>
    public partial class TechnologyService : ITechnologyService
    {
        private readonly IRepository<Technology> _technologyRepository;

        public TechnologyService(IRepository<Technology> technologyRepository)
        {
            _technologyRepository = technologyRepository;
        }

        #region Technology

        public async Task<Technology> GetTechnologyByIdAsync(int id)
        {
            return await _technologyRepository.GetByIdAsync(id);
        }

        public async Task<IList<Technology>> GetTechnologyByIdsAsync(int[] technologyIds)
        {
            return await _technologyRepository.GetByIdsAsync(technologyIds);
        }

        public async Task<IPagedList<Technology>> GetAllTechnologyAsync(
            string name = "",
            bool? published = null,
            int pageIndex = 0,
            int pageSize = int.MaxValue)
        {
            var query = await _technologyRepository.GetAllAsync(q =>
            {
                if (!string.IsNullOrWhiteSpace(name))
                    q = q.Where(x => x.Name.Contains(name));

                if (published.HasValue)
                    q = q.Where(x => x.Published == published.Value);

                return q.OrderBy(x => x.DisplayOrder)
                        .ThenBy(x => x.Name);
            });

            return new PagedList<Technology>(query, pageIndex, pageSize);
        }
        public async Task<IList<Technology>> GetAllPublishedTechnologiesAsync()
        {
            var query = await _technologyRepository.GetAllAsync(q =>
                 q.OrderBy(x => x.DisplayOrder)
                 .ThenBy(x => x.Name));

            return query.ToList();
        }

        public async Task InsertTechnologyAsync(Technology technology)
        {
            await _technologyRepository.InsertAsync(technology);
        }

        public async Task UpdateTechnologyAsync(Technology technology)
        {
            await _technologyRepository.UpdateAsync(technology);
        }

        public async Task DeleteTechnologyAsync(Technology technology)
        {
            await _technologyRepository.DeleteAsync(technology);
        }

        #endregion
    }
}