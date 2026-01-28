using App.Core.Domain.Logging;
using App.Services.Caching;

namespace App.Services.Logging.Caching
{
    /// <summary>
    /// Represents a log cache event consumer
    /// </summary>
    public partial class LogCacheEventConsumer : CacheEventConsumer<Log>
    {
    }
}
