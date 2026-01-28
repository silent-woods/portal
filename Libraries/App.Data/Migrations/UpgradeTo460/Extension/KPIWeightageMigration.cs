using System;
using System.Linq;
using App.Core.Domain.Customers;
using App.Core.Domain.PerformanceMeasurements;
using App.Core.Domain.Security;
using App.Core.Domain.TimeSheets;
using App.Data;
using App.Data.Extensions;
using App.Data.Mapping;
using App.Data.Migrations;
using FluentMigrator;

namespace Nop.Data.Migrations.UpgradeTo460
{
    [NopMigration("2024-02-27 01:01:12", "KPIWeightageMigration for 4.60.0", MigrationProcessType.Update)]
    public class KPIWeightageMigration : Migration
    {
        #region Fields

        private readonly INopDataProvider _dataProvider;

        #endregion

        #region Ctor

        public KPIWeightageMigration(INopDataProvider dataProvider)
        {
            _dataProvider = dataProvider;
        }

        #endregion

        /// <summary>
        /// Collect the UP migration expressions
        /// </summary>
        public override void Up()
        {
            if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(KPIWeightage))).Exists())
            {
                Create.TableFor<KPIWeightage>();
            }

            //#5607
            if (!_dataProvider.GetTable<PermissionRecord>().Any(pr => string.Compare(pr.SystemName, "ManageKPIWeightage", StringComparison.InvariantCultureIgnoreCase) == 0))
            {
                var multifactorAuthenticationPermissionRecord = _dataProvider.InsertEntity(
                    new PermissionRecord
                    {
                        SystemName = "ManageKPIWeightage",
                        Name = "Admin area.Manage KPIWeightage",
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
