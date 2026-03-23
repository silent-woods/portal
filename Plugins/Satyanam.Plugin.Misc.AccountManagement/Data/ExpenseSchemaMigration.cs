using App.Core.Domain.Customers;
using App.Core.Domain.Security;
using App.Data;
using App.Data.Extensions;
using App.Data.Migrations;
using FluentMigrator;
using Satyanam.Plugin.Misc.AccountManagement.Domain;
using Satyanam.Plugin.Misc.AccountManagement.Domain.Enums;
using System;
using System.Linq;

namespace Satyanam.Plugin.Misc.AccountManagement.Data;

[NopMigration("2026/03/04 10:00:00:0000001", "Misc.AccountManagement expense schema", MigrationProcessType.Installation)]
public partial class ExpenseSchemaMigration : AutoReversingMigration
{
    #region Fields

    protected readonly INopDataProvider _nopDataProvider;

    #endregion

    #region Ctor

    public ExpenseSchemaMigration(INopDataProvider nopDataProvider)
    {
        _nopDataProvider = nopDataProvider;
    }

    #endregion

    #region Methods

    public override void Up()
    {
        if (!Schema.Table(nameof(ExpenseCategory)).Exists())
            Create.TableFor<ExpenseCategory>();

        if (!Schema.Table(nameof(RecurringExpense)).Exists())
            Create.TableFor<RecurringExpense>();

        if (!Schema.Table(nameof(Expense)).Exists())
            Create.TableFor<Expense>();

        if (!Schema.Table(nameof(EmployeeMonthlySalary)).Exists())
            Create.TableFor<EmployeeMonthlySalary>();

        var adminRoles = _nopDataProvider.GetTable<CustomerRole>().Where(cr => cr.SystemName == NopCustomerDefaults.AdministratorsRoleName).ToList();

        var permissions = new[]
        {
            new PermissionRecord { Name = "Admin area. Account Management - Manage Expense Categories", SystemName = "ManageExpenseCategories", Category = "Account" },
            new PermissionRecord { Name = "Admin area. Account Management - Manage Expenses", SystemName = "ManageExpenses", Category = "Account" },
            new PermissionRecord { Name = "Admin area. Account Management - Manage Recurring Expenses", SystemName = "ManageRecurringExpenses", Category = "Account" },
            new PermissionRecord { Name = "Admin area. Account Management - Manage Salary Processing", SystemName = "ManageSalaryProcessing", Category = "Account" }
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
                    PermissionRecordId = insertedPermission.Id,
                    Full = true,
                    Add = true,
                    Edit = true,
                    Delete = true,
                    View = true,
                    CreatedOnUtc = DateTime.UtcNow,
                    UpdatedOnUtc = DateTime.UtcNow
                });
            }
        }

        if (!_nopDataProvider.GetTable<ExpenseCategory>().Any(c => c.IsSystem))
        {
            var systemCategories = new[]
            {
                new ExpenseCategory { Name = "Salary & Benefits", CategoryTypeId = (int)ExpenseCategoryTypeEnum.SalaryBenefits, IsSystem = true, IsActive = true, DisplayOrder = 1, CreatedOnUtc = DateTime.UtcNow, UpdatedOnUtc = DateTime.UtcNow, Deleted = false },
                new ExpenseCategory { Name = "Office Rent", CategoryTypeId = (int)ExpenseCategoryTypeEnum.OfficeRent, IsSystem = true, IsActive = true, DisplayOrder = 2, CreatedOnUtc = DateTime.UtcNow, UpdatedOnUtc = DateTime.UtcNow, Deleted = false },
                new ExpenseCategory { Name = "Utilities", CategoryTypeId = (int)ExpenseCategoryTypeEnum.Utilities, IsSystem = true, IsActive = true, DisplayOrder = 3, CreatedOnUtc = DateTime.UtcNow, UpdatedOnUtc = DateTime.UtcNow, Deleted = false },
                new ExpenseCategory { Name = "Tools & Subscriptions", CategoryTypeId = (int)ExpenseCategoryTypeEnum.ToolsSubscriptions, IsSystem = true, IsActive = true, DisplayOrder = 4, CreatedOnUtc = DateTime.UtcNow, UpdatedOnUtc = DateTime.UtcNow, Deleted = false },
                new ExpenseCategory { Name = "Marketing & Sales", CategoryTypeId = (int)ExpenseCategoryTypeEnum.MarketingSales, IsSystem = true, IsActive = true, DisplayOrder = 5, CreatedOnUtc = DateTime.UtcNow, UpdatedOnUtc = DateTime.UtcNow, Deleted = false },
                new ExpenseCategory { Name = "Travel / Client", CategoryTypeId = (int)ExpenseCategoryTypeEnum.TravelClient, IsSystem = true, IsActive = true, DisplayOrder = 6, CreatedOnUtc = DateTime.UtcNow, UpdatedOnUtc = DateTime.UtcNow, Deleted = false },
                new ExpenseCategory { Name = "Other Operational", CategoryTypeId = (int)ExpenseCategoryTypeEnum.OtherOperational, IsSystem = true, IsActive = true, DisplayOrder = 7, CreatedOnUtc = DateTime.UtcNow, UpdatedOnUtc = DateTime.UtcNow, Deleted = false }
            };

            foreach (var category in systemCategories)
                _nopDataProvider.InsertEntity(category);
        }
    }

    #endregion
}
