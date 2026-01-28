using App.Core.Domain.Polls;
using App.Services.Caching;

namespace App.Services.Polls.Caching
{
    /// <summary>
    /// Represents a poll answer cache event consumer
    /// </summary>
    public partial class PollAnswerCacheEventConsumer : CacheEventConsumer<PollAnswer>
    {
    }
}