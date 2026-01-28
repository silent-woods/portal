using App.Core;
using App.Core.Domain.Employees;
using App.Core.Domain.Extension.TimeSheets;
using Satyanam.Plugin.Misc.AccountManagement.Domain;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Satyanam.Plugin.Misc.AccountManagement.Services;

public partial interface IAccountManagementService
{
    #region Account Groups Methods

    Task InsertAccountGroupAsync(AccountGroup accountGroup);

    Task UpdateAccountGroupAsync(AccountGroup accountGroup);

    Task DeleteAccountGroupAsync(AccountGroup accountGroup);

    Task<AccountGroup> GetAccountGroupByIdAsync(int id = 0);

    Task<IPagedList<AccountGroup>> GetAllAccountGroupsAsync(int accountCategoryId = 0, bool showHidden = false, int pageIndex = 0, int pageSize = int.MaxValue);

    #endregion

    #region Bank Account Methods

    Task InsertBankAccountAsync(BankAccount bankAccount);

    Task UpdateBankAccountAsync(BankAccount bankAccount);

    Task DeleteBankAccountAsync(BankAccount bankAccount);

    Task<BankAccount> GetBankAccountByIdAsync(int id = 0);

    Task<BankAccount> GetDefaultBankAccountAsync();

    Task<IPagedList<BankAccount>> GetAllBankAccountsAsync(bool showHidden = false, int pageIndex = 0, int pageSize = int.MaxValue);

    #endregion

    #region Payment Terms Methods

    Task InsertPaymentTermAsync(PaymentTerm paymentTerm);

    Task UpdatePaymentTermAsync(PaymentTerm paymentTerm);

    Task DeletePaymentTermAsync(PaymentTerm paymentTerm);

    Task<PaymentTerm> GetPaymentTermByIdAsync(int id = 0);

    Task<IPagedList<PaymentTerm>> GetAllPaymentTermsAsync(bool showHidden = false, int pageIndex = 0, int pageSize = int.MaxValue);

    #endregion

    #region Project Billing Methods

    Task InsertProjectBillingAsync(ProjectBilling projectBilling);

    Task UpdateProjectBillingAsync(ProjectBilling projectBilling);

    Task DeleteProjectBillingAsync(ProjectBilling projectBilling);

    Task<ProjectBilling> GetProjectBillingByIdAsync(int id = 0);

    Task<ProjectBilling> GetProjectBillingByProjectIdAsync(int projectId = 0);

    Task<IPagedList<ProjectBilling>> GetAllProjectBillingsAsync(string billingName = null, int projectId = 0, int companyId = 0,
        int billingTypeId = 0, int paymentTermId = 0, bool showHidden = false, int pageIndex = 0, int pageSize = int.MaxValue);

    #endregion

    #region Invoice Methods

    Task InsertInvoiceAsync(Invoice invoice);

    Task UpdateInvoiceAsync(Invoice invoice);

    Task DeleteInvoiceAsync(Invoice invoice);

    Task<Invoice> GetInvoiceByIdAsync(int id = 0);

    Task<IPagedList<Invoice>> GetAllInvoicesAsync(DateTime? createdFromUTC = null, DateTime? createdToUTC = null, int companyId = 0, int statusId = 0,
        int monthId = 0, int yearId = 0, int invoiceNumber = 0, bool showHidden = false, int pageIndex = 0, int pageSize = int.MaxValue);

    #endregion

    #region Invoice Item Methods

    Task InsertInvoiceItemAsync(InvoiceItem invoiceItem);

    Task UpdateInvoiceItemAsync(InvoiceItem invoiceItem);

    Task DeleteInvoiceItemAsync(InvoiceItem invoiceItem);

    Task<InvoiceItem> GetInvoiceItemByIdAsync(int id = 0);

    Task<IPagedList<InvoiceItem>> GetAllInvoiceItemsAsync(int invoiceId = 0, int pageIndex = 0, int pageSize = int.MaxValue);

    #endregion

    #region Invoice Payment Histories Methods

    Task InsertInvoicePaymentHistoryAsync(InvoicePaymentHistory invoicePaymentHistory);

    Task UpdateInvoicePaymentHistoryAsync(InvoicePaymentHistory invoicePaymentHistory);

    Task DeleteInvoicePaymentHistoryAsync(InvoicePaymentHistory invoicePaymentHistory);

    Task<InvoicePaymentHistory> GetInvoicePaymentHistoryByIdAsync(int id = 0);

    Task<IPagedList<InvoicePaymentHistory>> GetAllInvoicePaymentHistoriesAsync(int invoiceId = 0, int pageIndex = 0, int pageSize = int.MaxValue);

    #endregion

    #region Account Transaction Methods

    Task InsertAccountTransactionAsync(AccountTransaction accountTransaction);

    Task UpdateAccountTransactionAsync(AccountTransaction accountTransaction);

    Task DeleteAccountTransactionAsync(AccountTransaction accountTransaction);

    Task<AccountTransaction> GetAccountTransactionByIdAsync(int id = 0);

    Task<AccountTransaction> GetAccountTransactionByInvoicePaymentHistoryIdAsync(int invoicePaymentHistoryId = 0);

    Task<IPagedList<AccountTransaction>> GetAllAccountTransactionsAsync(int transactionTypeId = 0, int paymentMethodId = 0, int pageIndex = 0,
        int pageSize = int.MaxValue);

    #endregion

    #region Send Invoice Mail Methods

    Task<IList<int>> SendInvoiceEmailAsync(IList<string> contactEmails, string companyName, int invoiceFileId, int timesheetFileId, int languageId);

    #endregion

    #region Time Summary Report Methods

    Task<IList<TimeSheetReport>> GetReportByEmployeeListWithProjectsAsync(DateTime? from, DateTime? to, int employeeId = 0,
        int projectId = 0, int periodId = 0, int hoursId = 0);

    #endregion
}
