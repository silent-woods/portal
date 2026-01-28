using App.Core.Domain.Media;
using App.Services.Caching;

namespace App.Services.Media.Caching
{
    /// <summary>
    /// Represents a download cache event consumer
    /// </summary>
    public partial class DownloadCacheEventConsumer : CacheEventConsumer<Download>
    {
    }
}
