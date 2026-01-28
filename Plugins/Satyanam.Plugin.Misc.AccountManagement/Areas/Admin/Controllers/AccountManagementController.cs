using App.Core;
using App.Core.Domain.Directory;
using App.Core.Domain.Extension.TimeSheets;
using App.Core.Domain.Messages;
using App.Core.Domain.Security;
using App.Core.Infrastructure;
using App.Services.Common;
using App.Services.Configuration;
using App.Services.Directory;
using App.Services.Employees;
using App.Services.Holidays;
using App.Services.Localization;
using App.Services.Media;
using App.Services.Messages;
using App.Services.Projects;
using App.Services.Security;
using App.Web.Areas.Admin.Controllers;
using App.Web.Framework.Mvc;
using App.Web.Framework.Mvc.Filters;
using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.AspNetCore.Mvc;
using NUglify.Helpers;
using QuestPDF.Fluent;
using Satyanam.Nop.Core.Services;
using Satyanam.Plugin.Misc.AccountManagement;
using Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Factories;
using Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Models.AccountGroups;
using Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Models.AccountTransactions;
using Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Models.BankAccounts;
using Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Models.Configuration;
using Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Models.Enums;
using Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Models.InvoiceItems;
using Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Models.InvoicePaymentHistories;
using Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Models.Invoices;
using Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Models.PaymentTerms;
using Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Models.PdfInvoice;
using Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Models.ProjectBillings;
using Satyanam.Plugin.Misc.AccountManagement.Domain;
using Satyanam.Plugin.Misc.AccountManagement.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Satyanam.Plugin.Misc.TrackerAPI.Areas.Admin.Controllers;

public partial class AccountManagementController : BaseAdminController
{
    #region Fields

    protected readonly IAccountManagementModelFactory _accountManagementModelFactory;
    protected readonly IAccountManagementService _accountManagementService;
    protected readonly IAddressService _addressService;
    protected readonly ICompanyService _companyService;
    protected readonly IContactsService _contactsService;
    protected readonly ICountryService _countryService;
    protected readonly ICurrencyService _currencyService;
    protected readonly IDownloadService _downloadService;
    protected readonly IEmailAccountService _emailAccountService;
    protected readonly IEmployeeService _employeeService;
    protected readonly IEncryptionService _encryptionService;
    protected readonly IHolidayService _holidayService;
    protected readonly ILocalizationService _localizationService;
    protected readonly INopFileProvider _nopFileProvider;
    protected readonly INotificationService _notificationService;
    protected readonly IPictureService _pictureService;
    protected readonly IPermissionService _permissionService;
    protected readonly IProjectsService _projectsService;
    protected readonly IQueuedEmailService _queuedEmailService;
    protected readonly ISettingService _settingService;
    protected readonly IStateProvinceService _stateProvinceService;
    protected readonly IWorkContext _workContext;
    protected readonly EmailAccountSettings _emailAccountSettings;

    #endregion

    #region Ctor

    public AccountManagementController(IAccountManagementModelFactory accountManagementModelFactory,
        IAccountManagementService accountManagementService,
        IAddressService addressService,
        ICompanyService companyService,
        IContactsService contactsService,
        ICountryService countryService,
        ICurrencyService currencyService,
        IDownloadService downloadService,
        IEmailAccountService emailAccountService,
        IEmployeeService employeeService,
        IEncryptionService encryptionService,
        IHolidayService holidayService,
        ILocalizationService localizationService,
        INopFileProvider nopFileProvider,
        INotificationService notificationService,
        IPermissionService permissionService,
        IProjectsService projectsService,
        IQueuedEmailService queuedEmailService,
        IPictureService pictureService,
        ISettingService settingService,
        IStateProvinceService stateProvinceService,
        IWorkContext workContext,
        EmailAccountSettings emailAccountSettings)
    {
        _accountManagementModelFactory = accountManagementModelFactory;
        _accountManagementService = accountManagementService;
        _addressService = addressService;
        _companyService = companyService;
        _contactsService = contactsService;
        _countryService = countryService;
        _currencyService = currencyService;
        _downloadService = downloadService;
        _emailAccountService = emailAccountService;
        _employeeService = employeeService;
        _encryptionService = encryptionService;
        _holidayService = holidayService;
        _localizationService = localizationService;
        _nopFileProvider = nopFileProvider;
        _notificationService = notificationService;
        _permissionService = permissionService;
        _projectsService = projectsService;
        _queuedEmailService = queuedEmailService;
        _pictureService = pictureService;
        _settingService = settingService;
        _stateProvinceService = stateProvinceService;
        _workContext = workContext;
        _emailAccountSettings = emailAccountSettings;
    }

    #endregion

    #region Utilities

    protected virtual string SaveFileToFolder(string fileName, byte[] fileBytes)
    {
        var filePath = _nopFileProvider.MapPath("~/wwwroot/files/account");

        if (!Directory.Exists(filePath))
            Directory.CreateDirectory(filePath);

        var fullFilePath = Path.Combine(filePath, fileName);

        System.IO.File.WriteAllBytes(fullFilePath, fileBytes);

        return fullFilePath;
    }

    protected virtual async Task<string> SaveZipFileToFolderAsync(string zipFileName, Dictionary<string, byte[]> files)
    {
        var filePath = _nopFileProvider.MapPath("~/wwwroot/files/account");

        if (!Directory.Exists(filePath))
            Directory.CreateDirectory(filePath);

        var zippedFilePath = Path.Combine(filePath, zipFileName);

        using (var fs = new FileStream(zippedFilePath, FileMode.Create))
        using (var archive = new ZipArchive(fs, ZipArchiveMode.Create, leaveOpen: false))
        {
            foreach (var file in files)
            {
                var entry = archive.CreateEntry(file.Key);
                using var entryStream = entry.Open();
                await entryStream.WriteAsync(file.Value, 0, file.Value.Length);
            }
        }

        return zippedFilePath;
    }

    protected virtual string AddTimestampToFileName(string fileName)
    {
        var timestamp = DateTime.Now.ToString("dd_MM_yyyy_HH_mm_ss");
        var extension = Path.GetExtension(fileName);
        var nameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);

