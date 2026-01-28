using App.Core.Domain.Customers;
using App.Services.Caching;

namespace App.Services.Customers.Caching
{
    /// <summary>
    /// Represents a customer password cache event consumer
    /// </summary>
    public partial class CustomerPasswordCacheEventConsumer : CacheEventConsumer<CustomerPassword>
    {
    }
}