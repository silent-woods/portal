using System.Threading.Tasks;
using App.Core.Domain.Common;
using App.Services.Caching;

namespace App.Services.Common.Caching
{
    /// <summary>
    /// Represents a address attribute cache event consumer
    /// </summary>
    public partial class AddressAttributeCacheEventConsumer : CacheEventConsumer<AddressAttribute>
    {
        /// <summary>
        /// Clear cache data
        /// </summary>
        /// <param name="entity">Entity</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected override async Task ClearCacheAsync(AddressAttribute entity)
        {
            await RemoveAsync(NopCommonDefaults.AddressAttributeValuesByAttributeCacheKey, entity);
        }
    }
}
