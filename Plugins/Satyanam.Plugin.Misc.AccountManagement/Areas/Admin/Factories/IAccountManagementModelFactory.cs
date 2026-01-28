using Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Models.AccountGroups;
using Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Models.AccountTransactions;
using Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Models.BankAccounts;
using Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Models.InvoiceItems;
using Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Models.InvoicePaymentHistories;
using Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Models.Invoices;
using Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Models.PaymentTerms;
using Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Models.ProjectBillings;
using Satyanam.Plugin.Misc.AccountManagement.Domain;
using System.Threading.Tasks;

namespace Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Factories;

public partial interface IAccountManagementModelFactory
{
    #region Account Groups Methods

    Task<AccountGroupSearchModel> PrepareAccountGroupSearchModelAsync(AccountGroupSearchModel searchModel);

    Task<AccountGroupListModel> PrepareAccountGroupListModelAsync(AccountGroupSearchModel searchModel);

    Task<AccountGroupModel> PrepareAccountGroupModelAsync(AccountGroupModel model, AccountGroup accountGroup);

    #endregion

    #region Bank Accounts Methods

    Task<BankAccountSearchModel> PrepareBankAccountSearchModelAsync(BankAccountSearchModel searchModel);

    Task<BankAccountListModel> PrepareBankAccountListModelAsync(BankAccountSearchModel searchModel);

    Task<BankAccountModel> PrepareBankAccountModelAsync(BankAccountModel model, BankAccount bankAccount);

    #endregion

    #region Payment Terms Methods

    Task<PaymentTermSearchModel> PreparePaymentTermSearchModelAsync(PaymentTermSearchModel searchModel);

    Task<PaymentTermListModel> PreparePaymentTermListModellAsync(PaymentTermSearchModel searchModel);

    Task<PaymentTermModel> PreparePaymentTermModelAsync(PaymentTermModel model, PaymentTerm paymentTerm);

    #endregion

    #region Project Billing Methods

    Task<ProjectBillingSearchModel> PrepareProjectBillingSearchModelAsync(ProjectBillingSearchModel searchModel);

    Task<ProjectBillingListModel> PrepareProjectBillingListModelAsync(ProjectBillingSearchModel searchModel);

    Task<ProjectBillingModel> PrepareProjectBillingModelAsync(ProjectBillingModel model, ProjectBilling projectBilling);

    #endregion

    #region Invoice Methods

    Task<InvoiceSearchModel> PrepareInvoiceSearchModelAsync(InvoiceSearchModel searchModel);

    Task<InvoiceListModel> PrepareInvoiceListModelAsync(InvoiceSearchModel searchModel);

    Task<InvoiceModel> PrepareInvoiceModelAsync(InvoiceModel model, Invoice invoice);

    #endregion

    #region Invoice Items Methods

    Task<InvoiceItemListModel> PrepareInvoiceItemListModelAsync(InvoiceItemSearchModel searchModel);

    Task<InvoiceItemModel> PrepareInvoiceItemModelAsync(InvoiceItemModel model, Invoice invoice, 
        InvoiceItem invoiceItem);

    #endregion

    #region Invoice Payment Histories Methods

    Task<InvoicePaymentHistoryListModel> PrepareInvoicePaymentHistoryListModelAsync(InvoicePaymentHistorySearchModel searchModel);

    Task<InvoicePaymentHistoryModel> PrepareInvoicePaymentHistoryModelAsync(InvoicePaymentHistoryModel model, Invoice invoice,
        InvoicePaymentHistory invoicePaymentHistory);

    #endregion

    #region Account Transaction Methods

    Task<AccountTransactionSearchModel> PrepareAccountTransactionSearchModelAsync(AccountTransactionSearchModel searchModel);

    Task<AccountTransactionListModel> PrepareAccountTransactionListModelAsync(AccountTransactionSearchModel searchModel);

    Task<AccountTransactionModel> PrepareAccountTransactionModelAsync(AccountTransactionModel model, AccountTransaction accountTransaction);

    #endregion
}
