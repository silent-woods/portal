using App.Core.Domain.Projects;
using App.Data.Extensions;
using App.Services;
using App.Services.Directory;
using App.Services.Employees;
using App.Services.Helpers;
using App.Services.Localization;
using App.Services.Projects;
using App.Web.Framework.Models.Extensions;
using App.Web.Models.Boards;
using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.Vml.Spreadsheet;
using Microsoft.AspNetCore.Mvc.Rendering;
using Satyanam.Nop.Core.Services;
using Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Models.AccountGroups;
using Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Models.AccountTransactions;
using Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Models.BankAccounts;
using Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Models.Enums;
using Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Models.InvoiceItems;
using Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Models.InvoicePaymentHistories;
using Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Models.Invoices;
using Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Models.PaymentTerms;
using Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Models.ProjectBillings;
using Satyanam.Plugin.Misc.AccountManagement.Domain;
using Satyanam.Plugin.Misc.AccountManagement.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Factories;

public partial class AccountManagementModelFactory : IAccountManagementModelFactory
{
	#region Fields

	protected readonly IAccountManagementService _accountManagementService;
    protected readonly ICompanyService _companyService;
    protected readonly IContactsService _contactsService;
    protected readonly ICurrencyService _currencyService;
    protected readonly IDateTimeHelper _dateTimeHelper;
    protected readonly IEmployeeService _employeeService;
    protected readonly IEmployeeSalaryService _employeeSalaryService;
    protected readonly ILocalizationService _localizationService;
    protected readonly IProjectsService _projectsService;

	#endregion

	#region Ctor

	public AccountManagementModelFactory(IAccountManagementService accountManagementService,
        ICompanyService companyService,
        IContactsService contactsService,
        ICurrencyService currencyService,
        IDateTimeHelper dateTimeHelper,
        IEmployeeService employeeService,
        IEmployeeSalaryService employeeSalaryService,
        ILocalizationService localizationService,
        IProjectsService projectsService)
	{
		_accountManagementService = accountManagementService;
        _companyService = companyService;
        _contactsService = contactsService;
        _currencyService = currencyService;
        _dateTimeHelper = dateTimeHelper;
        _employeeService = employeeService;
        _employeeSalaryService = employeeSalaryService;
        _localizationService = localizationService;
        _projectsService = projectsService;
	}

    #endregion

    #region Utilties

    protected async Task<IList<SelectListItem>> AddCommonOptionsAsync(IList<SelectListItem> items, bool includeAll = false, bool includeSelect = false)
    {
        if (includeSelect)
        {
            items.Insert(0, new SelectListItem
            {
                Text = await _localizationService.GetResourceAsync("Admin.Common.Select"),
                Value = "0"
            });
        }

        if (includeAll)
        {
            items.Insert(0, new SelectListItem
            {
                Text = await _localizationService.GetResourceAsync("Admin.Common.All"),
                Value = "0"
            });
        }

        return items;
    }

    public async Task<IList<SelectListItem>> PrepareAvailableMonthsAsync(int selectedMonth = 0, bool includeAll = false, bool includeSelect = false)
    {
        var currentMonth = DateTime.Now.Month;
        var months = new List<SelectListItem>();

        for (var i = 1; i <= 12; i++)
        {
            var monthName = new DateTime(1, i, 1).ToString("MMMM");

            months.Add(new SelectListItem
            {
                Text = monthName,
                Value = i.ToString(),
                Selected = i == (selectedMonth > 0 ? selectedMonth : currentMonth)
            });
        }

        return await AddCommonOptionsAsync(months, includeAll, includeSelect); ;
    }

    public async Task<IList<SelectListItem>> PrepareAvailableYearsAsync(bool includeAll = false, bool includeSelect = false)
    {
        var currentMonth = DateTime.Now.Month;
        var years = new List<SelectListItem>();

        for (var i = 0; i < 15; i++)
        {
            var year = Convert.ToString(DateTime.Now.Year + i);
            years.Add(new SelectListItem
            {
                Text = year,
                Value = year
            });
        }

        return await AddCommonOptionsAsync(years, includeAll, includeSelect); ;
    }

    protected virtual async Task<IList<SelectListItem>> PrepareAvailableProjectsAsync(bool includeAll = false, bool includeSelect = false)
    {
        var availableProjects = await _projectsService.GetAllProjectsListAsync();
        var projects = availableProjects.Select(availableProject => new SelectListItem
        {
            Text = availableProject.ProjectTitle,
            Value = availableProject.Id.ToString()
        }).ToList();

        return await AddCommonOptionsAsync(projects, includeAll, includeSelect);
    }

    protected virtual async Task<IList<SelectListItem>> PrepareAvailableCompaniesAsync(bool includeAll = false, bool includeSelect = false)
    {
        var availableCompanies = await _companyService.GetAllCompanyAsync(name: null, website: null);
        var companies = availableCompanies.Select(availableCompany => new SelectListItem
        {
            Text = availableCompany.CompanyName,
            Value = availableCompany.Id.ToString()
        }).ToList();

        return await AddCommonOptionsAsync(companies, includeAll, includeSelect);
    }

    protected virtual async Task<IList<SelectListItem>> PrepareAvailableBillingTypesAsync(bool includeAll = false, bool includeSelect = false)
    {
        var availableBillingTypes = await ProjectBillingEnum.PayByHourly.ToSelectListAsync();
        var billingTypes = availableBillingTypes.Select(availableBillingType => new SelectListItem
        {
            Text = availableBillingType.Text,
            Value = availableBillingType.Value
        }).ToList();

        return await AddCommonOptionsAsync(billingTypes, includeAll, includeSelect);
    }

    protected virtual async Task<IList<SelectListItem>> PrepareAvailablePaymentTermsAsync(bool includeAll = false, bool includeSelect = false)
    {
        var availablePaymentTerms = await _accountManagementService.GetAllPaymentTermsAsync(showHidden: true);
        var paymentTerms = availablePaymentTerms.Select(availablePaymentTerm => new SelectListItem
        {
            Text = availablePaymentTerm.Name,
            Value = availablePaymentTerm.Id.ToString()
        }).ToList();

        return await AddCommonOptionsAsync(paymentTerms, includeAll, includeSelect);
    }

    protected virtual async Task<IList<SelectListItem>> PrepareAvailableCurrenciesAsync(bool includeAll = false, bool includeSelect = false)
    {
        var availableCurrencies = await _currencyService.GetAllCurrenciesAsync();
        var currencies = availableCurrencies.Select(availableCurrency => new SelectListItem
        {
            Text = availableCurrency.Name,
            Value = availableCurrency.Id.ToString()
        }).ToList();

        return await AddCommonOptionsAsync(currencies, includeAll, includeSelect);
    }

