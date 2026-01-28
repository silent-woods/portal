using App.Core.Domain.Customers;
using App.Services.Caching;

namespace App.Services.Customers.Caching
{
    /// <summary>
    /// Represents a reward point history cache event consumer
    /// </summary>
    public partial class RewardPointsHistoryCacheEventConsumer : CacheEventConsumer<RewardPointsHistory>
    {
    }
}
