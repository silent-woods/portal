using App.Core.Domain.Forums;
using App.Services.Caching;

namespace App.Services.Forums.Caching
{
    /// <summary>
    /// Represents a forum topic cache event consumer
    /// </summary>
    public partial class ForumTopicCacheEventConsumer : CacheEventConsumer<ForumTopic>
    {
    }
}
