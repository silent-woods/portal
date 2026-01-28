using System;
using System.Linq;
using App.Core.Domain.Customers;
using App.Core.Domain.Employees;
using App.Core.Domain.Security;
using App.Data;
using App.Data.Extensions;
using App.Data.Mapping;
using App.Data.Migrations;
using FluentMigrator;

namespace Nop.Data.Migrations.UpgradeTo460.Extension
{
    [NopMigration("2024-03-05 01:01:26", "ExperienceMigration for 4.60.0", MigrationProcessType.Update)]
    public class ExperienceMigration : Migration
    {
        #region Fields

        private readonly INopDataProvider _dataProvider;

        #endregion

        #region Ctor

        public ExperienceMigration(INopDataProvider dataProvider)
        {
            _dataProvider = dataProvider;
        }

        #endregion

        /// <summary>
        /// Collect the UP migration expressions
        /// </summary>
        public override void Up()
        {
            if (!Schema.Table(NameCompatibilityManager.GetTableName(typeof(Experience))).Exists())
            {
                Create.TableFor<Experience>();
            }

            //#5607
            if (!_dataProvider.GetTable<PermissionRecord>().Any(pr => string.Compare(pr.SystemName, "ManageExperience", StringComparison.InvariantCultureIgnoreCase) == 0))
            {
                var multifactorAuthenticationPermissionRecord = _dataProvider.InsertEntity(
                    new PermissionRecord
                    {
                        SystemName = "ManageExperience",
                        Name = "Admin area. Manage Experience",
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
