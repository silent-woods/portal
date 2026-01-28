using App.Core.Domain.Customers;
using App.Core.Domain.Security;
using App.Services.Security;
using System.Collections.Generic;

namespace Satyanam.Plugin.Misc.TrackerAPI.Services;

public partial class TrackerAPIPermissionProvider : IPermissionProvider
{
    #region Permission Names

    public static readonly PermissionRecord ManageTrackerAPIConfiguration = new() { Name = "Admin area. Manage TrackerAPI Configuration", SystemName = "ManageTrackerAPIConfiguration", Category = "Account" };
    public static readonly PermissionRecord ManageTrackerAPILog = new() { Name = "Admin area. Manage TrackerAPI Log", SystemName = "ManageTrackerAPILog", Category = "Account" };

    #endregion

    #region Get Permission Methods

    public virtual IEnumerable<PermissionRecord> GetPermissions()
    {
        return new[]
        {
            ManageTrackerAPIConfiguration,
            ManageTrackerAPILog
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
                    ManageTrackerAPIConfiguration,
                    ManageTrackerAPILog
                }
            ),

        };
    }

    #endregion
}
