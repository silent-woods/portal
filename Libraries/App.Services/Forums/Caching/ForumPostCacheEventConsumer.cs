using App.Core.Domain.Forums;
using App.Services.Caching;

namespace App.Services.Forums.Caching
{
    /// <summary>
    /// Represents a forum post cache event consumer
    /// </summary>
    public partial class ForumPostCacheEventConsumer : CacheEventConsumer<ForumPost>
    {
    }
}