        return $"{nameWithoutExtension}_{timestamp}{extension}";
    }

    protected virtual async Task<InvoiceItem> CreateInvoiceItemAsync(int invoiceId, string title, string role, decimal totalHours, int billingRate)
    {
        var invoiceItem = new InvoiceItem
        {
            InvoiceId = invoiceId,
            Description = $"{title} ({role})",
            Hours = totalHours,
            Rate = billingRate
        };
        invoiceItem.Amount = Math.Round(invoiceItem.Hours * billingRate, 2);

        await _accountManagementService.InsertInvoiceItemAsync(invoiceItem);
        return invoiceItem;
    }

    #endregion

    #region Plugin Configuration Methods

    public virtual async Task<IActionResult> Configure()
    {
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageAccountManagementConfiguration, PermissionAction.View))
            return AccessDeniedView();

        var settings = await _settingService.LoadSettingAsync<AccountManagementSettings>();

        var model = new ConfigurationModel()
        {
            EnablePlugin = settings.EnablePlugin,
            InvoiceNumber = settings.InvoiceNumber,
            InvoiceLogoId = settings.InvoiceLogoId
        };

        return View(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> Configure(ConfigurationModel model)
    {
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageAccountManagementConfiguration, PermissionAction.Edit))
            return AccessDeniedView();

        var settings = new AccountManagementSettings()
        {
            EnablePlugin = model.EnablePlugin,
            InvoiceNumber = model.InvoiceNumber,
            InvoiceLogoId = model.InvoiceLogoId
        };
        await _settingService.SaveSettingAsync(settings);

        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

        return await Configure();
    }

    #endregion

    #region Account Groups Methods

    public virtual async Task<IActionResult> AccountGroups()
    {
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageAccountGroups, PermissionAction.View))
            return AccessDeniedView();

        var model = await _accountManagementModelFactory.PrepareAccountGroupSearchModelAsync(new AccountGroupSearchModel());

        return View(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> AccountGroups(AccountGroupSearchModel searchModel)
    {
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageAccountGroups, PermissionAction.View))
            return AccessDeniedView();

        var model = await _accountManagementModelFactory.PrepareAccountGroupListModelAsync(searchModel);

        return Json(model);
    }

    public virtual async Task<IActionResult> AccountGroupCreate()
    {
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageAccountGroups, PermissionAction.Add))
            return AccessDeniedView();

        var model = await _accountManagementModelFactory.PrepareAccountGroupModelAsync(new AccountGroupModel(), null);

        return View(model);
    }

    [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
    public virtual async Task<IActionResult> AccountGroupCreate(AccountGroupModel model, bool continueEditing)
    {
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageAccountGroups, PermissionAction.Add))
            return AccessDeniedView();

        if (ModelState.IsValid)
        {
            var accountGroup = new AccountGroup()
            {
                Name = model.Name,
                AccountCategoryId = model.AccountCategoryId,
                IsActive = model.IsActive,
                DisplayOrder = model.DisplayOrder,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow
            };
            await _accountManagementService.InsertAccountGroupAsync(accountGroup);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.AccountGroup.Added"));

            if (!continueEditing)
                return RedirectToAction(nameof(AccountGroups));

            return RedirectToAction(nameof(AccountGroupEdit), new { id = accountGroup.Id });
        }

        model = await _accountManagementModelFactory.PrepareAccountGroupModelAsync(model, null);

        return View(model);
    }

    public virtual async Task<IActionResult> AccountGroupEdit(int id)
    {
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageAccountGroups, PermissionAction.Edit))
            return AccessDeniedView();

        var existingAccountGroup = await _accountManagementService.GetAccountGroupByIdAsync(id);
        if (existingAccountGroup == null)
            return RedirectToAction(nameof(AccountGroups));

        var model = await _accountManagementModelFactory.PrepareAccountGroupModelAsync(null, existingAccountGroup);

        return View(model);
    }

    [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
    public virtual async Task<IActionResult> AccountGroupEdit(AccountGroupModel model, bool continueEditing)
    {
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageAccountGroups, PermissionAction.Edit))
            return AccessDeniedView();

        var existingAccountGroup = await _accountManagementService.GetAccountGroupByIdAsync(model.Id);
        if (existingAccountGroup == null)
            return RedirectToAction(nameof(AccountGroups));

        if (ModelState.IsValid)
        {
            existingAccountGroup.Name = model.Name;
            existingAccountGroup.AccountCategoryId = model.AccountCategoryId;
            existingAccountGroup.IsActive = model.IsActive;
            existingAccountGroup.DisplayOrder = model.DisplayOrder;
            existingAccountGroup.UpdatedOnUtc = DateTime.UtcNow;
            await _accountManagementService.UpdateAccountGroupAsync(existingAccountGroup);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.AccountGroup.Updated"));

            if (!continueEditing)
                return RedirectToAction(nameof(AccountGroups));

            return RedirectToAction(nameof(AccountGroupEdit), new { id = existingAccountGroup.Id });
        }

        model = await _accountManagementModelFactory.PrepareAccountGroupModelAsync(model, existingAccountGroup);

        return View(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> AccountGroupDelete(int id)
    {
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageAccountGroups, PermissionAction.Delete))
            return AccessDeniedView();

        var existingAccountGroup = await _accountManagementService.GetAccountGroupByIdAsync(id);
        if (existingAccountGroup == null)
            return RedirectToAction(nameof(AccountGroups));

        await _accountManagementService.DeleteAccountGroupAsync(existingAccountGroup);

        return new NullJsonResult();
    }

    #endregion

    #region Bank Accounts Methods

    public virtual async Task<IActionResult> BankAccounts()
    {
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageBankAccounts, PermissionAction.View))
            return AccessDeniedView();

        var model = await _accountManagementModelFactory.PrepareBankAccountSearchModelAsync(new BankAccountSearchModel());

        return View(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> BankAccounts(BankAccountSearchModel searchModel)
    {
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageBankAccounts, PermissionAction.View))
            return AccessDeniedView();

        var model = await _accountManagementModelFactory.PrepareBankAccountListModelAsync(searchModel);

        return Json(model);
    }

    public virtual async Task<IActionResult> BankAccountCreate()
    {
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageBankAccounts, PermissionAction.Add))
            return AccessDeniedView();

        var model = await _accountManagementModelFactory.PrepareBankAccountModelAsync(new BankAccountModel(), null);

        return View(model);
    }

    [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
    public virtual async Task<IActionResult> BankAccountCreate(BankAccountModel model, bool continueEditing)
    {
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageBankAccounts, PermissionAction.Add))
            return AccessDeniedView();

        if (ModelState.IsValid)
        {
            var bankAccount = new BankAccount()
            {
                Title = model.Title,
                BankName = model.BankName,
                AccountNo = model.AccountNo,
                AccountName = model.AccountName,
                SwiftCode = model.SwiftCode,
                IFSCCode = model.IFSCCode,
                AccountType = model.AccountType,
                Branch = model.Branch,
                Address = model.Address,
                Currency = model.Currency,
                Notes = model.Notes,
                IsDefault = model.IsDefault,
                IsActive = model.IsActive,
                DisplayOrder = model.DisplayOrder,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow
            };

            if (model.IsDefault)
            {
                var existingBankAccounts = await _accountManagementService.GetAllBankAccountsAsync();

                foreach (var existingBankAccount in existingBankAccounts)
                {
                    if (existingBankAccount.IsDefault)
                    {
                        existingBankAccount.IsDefault = false;
                        existingBankAccount.UpdatedOnUtc = DateTime.UtcNow;

                        await _accountManagementService.UpdateBankAccountAsync(existingBankAccount);
                    }
                }
            }

            await _accountManagementService.InsertBankAccountAsync(bankAccount);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.BankAccount.Added"));

            if (!continueEditing)
                return RedirectToAction(nameof(BankAccounts));

            return RedirectToAction(nameof(BankAccountEdit), new { id = bankAccount.Id });
        }

        model = await _accountManagementModelFactory.PrepareBankAccountModelAsync(model, null);

        return View(model);
    }

    public virtual async Task<IActionResult> BankAccountEdit(int id)
    {
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageBankAccounts, PermissionAction.Edit))
            return AccessDeniedView();

        var existingBankAccount = await _accountManagementService.GetBankAccountByIdAsync(id);
        if (existingBankAccount == null)
            return RedirectToAction(nameof(BankAccounts));

        var model = await _accountManagementModelFactory.PrepareBankAccountModelAsync(null, existingBankAccount);

        return View(model);
    }

    [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
    public virtual async Task<IActionResult> BankAccountEdit(BankAccountModel model, bool continueEditing)
    {
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageBankAccounts, PermissionAction.Edit))
            return AccessDeniedView();

        var existingBankAccount = await _accountManagementService.GetBankAccountByIdAsync(model.Id);
        if (existingBankAccount == null)
            return RedirectToAction(nameof(BankAccounts));

        if (ModelState.IsValid)
        {
            if (model.IsDefault)
            {
                var existingBankAccounts = await _accountManagementService.GetAllBankAccountsAsync();

                foreach (var account in existingBankAccounts)
                {
                    if (account.Id != existingBankAccount.Id && account.IsDefault)
                    {
                        account.IsDefault = false;
                        account.UpdatedOnUtc = DateTime.UtcNow;
                        await _accountManagementService.UpdateBankAccountAsync(account);
                    }
                }
            }

            existingBankAccount.Title = model.Title;
            existingBankAccount.BankName = model.BankName;
            existingBankAccount.AccountNo = model.AccountNo;
            existingBankAccount.AccountName = model.AccountName;
            existingBankAccount.SwiftCode = model.SwiftCode;
            existingBankAccount.IFSCCode = model.IFSCCode;
            existingBankAccount.AccountType = model.AccountType;
            existingBankAccount.Branch = model.Branch;
            existingBankAccount.Address = model.Address;
            existingBankAccount.Currency = model.Currency;
            existingBankAccount.Notes = model.Notes;
            existingBankAccount.IsDefault = model.IsDefault;
            existingBankAccount.IsActive = model.IsActive;
            existingBankAccount.DisplayOrder = model.DisplayOrder;
            existingBankAccount.UpdatedOnUtc = DateTime.UtcNow;
            await _accountManagementService.UpdateBankAccountAsync(existingBankAccount);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.BankAccount.Updated"));

            if (!continueEditing)
                return RedirectToAction(nameof(BankAccounts));

            return RedirectToAction(nameof(BankAccountEdit), new { id = existingBankAccount.Id });
        }

        model = await _accountManagementModelFactory.PrepareBankAccountModelAsync(model, existingBankAccount);

        return View(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> BankAccountDelete(int id)
    {
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageBankAccounts, PermissionAction.Delete))
            return AccessDeniedView();

        var existingBankAccount = await _accountManagementService.GetBankAccountByIdAsync(id);
        if (existingBankAccount == null)
            return RedirectToAction(nameof(BankAccounts));

        await _accountManagementService.DeleteBankAccountAsync(existingBankAccount);

        return new NullJsonResult();
    }

    #endregion

    #region Payment Terms Methods

    public virtual async Task<IActionResult> PaymentTerms()
    {
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManagePaymentTerms, PermissionAction.View))
            return AccessDeniedView();

        var model = await _accountManagementModelFactory.PreparePaymentTermSearchModelAsync(new PaymentTermSearchModel());

        return View(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> PaymentTerms(PaymentTermSearchModel searchModel)
    {
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManagePaymentTerms, PermissionAction.View))
            return AccessDeniedView();

        var model = await _accountManagementModelFactory.PreparePaymentTermListModellAsync(searchModel);

        return Json(model);
    }

    public virtual async Task<IActionResult> PaymentTermCreate()
    {
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManagePaymentTerms, PermissionAction.Add))
            return AccessDeniedView();

        var model = await _accountManagementModelFactory.PreparePaymentTermModelAsync(new PaymentTermModel(), null);

        return View(model);
    }

    [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
    public virtual async Task<IActionResult> PaymentTermCreate(PaymentTermModel model, bool continueEditing)
    {
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManagePaymentTerms, PermissionAction.Add))
            return AccessDeniedView();

        if (ModelState.IsValid)
        {
            var paymentTerm = new PaymentTerm()
            {
                Name = model.Name,
                NumberOfDays = model.NumberOfDays,
                IsActive = model.IsActive,
                DisplayOrder = model.DisplayOrder,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow
            };
            await _accountManagementService.InsertPaymentTermAsync(paymentTerm);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.PaymentTerm.Added"));

            if (!continueEditing)
                return RedirectToAction(nameof(PaymentTerms));

            return RedirectToAction(nameof(PaymentTermEdit), new { id = paymentTerm.Id });
        }

        model = await _accountManagementModelFactory.PreparePaymentTermModelAsync(model, null);

        return View(model);
    }

    public virtual async Task<IActionResult> PaymentTermEdit(int id)
    {
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManagePaymentTerms, PermissionAction.Edit))
            return AccessDeniedView();

        var existingPaymentTerm = await _accountManagementService.GetPaymentTermByIdAsync(id);
        if (existingPaymentTerm == null)
            return RedirectToAction(nameof(PaymentTerms));

        var model = await _accountManagementModelFactory.PreparePaymentTermModelAsync(null, existingPaymentTerm);

        return View(model);
    }

    [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
    public virtual async Task<IActionResult> PaymentTermEdit(PaymentTermModel model, bool continueEditing)
    {
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManagePaymentTerms, PermissionAction.Edit))
            return AccessDeniedView();

        var existingPaymentTerm = await _accountManagementService.GetPaymentTermByIdAsync(model.Id);
        if (existingPaymentTerm == null)
            return RedirectToAction(nameof(PaymentTerms));

        if (ModelState.IsValid)
        {
            existingPaymentTerm.Name = model.Name;
            existingPaymentTerm.NumberOfDays = model.NumberOfDays;
            existingPaymentTerm.IsActive = model.IsActive;
            existingPaymentTerm.DisplayOrder = model.DisplayOrder;
            existingPaymentTerm.UpdatedOnUtc = DateTime.UtcNow;
            await _accountManagementService.UpdatePaymentTermAsync(existingPaymentTerm);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.PaymentTerm.Updated"));

            if (!continueEditing)
                return RedirectToAction(nameof(PaymentTerms));

            return RedirectToAction(nameof(PaymentTermEdit), new { id = existingPaymentTerm.Id });
        }

        model = await _accountManagementModelFactory.PreparePaymentTermModelAsync(model, existingPaymentTerm);

        return View(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> PaymentTermDelete(int id)
    {
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManagePaymentTerms, PermissionAction.Delete))
            return AccessDeniedView();

        var existingPaymentTerm = await _accountManagementService.GetPaymentTermByIdAsync(id);
        if (existingPaymentTerm == null)
            return RedirectToAction(nameof(PaymentTerms));

        await _accountManagementService.DeletePaymentTermAsync(existingPaymentTerm);

        return new NullJsonResult();
    }

    #endregion

    #region Project Billing Methods

    public virtual async Task<IActionResult> ProjectBillings()
    {
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageProjectBillings, PermissionAction.View))
            return AccessDeniedView();

        var model = await _accountManagementModelFactory.PrepareProjectBillingSearchModelAsync(new ProjectBillingSearchModel());

        return View(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> ProjectBillings(ProjectBillingSearchModel searchModel)
    {
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageProjectBillings, PermissionAction.View))
            return AccessDeniedView();

        var model = await _accountManagementModelFactory.PrepareProjectBillingListModelAsync(searchModel);

        return Json(model);
    }

    public virtual async Task<IActionResult> ProjectBillingCreate()
    {
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageProjectBillings, PermissionAction.Add))
            return AccessDeniedView();

        var model = await _accountManagementModelFactory.PrepareProjectBillingModelAsync(new ProjectBillingModel(), null);

        return View(model);
    }

    [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
    public virtual async Task<IActionResult> ProjectBillingCreate(ProjectBillingModel model, bool continueEditing)
    {
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageProjectBillings, PermissionAction.Add))
            return AccessDeniedView();

        if (ModelState.IsValid)
        {
            var projectBilling = new ProjectBilling()
            {
                BillingName = model.BillingName,
                ProjectId = model.ProjectId,
                CompanyId = model.CompanyId,
                PaymentTermId = model.PaymentTermId,
                BillingTypeId = model.BillingTypeId,
                BillingRate = model.BillingRate,
                PrimaryCurrencyId = model.PrimaryCurrencyId,
                PaymentCurrencyId = model.PaymentCurrencyId,
                IsActive = model.IsActive,
                DisplayOrder = model.DisplayOrder,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow
            };
            await _accountManagementService.InsertProjectBillingAsync(projectBilling);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.ProjectBilling.Added"));

            if (!continueEditing)
                return RedirectToAction(nameof(ProjectBillings));

            return RedirectToAction(nameof(ProjectBillingEdit), new { id = projectBilling.Id });
        }

        model = await _accountManagementModelFactory.PrepareProjectBillingModelAsync(model, null);

        return View(model);
    }

    public virtual async Task<IActionResult> ProjectBillingEdit(int id)
    {
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageProjectBillings, PermissionAction.Edit))
            return AccessDeniedView();

        var existingProjectBilling = await _accountManagementService.GetProjectBillingByIdAsync(id);
        if (existingProjectBilling == null)
            return RedirectToAction(nameof(ProjectBillings));

        var model = await _accountManagementModelFactory.PrepareProjectBillingModelAsync(null, existingProjectBilling);

        return View(model);
    }

    [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
    public virtual async Task<IActionResult> ProjectBillingEdit(ProjectBillingModel model, bool continueEditing)
    {
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageProjectBillings, PermissionAction.Edit))
            return AccessDeniedView();

        var existingProjectBilling = await _accountManagementService.GetProjectBillingByIdAsync(model.Id);
        if (existingProjectBilling == null)
            return RedirectToAction(nameof(ProjectBillings));

        if (ModelState.IsValid)
        {
            existingProjectBilling.BillingName = model.BillingName;
            existingProjectBilling.ProjectId = model.ProjectId;
            existingProjectBilling.CompanyId = model.CompanyId;
            existingProjectBilling.PaymentTermId = model.PaymentTermId;
            existingProjectBilling.BillingTypeId = model.BillingTypeId;
            existingProjectBilling.BillingRate = model.BillingRate;
            existingProjectBilling.PrimaryCurrencyId = model.PrimaryCurrencyId;
            existingProjectBilling.PaymentCurrencyId = model.PaymentCurrencyId;
            existingProjectBilling.IsActive = model.IsActive;
            existingProjectBilling.DisplayOrder = model.DisplayOrder;
            existingProjectBilling.UpdatedOnUtc = DateTime.UtcNow;
            await _accountManagementService.UpdateProjectBillingAsync(existingProjectBilling);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.ProjectBilling.Updated"));

            if (!continueEditing)
                return RedirectToAction(nameof(ProjectBillings));

            return RedirectToAction(nameof(ProjectBillingEdit), new { id = existingProjectBilling.Id });
        }

        model = await _accountManagementModelFactory.PrepareProjectBillingModelAsync(model, existingProjectBilling);

        return View(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> ProjectBillingDelete(int id)
    {
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageProjectBillings, PermissionAction.Delete))
            return AccessDeniedView();

        var existingProjectBilling = await _accountManagementService.GetProjectBillingByIdAsync(id);
        if (existingProjectBilling == null)
            return RedirectToAction(nameof(ProjectBillings));

        await _accountManagementService.DeleteProjectBillingAsync(existingProjectBilling);

        return new NullJsonResult();
    }

    #endregion

    #region Invoice Methods

    public virtual async Task<IActionResult> Invoices()
    {
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageInvoices, PermissionAction.View))
            return AccessDeniedView();

        var model = await _accountManagementModelFactory.PrepareInvoiceSearchModelAsync(new InvoiceSearchModel());

        return View(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> Invoices(InvoiceSearchModel searchModel)
    {
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageInvoices, PermissionAction.View))
            return AccessDeniedView();

        var model = await _accountManagementModelFactory.PrepareInvoiceListModelAsync(searchModel);

        return Json(model);
    }

    public virtual async Task<IActionResult> InvoiceCreate()
    {
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageInvoices, PermissionAction.Add))
            return AccessDeniedView();

        var model = await _accountManagementModelFactory.PrepareInvoiceModelAsync(new InvoiceModel(), null);

        var settings = await _settingService.LoadSettingAsync<AccountManagementSettings>();
        model.InvoiceNumber = settings.InvoiceNumber + 1;

        return View(model);
    }

    [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
    public virtual async Task<IActionResult> InvoiceCreate(InvoiceModel model, bool continueEditing)
    {
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageInvoices, PermissionAction.Add))
            return AccessDeniedView();

        if (ModelState.IsValid)
        {
            var invoice = new Invoice()
            {
                InvoiceNumber = model.InvoiceNumber,
                Title = model.Title,
                ProjectBillingId = model.ProjectBillingId,
                AccountGroupId = model.AccountGroupId,
                StatusId = model.StatusId,
                BankAccountId = model.BankAccountId,
                SubTotalAmount = model.SubTotalAmount,
                TaxAmount = model.TaxAmount,
                DiscountAmount = model.DiscountAmount,
                TotalPrimaryAmount = model.TotalPrimaryAmount,
                TotalPaymentAmount = model.TotalPaymentAmount,
                InvoiceFileId = model.InvoiceFileId,
                TimeSheetFileId = model.TimeSheetFileId,
                MonthId = model.MonthId,
                YearId = model.YearId,
                Notes = model.Notes,
                InvoiceDate = DateTime.UtcNow,
                IsActive = model.IsActive,
                DisplayOrder = model.DisplayOrder,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow
            };

            var existingProjectBilling = await _accountManagementService.GetProjectBillingByIdAsync(model.ProjectBillingId);
            if (existingProjectBilling != null)
            {
                var existingPaymentTerm = await _accountManagementService.GetPaymentTermByIdAsync(existingProjectBilling.PaymentTermId);
                if (existingPaymentTerm != null)
                    invoice.DueDate = DateTime.UtcNow.AddDays(existingPaymentTerm.NumberOfDays);
            }

            await _accountManagementService.InsertInvoiceAsync(invoice);

            var settings = await _settingService.LoadSettingAsync<AccountManagementSettings>();
            settings.InvoiceNumber = model.InvoiceNumber;
            await _settingService.SaveSettingAsync(settings);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.Invoice.Added"));

            if (!continueEditing)
                return RedirectToAction(nameof(Invoices));

            return RedirectToAction(nameof(InvoiceEdit), new { id = invoice.Id });
        }

        model = await _accountManagementModelFactory.PrepareInvoiceModelAsync(model, null);

        return View(model);
    }

    public virtual async Task<IActionResult> InvoiceEdit(int id)
    {
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageInvoices, PermissionAction.Edit))
            return AccessDeniedView();

        var existingInvoice = await _accountManagementService.GetInvoiceByIdAsync(id);
        if (existingInvoice == null)
            return RedirectToAction(nameof(Invoices));

        var existingProjectBilling = await _accountManagementService.GetProjectBillingByIdAsync(existingInvoice.ProjectBillingId);
        if (existingProjectBilling == null)
            return RedirectToAction(nameof(Invoices));

        var model = await _accountManagementModelFactory.PrepareInvoiceModelAsync(null, existingInvoice);

        return View(model);
    }

    [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
    public virtual async Task<IActionResult> InvoiceEdit(InvoiceModel model, bool continueEditing)
    {
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageInvoices, PermissionAction.Edit))
            return AccessDeniedView();

        var existingInvoice = await _accountManagementService.GetInvoiceByIdAsync(model.Id);
        if (existingInvoice == null)
            return RedirectToAction(nameof(Invoices));

        if (ModelState.IsValid)
        {
            if (existingInvoice.ProjectBillingId != model.ProjectBillingId)
            {
                var existingProjectBilling = await _accountManagementService.GetProjectBillingByIdAsync(model.ProjectBillingId);
                if (existingProjectBilling != null)
                {
                    var existingPaymentTerm = await _accountManagementService.GetPaymentTermByIdAsync(existingProjectBilling.PaymentTermId);
                    if (existingPaymentTerm != null)
                        existingInvoice.DueDate = DateTime.UtcNow.AddDays(existingPaymentTerm.NumberOfDays);
                }
            }

            existingInvoice.InvoiceNumber = model.InvoiceNumber;
            existingInvoice.Title = model.Title;
            existingInvoice.ProjectBillingId = model.ProjectBillingId;
            existingInvoice.AccountGroupId = model.AccountGroupId;
            existingInvoice.StatusId = model.StatusId;
            existingInvoice.BankAccountId = model.BankAccountId;
            existingInvoice.SubTotalAmount = model.SubTotalAmount;
            existingInvoice.TaxAmount = model.TaxAmount;
            existingInvoice.DiscountAmount = model.DiscountAmount;
            existingInvoice.TotalPrimaryAmount = model.TotalPrimaryAmount;
            existingInvoice.TotalPaymentAmount = model.TotalPaymentAmount;
            existingInvoice.InvoiceFileId = model.InvoiceFileId;
            existingInvoice.TimeSheetFileId = model.TimeSheetFileId;
            existingInvoice.MonthId = model.MonthId;
            existingInvoice.YearId = model.YearId;
            existingInvoice.Notes = model.Notes;
            existingInvoice.IsActive = model.IsActive;
            existingInvoice.DisplayOrder = model.DisplayOrder;
            existingInvoice.UpdatedOnUtc = DateTime.UtcNow;

            await _accountManagementService.UpdateInvoiceAsync(existingInvoice);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.Invoice.Updated"));

            if (!continueEditing)
                return RedirectToAction(nameof(Invoices));

            return RedirectToAction(nameof(InvoiceEdit), new { id = existingInvoice.Id });
        }

        model = await _accountManagementModelFactory.PrepareInvoiceModelAsync(model, existingInvoice);

        return View(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> InvoiceDelete(int id)
    {
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageInvoices, PermissionAction.Delete))
            return AccessDeniedView();

        var existingInvoice = await _accountManagementService.GetInvoiceByIdAsync(id);
        if (existingInvoice == null)
            return RedirectToAction(nameof(Invoices));

        await _accountManagementService.DeleteInvoiceAsync(existingInvoice);

        return new NullJsonResult();
    }

    public virtual async Task<IActionResult> SendEmail(InvoiceModel model, string mailType)
    {
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageInvoices))
            return AccessDeniedView();

        try
        {
            if (string.IsNullOrWhiteSpace(model.SendEmail.Name))
            {
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.Invoice.SendEmail.Name.Required"));
                return RedirectToAction(nameof(InvoiceEdit), new { id = model.Id });
            }

            if (string.IsNullOrWhiteSpace(model.SendEmail.Email))
            {
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.Invoice.SendEmail.Email.Required"));
                return RedirectToAction(nameof(InvoiceEdit), new { id = model.Id });
            }

            if (string.IsNullOrWhiteSpace(model.SendEmail.Subject))
            {
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.Invoice.SendEmail.Subject.Required"));
                return RedirectToAction(nameof(InvoiceEdit), new { id = model.Id });
            }

            if (string.IsNullOrWhiteSpace(model.SendEmail.Body))
            {
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.Invoice.SendEmail.Body.Required"));
                return RedirectToAction(nameof(InvoiceEdit), new { id = model.Id });
            }

            var emailAccount = await _emailAccountService.GetEmailAccountByIdAsync(_emailAccountSettings.DefaultEmailAccountId);
            if (emailAccount == null)
                emailAccount = (await _emailAccountService.GetAllEmailAccountsAsync()).FirstOrDefault();
            if (emailAccount == null)
            {
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.Invoice.SendTestEmail.Email.Required"));
                return RedirectToAction(nameof(InvoiceEdit), new { id = model.Id });
            }

            var existingInvoice = await _accountManagementService.GetInvoiceByIdAsync(model.Id);
            if (existingInvoice == null)
                return RedirectToAction(nameof(InvoiceEdit), new { id = model.Id });

            var existingProjectBilling = await _accountManagementService.GetProjectBillingByIdAsync(existingInvoice.ProjectBillingId);
            if (existingProjectBilling == null)
                return RedirectToAction(nameof(InvoiceEdit), new { id = model.Id });

            var existingCompany = await _companyService.GetCompanyByIdAsync(existingProjectBilling.CompanyId);
            if (existingCompany == null)
                return RedirectToAction(nameof(InvoiceEdit), new { id = model.Id });

            var existingContactEmails = new List<string>();
            if (mailType == AccountManagementDefaults.Test)
                existingContactEmails.Add(model.SendEmail.Email);
            else
            {
                var existingContacts = await _contactsService.GetContactByCompanyIdAsync(companyId: existingProjectBilling.CompanyId, pageIndex: 0,
                    pageSize: int.MaxValue);
                foreach (var existingContact in existingContacts)
                    existingContactEmails.Add(existingContact.Email);
            }

            var attachedEmails = string.Join(";", existingContactEmails);

            var invoiceDownload = existingInvoice.InvoiceFileId > 0 ? await _downloadService.GetDownloadByIdAsync(existingInvoice.InvoiceFileId) : null;
            var timesheetDownload = existingInvoice.TimeSheetFileId > 0 ? await _downloadService.GetDownloadByIdAsync(existingInvoice.TimeSheetFileId) : null;

            var invoiceBytes = invoiceDownload?.DownloadBinary;
            var timesheetBytes = timesheetDownload?.DownloadBinary;

            var invoiceFileName = invoiceDownload != null ? $"{invoiceDownload.Filename}{invoiceDownload.Extension}" : null;
            var timesheetFileName = timesheetDownload != null ? $"{timesheetDownload.Filename}{timesheetDownload.Extension}" : null;

            string attachmentFilePath = null;
            string attachmentFileName = null;

            if (invoiceBytes != null && timesheetBytes != null)
            {
                var filesToZip = new Dictionary<string, byte[]>
            {
                { invoiceFileName, invoiceBytes },
                { timesheetFileName, timesheetBytes }
            };
                string combinedFileName = AddTimestampToFileName(AccountManagementDefaults.InvoiceAndTimesheet);
                attachmentFilePath = await SaveZipFileToFolderAsync(combinedFileName, filesToZip);
                attachmentFileName = combinedFileName;
            }
            else if (invoiceBytes != null)
            {
                invoiceFileName = AddTimestampToFileName(invoiceFileName);
                attachmentFilePath = SaveFileToFolder(AddTimestampToFileName(invoiceFileName), invoiceBytes);
                attachmentFileName = invoiceFileName;

            }
            else if (timesheetBytes != null)
            {
                timesheetFileName = AddTimestampToFileName(timesheetFileName);
                attachmentFilePath = SaveFileToFolder(timesheetFileName, timesheetBytes);
                attachmentFileName = timesheetFileName;
            }

            var queuedEmail = new QueuedEmail
            {
                Priority = QueuedEmailPriority.High,
                EmailAccountId = emailAccount.Id,
                FromName = emailAccount.DisplayName,
                From = emailAccount.Email,
                To = attachedEmails,
                ToName = model.SendEmail.Name,
                Subject = model.SendEmail.Subject,
                Body = model.SendEmail.Body,
                AttachmentFilePath = attachmentFilePath,
                AttachmentFileName = attachmentFileName,
                CreatedOnUtc = DateTime.UtcNow,
            };
            await _queuedEmailService.InsertQueuedEmailAsync(queuedEmail);

            if (mailType == AccountManagementDefaults.Test)
                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.Invoice.SendTestEmail.Sent"));
            else
                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.Invoice.SendEmail.Sent"));

            if (mailType != AccountManagementDefaults.Test)
            {
                existingInvoice.StatusId = (int)InvoiceEnum.Sent;
                existingInvoice.UpdatedOnUtc = DateTime.UtcNow;
                await _accountManagementService.UpdateInvoiceAsync(existingInvoice);
            }
            
            return RedirectToAction(nameof(InvoiceEdit), new { id = model.Id });
        }
        catch (Exception exception)
        {
            _notificationService.ErrorNotification(exception.Message);
            return RedirectToAction(nameof(Invoices));
        }
    }

    public virtual async Task<IActionResult> GenerateInvoice(int id)
    {
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageInvoices))
            return AccessDeniedView();

        var existingInvoice = await _accountManagementService.GetInvoiceByIdAsync(id);
        if (existingInvoice == null)
            return RedirectToAction(nameof(Invoices));

        var existingInvoiceItems = await _accountManagementService.GetAllInvoiceItemsAsync(existingInvoice.Id);

        var existingBankAccount = await _accountManagementService.GetBankAccountByIdAsync(existingInvoice.BankAccountId);
        if (existingBankAccount == null)
            return RedirectToAction(nameof(Invoices));

        var bankDetails = new Dictionary<string, string>
        {
            ["Account Holder"] = existingBankAccount.AccountName,
            ["Account"] = existingBankAccount.AccountNo,
            ["Bank"] = existingBankAccount.BankName,
            ["Branch"] = existingBankAccount.Branch,
            ["Address"] = string.IsNullOrEmpty(existingBankAccount.Address) ? null : Regex.Replace(existingBankAccount.Address, "<.*?>", "").Trim(),
            ["SWIFT Code"] = existingBankAccount.SwiftCode,
            ["IFSC Code"] = existingBankAccount.IFSCCode,
            ["Account Type"] = existingBankAccount.AccountType,
            ["Currency"] = existingBankAccount.Currency,
            ["Notes"] = string.IsNullOrEmpty(existingBankAccount.Notes) ? null : Regex.Replace(existingBankAccount.Notes, "<.*?>", "").Trim()
        }.Where(kv => !string.IsNullOrWhiteSpace(kv.Value)).Select(kv => $"{kv.Key}: {kv.Value}").Aggregate((a, b) => $"{a}\n{b}");

        string billTo = string.Empty;
        var existingProjectBilling = await _accountManagementService.GetProjectBillingByIdAsync(existingInvoice.ProjectBillingId);
        if (existingProjectBilling != null)
        {
            var existingCompany = await _companyService.GetCompanyByIdAsync(existingProjectBilling.CompanyId);
            if (existingCompany != null)
            {
                var address = await _addressService.GetAddressByIdAsync(existingCompany.BillingAddressId);
                if (address != null)
                {
                    string countryName = string.Empty; string stateName = string.Empty;
                    var country = await _countryService.GetCountryByIdAsync(address.CountryId ?? 0);
                    if (country != null)
                        countryName = country.Name;

                    var state = await _stateProvinceService.GetStateProvinceByIdAsync(address.StateProvinceId ?? 0);
                    if (state != null)
                        stateName = state.Name;

                    billTo = $"{existingCompany?.CompanyName}\n" +
                     $"{address?.Address1}{(string.IsNullOrEmpty(address?.Address2) ? "" : ", " + address.Address2)}\n" +
                     $"{address?.City}, {stateName} {address?.ZipPostalCode}\n" +
                     $"{countryName}";

                    if (string.IsNullOrWhiteSpace(countryName))
                    {
                        _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.Invoice.GenerateInvoice.NoBillingInformation"));
                        return RedirectToAction(nameof(InvoiceEdit), new { id = id });
                    }
                }
            }
        }

        var pdfInvoiceModel = new PdfInvoiceModel
        {
            InvoiceId = existingInvoice.InvoiceNumber,
            InvoiceDate = existingInvoice.InvoiceDate,
            DueDate = existingInvoice.DueDate,
            CompanyName = AccountManagementDefaults.CompanyName,
            CompanyAddress = AccountManagementDefaults.CompanyAddress,
            CompanyWebsite = AccountManagementDefaults.CompanyWebsite,
            BillTo = billTo,
            Items = existingInvoiceItems.Select(x => new PdfInvoiceItemModel
            {
                Description = x.Description,
                Hours = x.Hours,
                Rate = x.Rate,
                Amount = x.Amount
            }).ToList(),
            SubTotal = existingInvoice.SubTotalAmount,
            Discount = existingInvoice.DiscountAmount,
            Tax = existingInvoice.TaxAmount,
            Total = existingInvoice.TotalPaymentAmount,
            BankDetails = bankDetails,
            Notes = string.IsNullOrEmpty(existingInvoice.Notes) ? null : Regex.Replace(existingInvoice.Notes, "<.*?>", "").Trim()
        };

        if (existingInvoice.TotalPrimaryAmount != existingInvoice.TotalPaymentAmount)
        {
            pdfInvoiceModel.Total = decimal.Zero;
            pdfInvoiceModel.TotalPrimaryAmount = existingInvoice.TotalPrimaryAmount;
            pdfInvoiceModel.TotalPaymentAmount = existingInvoice.TotalPaymentAmount;
            pdfInvoiceModel.PrimaryCurrency = (await _currencyService.GetCurrencyByIdAsync(existingProjectBilling.PrimaryCurrencyId)).CurrencyCode;
            pdfInvoiceModel.PaymentCurrency = (await _currencyService.GetCurrencyByIdAsync(existingProjectBilling.PaymentCurrencyId)).CurrencyCode;
        }

        var settings = await _settingService.LoadSettingAsync<AccountManagementSettings>();
        if (settings.InvoiceLogoId > 0)
        {
            var picture = await _pictureService.GetPictureByIdAsync(settings.InvoiceLogoId);
            if (picture != null)
            {
                var imageBytes = await _pictureService.LoadPictureBinaryAsync(picture);
                if (imageBytes?.Length > 0)
                    pdfInvoiceModel.LogoImage = imageBytes;
            }
        }

        var document = new PdfInvoiceDocument(pdfInvoiceModel);
        var pdfBytes = document.GeneratePdf();

        return File(pdfBytes, "application/pdf", $"{existingInvoice.Title}.pdf");
    }

    public virtual async Task<IActionResult> CreateInvoiceFromTimeSummaryReport(DateTime from, DateTime to, int projectId, int periodId, int hoursId)
    {
        int createdInvoiceId = 1;

        if (projectId <= 0)
        {
            _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.Invoice.TimeSummaryReport.SelectProject"));
            return RedirectToAction("TimeSummaryReport", "Reports");
        }

        var existingProject = await _projectsService.GetProjectsByIdAsync(projectId);
        if (existingProject == null)
            return RedirectToAction(nameof(Invoices));

        var existingMappedProjectBilling = await _accountManagementService.GetProjectBillingByProjectIdAsync(projectId);
        if (existingMappedProjectBilling == null)
        {
            _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.Invoice.TimeSummaryReport.NoProjectMappedToProjectBilling"));
            return RedirectToAction("TimeSummaryReport", "Reports");
        }

        var existingAccountGroups = await _accountManagementService.GetAllAccountGroupsAsync(accountCategoryId: (int)AccountCategoryEnum.Income, showHidden: true);
        if (!existingAccountGroups.Any())
        {
            _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.Invoice.TimeSummaryReport.NoAccountGroupsFound"));
            return RedirectToAction("TimeSummaryReport", "Reports");
        }

        var allEmployees = await _employeeService.GetAllEmployeesAsync();
        var holidayList = await _holidayService.GetAllHolidaysAsync();

        var timeSheetReports = new List<TimeSheetReport>();
        foreach (var employee in allEmployees)
        {
            var existingTimeSheetReport = await _accountManagementService.GetReportByEmployeeListWithProjectsAsync(from, to, employee.Id, projectId,
                periodId, hoursId);

            existingTimeSheetReport.ForEach(report =>
            {
                if (employee != null)
                    report.EmployeeName = string.Format("{0} {1}", employee.FirstName, employee.LastName);

                if (holidayList != null)
                    report.IsHoliday = holidayList.Any(holiday => holiday.Date == report.SpentDate);

                report.IsWeekend = report.SpentDate.DayOfWeek == DayOfWeek.Saturday || report.SpentDate.DayOfWeek == DayOfWeek.Sunday;
            });

            timeSheetReports.AddRange(existingTimeSheetReport);
        }

        var qaEmployeeMapping = await _projectsService.GetProjectQAIdByIdAsync(projectId);

        var filteredTimeSummaryReports = timeSheetReports.Where(x => x.SpentHours > 0 || x.SpentMinutes > 0).ToList();

        var qaReports = filteredTimeSummaryReports.Where(x => x.EmployeeId == qaEmployeeMapping).ToList();
        var qaTotalMinutes = qaReports.Sum(x => (x.SpentHours * 60) + x.SpentMinutes);
        decimal qaTotalHours = qaTotalMinutes / 60;
        decimal qaRemainingMinutes = qaTotalMinutes % 60;
        qaTotalHours = Math.Round(qaTotalHours + (qaRemainingMinutes / 60), 2);

        var developerReports = filteredTimeSummaryReports.Where(x => x.EmployeeId != qaEmployeeMapping).ToList();
        var developerTotalMinutes = developerReports.Sum(x => (x.SpentHours * 60) + x.SpentMinutes);
        decimal developerTotalHours = developerTotalMinutes / 60;
        decimal developerRemainingMinutes = developerTotalMinutes % 60;
        developerTotalHours = Math.Round(developerTotalHours + (developerRemainingMinutes / 60), 2);

        var settings = await _settingService.LoadSettingAsync<AccountManagementSettings>();
        int invoiceNumber = settings.InvoiceNumber + 1;

        string title = string.Format("{0} - {1}", existingProject.ProjectTitle, from.ToString("MMMM"));
        var invoice = new Invoice()
        {
            InvoiceNumber = invoiceNumber,
            Title = title,
            ProjectBillingId = existingMappedProjectBilling.Id,
            AccountGroupId = existingAccountGroups.FirstOrDefault().Id,
            StatusId = (int)InvoiceEnum.Draft,
            SubTotalAmount = decimal.Zero,
            TaxAmount = decimal.Zero,
            DiscountAmount = decimal.Zero,
            TotalPrimaryAmount = decimal.Zero,
            TotalPaymentAmount = decimal.Zero,
            MonthId = from.Month,
            YearId = from.Year,
            InvoiceFileId = 0,
            TimeSheetFileId = 0,
            Notes = string.Empty,
            InvoiceDate = DateTime.UtcNow,
            IsActive = true,
            DisplayOrder = (await _accountManagementService.GetAllInvoicesAsync()).Count() + 1,
            CreatedOnUtc = DateTime.UtcNow,
            UpdatedOnUtc = DateTime.UtcNow
        };

        var existingDefaultBankAccount = await _accountManagementService.GetDefaultBankAccountAsync();
        if (existingDefaultBankAccount != null)
            invoice.BankAccountId = existingDefaultBankAccount.Id;

        var existingProjectBilling = await _accountManagementService.GetProjectBillingByIdAsync(existingMappedProjectBilling.Id);
        if (existingProjectBilling != null)
        {
            var existingPaymentTerm = await _accountManagementService.GetPaymentTermByIdAsync(existingProjectBilling.PaymentTermId);
            if (existingPaymentTerm != null)
                invoice.DueDate = DateTime.UtcNow.AddDays(existingPaymentTerm.NumberOfDays);
        }
        await _accountManagementService.InsertInvoiceAsync(invoice);

        settings.InvoiceNumber = invoiceNumber;
        await _settingService.SaveSettingAsync(settings);

        var existingInvoice = await _accountManagementService.GetInvoiceByIdAsync(invoice.Id);
        if (existingInvoice != null)
        {
            var developerInvoiceItem = await CreateInvoiceItemAsync(invoice.Id, title, "Developer", developerTotalHours, existingProjectBilling.BillingRate);
            var qaInvoiceItem = await CreateInvoiceItemAsync(invoice.Id, title, "QA", qaTotalHours, existingProjectBilling.BillingRate);

            existingInvoice.SubTotalAmount = developerInvoiceItem.Amount + qaInvoiceItem.Amount;
            existingInvoice.TotalPrimaryAmount = developerInvoiceItem.Amount + qaInvoiceItem.Amount;
            var sourceCurrency = await _currencyService.GetCurrencyByIdAsync(existingProjectBilling.PrimaryCurrencyId);
            var targetCurrency = await _currencyService.GetCurrencyByIdAsync(existingProjectBilling.PaymentCurrencyId);
            decimal amount = await _currencyService.ConvertCurrencyAsync(existingInvoice.TotalPrimaryAmount, sourceCurrency, targetCurrency);
            existingInvoice.TotalPaymentAmount = Math.Round(amount, 2);
            existingInvoice.UpdatedOnUtc = DateTime.UtcNow;
            await _accountManagementService.UpdateInvoiceAsync(existingInvoice);
        }

        return Json(new { redirectUrl = Url.Action("InvoiceEdit", "AccountManagement", new { id = invoice.Id }) });
    }

    #endregion

    #region Invoice Items Methods

    public virtual async Task<IActionResult> InvoiceItems(InvoiceItemSearchModel searchModel)
    {
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageInvoiceItems, PermissionAction.View))
            return AccessDeniedView();

        var existingInvoice = await _accountManagementService.GetInvoiceByIdAsync(searchModel.InvoiceId);
        if (existingInvoice == null)
            return RedirectToAction(nameof(Invoices));

        var model = await _accountManagementModelFactory.PrepareInvoiceItemListModelAsync(searchModel);

        return Json(model);
    }

    public virtual async Task<IActionResult> InvoiceItemCreatePopup(int invoiceId)
    {
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageInvoiceItems, PermissionAction.Add))
            return AccessDeniedView();

        var existingInvoice = await _accountManagementService.GetInvoiceByIdAsync(invoiceId);
        if (existingInvoice == null)
            return RedirectToAction(nameof(Invoices));

        var model = await _accountManagementModelFactory.PrepareInvoiceItemModelAsync(new InvoiceItemModel(), existingInvoice, null);

        return View(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> InvoiceItemCreatePopup(InvoiceItemModel model)
    {
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageInvoiceItems, PermissionAction.Add))
            return AccessDeniedView();

        var existingInvoice = await _accountManagementService.GetInvoiceByIdAsync(model.InvoiceId);
        if (existingInvoice == null)
            return RedirectToAction(nameof(Invoices));

        if (ModelState.IsValid)
        {
            var invoiceItem = new InvoiceItem()
            {
                InvoiceId = model.InvoiceId,
                Description = model.Description,
                Hours = model.Hours,
                Rate = model.Rate,
                Amount = model.Amount,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow
            };
            await _accountManagementService.InsertInvoiceItemAsync(invoiceItem);

            existingInvoice.SubTotalAmount += model.Amount;
            existingInvoice.TotalPrimaryAmount += model.Amount;
            var existingProjectBilling = await _accountManagementService.GetProjectBillingByIdAsync(existingInvoice.ProjectBillingId);
            if (existingProjectBilling != null)
            {
                var sourceCurrency = await _currencyService.GetCurrencyByIdAsync(existingProjectBilling.PrimaryCurrencyId);
                var targetCurrency = await _currencyService.GetCurrencyByIdAsync(existingProjectBilling.PaymentCurrencyId);
                decimal amount = await _currencyService.ConvertCurrencyAsync(existingInvoice.TotalPrimaryAmount, sourceCurrency, targetCurrency);
                existingInvoice.TotalPaymentAmount = Math.Round(amount, 2);
            }
            existingInvoice.UpdatedOnUtc = DateTime.UtcNow;
            await _accountManagementService.UpdateInvoiceAsync(existingInvoice);

            ViewBag.RefreshPage = true;

            return View(model);
        }

        model = await _accountManagementModelFactory.PrepareInvoiceItemModelAsync(model, existingInvoice, null);

        return View(model);
    }

    public virtual async Task<IActionResult> InvoiceItemEditPopup(int id)
    {
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageInvoiceItems, PermissionAction.Edit))
            return AccessDeniedView();

        var existingInvoiceItem = await _accountManagementService.GetInvoiceItemByIdAsync(id);
        if (existingInvoiceItem == null)
            return RedirectToAction(nameof(Invoices));

        var existingInvoice = await _accountManagementService.GetInvoiceByIdAsync(existingInvoiceItem.InvoiceId);
        if (existingInvoice == null)
            return RedirectToAction(nameof(Invoices));

        var model = await _accountManagementModelFactory.PrepareInvoiceItemModelAsync(null, existingInvoice, existingInvoiceItem);

        return View(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> InvoiceItemEditPopup(InvoiceItemModel model)
    {
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageInvoiceItems, PermissionAction.Edit))
            return AccessDeniedView();

        var existingInvoiceItem = await _accountManagementService.GetInvoiceItemByIdAsync(model.Id);
        if (existingInvoiceItem == null)
            return RedirectToAction(nameof(Invoices));

        var existingInvoice = await _accountManagementService.GetInvoiceByIdAsync(existingInvoiceItem.InvoiceId);
        if (existingInvoice == null)
            return RedirectToAction(nameof(Invoices));

        if (ModelState.IsValid)
        {
            existingInvoice.SubTotalAmount -= existingInvoiceItem.Amount;
            existingInvoice.TotalPrimaryAmount -= existingInvoiceItem.Amount;

            existingInvoiceItem.Description = model.Description;
            existingInvoiceItem.Hours = model.Hours;
            existingInvoiceItem.Rate = model.Rate;
            existingInvoiceItem.Amount = model.Amount;
            existingInvoiceItem.UpdatedOnUtc = DateTime.UtcNow;
            await _accountManagementService.UpdateInvoiceItemAsync(existingInvoiceItem);

            existingInvoice.SubTotalAmount += model.Amount;
            existingInvoice.TotalPrimaryAmount += model.Amount;
            var existingProjectBilling = await _accountManagementService.GetProjectBillingByIdAsync(existingInvoice.ProjectBillingId);
            if (existingProjectBilling != null)
            {
                var sourceCurrency = await _currencyService.GetCurrencyByIdAsync(existingProjectBilling.PrimaryCurrencyId);
                var targetCurrency = await _currencyService.GetCurrencyByIdAsync(existingProjectBilling.PaymentCurrencyId);
                decimal amount = await _currencyService.ConvertCurrencyAsync(existingInvoice.TotalPrimaryAmount, sourceCurrency, targetCurrency);
                existingInvoice.TotalPaymentAmount = Math.Round(amount, 2);
            }
            existingInvoice.UpdatedOnUtc = DateTime.UtcNow;
            await _accountManagementService.UpdateInvoiceAsync(existingInvoice);

            ViewBag.RefreshPage = true;

            return View(model);
        }

        model = await _accountManagementModelFactory.PrepareInvoiceItemModelAsync(model, existingInvoice, existingInvoiceItem);

        return View(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> InvoiceItemDelete(int id)
    {
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageInvoiceItems, PermissionAction.Delete))
            return AccessDeniedView();

        var existingInvoiceItem = await _accountManagementService.GetInvoiceItemByIdAsync(id)
            ?? throw new ArgumentException("No invoice item found with the specified id");

        await _accountManagementService.DeleteInvoiceItemAsync(existingInvoiceItem);

        var existingInvoice = await _accountManagementService.GetInvoiceByIdAsync(existingInvoiceItem.InvoiceId);
        existingInvoice.SubTotalAmount -= existingInvoiceItem.Amount;
        existingInvoice.TotalPrimaryAmount -= existingInvoiceItem.Amount;
        var existingProjectBilling = await _accountManagementService.GetProjectBillingByIdAsync(existingInvoice.ProjectBillingId);
        if (existingProjectBilling != null)
        {
            var sourceCurrency = await _currencyService.GetCurrencyByIdAsync(existingProjectBilling.PrimaryCurrencyId);
            var targetCurrency = await _currencyService.GetCurrencyByIdAsync(existingProjectBilling.PaymentCurrencyId);
            decimal amount = await _currencyService.ConvertCurrencyAsync(existingInvoice.TotalPrimaryAmount, sourceCurrency, targetCurrency);
            existingInvoice.TotalPaymentAmount = Math.Round(amount, 2);
        }
        existingInvoice.UpdatedOnUtc = DateTime.UtcNow;
        await _accountManagementService.UpdateInvoiceAsync(existingInvoice);

        return new NullJsonResult();
    }

    #endregion

    #region Invoice Payment Histories Methods

    public virtual async Task<IActionResult> InvoicePaymentHistories(InvoicePaymentHistorySearchModel searchModel)
    {
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageInvoicePaymentHistories, PermissionAction.View))
            return AccessDeniedView();

        var existingInvoice = await _accountManagementService.GetInvoiceByIdAsync(searchModel.InvoiceId);
        if (existingInvoice == null)
            return RedirectToAction(nameof(Invoices));

        var model = await _accountManagementModelFactory.PrepareInvoicePaymentHistoryListModelAsync(searchModel);

        return Json(model);
    }

    public virtual async Task<IActionResult> InvoicePaymentHistoryCreatePopup(int invoiceId)
    {
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageInvoicePaymentHistories, PermissionAction.Add))
            return AccessDeniedView();

        var existingInvoice = await _accountManagementService.GetInvoiceByIdAsync(invoiceId);
        if (existingInvoice == null)
            return RedirectToAction(nameof(Invoices));

        var model = await _accountManagementModelFactory.PrepareInvoicePaymentHistoryModelAsync(new InvoicePaymentHistoryModel(), existingInvoice, null);

        return View(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> InvoicePaymentHistoryCreatePopup(InvoicePaymentHistoryModel model)
    {
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageInvoicePaymentHistories, PermissionAction.Add))
            return AccessDeniedView();

        var existingInvoice = await _accountManagementService.GetInvoiceByIdAsync(model.InvoiceId);
        if (existingInvoice == null)
            return RedirectToAction(nameof(Invoices));

        if (ModelState.IsValid)
        {
            var invoicePaymentHistory = new InvoicePaymentHistory()
            {
                InvoiceId = model.InvoiceId,
                AmountInPaymentCurrency = model.AmountInPaymentCurrency,
                AmountInINR = model.AmountInINR,
                PaymentMethodId = model.PaymentMethodId,
                IsPartialPayment = model.IsPartialPayment,
                Notes = model.Notes,
                PaymentReceiptId = model.PaymentReceiptId,
                MonthId = model.MonthId,
                YearId = model.YearId,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow
            };
            await _accountManagementService.InsertInvoicePaymentHistoryAsync(invoicePaymentHistory);

            var accountTransaction = new AccountTransaction()
            {
                InvoicePaymentHistoryId = invoicePaymentHistory.Id,
                TransactionTypeId = (int)TransactionTypeEnum.Income,
                InvoiceId = model.InvoiceId,
                Amount = model.AmountInINR,
                PaymentMethodId = model.PaymentMethodId,
                MonthId = model.MonthId,
                YearId = model.YearId,
                CreatedBy = (await _workContext.GetCurrentCustomerAsync()).Id,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow
            };
            await _accountManagementService.InsertAccountTransactionAsync(accountTransaction);

            ViewBag.RefreshPage = true;

            return View(model);
        }

        model = await _accountManagementModelFactory.PrepareInvoicePaymentHistoryModelAsync(model, existingInvoice, null);

        return View(model);
    }

    public virtual async Task<IActionResult> InvoicePaymentHistoryEditPopup(int id)
    {
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageInvoicePaymentHistories, PermissionAction.Edit))
            return AccessDeniedView();

        var existingInvoicePaymentHistory = await _accountManagementService.GetInvoicePaymentHistoryByIdAsync(id);
        if (existingInvoicePaymentHistory == null)
            return RedirectToAction(nameof(Invoices));

        var existingInvoice = await _accountManagementService.GetInvoiceByIdAsync(existingInvoicePaymentHistory.InvoiceId);
        if (existingInvoice == null)
            return RedirectToAction(nameof(Invoices));

        var model = await _accountManagementModelFactory.PrepareInvoicePaymentHistoryModelAsync(null, existingInvoice, existingInvoicePaymentHistory);

        return View(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> InvoicePaymentHistoryEditPopup(InvoicePaymentHistoryModel model)
    {
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageInvoicePaymentHistories, PermissionAction.Edit))
            return AccessDeniedView();

        var existingInvoicePaymentHistory = await _accountManagementService.GetInvoicePaymentHistoryByIdAsync(model.Id);
        if (existingInvoicePaymentHistory == null)
            return RedirectToAction(nameof(Invoices));

        var existingInvoice = await _accountManagementService.GetInvoiceByIdAsync(existingInvoicePaymentHistory.InvoiceId);
        if (existingInvoice == null)
            return RedirectToAction(nameof(Invoices));

        if (ModelState.IsValid)
        {
            existingInvoicePaymentHistory.AmountInPaymentCurrency = model.AmountInPaymentCurrency;
            existingInvoicePaymentHistory.AmountInINR = model.AmountInINR;
            existingInvoicePaymentHistory.PaymentMethodId = model.PaymentMethodId;
            existingInvoicePaymentHistory.IsPartialPayment = model.IsPartialPayment;
            existingInvoicePaymentHistory.Notes = model.Notes;
            existingInvoicePaymentHistory.PaymentReceiptId = model.PaymentReceiptId;
            existingInvoicePaymentHistory.MonthId = model.MonthId;
            existingInvoicePaymentHistory.YearId = model.YearId;
            existingInvoicePaymentHistory.UpdatedOnUtc = DateTime.UtcNow;
            await _accountManagementService.UpdateInvoicePaymentHistoryAsync(existingInvoicePaymentHistory);

            var existingAccountTransaction = await _accountManagementService.GetAccountTransactionByInvoicePaymentHistoryIdAsync(existingInvoicePaymentHistory.Id);
            if (existingAccountTransaction != null)
            {
                existingAccountTransaction.Amount = model.AmountInINR;
                existingAccountTransaction.PaymentMethodId = model.PaymentMethodId;
                existingAccountTransaction.MonthId = model.MonthId;
                existingAccountTransaction.YearId = model.YearId;
                existingAccountTransaction.CreatedBy = (await _workContext.GetCurrentCustomerAsync()).Id;
                existingAccountTransaction.UpdatedOnUtc = DateTime.UtcNow;
                await _accountManagementService.UpdateAccountTransactionAsync(existingAccountTransaction);
            }

            ViewBag.RefreshPage = true;

            return View(model);
        }

        model = await _accountManagementModelFactory.PrepareInvoicePaymentHistoryModelAsync(model, existingInvoice, existingInvoicePaymentHistory);

        return View(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> InvoicePaymentHistoryDelete(int id)
    {
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageInvoicePaymentHistories, PermissionAction.Delete))
            return AccessDeniedView();

        var existingInvoicePaymentHistory = await _accountManagementService.GetInvoicePaymentHistoryByIdAsync(id)
            ?? throw new ArgumentException("No invoice payment history found with the specified id");
        await _accountManagementService.DeleteInvoicePaymentHistoryAsync(existingInvoicePaymentHistory);

        var existingAccountTransaction = await _accountManagementService.GetAccountTransactionByInvoicePaymentHistoryIdAsync(existingInvoicePaymentHistory.Id)
            ?? throw new ArgumentException("No transaction found with the specified id");
        await _accountManagementService.DeleteAccountTransactionAsync(existingAccountTransaction);

        return new NullJsonResult();
    }

    public async Task<IActionResult> ConvertedCurrencyAmount(int invoiceId, decimal amountInPaymentCurrency, bool inrConvert = false)
    {
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageInvoices))
            return AccessDeniedView();

        var existingInvoice = await _accountManagementService.GetInvoiceByIdAsync(invoiceId);
        if (existingInvoice == null)
            return Json(new { result = false });

        var existingProjectBilling = await _accountManagementService.GetProjectBillingByIdAsync(existingInvoice.ProjectBillingId);
        if (existingProjectBilling == null)
            return Json(new { result = false });

        Currency sourceCurrency = null;
        Currency targetCurrency = null;

        if (inrConvert)
        {
            sourceCurrency = await _currencyService.GetCurrencyByIdAsync(existingProjectBilling.PaymentCurrencyId);
            targetCurrency = (await _currencyService.GetAllCurrenciesAsync(showHidden: true)).Where(c => c.CurrencyCode == "INR").FirstOrDefault();
        }
        else
        {
            sourceCurrency = await _currencyService.GetCurrencyByIdAsync(existingProjectBilling.PrimaryCurrencyId);
            targetCurrency = await _currencyService.GetCurrencyByIdAsync(existingProjectBilling.PaymentCurrencyId);
        }

        if (sourceCurrency == null || targetCurrency == null)
            return Json(new { result = false });

        decimal amount = await _currencyService.ConvertCurrencyAsync(amountInPaymentCurrency, sourceCurrency, targetCurrency);

        return Json(new
        {
            result = true,
            convertedAmount = Math.Round(amount, 2)
        });
    }


    #endregion

    #region Account Transaction Methods

    public virtual async Task<IActionResult> AccountTransactions()
    {
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageTransactions, PermissionAction.View))
            return AccessDeniedView();

        var model = await _accountManagementModelFactory.PrepareAccountTransactionSearchModelAsync(new AccountTransactionSearchModel());

        return View(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> AccountTransactions(AccountTransactionSearchModel searchModel)
    {
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageTransactions, PermissionAction.View))
            return AccessDeniedView();

        var model = await _accountManagementModelFactory.PrepareAccountTransactionListModelAsync(searchModel);

        return Json(model);
    }

    public virtual async Task<IActionResult> AccountTransactionCreate()
    {
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageTransactions, PermissionAction.Add))
            return AccessDeniedView();

        var model = await _accountManagementModelFactory.PrepareAccountTransactionModelAsync(new AccountTransactionModel(), null);

        return View(model);
    }

    [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
    public virtual async Task<IActionResult> AccountTransactionCreate(AccountTransactionModel model, bool continueEditing)
    {
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageTransactions, PermissionAction.Add))
            return AccessDeniedView();

        if (ModelState.IsValid)
        {
            var accountTransaction = new AccountTransaction()
            {
                TransactionTypeId = model.TransactionTypeId,
                InvoiceId = model.InvoiceId,
                AccountGroupId = model.AccountGroupId,
                Amount = model.Amount,
                Currency = model.Currency,
                PaymentMethodId = model.PaymentMethodId,
                ReferenceNo = model.ReferenceNo,
                Notes = model.Notes,
                MonthId = model.MonthId,
                YearId = model.YearId,
                CreatedBy = (await _workContext.GetCurrentCustomerAsync()).Id,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow
            };
            await _accountManagementService.InsertAccountTransactionAsync(accountTransaction);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.AccountTransaction.Added"));

            if (!continueEditing)
                return RedirectToAction(nameof(AccountTransactions));
        }

        model = await _accountManagementModelFactory.PrepareAccountTransactionModelAsync(model, null);

        return View(model);
    }

    public async Task<IActionResult> GetAccountGroupsByTransactionType(int transactionTypeId)
    {
        var availableAccountGroups = await _accountManagementService.GetAllAccountGroupsAsync(accountCategoryId: transactionTypeId, showHidden: true);

        var accountGroups = availableAccountGroups.Select(availableAccountGroup => new {
            id = availableAccountGroup.Id,
            name = availableAccountGroup.Name
        });

        return Json(accountGroups);
    }


    #endregion
}
