using App.Core;
using App.Core.Caching;
using App.Data;
using App.Services.Catalog;
using App.Services.Stores;
using Nop.Core.Domain.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Services.Recruitements
{
    /// <summary>
    /// Represents service shipping by weight service implementation
    /// </summary>
    public class RecruitementService : IRecruitementService
    {
        #region Constants

        /// <summary>
        /// Key for caching all records
        /// </summary>
        private readonly CacheKey _recruitementAllKey = new("Nop.Recruitement.all", RECRUITEMEN_PATTERN_KEY);
        private const string RECRUITEMEN_PATTERN_KEY = "Nop.Recruitement.";

        #endregion

        #region Fields

        private readonly IRepository<Questions> _questions;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly IStoreContext _storeContext;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IWorkContext _workContext;

        #endregion

        #region Ctor

        public RecruitementService(IRepository<Questions> questions,
            IStaticCacheManager staticCacheManager,
             IStoreContext storeContext,
             IStoreMappingService storeMappingServic, IWorkContext workContext
             )
        {
            _questions = questions;
            _staticCacheManager = staticCacheManager;
            _storeContext = storeContext;
            _storeMappingService = storeMappingServic;
            _workContext = workContext;

        }

        #endregion

        #region Methods

        /// <summary>
        /// Get all shipping by weight records
        /// </summary>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the list of the shipping by weight record
        /// </returns>




        #endregion
        public virtual async Task<Questions> GetrecruitementByIdAsync(int recid)
        {
            return await _questions.GetByIdAsync(recid, cache => default);
        }
        public async Task<IPagedList<Questions>> GetPagedReqlistAsync(string Category, int pageIndex = 0, int pageSize = int.MaxValue, DateTime? createdToUtc = null, DateTime? createdFromUtc = null)
        {
            //filter by the Questions
            var query = await _questions.GetAllAsync(async query =>
            {
                if (!string.IsNullOrWhiteSpace(Category))
                    //query = query.Where(c => c.CategoryId;
                    if (createdFromUtc.HasValue)
                        query = query.Where(c => createdFromUtc.Value <= c.CreatedOn);
                if (createdToUtc.HasValue)
                    query = query.Where(c => createdToUtc.Value >= c.CreatedOn);
                query = query.OrderBy(configuration => configuration.Id);
                query = query.OrderBy(category => category.DisplayOrder).ThenBy(category => category.Id);
                // query = query.OrderByDescending(c => c.CreatedOn);
                return query.OrderBy(c => c.DisplayOrder).ThenBy(c => c.Id);
            });


            return new PagedList<Questions>(query, pageIndex, pageSize);
        }


        public virtual async Task UpdaterecruitementAsync(Questions questions)
        {
            if (questions == null)
                throw new ArgumentNullException(nameof(questions));

            //validate category hierarchy
            var parentCategory = await GetrecruitementByIdAsync(questions.Id);
            while (parentCategory != null)
            {
                if (questions.Id == parentCategory.Id)
                {
                    questions.Id = 0;
                    break;
                }
                parentCategory = await GetrecruitementByIdAsync(questions.Id);
            }



            await _staticCacheManager.RemoveByPrefixAsync(RECRUITEMEN_PATTERN_KEY);

        }
        public virtual async Task<IList<Questions>> GetrecruitementIdsAsync(int[] questionsIds)
        {
            return await _questions.GetByIdsAsync(questionsIds, includeDeleted: false);
        }




        public virtual async Task DeleterecruitementAsync(IList<Questions> questions)
        {
            if (questions == null)
                throw new ArgumentNullException(nameof(questions));

            foreach (var question in questions)
                await DeleteAsync(question);
        }

        public async Task InsertrecAsync(Questions questions)
        {
            await _questions.InsertAsync(questions);

        }
        public async Task UpdateAsync(Questions questions)
        {
            await _questions.UpdateAsync(questions);

        }

        public virtual async Task<IList<Questions>> GetAllCategoriesByParentCategoryIdAsync(int parentCategoryId,
           bool showHidden = false)
        {
            var store = await _storeContext.GetCurrentStoreAsync();
            var customer = await _workContext.GetCurrentCustomerAsync();
            var categories = await _questions.GetAllAsync(async query =>
            {
                if (!showHidden)
                {
                    query = query.Where(c => c.Published);

                    //apply store mapping constraints
                    //query = await _storeMappingService.ApplyStoreMapping(query, store.Id);

                    //apply ACL constraints
                    // query = await _aclService.ApplyAcl(query, customer);
                }

                //  query = query.Where(c => !c.Deleted && c.ParentCategoryId == parentCategoryId);

                return query.OrderBy(c => c.Id).ThenBy(c => c.Id);

            }, cache => cache.PrepareKeyForDefaultCache(NopCatalogDefaults.CategoriesByParentCategoryCacheKey,
                parentCategoryId, showHidden, customer, store));

            return categories;
        }

        public virtual async Task DeleteAsync(Questions rts)
        {
            await _questions.DeleteAsync(rts);
            var subcategories = await GetAllCategoriesByParentCategoryIdAsync(rts.Id, true);
            foreach (var subcategory in subcategories)
            {
                subcategory.Id = 0;
                await UpdaterecruitementAsync(subcategory);
            }
        }
    }
}