    protected virtual async Task<IList<SelectListItem>> PrepareAvailableTransactionTypesAsync(bool includeAll = false, bool includeSelect = false)
    {
        var availableTransactionTypes = await TransactionTypeEnum.Income.ToSelectListAsync();
        var transactionTypes = availableTransactionTypes.Select(availableTransactionType => new SelectListItem
        {
            Text = availableTransactionType.Text,
            Value = availableTransactionType.Value
        }).ToList();

        return await AddCommonOptionsAsync(transactionTypes, includeAll, includeSelect);
    }

    protected virtual async Task<IList<SelectListItem>> PrepareAvailableBankAccountsAsync(bool includeAll = false, bool includeSelect = false)
    {
        var availableBankAccounts = await _accountManagementService.GetAllBankAccountsAsync(showHidden: true);
        var bankAccounts = availableBankAccounts.Select(availableBankAccount => new SelectListItem
        {
            Text = availableBankAccount.Title,
            Value = availableBankAccount.Id.ToString()
        }).ToList();

        return await AddCommonOptionsAsync(bankAccounts, includeAll, includeSelect);
    }

    protected virtual async Task<IList<SelectListItem>> PrepareAvailablePaymentTypesAsync(bool includeAll = false, bool includeSelect = false)
    {
        var availablePaymentTypes = await PaymentTypeEnum.Bank.ToSelectListAsync();
        var paymentTypes = availablePaymentTypes.Select(availablePaymentType => new SelectListItem
        {
            Text = availablePaymentType.Text,
            Value = availablePaymentType.Value
        }).ToList();

        return await AddCommonOptionsAsync(paymentTypes, includeAll, includeSelect);
    }

    protected virtual async Task<IList<SelectListItem>> PrepareAvailableStatusesAsync(bool includeAll = false, bool includeSelect = false)
    {
        var availableStatuses = await InvoiceEnum.Draft.ToSelectListAsync();
        var statuses = availableStatuses.Select(availableStatus => new SelectListItem
        {
            Text = availableStatus.Text,
            Value = availableStatus.Value
        }).ToList();

        return await AddCommonOptionsAsync(statuses, includeAll, includeSelect);
    }

    protected virtual async Task<IList<SelectListItem>> PrepareAvailableAccountGroupsAsync(bool includeAll = false, bool includeSelect = false, int accountCategoryId = (int)AccountCategoryEnum.Income)
    {
        var availableAccountGroups = await _accountManagementService.GetAllAccountGroupsAsync(accountCategoryId: accountCategoryId, showHidden: true);
        var accountGroups = availableAccountGroups.Select(availableAccountGroup => new SelectListItem
        {
            Text = availableAccountGroup.Name,
            Value = availableAccountGroup.Id.ToString()
        }).ToList();

        return await AddCommonOptionsAsync(accountGroups, includeAll, includeSelect);
    }

    protected virtual async Task<IList<SelectListItem>> PrepareAvailableProjectBillingsAsync(bool includeAll = false, bool includeSelect = false)
    {
        var availableProjectBillings = await _accountManagementService.GetAllProjectBillingsAsync(showHidden: true);
        var projectBillings = availableProjectBillings.Select(availableProjectBilling => new SelectListItem
        {
            Text = availableProjectBilling.BillingName,
            Value = availableProjectBilling.Id.ToString()
        }).ToList();

        return await AddCommonOptionsAsync(projectBillings, includeAll, includeSelect);
    }

    protected virtual async Task<IList<SelectListItem>> PrepareAvailableInvoicesAsync(bool includeAll = false, bool includeSelect = false)
    {
        var availableInvoices = await _accountManagementService.GetAllInvoicesAsync(showHidden: true);
        var invoices = availableInvoices.Select(availableInvoice => new SelectListItem
        {
            Text = availableInvoice.Title,
            Value = availableInvoice.Id.ToString()
        }).ToList();

        return await AddCommonOptionsAsync(invoices, includeAll, includeSelect);
    }

    protected virtual async Task<InvoiceItemSearchModel> PrepareInvoiceItemSearchModelAsync(int invoiceId,
        InvoiceItemSearchModel searchModel)
    {
        ArgumentNullException.ThrowIfNull(nameof(searchModel));

        searchModel.InvoiceId = invoiceId;

        searchModel.SetGridPageSize();

        return searchModel;
    }

    protected virtual async Task<InvoicePaymentHistorySearchModel> PrepareInvoicePaymentHistorySearchModelAsync(int invoiceId,
        InvoicePaymentHistorySearchModel searchModel)
    {
        ArgumentNullException.ThrowIfNull(nameof(searchModel));

        searchModel.InvoiceId = invoiceId;

        searchModel.SetGridPageSize();

        return searchModel;
    }

    #endregion

    #region Account Groups Methods

    public virtual async Task<AccountGroupSearchModel> PrepareAccountGroupSearchModelAsync(AccountGroupSearchModel searchModel)
    {
        ArgumentNullException.ThrowIfNull(nameof(searchModel));

        searchModel.SetGridPageSize();

        return searchModel;
    }

    public virtual async Task<AccountGroupListModel> PrepareAccountGroupListModelAsync(AccountGroupSearchModel searchModel)
    {
        ArgumentNullException.ThrowIfNull(nameof(searchModel));

        var accountGroups = await _accountManagementService.GetAllAccountGroupsAsync(showHidden: false, pageIndex: searchModel.Page - 1,
            pageSize: searchModel.PageSize);

        var model = await new AccountGroupListModel().PrepareToGridAsync(searchModel, accountGroups, () =>
        {
            return accountGroups.SelectAwait(async accountGroup =>
            {
                var accountGroupModel = new AccountGroupModel
                {
                    Id = accountGroup.Id,
                    Name = accountGroup.Name,
                    AccountCategory = Enum.GetName(typeof(AccountCategoryEnum), accountGroup.AccountCategoryId),
                    IsActive = accountGroup.IsActive,
                    DisplayOrder = accountGroup.DisplayOrder
                };

                return accountGroupModel;
            });
        });

        return model;
    }

    public virtual async Task<AccountGroupModel> PrepareAccountGroupModelAsync(AccountGroupModel model, AccountGroup accountGroup)
    {
        if (accountGroup != null)
        {
            model = model ?? new AccountGroupModel();

            model.Id = accountGroup.Id;
            model.Name = accountGroup.Name;
            model.AccountCategoryId = accountGroup.AccountCategoryId;
            model.IsActive = accountGroup.IsActive;
            model.DisplayOrder = accountGroup.DisplayOrder;
        }

        var accountCategories = await AccountCategoryEnum.Income.ToSelectListAsync();
        model.AccountCategories = await accountCategories.Select(accountCategory => new SelectListItem
        {
            Text = accountCategory.Text,
            Value = accountCategory.Value
        }).ToListAsync();

        return model;
    }

