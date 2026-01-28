using System.Threading.Tasks;
using App.Core.Domain.Forums;
using App.Services.Caching;

namespace App.Services.Forums.Caching
{
    /// <summary>
    /// Represents a forum cache event consumer
    /// </summary>
    public partial class ForumCacheEventConsumer : CacheEventConsumer<Forum>
    {
        /// <summary>
        /// Clear cache data
        /// </summary>
        /// <param name="entity">Entity</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected override async Task ClearCacheAsync(Forum entity)
        {
            await RemoveAsync(NopForumDefaults.ForumByForumGroupCacheKey, entity.ForumGroupId);
        }
    }
}
