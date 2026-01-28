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
    /// Categorys service
    /// </summary>
    public partial class CategorysService : ICategorysService
    {
        #region Fields

        private readonly IRepository<Categorys> _categorysRepository;

        #endregion

        #region Ctor

        public CategorysService(
            IRepository<Categorys> categorysRepository)
        {
            _categorysRepository = categorysRepository;
        }

        #endregion

        #region Methods

        #region Categorys

        /// <summary>
        /// Gets all Categorys
        /// </summary>
        /// <param name="name">name</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <param name="categorys">Filter by categorys name</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the categorys
        /// </returns>

        public virtual async Task<IPagedList<Categorys>> GetAllCategorysAsync(string name, int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false)
        {
            var query = await _categorysRepository.GetAllAsync(async query =>
            {
                if (!string.IsNullOrWhiteSpace(name))
                    query = query.Where(c => c.Name.Contains(name));

                return query.OrderBy(c => c.Id);
            });
            //paging
            return new PagedList<Categorys>(query.ToList(), pageIndex, pageSize);
        }

        /// <summary>
        /// Gets a Categorys
        /// </summary>
        /// <param name="categorysId">categorys identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the categorys
        /// </returns>
        public virtual async Task<Categorys> GetCategorysByIdAsync(int categorysId)
        {
            return await _categorysRepository.GetByIdAsync(categorysId);
        }

        public virtual async Task<IList<Categorys>> GetCategorysByIdsAsync(int[] categorysIds)
        {
            return await _categorysRepository.GetByIdsAsync(categorysIds);
        }


        /// <summary>
        /// Inserts a Categorys
        /// </summary>
        /// <param name="categorys">Categorys</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task InsertCategorysAsync(Categorys categorys)
        {
            await _categorysRepository.InsertAsync(categorys);
        }

        /// <summary>
        /// Updates the Categorys
        /// </summary>
        /// <param name="categorys">categorys</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task UpdateCategorysAsync(Categorys categorys)
        {
            await _categorysRepository.UpdateAsync(categorys);
        }

        /// <summary>
        /// Deletes a Categorys
        /// </summary>
        /// <param name="categorys">Categorys</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task DeleteCategorysAsync(Categorys categorys)
        {
            await _categorysRepository.DeleteAsync(categorys);
        }
        public virtual async Task<IPagedList<Categorys>> GetAllCategorysByNameAsync(string categoryName,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, bool? overridePublished = null)
        {
            var query = await _categorysRepository.GetAllAsync(async query =>
            {
                return query.OrderByDescending(c => c.Id);
            });
            //paging
            return new PagedList<Categorys>(query.ToList(), pageIndex, pageSize);
        }

        public async Task<Categorys> GetOrCreateCategorysByNameAsync(string categoryName)
        {
            if (string.IsNullOrWhiteSpace(categoryName))
                return null;

            var category = await _categorysRepository.Table
                .FirstOrDefaultAsync(i => i.Name.ToLower() == categoryName.ToLower());

            if (category == null)
            {
                category = new Categorys { Name = categoryName };
                await _categorysRepository.InsertAsync(category);
            }

            return category;
        }
        #endregion

        #endregion
    }
}