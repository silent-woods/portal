using System;
using System.Linq;
using App.Core.Domain.Activities;
using App.Core.Domain.ActivityEvents;
using App.Core.Domain.Customers;
using App.Core.Domain.Security;
using App.Core.Domain.TimeSheets;
using App.Data;
using App.Data.Extensions;
using App.Data.Mapping;
using App.Data.Migrations;
using FluentMigrator;

namespace Nop.Data.Migrations.UpgradeTo460
{
    [NopMigration("2024-01-12 01:01:16", "TimeSheetMigration for 4.60.0", MigrationProcessType.Update)]
    public class ActivityEventMigration : Migration
    {
        #region Fields

        private readonly INopDataProvider _dataProvider;

        #endregion

        #region Ctor

        public ActivityEventMigration(INopDataProvider dataProvider)
        {
            _dataProvider = dataProvider;
        }

        #endregion

        /// <summary>
        /// Collect the UP migration expressions
        /// </summary>
        public override void Up()
        {
            if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(ActivityEvent))).Exists())
            {
                Create.TableFor<ActivityEvent>();
            }

            //#5607
            if (!_dataProvider.GetTable<PermissionRecord>().Any(pr => string.Compare(pr.SystemName, "ManageActivity", StringComparison.InvariantCultureIgnoreCase) == 0))
            {
                var multifactorAuthenticationPermissionRecord = _dataProvider.InsertEntity(
                    new PermissionRecord
                    {
                        SystemName = "ManageActivity",
                        Name = "Admin area.Manage Activity",
                        Category = "Standard"
                    }
                );

                var customerRoles = _dataProvider.GetTable<CustomerRole>().Where(cr => cr.SystemName == NopCustomerDefaults.AdministratorsRoleName);

                foreach (var role in customerRoles.ToList())
                {
                    _dataProvider.InsertEntity(
                        new PermissionRecordCustomerRoleMapping
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
            //add the downgrade logic if necessary 
        }
    }
}
