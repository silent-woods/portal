using App.Core.Domain.Customers;
using App.Core.Domain.Security;
using App.Core.Domain.TaskAlerts;
using App.Data.Extensions;
using App.Data.Mapping;
using FluentMigrator;
using System.Linq;

namespace App.Data.Migrations.UpgradeTo460.Extension;

[NopMigration("2025-09-26 11:54:18", "TaskAlertMigration for 4.60.0", MigrationProcessType.Update)]
public partial class TaskAlertMigration : Migration
{
    #region Fields

    private readonly INopDataProvider _nopDataProvider;

    #endregion

    #region Ctor

    public TaskAlertMigration(INopDataProvider nopDataProvider)
    {
        _nopDataProvider = nopDataProvider;
    }

    #endregion

    #region Methods

    public override void Up()
    {
        if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(TaskAlertConfiguration))).Exists())
            Create.TableFor<TaskAlertConfiguration>();

        if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(TaskAlertReason))).Exists())
            Create.TableFor<TaskAlertReason>();

        if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(TaskAlertLog))).Exists())
            Create.TableFor<TaskAlertLog>();

        var adminRoles = _nopDataProvider.GetTable<CustomerRole>().Where(cr => cr.SystemName == NopCustomerDefaults.AdministratorsRoleName).ToList();

        var permissions = new[]
        {
            new PermissionRecord { Name = "Admin area. Manage Alert Configuration", SystemName = "ManageAlertConfiguration", Category = "Configuration" },
            new PermissionRecord { Name = "Admin area. Manage Alert Reason", SystemName = "ManageAlertReason", Category = "Configuration" },
            new PermissionRecord { Name = "Admin area. Manage Alert Report", SystemName = "ManageAlertReport", Category = "Configuration" },
        };

        var existingPermissionSystemNames = _nopDataProvider.GetTable<PermissionRecord>().Select(p => p.SystemName.ToLowerInvariant()).ToHashSet();

        foreach (var permission in permissions)
        {
            var systemNameLower = permission.SystemName.ToLowerInvariant();

            if (!existingPermissionSystemNames.Contains(systemNameLower))
            {
                var insertedPermission = _nopDataProvider.InsertEntity(permission);

                foreach (var adminRole in adminRoles)
                {
                    _nopDataProvider.InsertEntity(new PermissionRecordCustomerRoleMapping
                    {
                        CustomerRoleId = adminRole.Id,
                        PermissionRecordId = insertedPermission.Id
                    });
                }
            }
        }
    }

    public override void Down()
    {
    }

    #endregion
}
