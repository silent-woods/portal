using App.Core.Domain.Affiliates;
using App.Services.Caching;

namespace App.Services.Affiliates.Caching
{
    /// <summary>
    /// Represents an affiliate cache event consumer
    /// </summary>
    public partial class AffiliateCacheEventConsumer : CacheEventConsumer<Affiliate>
    {
    }
}
