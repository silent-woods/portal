using App.Core.Domain.Logging;
using App.Services.Caching;
using System.Threading.Tasks;

namespace App.Services.Logging.Caching
{
    /// <summary>
    /// Represents a activity log type cache event consumer
    /// </summary>
    public partial class ActivityLogTypeCacheEventConsumer : CacheEventConsumer<ActivityLogType>
    {
    }
}