    #endregion

    #region Bank Accounts Methods

    public virtual async Task<BankAccountSearchModel> PrepareBankAccountSearchModelAsync(BankAccountSearchModel searchModel)
    {
        ArgumentNullException.ThrowIfNull(nameof(searchModel));

        searchModel.SetGridPageSize();

        return searchModel;
    }

    public virtual async Task<BankAccountListModel> PrepareBankAccountListModelAsync(BankAccountSearchModel searchModel)
    {
        ArgumentNullException.ThrowIfNull(nameof(searchModel));

        var bankAccounts = await _accountManagementService.GetAllBankAccountsAsync(showHidden: false, pageIndex: searchModel.Page - 1,
            pageSize: searchModel.PageSize);

        var model = await new BankAccountListModel().PrepareToGridAsync(searchModel, bankAccounts, () =>
        {
            return bankAccounts.SelectAwait(async bankAccount =>
            {
                var bankAccountModel = new BankAccountModel
                {
                    Id = bankAccount.Id,
                    Title = bankAccount.Title,
                    BankName = bankAccount.BankName,
                    IsDefault = bankAccount.IsDefault,
                    IsActive = bankAccount.IsActive,
                    DisplayOrder = bankAccount.DisplayOrder
                };

                return bankAccountModel;
            });
        });

        return model;
    }

    public virtual async Task<BankAccountModel> PrepareBankAccountModelAsync(BankAccountModel model, BankAccount bankAccount)
    {
        if (bankAccount != null)
        {
            model = model ?? new BankAccountModel();

            model.Id = bankAccount.Id;
            model.Title = bankAccount.Title;
            model.BankName = bankAccount.BankName;
            model.AccountNo = bankAccount.AccountNo;
            model.AccountName = bankAccount.AccountName;
            model.SwiftCode = bankAccount.SwiftCode;
            model.IFSCCode = bankAccount.IFSCCode;
            model.AccountType = bankAccount.AccountType;
            model.Branch = bankAccount.Branch;
            model.Address = bankAccount.Address;
            model.Currency = bankAccount.Currency;
            model.Notes = bankAccount.Notes;
            model.IsDefault = bankAccount.IsDefault;
            model.IsActive = bankAccount.IsActive;
            model.DisplayOrder = bankAccount.DisplayOrder;
        }

        return model;
    }

    #endregion

    #region Payment Terms Methods

    public virtual async Task<PaymentTermSearchModel> PreparePaymentTermSearchModelAsync(PaymentTermSearchModel searchModel)
    {
        ArgumentNullException.ThrowIfNull(nameof(searchModel));

        searchModel.SetGridPageSize();

        return searchModel;
    }

    public virtual async Task<PaymentTermListModel> PreparePaymentTermListModellAsync(PaymentTermSearchModel searchModel)
    {
        ArgumentNullException.ThrowIfNull(nameof(searchModel));

        var paymentTerms = await _accountManagementService.GetAllPaymentTermsAsync(showHidden: false, pageIndex: searchModel.Page - 1,
            pageSize: searchModel.PageSize);

        var model = await new PaymentTermListModel().PrepareToGridAsync(searchModel, paymentTerms, () =>
        {
            return paymentTerms.SelectAwait(async paymentTerm =>
            {
                var paymentTermModel = new PaymentTermModel
                {
                    Id = paymentTerm.Id,
                    Name = paymentTerm.Name,
                    NumberOfDays = paymentTerm.NumberOfDays,
                    IsActive = paymentTerm.IsActive,
                    DisplayOrder = paymentTerm.DisplayOrder
                };

                return paymentTermModel;
            });
        });

        return model;
    }

    public virtual async Task<PaymentTermModel> PreparePaymentTermModelAsync(PaymentTermModel model, PaymentTerm paymentTerm)
    {
        if (paymentTerm != null)
        {
            model = model ?? new PaymentTermModel();

            model.Id = paymentTerm.Id;
            model.Name = paymentTerm.Name;
            model.NumberOfDays = paymentTerm.NumberOfDays;
            model.IsActive = paymentTerm.IsActive;
            model.DisplayOrder = paymentTerm.DisplayOrder;
        }

        return model;
    }

    #endregion

    #region Project Billing Methods

    public virtual async Task<ProjectBillingSearchModel> PrepareProjectBillingSearchModelAsync(ProjectBillingSearchModel searchModel)
    {
        ArgumentNullException.ThrowIfNull(nameof(searchModel));

        searchModel.AvailableProjects = await PrepareAvailableProjectsAsync(true, false);
        searchModel.AvailableCompanies = await PrepareAvailableCompaniesAsync(true, false);
        searchModel.AvailableBillingTypes = await PrepareAvailableBillingTypesAsync(true, false);
        searchModel.AvailablePaymentTerms = await PrepareAvailablePaymentTermsAsync(true, false);

        searchModel.SetGridPageSize();

        return searchModel;
    }

