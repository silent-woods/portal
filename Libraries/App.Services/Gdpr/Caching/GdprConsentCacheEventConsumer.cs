using System.Threading.Tasks;
using App.Core.Domain.Gdpr;
using App.Services.Caching;

namespace App.Services.Gdpr.Caching
{
    /// <summary>
    /// Represents a GDPR consent cache event consumer
    /// </summary>
    public partial class GdprConsentCacheEventConsumer : CacheEventConsumer<GdprConsent>
    {
    }
}