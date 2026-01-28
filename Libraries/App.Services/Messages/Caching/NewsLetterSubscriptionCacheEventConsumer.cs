using App.Core.Domain.Messages;
using App.Services.Caching;

namespace App.Services.Messages.Caching
{
    /// <summary>
    /// Represents news letter subscription cache event consumer
    /// </summary>
    public partial class NewsLetterSubscriptionCacheEventConsumer : CacheEventConsumer<NewsLetterSubscription>
    {    
    }
}