    public virtual async Task<ProjectBillingListModel> PrepareProjectBillingListModelAsync(ProjectBillingSearchModel searchModel)
    {
        ArgumentNullException.ThrowIfNull(nameof(searchModel));

        var projectBillings = await _accountManagementService.GetAllProjectBillingsAsync(billingName: searchModel.SearchBillingName,
            projectId: searchModel.SearchProjectId, companyId: searchModel.SearchCompanyId, billingTypeId: searchModel.SearchBillingTypeId,
            paymentTermId: searchModel.SearchPaymentTermId, showHidden: false, pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

        var model = await new ProjectBillingListModel().PrepareToGridAsync(searchModel, projectBillings, () =>
        {
            return projectBillings.SelectAwait(async projectBilling =>
            {
                var projectBillingModel = new ProjectBillingModel
                {
                    Id = projectBilling.Id,
                    BillingName = projectBilling.BillingName,
                    ProjectName = (await _projectsService.GetProjectsByIdAsync(projectBilling.ProjectId)).ProjectTitle,
                    CompanyName = (await _companyService.GetCompanyByIdAsync(projectBilling.CompanyId)).CompanyName,
                    PaymentTerm = (await _accountManagementService.GetPaymentTermByIdAsync(projectBilling.PaymentTermId)).Name,
                    BillingType = Enum.GetName(typeof(ProjectBillingEnum), projectBilling.BillingTypeId),
                    BillingRate = projectBilling.BillingRate,
                    IsActive = projectBilling.IsActive,
                    DisplayOrder = projectBilling.DisplayOrder
                };

                return projectBillingModel;
            });
        });

        return model;
    }

    public virtual async Task<ProjectBillingModel> PrepareProjectBillingModelAsync(ProjectBillingModel model, ProjectBilling projectBilling)
    {
        if (projectBilling != null)
        {
            model = model ?? new ProjectBillingModel();

            model.Id = projectBilling.Id;
            model.BillingName = projectBilling.BillingName;
            model.ProjectId = projectBilling.ProjectId;
            model.CompanyId = projectBilling.CompanyId;
            model.PaymentTermId = projectBilling.PaymentTermId;
            model.BillingTypeId = projectBilling.BillingTypeId;
            model.BillingRate = projectBilling.BillingRate;
            model.PrimaryCurrencyId = projectBilling.PrimaryCurrencyId;
            model.PaymentCurrencyId = projectBilling.PaymentCurrencyId;
            model.IsActive = projectBilling.IsActive;
            model.DisplayOrder = projectBilling.DisplayOrder;
        }

        if (projectBilling == null)
            model.BillingRate = AccountManagementDefaults.BillingRate;

        model.AvailableProjects = await PrepareAvailableProjectsAsync(false, true);
        model.AvailableCompanies = await PrepareAvailableCompaniesAsync(false, true);
        model.AvailableBillingTypes = await PrepareAvailableBillingTypesAsync(false, true);
        model.AvailablePaymentTerms = await PrepareAvailablePaymentTermsAsync(false, true);
        model.AvailableCurrencies = await PrepareAvailableCurrenciesAsync(false, true);

        return model;
    }

    #endregion

    #region Invoice Methods

    public virtual async Task<InvoiceSearchModel> PrepareInvoiceSearchModelAsync(InvoiceSearchModel searchModel)
    {
        ArgumentNullException.ThrowIfNull(nameof(searchModel));

        searchModel.AvailableCompanies = await PrepareAvailableCompaniesAsync(true, false);
        searchModel.AvailableStatuses = await PrepareAvailableStatusesAsync(true, false);
        searchModel.AvailableMonths = await PrepareAvailableMonthsAsync(searchModel.SearchMonthId, true, false);
        searchModel.AvailableYears = await PrepareAvailableYearsAsync(true, false);

        searchModel.SetGridPageSize();

        return searchModel;
    }

    public virtual async Task<InvoiceListModel> PrepareInvoiceListModelAsync(InvoiceSearchModel searchModel)
    {
        ArgumentNullException.ThrowIfNull(nameof(searchModel));

        var createdOnFromValue = searchModel.SearchCreatedOnFrom == null ? null
            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.SearchCreatedOnFrom.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());
        var createdOnToValue = searchModel.SearchCreatedOnTo == null ? null
            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.SearchCreatedOnTo.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync()).AddDays(1);

        var invoices = await _accountManagementService.GetAllInvoicesAsync(createdFromUTC: createdOnFromValue, createdToUTC: createdOnToValue,
            companyId: searchModel.SearchCompanyId, statusId: searchModel.SearchStatusId, monthId: searchModel.SearchMonthId, yearId: searchModel.SearchYearId,
            invoiceNumber: searchModel.SearchInvoiceNumber, showHidden: false, pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

        var model = await new InvoiceListModel().PrepareToGridAsync(searchModel, invoices, () =>
        {
            return invoices.SelectAwait(async invoice =>
            {
                var invoiceModel = new InvoiceModel
                {
                    Id = invoice.Id,
                    InvoiceNumber = invoice.InvoiceNumber,
                    TotalPaymentAmount = invoice.TotalPaymentAmount,
                    MonthId = invoice.MonthId,
                    YearId = invoice.YearId,
                    InvoiceStatus = Enum.GetName(typeof(InvoiceEnum), invoice.StatusId),
                    IsActive = invoice.IsActive,
                    DisplayOrder = invoice.DisplayOrder,
                    CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(invoice.CreatedOnUtc, DateTimeKind.Utc)
                };

                var existingProjectBilling = await _accountManagementService.GetProjectBillingByIdAsync(invoice.ProjectBillingId);
                if (existingProjectBilling != null)
                {
                    invoiceModel.ProjectBillingName = existingProjectBilling.BillingName;
                    var existingCompany = await _companyService.GetCompanyByIdAsync(existingProjectBilling.CompanyId);
                    if (existingCompany != null)
                        invoiceModel.CompanyName = existingCompany.CompanyName;
                }

                var invoicePaymentHistories = await _accountManagementService.GetAllInvoicePaymentHistoriesAsync(invoiceId: invoice.Id);
                foreach (var invoicePaymentHistory in invoicePaymentHistories)
                    invoiceModel.TotalPaidAmount += invoicePaymentHistory.AmountInPaymentCurrency;

                return invoiceModel;
            });
        });

        return model;
    }

