using System.Threading.Tasks;
using App.Core.Domain.Customers;
using App.Services.Caching;

namespace App.Services.Customers.Caching
{
    /// <summary>
    /// Represents a customer role cache event consumer
    /// </summary>
    public partial class CustomerRoleCacheEventConsumer : CacheEventConsumer<CustomerRole>
    {
        /// <summary>
        /// Clear cache data
        /// </summary>
        /// <param name="entity">Entity</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected override async Task ClearCacheAsync(CustomerRole entity)
        {
            await RemoveByPrefixAsync(NopCustomerServicesDefaults.CustomerRolesBySystemNamePrefix);
            await RemoveByPrefixAsync(NopCustomerServicesDefaults.CustomerCustomerRolesPrefix);
        }
    }
}
