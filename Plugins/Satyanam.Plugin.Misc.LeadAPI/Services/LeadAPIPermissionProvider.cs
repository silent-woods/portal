using App.Core.Domain.Customers;
using App.Core.Domain.Security;
using App.Services.Security;
using System.Collections.Generic;

namespace Satyanam.Plugin.Misc.LeadAPI.Services;

public partial class LeadAPIPermissionProvider : IPermissionProvider
{
    #region Permission Names

    public static readonly PermissionRecord ManageLeadAPIConfiguration = new() { Name = "Admin area. Lead API - Manage LeadAPI Configuration", SystemName = "ManageLeadAPIConfiguration", Category = "Account" };
    public static readonly PermissionRecord ManageLeadAPILog = new() { Name = "Admin area. Lead API - Manage LeadAPI Log", SystemName = "ManageLeadAPILog", Category = "Account" };

    #endregion

    #region Get Permission Methods

    public virtual IEnumerable<PermissionRecord> GetPermissions()
    {
        return new[]
        {
            ManageLeadAPIConfiguration,
            ManageLeadAPILog
        };
    }

    public virtual HashSet<(string systemRoleName, PermissionRecord[] permissions)> GetDefaultPermissions()
    {
        return new HashSet<(string, PermissionRecord[])>
    {
        (
            NopCustomerDefaults.AdministratorsRoleName,
            new[]
            {
                ManageLeadAPIConfiguration,
                ManageLeadAPILog
            }
        ),

    };
    }

    #endregion
}
