using App.Core.Domain.Logging;
using App.Services.Caching;

namespace App.Services.Logging.Caching
{
    /// <summary>
    /// Represents an activity log cache event consumer
    /// </summary>
    public partial class ActivityLogCacheEventConsumer : CacheEventConsumer<ActivityLog>
    {
    }
}