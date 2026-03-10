using App.Core.Domain.Customers;
using App.Core.Domain.Security;
using App.Data;
using App.Data.Extensions;
using App.Data.Migrations;
using DocumentFormat.OpenXml.Wordprocessing;
using FluentMigrator;
using LinqToDB.DataProvider;
using Satyanam.Plugin.Misc.AccountManagement.Domain;
using System.Linq;

namespace Satyanam.Plugin.Misc.AccountManagement.Data;

[NopMigration("2025/08/25 09:36:17:6445222", "Misc.AccountManagement base schema", MigrationProcessType.Installation)]
public partial class AccountManagementSchemaMigration : AutoReversingMigration
{
    #region Fields

    protected readonly INopDataProvider _nopDataProvider;

    #endregion

    #region Ctor

    public AccountManagementSchemaMigration(INopDataProvider nopDataProvider)
    {
        _nopDataProvider = nopDataProvider;
    }

    #endregion

    #region Methods

    public override void Up()
    {
        Create.TableFor<AccountGroup>();
        Create.TableFor<AccountTransaction>();
        Create.TableFor<BankAccount>();
        Create.TableFor<Invoice>();
        Create.TableFor<InvoiceItem>();
        Create.TableFor<InvoicePaymentHistory>();
        Create.TableFor<PaymentTerm>();
        Create.TableFor<ProjectBilling>();

        var adminRoles = _nopDataProvider.GetTable<CustomerRole>().Where(cr => cr.SystemName == NopCustomerDefaults.AdministratorsRoleName).ToList();

        var permissions = new[]
        {
            new PermissionRecord { Name = "Admin area. Manage Account Groups", SystemName = "ManageAccountGroups", Category = "Account" },
            new PermissionRecord { Name = "Admin area. Manage Payment Terms", SystemName = "ManagePaymentTerms", Category = "Account" },
            new PermissionRecord { Name = "Admin area. Manage Bank Accounts", SystemName = "ManageBankAccounts", Category = "Account" },
            new PermissionRecord { Name = "Admin area. Manage Project Billings", SystemName = "ManageProjectBillings", Category = "Account" },
            new PermissionRecord { Name = "Admin area. Manage Invoices", SystemName = "ManageInvoices", Category = "Account" },
            new PermissionRecord { Name = "Admin area. Manage Transactions", SystemName = "ManageTransactions", Category = "Account" },
            new PermissionRecord { Name = "Admin area. Manage Reports", SystemName = "ManageReports", Category = "Account" }
        };

        foreach (var permission in permissions)
        {
            if (_nopDataProvider.GetTable<PermissionRecord>().Any(p => p.SystemName == permission.SystemName))
                continue;

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

    #endregion
}
