using App.Core.Domain.Customers;
using App.Core.Domain.ProjectIntegrations;
using App.Core.Domain.Security;
using App.Data.Extensions;
using App.Data.Mapping;
using FluentMigrator;
using System;
using System.Linq;

namespace App.Data.Migrations.UpgradeTo460.Extension;

[NopMigration("2025-07-07 01:01:12", "ProjectIntegrationMigration for 4.60.0", MigrationProcessType.Update)]
public partial class ProjectIntegrationMigration : Migration
{
    #region Fields

    protected readonly INopDataProvider _nopDataProvider;

    #endregion

    #region Ctor

    public ProjectIntegrationMigration(INopDataProvider nopDataProvider)
    {
        _nopDataProvider = nopDataProvider;
    }

    #endregion

    #region Methods

    public override void Up()
    {
        if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(ProjectIntegration))).Exists())
            Create.TableFor<ProjectIntegration>();

        if (!_nopDataProvider.GetTable<PermissionRecord>().Any(pr => string.Compare(pr.SystemName, "ManageProjectIntegration", StringComparison.InvariantCultureIgnoreCase) == 0))
        {
            var multifactorAuthenticationPermissionRecord = _nopDataProvider.InsertEntity(new PermissionRecord
            {
                SystemName = "ManageProjectIntegration",
                Name = "Admin area. Manage Project Integration",
                Category = "Standard"
            });

            var customerRoles = _nopDataProvider.GetTable<CustomerRole>().Where(cr => cr.SystemName == NopCustomerDefaults.AdministratorsRoleName);
            foreach (var role in customerRoles.ToList())
            {
                _nopDataProvider.InsertEntity(new PermissionRecordCustomerRoleMapping
                    {
                        CustomerRoleId = role.Id,
                        PermissionRecordId = multifactorAuthenticationPermissionRecord.Id
                    }
                );
            }
        }
    }

    public override void Down()
    {
    }

    #endregion
}
