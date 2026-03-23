using App.Core.Domain.Customers;
using App.Core.Domain.Security;
using App.Services.Security;
using System.Collections.Generic;

namespace Satyanam.Plugin.Misc.AccountManagement.Services;

public partial class AccountManagementPermissionProvider : IPermissionProvider
{
    #region Permission Names

    public static readonly PermissionRecord ManageAccountManagementConfiguration = new() { Name = "Admin area. Account Management - Manage Account Management Configuration", SystemName = "ManageAccountManagementConfiguration", Category = "Account" };
    public static readonly PermissionRecord ManageAccountGroups = new() { Name = "Admin area. Account Management - Manage Account Groups", SystemName = "ManageAccountGroups", Category = "Account" };
    public static readonly PermissionRecord ManagePaymentTerms = new() { Name = "Admin area. Account Management - Manage Payment Terms", SystemName = "ManagePaymentTerms", Category = "Account" };
    public static readonly PermissionRecord ManageBankAccounts = new() { Name = "Admin area. Account Management - Manage Bank Accounts", SystemName = "ManageBankAccounts", Category = "Account" };
    public static readonly PermissionRecord ManageProjectBillings = new() { Name = "Admin area. Account Management - Manage Project Billings", SystemName = "ManageProjectBillings", Category = "Account" };
    public static readonly PermissionRecord ManageInvoices = new() { Name = "Admin area. Account Management - Manage Invoices", SystemName = "ManageInvoices", Category = "Account" };
    public static readonly PermissionRecord ManageInvoiceItems = new() { Name = "Admin area. Account Management - Manage Invoice Items", SystemName = "ManageInvoiceItems", Category = "Account" };
    public static readonly PermissionRecord ManageInvoicePaymentHistories = new() { Name = "Admin area. Account Management - Manage Invoice Payment Histories", SystemName = "ManageInvoicePaymentHistories", Category = "Account" };
    public static readonly PermissionRecord ManageTransactions = new() { Name = "Admin area. Account Management - Manage Transactions", SystemName = "ManageTransactions", Category = "Account" };
    public static readonly PermissionRecord ManageReports = new() { Name = "Admin area. Account Management - Manage Reports", SystemName = "ManageReports", Category = "Account" };

    public static readonly PermissionRecord ManageExpenseCategories = new() { Name = "Admin area. Account Management - Manage Expense Categories", SystemName = "ManageExpenseCategories", Category = "Account" };
    public static readonly PermissionRecord ManageExpenses = new() { Name = "Admin area. Account Management - Manage Expenses", SystemName = "ManageExpenses", Category = "Account" };
    public static readonly PermissionRecord ManageRecurringExpenses = new() { Name = "Admin area. Account Management - Manage Recurring Expenses", SystemName = "ManageRecurringExpenses", Category = "Account" };
    public static readonly PermissionRecord ManageSalaryProcessing = new() { Name = "Admin area. Account Management - Manage Salary Processing", SystemName = "ManageSalaryProcessing", Category = "Account" };

    #endregion

    #region Get Permission Methods

    public virtual IEnumerable<PermissionRecord> GetPermissions()
    {
        return new[]
        {
            ManageAccountManagementConfiguration,
            ManageAccountGroups,
            ManagePaymentTerms,
            ManageBankAccounts,
            ManageProjectBillings,
            ManageInvoices,
            ManageInvoiceItems,
            ManageInvoicePaymentHistories,
            ManageTransactions,
            ManageReports,
            ManageExpenseCategories,
            ManageExpenses,
            ManageRecurringExpenses,
            ManageSalaryProcessing
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
                    ManageAccountManagementConfiguration,
                    ManageAccountGroups,
                    ManagePaymentTerms,
                    ManageBankAccounts,
                    ManageProjectBillings,
                    ManageInvoices,
                    ManageInvoiceItems,
                    ManageInvoicePaymentHistories,
                    ManageTransactions,
                    ManageReports,
                    ManageExpenseCategories,
                    ManageExpenses,
                    ManageRecurringExpenses,
                    ManageSalaryProcessing
                }
            ),

        };
    }

    #endregion
}
