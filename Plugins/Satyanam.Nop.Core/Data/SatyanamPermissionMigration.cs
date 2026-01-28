using App.Core.Domain.Customers;
using App.Core.Domain.Security;
using App.Data;
using App.Data.Migrations;
using FluentMigrator;
using System.Linq;

namespace Satyanam.Nop.Core.Data
{
    [NopMigration("2025/04/04 12:30:00", "Satyanam CRM Permissions", MigrationProcessType.Installation)]
    public class SatyanamPermissionMigration : Migration
    {
        private readonly INopDataProvider _dataProvider;

        public SatyanamPermissionMigration(INopDataProvider dataProvider)
        {
            _dataProvider = dataProvider;
        }

        public override void Up()
        {
            var adminRoles = _dataProvider.GetTable<CustomerRole>()
                .Where(cr => cr.SystemName == NopCustomerDefaults.AdministratorsRoleName)
                .ToList();

            var permissions = new[]
            {
                new PermissionRecord { SystemName = "ManageTitles", Name = "Admin area. Manage Titles", Category = "CRM" },
                new PermissionRecord { SystemName = "ManageTags", Name = "Admin area. Manage Tags", Category = "CRM" },
                new PermissionRecord { SystemName = "ManageLeadStatuses", Name = "Admin area. Manage Lead Statuses", Category = "CRM" },
                new PermissionRecord { SystemName = "ManageLeadSources", Name = "Admin area. Manage Lead Sources", Category = "CRM" },
                new PermissionRecord { SystemName = "ManageIndustries", Name = "Admin area. Manage Industries", Category = "CRM" },
                new PermissionRecord { SystemName = "ManageCategories", Name = "Admin area. Manage Categories", Category = "CRM" },
                new PermissionRecord { SystemName = "ManageLeads", Name = "Admin area. Manage Leads", Category = "CRM" },
                new PermissionRecord { SystemName = "ManageCampaigns", Name = "Admin area. Manage Campaigns", Category = "CRM" },
                new PermissionRecord { SystemName = "ManageContacts", Name = "Admin area. Manage Contacts", Category = "CRM" },
                new PermissionRecord { SystemName = "ManageAccountTypes", Name = "Admin area. Manage Account Types", Category = "CRM" },
                new PermissionRecord { SystemName = "ManageCompanies", Name = "Admin area. Manage Companies", Category = "CRM" },
                new PermissionRecord { SystemName = "ManageDeals", Name = "Admin area. Manage Deals", Category = "CRM" },
            };

            foreach (var permission in permissions)
            {
                if (!_dataProvider.GetTable<PermissionRecord>().Any(p => p.SystemName == permission.SystemName))
                {
                    var insertedPermission = _dataProvider.InsertEntity(permission);

                    foreach (var role in adminRoles)
                    {
                        _dataProvider.InsertEntity(new PermissionRecordCustomerRoleMapping
                        {
                            CustomerRoleId = role.Id,
                            PermissionRecordId = insertedPermission.Id
                        });
                    }
                }
            }
        }

        public override void Down()
        {
            // Optionally remove permissions here
        }
    }
}
