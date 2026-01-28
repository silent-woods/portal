using App.Core.Domain.Messages;
using App.Services.Caching;

namespace App.Services.Messages.Caching
{
    /// <summary>
    /// Represents an queued email cache event consumer
    /// </summary>
    public partial class QueuedEmailCacheEventConsumer : CacheEventConsumer<QueuedEmail>
    {
    }
}