    public virtual async Task<InvoiceModel> PrepareInvoiceModelAsync(InvoiceModel model, Invoice invoice)
    {
        if (invoice != null)
        {
            model = model ?? new InvoiceModel();

            model.Id = invoice.Id;
            model.InvoiceNumber = invoice.InvoiceNumber;
            model.Title = invoice.Title;
            model.ProjectBillingId = invoice.ProjectBillingId;
            model.AccountGroupId = invoice.AccountGroupId;
            model.StatusId = invoice.StatusId;
            model.BankAccountId = invoice.BankAccountId;
            model.SubTotalAmount = invoice.SubTotalAmount;
            model.TaxAmount = invoice.TaxAmount;
            model.DiscountAmount = invoice.DiscountAmount;
            model.TotalPrimaryAmount = invoice.TotalPrimaryAmount;
            model.TotalPaymentAmount = invoice.TotalPaymentAmount;
            model.InvoiceFileId = invoice.InvoiceFileId;
            model.TimeSheetFileId = invoice.TimeSheetFileId;
            model.MonthId = invoice.MonthId;
            model.YearId = invoice.YearId;
            model.Notes = invoice.Notes;
            model.IsActive = invoice.IsActive;
            model.DisplayOrder = invoice.DisplayOrder;
            model.InvoiceItems = (await _accountManagementService.GetAllInvoiceItemsAsync(invoice.Id))?.Any() == true;
            if (invoice.StatusId != (int)InvoiceEnum.Draft)
                model.ShowPaymentHistories = true;

            await PrepareInvoiceItemSearchModelAsync(invoice.Id, model.InvoiceItemSearchModel);
            await PrepareInvoicePaymentHistorySearchModelAsync(invoice.Id, model.InvoicePaymentHistorySearchModel);
        }

        if (invoice == null)
            model.BankAccountId = (await _accountManagementService.GetDefaultBankAccountAsync()).Id;

        if (model.InvoiceItems)
        {
            string contactName = string.Empty;
            var existingProjectBilling = await _accountManagementService.GetProjectBillingByIdAsync(invoice.ProjectBillingId);
            if (existingProjectBilling != null)
            {
                var existingCompany = await _companyService.GetCompanyByIdAsync(existingProjectBilling.CompanyId);
                if (existingCompany != null)
                {
                    var existingContacts = await _contactsService.GetContactByCompanyIdAsync(companyId: existingProjectBilling.CompanyId, pageIndex: 0,
                        pageSize: int.MaxValue);
                    if (existingContacts.Any())
                    {
                        var exisingContact = existingContacts.FirstOrDefault();
                        if (exisingContact != null)
                        {
                            contactName = exisingContact.FirstName + " " + exisingContact.LastName;
                            model.SendEmail.Name = contactName;
                            model.SendEmail.Email = exisingContact.Email;
                        }
                    }
                    model.SendEmail.Subject = "Satyanam: Invoice - " + invoice.InvoiceNumber;
                    model.SendEmail.Body = $@"<p>Hello <strong>[{contactName}]</strong>,</p><p>I hope you are doing well.</p>
                        <p>Please find the attached invoice for <strong>{invoice.Title}</strong>.</p><p>Feel free to reach out if you have any concerns.</p><p>Thanks,<br />Vipul</p>";
                }
            }
        }

        model.AvailableMonths = await PrepareAvailableMonthsAsync(model.MonthId);
        model.AvailableYears = await PrepareAvailableYearsAsync();
        model.AvailableStatuses = await PrepareAvailableStatusesAsync(false, false);
        model.AvailableProjectBillings = await PrepareAvailableProjectBillingsAsync(false, true);
        model.AvailableAccountGroups = await PrepareAvailableAccountGroupsAsync(false, true);
        model.AvailableBankAccounts = await PrepareAvailableBankAccountsAsync(false, true);

        if (model.MonthId == 0)
        {
            var selectedMonth = model.AvailableMonths.FirstOrDefault(m => m.Selected);
            if (selectedMonth != null)
                model.MonthId = int.Parse(selectedMonth.Value);
        }

        return model;
    }

    #endregion

    #region Invoice Items Methods

    public virtual async Task<InvoiceItemListModel> PrepareInvoiceItemListModelAsync(InvoiceItemSearchModel searchModel)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        var invoiceItems = await _accountManagementService.GetAllInvoiceItemsAsync(invoiceId: searchModel.InvoiceId,
            pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

        var model = await new InvoiceItemListModel().PrepareToGridAsync(searchModel, invoiceItems, () =>
        {
            return invoiceItems.SelectAwait(async invoiceItem =>
            {
                var invoiceItemModel = new InvoiceItemModel()
                {
                    Id = invoiceItem.Id,
                    Description = invoiceItem.Description,
                    Hours = invoiceItem.Hours,
                    Rate = invoiceItem.Rate,
                    Amount = invoiceItem.Amount
                };

                return invoiceItemModel;
            });
        });

        return model;
    }

    public virtual async Task<InvoiceItemModel> PrepareInvoiceItemModelAsync(InvoiceItemModel model,
        Invoice invoice, InvoiceItem invoiceItem)
    {
        if (invoiceItem != null)
        {
            model = model ?? new InvoiceItemModel();

            model.Id = invoiceItem.Id;
            model.Description = invoiceItem.Description;
            model.Hours = invoiceItem.Hours;
            model.Rate = invoiceItem.Rate;
            model.Amount = invoiceItem.Amount;
        }

        if (invoiceItem == null)
        {
            var existingProjectBilling = await _accountManagementService.GetProjectBillingByIdAsync(invoice.ProjectBillingId);
            if (existingProjectBilling != null)
                model.Rate = existingProjectBilling.BillingRate;
        }

        model.InvoiceId = invoice.Id;

        return model;
    }

    #endregion

    #region Invoice Payment Histories Methods

    public virtual async Task<InvoicePaymentHistoryListModel> PrepareInvoicePaymentHistoryListModelAsync(InvoicePaymentHistorySearchModel searchModel)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        var invoicePaymentHistories = await _accountManagementService.GetAllInvoicePaymentHistoriesAsync(invoiceId: searchModel.InvoiceId,
            pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

        var model = await new InvoicePaymentHistoryListModel().PrepareToGridAsync(searchModel, invoicePaymentHistories, () =>
        {
            return invoicePaymentHistories.SelectAwait(async invoicePaymentHistory =>
            {
                var invoicePaymentHistoryModel = new InvoicePaymentHistoryModel()
                {
                    Id = invoicePaymentHistory.Id,
                    PaymentMethod = Enum.GetName(typeof(PaymentTypeEnum), invoicePaymentHistory.PaymentMethodId),
                    AmountInPaymentCurrency = invoicePaymentHistory.AmountInPaymentCurrency,
                    IsPartialPayment = invoicePaymentHistory.IsPartialPayment,
                    MonthId = invoicePaymentHistory.MonthId,
                    YearId = invoicePaymentHistory.YearId,
                };

                return invoicePaymentHistoryModel;
            });
        });

        return model;
    }

    public virtual async Task<InvoicePaymentHistoryModel> PrepareInvoicePaymentHistoryModelAsync(InvoicePaymentHistoryModel model, Invoice invoice,
        InvoicePaymentHistory invoicePaymentHistory)
    {
        if (invoicePaymentHistory != null)
        {
            model = model ?? new InvoicePaymentHistoryModel();

            model.Id = invoicePaymentHistory.Id;
            model.AmountInPaymentCurrency = invoicePaymentHistory.AmountInPaymentCurrency;
            model.AmountInINR = invoicePaymentHistory.AmountInINR;
            model.PaymentMethodId = invoicePaymentHistory.PaymentMethodId;
            model.IsPartialPayment = invoicePaymentHistory.IsPartialPayment;
            model.PaymentReceiptId = invoicePaymentHistory.PaymentReceiptId;
            model.MonthId = invoicePaymentHistory.MonthId;
            model.YearId = invoicePaymentHistory.YearId;
        }

        if (invoicePaymentHistory == null)
        {
            model.AmountInPaymentCurrency = invoice.TotalPaymentAmount;
            var existingProjectBilling = await _accountManagementService.GetProjectBillingByIdAsync(invoice.ProjectBillingId);
            if (existingProjectBilling != null)
            {
                var sourceCurrency = await _currencyService.GetCurrencyByIdAsync(existingProjectBilling.PaymentCurrencyId);
                var targetCurrency = (await _currencyService.GetAllCurrenciesAsync(showHidden: true)).Where(c => c.CurrencyCode == "INR").FirstOrDefault();
                decimal amount = await _currencyService.ConvertCurrencyAsync(invoice.TotalPaymentAmount, sourceCurrency, targetCurrency);
                model.AmountInINR = Math.Round(amount, 2);
            }
        }

        model.AvailablePaymentMethods = await PrepareAvailablePaymentTypesAsync(false, true);
        model.AvailableMonths = await PrepareAvailableMonthsAsync(model.MonthId);
        model.AvailableYears = await PrepareAvailableYearsAsync();

        if (model.MonthId == 0)
        {
            var selectedMonth = model.AvailableMonths.FirstOrDefault(m => m.Selected);
            if (selectedMonth != null)
                model.MonthId = int.Parse(selectedMonth.Value);
        }

        model.InvoiceId = invoice.Id;

        return model;
    }

