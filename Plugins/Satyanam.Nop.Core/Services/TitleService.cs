using App.Core;
using App.Data;
using App.Data.Extensions;
using Satyanam.Nop.Core.Domains;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Satyanam.Nop.Core.Services
{
    /// <summary>
    /// Title service
    /// </summary>
    public partial class TitleService : ITitleService
    {
        #region Fields

        private readonly IRepository<Title> _titleRepository;

        #endregion

        #region Ctor

        public TitleService(
            IRepository<Title> titleRepository)
        {
            _titleRepository = titleRepository;
        }

        #endregion

        #region Methods

        #region Title

        /// <summary>
        /// Gets all Title
        /// </summary>
        /// <param name="name">name</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <param name="title">Filter by title name</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the title
        /// </returns>

        public virtual async Task<IPagedList<Title>> GetAllTitleAsync(string name, int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false)
        {
            var query = await _titleRepository.GetAllAsync(async query =>
            {
                if (!string.IsNullOrWhiteSpace(name))
                    query = query.Where(c => c.Name.Contains(name));

                return query.OrderBy(c => c.Id);
            });
            //paging
            return new PagedList<Title>(query.ToList(), pageIndex, pageSize);
        }

        /// <summary>
        /// Gets a Title
        /// </summary>
        /// <param name="titleId">title identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the title
        /// </returns>
        public virtual async Task<Title> GetTitleByIdAsync(int titleId)
        {
            return await _titleRepository.GetByIdAsync(titleId);
        }

        public virtual async Task<IList<Title>> GetTitleByIdsAsync(int[] titleIds)
        {
            return await _titleRepository.GetByIdsAsync(titleIds);
        }


        /// <summary>
        /// Inserts a Title
        /// </summary>
        /// <param name="title">Title</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task InsertTitleAsync(Title title)
        {
            await _titleRepository.InsertAsync(title);
        }

        /// <summary>
        /// Updates the Title
        /// </summary>
        /// <param name="title">title</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task UpdateTitleAsync(Title title)
        {
            await _titleRepository.UpdateAsync(title);
        }

        /// <summary>
        /// Deletes a Title
        /// </summary>
        /// <param name="title">Title</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task DeleteTitleAsync(Title title)
        {
            await _titleRepository.DeleteAsync(title);
        }
        public async Task<bool> TitleExistsAsync(string name, int excludeId = 0)
        {
            return await _titleRepository.Table
                .AnyAsync(t => t.Name.ToLower() == name.ToLower() && t.Id != excludeId);
        }
        public virtual async Task<IPagedList<Title>> GetAllTitleByNameAsync(string titleName,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, bool? overridePublished = null)
        {
            var query = await _titleRepository.GetAllAsync(async query =>
            {
                return query.OrderByDescending(c => c.Id);
            });
            //paging
            return new PagedList<Title>(query.ToList(), pageIndex, pageSize);
        }


        private static readonly Dictionary<string, string> TitleMappings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
{
    { "CEO", "CEO" },
    { "CEO & Founder", "CEO" },
    { "CEO and President", "CEO" },
    { "President & CEO", "CEO" },
    { "CEO/President", "CEO" },
    { "Chief Executive Officer", "CEO" },
    { "Owner", "Owner" },
    { "Co Owner", "Owner" },
    { "Owner CEO", "Owner" },
    { "Company Owner", "Owner" },
    { "Owner/Director", "Owner" },
    { "Small Business Owner", "Owner" },
    { "Business Owner", "Owner" },
    { "President", "President" },
    { "President/Owner", "President" },
    { "Co-Founder", "Co-Founder" },
    { "Co-Founder & CEO", "Co-Founder" },
    { "Co-Founder and CEO", "Co-Founder" },
    { "Co-Founder / CEO", "Co-Founder" }
};


        private string NormalizeTitle(string titleName)
        {
            if (string.IsNullOrWhiteSpace(titleName))
                return null;

            // Check if the title exists in the mapping
            return TitleMappings.TryGetValue(titleName.Trim(), out var normalizedTitle) ? normalizedTitle : titleName.Trim();
        }
        public async Task<Title> GetOrCreateTitleByNameAsync(string titleName)
        {
            if (string.IsNullOrWhiteSpace(titleName))
                return null;

            var normalizedTitleName = NormalizeTitle(titleName);

            // Try to find an existing title
            var existingTitle = (await GetAllTitleAsync(normalizedTitleName)).FirstOrDefault();
            if (existingTitle != null)
                return existingTitle;

            // Create new title if not found
            var newTitle = new Title { Name = normalizedTitleName };
            await _titleRepository.InsertAsync(newTitle);
            return newTitle;
        }
        #endregion

        #endregion
    }
}