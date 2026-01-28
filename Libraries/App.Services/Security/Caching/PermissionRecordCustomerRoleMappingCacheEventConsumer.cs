using App.Core.Domain.Security;
using App.Services.Caching;

namespace App.Services.Security.Caching
{
    /// <summary>
    /// Represents a permission record-customer role mapping cache event consumer
    /// </summary>
    public partial class PermissionRecordCustomerRoleMappingCacheEventConsumer : CacheEventConsumer<PermissionRecordCustomerRoleMapping>
    {
    }
}