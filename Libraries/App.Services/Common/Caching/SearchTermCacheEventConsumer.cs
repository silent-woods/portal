using App.Core.Domain.Common;
using App.Services.Caching;

namespace App.Services.Common.Caching
{
    /// <summary>
    /// Represents a search term cache event consumer
    /// </summary>
    public partial class SearchTermCacheEventConsumer : CacheEventConsumer<SearchTerm>
    {
    }
}
