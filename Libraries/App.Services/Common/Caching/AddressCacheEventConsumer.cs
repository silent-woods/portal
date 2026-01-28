using System.Threading.Tasks;
using App.Core.Domain.Common;
using App.Services.Caching;
using App.Services.Customers;

namespace App.Services.Common.Caching
{
    /// <summary>
    /// Represents a address cache event consumer
    /// </summary>
    public partial class AddressCacheEventConsumer : CacheEventConsumer<Address>
    {
        /// <summary>
        /// Clear cache data
        /// </summary>
        /// <param name="entity">Entity</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected override async Task ClearCacheAsync(Address entity)
        {
            await RemoveByPrefixAsync(NopCustomerServicesDefaults.CustomerAddressesPrefix);
        }
    }
}
