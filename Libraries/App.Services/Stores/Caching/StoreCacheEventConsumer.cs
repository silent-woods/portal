using App.Core.Caching;
using App.Core.Domain.Stores;
using App.Services.Caching;
using App.Services.Localization;
using System.Threading.Tasks;

namespace App.Services.Stores.Caching
{
    /// <summary>
    /// Represents a store cache event consumer
    /// </summary>
    public partial class StoreCacheEventConsumer : CacheEventConsumer<Store>
    {
        /// <summary>
        /// Clear cache data
        /// </summary>
        /// <param name="entity">Entity</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected override async Task ClearCacheAsync(Store entity)
        {
            await RemoveByPrefixAsync(NopLocalizationDefaults.LanguagesByStorePrefix, entity);
        }
    }
}
