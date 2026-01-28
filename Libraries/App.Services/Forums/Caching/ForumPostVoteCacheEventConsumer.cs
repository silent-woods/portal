using App.Core.Domain.Forums;
using App.Services.Caching;

namespace App.Services.Forums.Caching
{
    /// <summary>
    /// Represents a forum post vote cache event consumer
    /// </summary>
    public partial class ForumPostVoteCacheEventConsumer : CacheEventConsumer<ForumPostVote>
    {
    }
}