    #endregion

    #region Account Transaction Utilities

    protected virtual async Task<string> ResolveTransactionSourceNameAsync(AccountTransaction transaction)
    {
        if (transaction.EmployeeId > 0)
        {
            var employee = await _employeeService.GetEmployeeByIdAsync(transaction.EmployeeId);
            if (employee != null)
                return $"{employee.FirstName} {employee.LastName}".Trim();
        }
        if (transaction.InvoiceId > 0)
        {
            var invoice = await _accountManagementService.GetInvoiceByIdAsync(transaction.InvoiceId);
            if (invoice != null && invoice.ProjectBillingId > 0)
            {
                var projectBilling = await _accountManagementService.GetProjectBillingByIdAsync(invoice.ProjectBillingId);
                if (projectBilling != null && projectBilling.CompanyId > 0)
                {
                    var company = await _companyService.GetCompanyByIdAsync(projectBilling.CompanyId);
                    if (company != null)
                        return company.CompanyName;
                }
            }
        }

        return string.Empty;
    }

    #endregion

    #region Account Transaction Methods

    public virtual async Task<AccountTransactionSearchModel> PrepareAccountTransactionSearchModelAsync(AccountTransactionSearchModel searchModel)
    {
        ArgumentNullException.ThrowIfNull(nameof(searchModel));

        searchModel.AvailableTransactionTypes = await PrepareAvailableTransactionTypesAsync(true, false);
        searchModel.AvailablePaymentMethods = await PrepareAvailablePaymentTypesAsync(true, false);
        searchModel.AvailableAccountGroups = await PrepareAvailableAccountGroupsAsync(true, false, accountCategoryId: 0);

        var allMonths = new List<SelectListItem>
        {
            new SelectListItem { Value = "0", Text = await _localizationService.GetResourceAsync("Admin.Common.All") }
        };
        for (var m = 1; m <= 12; m++)
        {
            allMonths.Add(new SelectListItem
            {
                Value = m.ToString(),
                Text = System.Globalization.CultureInfo.InvariantCulture.DateTimeFormat.GetMonthName(m)
            });
        }
        searchModel.AvailableMonths = allMonths;
        var currentYear = DateTime.UtcNow.Year;
        var allYears = new List<SelectListItem>
        {
            new SelectListItem { Value = "0", Text = await _localizationService.GetResourceAsync("Admin.Common.All") }
        };
        for (var y = currentYear - 3; y <= currentYear; y++)
        {
            allYears.Add(new SelectListItem { Value = y.ToString(), Text = y.ToString() });
        }
        searchModel.AvailableYears = allYears;

        searchModel.AvailablePeriods = new List<SelectListItem>
        {
            new SelectListItem { Value = ((int)SearchPeriodEnum.All).ToString(),       Text = await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.Period.All") },
            new SelectListItem { Value = ((int)SearchPeriodEnum.Today).ToString(),     Text = await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.Period.Today") },
            new SelectListItem { Value = ((int)SearchPeriodEnum.Yesterday).ToString(), Text = await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.Period.Yesterday") },
            new SelectListItem { Value = ((int)SearchPeriodEnum.ThisWeek).ToString(),  Text = await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.Period.ThisWeek") },
            new SelectListItem { Value = ((int)SearchPeriodEnum.ThisMonth).ToString(), Text = await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.Period.ThisMonth") },
            new SelectListItem { Value = ((int)SearchPeriodEnum.LastMonth).ToString(), Text = await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.Period.LastMonth") },
            new SelectListItem { Value = ((int)SearchPeriodEnum.ThisYear).ToString(),  Text = await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.Period.ThisYear") },
            new SelectListItem { Value = ((int)SearchPeriodEnum.LastYear).ToString(),  Text = await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.Period.LastYear") },
            new SelectListItem { Value = ((int)SearchPeriodEnum.Custom).ToString(),    Text = await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.Period.Custom") },
        };

        searchModel.SetGridPageSize();

        return searchModel;
    }

    public virtual async Task<AccountTransactionListModel> PrepareAccountTransactionListModelAsync(AccountTransactionSearchModel searchModel)
    {
        ArgumentNullException.ThrowIfNull(nameof(searchModel));

        DateTime? dateFrom = null;
        DateTime? dateTo = null;

        var istZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        var nowIst = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, istZone);

        switch ((SearchPeriodEnum)searchModel.SearchPeriodId)
        {
            case SearchPeriodEnum.Today:
                dateFrom = TimeZoneInfo.ConvertTimeToUtc(nowIst.Date, istZone);
                dateTo = TimeZoneInfo.ConvertTimeToUtc(nowIst.Date.AddDays(1).AddTicks(-1), istZone);
                break;
            case SearchPeriodEnum.Yesterday:
                dateFrom = TimeZoneInfo.ConvertTimeToUtc(nowIst.Date.AddDays(-1), istZone);
                dateTo = TimeZoneInfo.ConvertTimeToUtc(nowIst.Date.AddTicks(-1), istZone);
                break;
            case SearchPeriodEnum.ThisWeek:
                var daysToMonday = nowIst.DayOfWeek == DayOfWeek.Sunday ? 6 : (int)nowIst.DayOfWeek - 1;
                var weekStart = nowIst.Date.AddDays(-daysToMonday);
                dateFrom = TimeZoneInfo.ConvertTimeToUtc(weekStart, istZone);
                dateTo = TimeZoneInfo.ConvertTimeToUtc(nowIst.Date.AddDays(1).AddTicks(-1), istZone);
                break;
            case SearchPeriodEnum.ThisMonth:
                dateFrom = TimeZoneInfo.ConvertTimeToUtc(new DateTime(nowIst.Year, nowIst.Month, 1), istZone);
                dateTo = TimeZoneInfo.ConvertTimeToUtc(nowIst.Date.AddDays(1).AddTicks(-1), istZone);
                break;
            case SearchPeriodEnum.LastMonth:
                var lastMonth = nowIst.AddMonths(-1);
                dateFrom = TimeZoneInfo.ConvertTimeToUtc(new DateTime(lastMonth.Year, lastMonth.Month, 1), istZone);
                dateTo = TimeZoneInfo.ConvertTimeToUtc(new DateTime(nowIst.Year, nowIst.Month, 1).AddTicks(-1), istZone);
                break;
            case SearchPeriodEnum.ThisYear:
                dateFrom = TimeZoneInfo.ConvertTimeToUtc(new DateTime(nowIst.Year, 1, 1), istZone);
                dateTo = TimeZoneInfo.ConvertTimeToUtc(nowIst.Date.AddDays(1).AddTicks(-1), istZone);
                break;
            case SearchPeriodEnum.LastYear:
                dateFrom = TimeZoneInfo.ConvertTimeToUtc(new DateTime(nowIst.Year - 1, 1, 1), istZone);
                dateTo = TimeZoneInfo.ConvertTimeToUtc(new DateTime(nowIst.Year, 1, 1).AddTicks(-1), istZone);
                break;
            case SearchPeriodEnum.Custom:
                if (DateTime.TryParse(searchModel.SearchDateFrom, out var parsedFrom))
                    dateFrom = TimeZoneInfo.ConvertTimeToUtc(parsedFrom.Date, istZone);
                if (DateTime.TryParse(searchModel.SearchDateTo, out var parsedTo))
                    dateTo = TimeZoneInfo.ConvertTimeToUtc(parsedTo.Date.AddDays(1).AddTicks(-1), istZone);
                break;
        }

        var (resolvedEmployeeIds, resolvedInvoiceIds) = await ResolveEntityFilterAsync(searchModel.SearchEntityValues);

        var accountTransactions = await _accountManagementService.GetAllAccountTransactionsAsync(
            transactionTypeId: searchModel.SearchTransactionTypeId,
            paymentMethodId: searchModel.SearchPaymentMethodId,
            accountGroupId: searchModel.SearchAccountGroupId,
            dateFrom: dateFrom,
            dateTo: dateTo,
            monthId: searchModel.SearchMonthId,
            yearId: searchModel.SearchYearId,
            employeeIds: resolvedEmployeeIds,
            invoiceIds: resolvedInvoiceIds,
            pageIndex: searchModel.Page - 1,
            pageSize: searchModel.PageSize);

        var model = await new AccountTransactionListModel().PrepareToGridAsync(searchModel, accountTransactions, () =>
        {
            return accountTransactions.SelectAwait(async accountTransaction =>
            {
                var accountGroup = await _accountManagementService.GetAccountGroupByIdAsync(accountTransaction.AccountGroupId);
                var createdOnIst = TimeZoneInfo.ConvertTimeFromUtc(accountTransaction.CreatedOnUtc, istZone);
                var monthYear = accountTransaction.MonthId > 0
                    ? System.Globalization.CultureInfo.InvariantCulture.DateTimeFormat.GetAbbreviatedMonthName(accountTransaction.MonthId) + " " + accountTransaction.YearId
                    : string.Empty;
                var sourceName = await ResolveTransactionSourceNameAsync(accountTransaction);
                string linkedEntityLabel = string.Empty;
                string linkedEntityUrl = string.Empty;

                if (accountTransaction.InvoiceId > 0)
                {
                    var invoice = await _accountManagementService.GetInvoiceByIdAsync(accountTransaction.InvoiceId);
                    if (invoice != null)
                    {
                        linkedEntityLabel = $"#{invoice.InvoiceNumber} - {invoice.Title}";
                        linkedEntityUrl = $"/Admin/AccountManagement/InvoiceEdit/{accountTransaction.InvoiceId}";
                    }
                }
                else if (accountTransaction.EmployeeId > 0)
                {
                    var salaryRecord = await _employeeSalaryService.GetSalaryRecordByAccountTransactionIdAsync(accountTransaction.Id);
                    if (salaryRecord != null)
                    {
                        var monthName = salaryRecord.MonthId > 0
                            ? System.Globalization.CultureInfo.InvariantCulture.DateTimeFormat.GetAbbreviatedMonthName(salaryRecord.MonthId)
                            : string.Empty;
                        linkedEntityLabel = $"Salary ({monthName} {salaryRecord.YearId})";
                        linkedEntityUrl = $"/Admin/EmployeeSalary/EmployeeSalaryEdit/{salaryRecord.Id}";
                    }
                }

                var accountTransactionModel = new AccountTransactionModel
                {
                    Id = accountTransaction.Id,
                    TransactionType = Enum.GetName(typeof(TransactionTypeEnum), accountTransaction.TransactionTypeId),
                    AccountGroup = accountGroup?.Name ?? string.Empty,
                    PaymentMethod = Enum.GetName(typeof(PaymentTypeEnum), accountTransaction.PaymentMethodId),
                    Amount = accountTransaction.Amount,
                    Currency = accountTransaction.Currency,
                    ReferenceNo = accountTransaction.ReferenceNo,
                    Notes = accountTransaction.Notes,
                    MonthId = accountTransaction.MonthId,
                    YearId = accountTransaction.YearId,
                    MonthYear = monthYear,
                    SourceName = sourceName,
                    LinkedEntityLabel = linkedEntityLabel,
                    LinkedEntityUrl = linkedEntityUrl,
                    CreatedOnIst = createdOnIst.ToString("dd MMM yyyy HH:mm"),
                };

                return accountTransactionModel;
            });
        });

        return model;
    }

    public virtual async Task<AccountTransactionModel> PrepareAccountTransactionModelAsync(AccountTransactionModel model, AccountTransaction accountTransaction)
    {
        if (accountTransaction != null)
        {
            model = model ?? new AccountTransactionModel();

            model.Id = accountTransaction.Id;
            model.TransactionTypeId = accountTransaction.TransactionTypeId;
            model.InvoiceId = accountTransaction.InvoiceId;
            model.AccountGroupId = accountTransaction.AccountGroupId;
            model.Amount = accountTransaction.Amount;
            model.Currency = accountTransaction.Currency;
            model.PaymentMethodId = accountTransaction.PaymentMethodId;
            model.ReferenceNo = accountTransaction.ReferenceNo;
            model.Notes = accountTransaction.Notes;
            model.MonthId = accountTransaction.MonthId;
            model.YearId = accountTransaction.YearId;
        }

        model.AvailableTransactionTypes = await PrepareAvailableTransactionTypesAsync(false, false);
        model.AvailablePaymentMethods = await PrepareAvailablePaymentTypesAsync(false, true);
        model.AvailableAccountGroups = await PrepareAvailableAccountGroupsAsync(false, false);
        model.AvailableInvoices = await PrepareAvailableInvoicesAsync(false, false);
        model.AvailableMonths = await PrepareAvailableMonthsAsync(model.MonthId);
        model.AvailableYears = await PrepareAvailableYearsAsync();

        if (model.MonthId == 0)
        {
            var selectedMonth = model.AvailableMonths.FirstOrDefault(m => m.Selected);
            if (selectedMonth != null)
                model.MonthId = int.Parse(selectedMonth.Value);
        }

        return model;
    }

    public virtual async Task<(decimal TotalIncome, decimal TotalExpense, int TotalCount)> GetAccountTransactionSummaryAsync(AccountTransactionSearchModel searchModel)
    {
        ArgumentNullException.ThrowIfNull(searchModel);

        var (dateFrom, dateTo) = ResolvePeriodDates(searchModel);
        var (employeeIds, invoiceIds) = await ResolveEntityFilterAsync(searchModel.SearchEntityValues);

        return await _accountManagementService.GetAccountTransactionSummaryAsync(
            transactionTypeId: searchModel.SearchTransactionTypeId,
            paymentMethodId: searchModel.SearchPaymentMethodId,
            accountGroupId: searchModel.SearchAccountGroupId,
            dateFrom: dateFrom,
            dateTo: dateTo,
            monthId: searchModel.SearchMonthId,
            yearId: searchModel.SearchYearId,
            employeeIds: employeeIds,
            invoiceIds: invoiceIds);
    }

    protected virtual (DateTime? dateFrom, DateTime? dateTo) ResolvePeriodDates(AccountTransactionSearchModel searchModel)
    {
        DateTime? dateFrom = null;
        DateTime? dateTo = null;
        var istZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        var nowIst = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, istZone);

        switch ((SearchPeriodEnum)searchModel.SearchPeriodId)
        {
            case SearchPeriodEnum.Today:
                dateFrom = TimeZoneInfo.ConvertTimeToUtc(nowIst.Date, istZone);
                dateTo = TimeZoneInfo.ConvertTimeToUtc(nowIst.Date.AddDays(1).AddTicks(-1), istZone);
                break;
            case SearchPeriodEnum.Yesterday:
                dateFrom = TimeZoneInfo.ConvertTimeToUtc(nowIst.Date.AddDays(-1), istZone);
                dateTo = TimeZoneInfo.ConvertTimeToUtc(nowIst.Date.AddTicks(-1), istZone);
                break;
            case SearchPeriodEnum.ThisWeek:
                var daysToMonday = nowIst.DayOfWeek == DayOfWeek.Sunday ? 6 : (int)nowIst.DayOfWeek - 1;
                dateFrom = TimeZoneInfo.ConvertTimeToUtc(nowIst.Date.AddDays(-daysToMonday), istZone);
                dateTo = TimeZoneInfo.ConvertTimeToUtc(nowIst.Date.AddDays(1).AddTicks(-1), istZone);
                break;
            case SearchPeriodEnum.ThisMonth:
                dateFrom = TimeZoneInfo.ConvertTimeToUtc(new DateTime(nowIst.Year, nowIst.Month, 1), istZone);
                dateTo = TimeZoneInfo.ConvertTimeToUtc(nowIst.Date.AddDays(1).AddTicks(-1), istZone);
                break;
            case SearchPeriodEnum.LastMonth:
                var lastMonth = nowIst.AddMonths(-1);
                dateFrom = TimeZoneInfo.ConvertTimeToUtc(new DateTime(lastMonth.Year, lastMonth.Month, 1), istZone);
                dateTo = TimeZoneInfo.ConvertTimeToUtc(new DateTime(nowIst.Year, nowIst.Month, 1).AddTicks(-1), istZone);
                break;
            case SearchPeriodEnum.ThisYear:
                dateFrom = TimeZoneInfo.ConvertTimeToUtc(new DateTime(nowIst.Year, 1, 1), istZone);
                dateTo = TimeZoneInfo.ConvertTimeToUtc(nowIst.Date.AddDays(1).AddTicks(-1), istZone);
                break;
            case SearchPeriodEnum.LastYear:
                dateFrom = TimeZoneInfo.ConvertTimeToUtc(new DateTime(nowIst.Year - 1, 1, 1), istZone);
                dateTo = TimeZoneInfo.ConvertTimeToUtc(new DateTime(nowIst.Year, 1, 1).AddTicks(-1), istZone);
                break;
            case SearchPeriodEnum.Custom:
                if (DateTime.TryParse(searchModel.SearchDateFrom, out var parsedFrom))
                    dateFrom = TimeZoneInfo.ConvertTimeToUtc(parsedFrom.Date, istZone);
                if (DateTime.TryParse(searchModel.SearchDateTo, out var parsedTo))
                    dateTo = TimeZoneInfo.ConvertTimeToUtc(parsedTo.Date.AddDays(1).AddTicks(-1), istZone);
                break;
        }
        return (dateFrom, dateTo);
    }

    protected virtual async Task<(int[] employeeIds, int[] invoiceIds)> ResolveEntityFilterAsync(string entityValues)
    {
        if (string.IsNullOrWhiteSpace(entityValues))
            return (Array.Empty<int>(), Array.Empty<int>());

        var empIds = new List<int>();
        var companyIds = new List<int>();

        foreach (var part in entityValues.Split(',', StringSplitOptions.RemoveEmptyEntries))
        {
            var trimmed = part.Trim();
            if (trimmed.StartsWith("e_") && int.TryParse(trimmed[2..], out var empId))
                empIds.Add(empId);
            else if (trimmed.StartsWith("c_") && int.TryParse(trimmed[2..], out var compId))
                companyIds.Add(compId);
        }

        var invoiceIds = new List<int>();
        foreach (var companyId in companyIds)
        {
            var invoices = await _accountManagementService.GetAllInvoicesAsync(companyId: companyId, pageSize: int.MaxValue);
            invoiceIds.AddRange(invoices.Select(i => i.Id));
        }

        return (empIds.ToArray(), invoiceIds.ToArray());
    }

    #endregion
}
