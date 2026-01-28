using App.Core;
using App.Data;
using App.Data.Extensions;
using Satyanam.Nop.Core.Domains;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Satyanam.Nop.Core.Services
{
    /// <summary>
    /// Tags service
    /// </summary>
    public partial class TagsService : ITagsService
    {
        #region Fields

        private readonly IRepository<Tags> _tagsRepository;

        #endregion

        #region Ctor

        public TagsService(
            IRepository<Tags> tagsRepository)
        {
            _tagsRepository = tagsRepository;
        }

        #endregion

        #region Methods

        #region Tags

        /// <summary>
        /// Gets all Tags
        /// </summary>
        /// <param name="name">name</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <param name="tags">Filter by tags name</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the tags
        /// </returns>

        public virtual async Task<IPagedList<Tags>> GetAllTagsAsync(string name, int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false)
        {
            var query = await _tagsRepository.GetAllAsync(async query =>
            {
                if (!string.IsNullOrWhiteSpace(name))
                    query = query.Where(c => c.Name.Contains(name));

                return query.OrderBy(c => c.Id);
            });
            //paging
            return new PagedList<Tags>(query.ToList(), pageIndex, pageSize);
        }

        /// <summary>
        /// Gets a Tags
        /// </summary>
        /// <param name="tagsId">tags identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the tags
        /// </returns>
        public virtual async Task<Tags> GetTagsByIdAsync(int tagsId)
        {
            return await _tagsRepository.GetByIdAsync(tagsId);
        }

        public virtual async Task<IList<Tags>> GetTagsByIdsAsync(int[] tagsIds)
        {
            return await _tagsRepository.GetByIdsAsync(tagsIds);
        }


        /// <summary>
        /// Inserts a Tags
        /// </summary>
        /// <param name="tags">Tags</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task InsertTagsAsync(Tags tags)
        {
            await _tagsRepository.InsertAsync(tags);
        }

        /// <summary>
        /// Updates the Tags
        /// </summary>
        /// <param name="tags">Tags</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task UpdateTagsAsync(Tags tags)
        {
            await _tagsRepository.UpdateAsync(tags);
        }

        /// <summary>
        /// Deletes a Tags
        /// </summary>
        /// <param name="tags">Tags</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task DeleteTagsAsync(Tags tags)
        {
            await _tagsRepository.DeleteAsync(tags);
        }

        public async Task<Tags> GetOrCreateTagsByNameAsync(string tagsName)
        {
            if (string.IsNullOrWhiteSpace(tagsName))
                return null;

            // Check if tags exists (case-insensitive search)
            var tag = await _tagsRepository.Table
                .FirstOrDefaultAsync(t => t.Name.ToLower() == tagsName.ToLower());

            if (tag == null)
            {
                // Tags not found, create a new one
                tag = new Tags { Name = tagsName };
                await _tagsRepository.InsertAsync(tag);
            }

            return tag;
        }
        #endregion

        #endregion
    }
}