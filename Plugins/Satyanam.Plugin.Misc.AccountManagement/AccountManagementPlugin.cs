using App.Core;
using App.Core.Domain.Localization;
using App.Core.Domain.Messages;
using App.Core.Domain.Security;
using App.Core.Infrastructure;
using App.Data;
using App.Services.Common;
using App.Services.Configuration;
using App.Services.Localization;
using App.Services.Messages;
using App.Services.Plugins;
using App.Services.ScheduleTasks;
using App.Services.Security;
using App.Web.Framework.Menu;
using Microsoft.AspNetCore.Routing;
using Satyanam.Plugin.Misc.AccountManagement.Domain;
using Satyanam.Plugin.Misc.AccountManagement.Domain.Enums;
using Satyanam.Plugin.Misc.AccountManagement.Services;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Satyanam.Plugin.Misc.AccountManagement;

public partial class AccountManagementPlugin : BasePlugin,  IAdminMenuPlugin , IMiscPlugin
{
    #region Fields

    protected readonly IPermissionService _permissionService;
    protected readonly ILocalizationService _localizationService;
    protected readonly IMessageTemplateService _messageTemplateService;
    protected readonly INopFileProvider _nopFileProvider;
    protected readonly IRepository<Language> _languageRepository;
    protected readonly IScheduleTaskService _scheduleTaskService;
    protected readonly ISettingService _settingService;
    protected readonly ISalaryComponentConfigService _salaryComponentConfigService;
    protected readonly IWebHelper _webHelper;

    #endregion

    #region Ctor

    public AccountManagementPlugin(IPermissionService permissionService,
        ILocalizationService localizationService,
        IMessageTemplateService messageTemplateService,
        INopFileProvider nopFileProvider,
        IRepository<Language> languageRepository,
        IScheduleTaskService scheduleTaskService,
        ISettingService settingService,
        ISalaryComponentConfigService salaryComponentConfigService,
        IWebHelper webHelper)
    {
        _permissionService = permissionService;
        _salaryComponentConfigService = salaryComponentConfigService;
        _localizationService = localizationService;
        _messageTemplateService = messageTemplateService;
        _nopFileProvider = nopFileProvider;
        _languageRepository = languageRepository;
        _scheduleTaskService = scheduleTaskService;
        _settingService = settingService;
        _webHelper = webHelper;
    }

    #endregion

    #region Plugin Configuration Methods

    public override string GetConfigurationPageUrl()
    {
        return _webHelper.GetStoreLocation() + "Admin/AccountManagement/Configure";
    }

    #endregion

    #region Insert/Delete Message Templates

    public virtual async Task InsertMessageTemplatesAsync()
    {
        var sendInvoiceMessageTemplate = new MessageTemplate()
        {
            Name = AccountManagementDefaults.SendInvoiceNotification,
            BccEmailAddresses = null,
            Subject = AccountManagementDefaults.SendInvoiceSubject,
            EmailAccountId = 1,
            Body = "<p>This is a demo invoice.</p>",
            IsActive = true,
            DelayBeforeSend = 0,
            DelayPeriodId = 0,
            AttachedDownloadId = 0,
            LimitedToStores = false
        };
        await _messageTemplateService.InsertMessageTemplateAsync(sendInvoiceMessageTemplate);
    }

    public virtual async Task DeleteMessageTemplatesAsync()
    {
        var sendInvoiceMessageTemplate = (await _messageTemplateService.GetMessageTemplatesByNameAsync(AccountManagementDefaults.SendInvoiceNotification)).FirstOrDefault();
        if (sendInvoiceMessageTemplate != null)
            await _messageTemplateService.DeleteMessageTemplateAsync(sendInvoiceMessageTemplate);
    }


    #endregion

    #region Insert/Delete Local Resources

    protected virtual async Task InsertLocalResourcesAsync()
    {
        var languages = _languageRepository.Table.Where(l => l.Published).ToList();
        foreach (var language in languages)
        {
            foreach (var filePath in Directory.EnumerateFiles(_nopFileProvider.MapPath($"~/Plugins/Misc.AccountManagement/Localization"), "ResourceStrings.xml", SearchOption.TopDirectoryOnly))
                using (var streamReader = new StreamReader(filePath))
                    await _localizationService.ImportResourcesFromXmlAsync(language, streamReader);
        }
    }

    protected virtual async Task DeleteLocalResourcesAsync()
    {
        var file = Path.Combine(_nopFileProvider.MapPath($"~/Plugins/Misc.AccountManagement/Localization"), "ResourceStrings.xml");
        var languageResourceNames = from name in XDocument.Load(file).Document.Descendants("LocaleResource")
                                    select name.Attribute("Name").Value;

        foreach (var languageResourceName in languageResourceNames)
            await _localizationService.DeleteLocaleResourcesAsync(languageResourceName);
    }

    #endregion

    #region Plugin Install/Uninstall Methods

    public override async Task InstallAsync()
    {
        await InsertLocalResourcesAsync();

        await InsertMessageTemplatesAsync();

        await _permissionService.InstallPermissionsAsync(new AccountManagementPermissionProvider());

        var accountManagementSettings = new AccountManagementSettings()
        {
            EnablePlugin = true
        };
        await _settingService.SaveSettingAsync(accountManagementSettings);

        var expenseManagementSettings = new ExpenseManagementSettings
        {
            SalaryProcessingDay = 1,
            SalaryExpenseCategoryId = 0,
            SendEmailAfterSalaryProcessing = false,
            SalaryProcessingNotifyEmails = string.Empty
        };
        await _settingService.SaveSettingAsync(expenseManagementSettings);

        await base.InstallAsync();

        // Seed default salary components if not already seeded
        var existingComponents = await _salaryComponentConfigService.GetAllActiveComponentsAsync();
        if (existingComponents.Count == 0)
        {
            var now = DateTime.UtcNow;
            // Structure: Basic 45%, DA = 30% of Basic = 13.5% of Gross,
            // HRA = 40% of (Basic+DA) = 23.4% of Gross, Conveyance fixed ₹1600,
            // Medical = Remainder (Gross - Tax - other earnings), Professional Tax ₹200 fixed.
            var defaults = new[]
            {
                new SalaryComponentConfig { Name = "Basic Pay",            ComponentTypeId = (int)SalaryComponentTypeEnum.Earning,   IsPercentage = true,  Value = 45.00m,  IsRemainder = false, IsActive = true, DisplayOrder = 1, CreatedOnUtc = now, UpdatedOnUtc = now },
                new SalaryComponentConfig { Name = "Dearness Allowance",   ComponentTypeId = (int)SalaryComponentTypeEnum.Earning,   IsPercentage = true,  Value = 13.50m,  IsRemainder = false, IsActive = true, DisplayOrder = 2, CreatedOnUtc = now, UpdatedOnUtc = now },
                new SalaryComponentConfig { Name = "House Rent Allowance", ComponentTypeId = (int)SalaryComponentTypeEnum.Earning,   IsPercentage = true,  Value = 23.40m,  IsRemainder = false, IsActive = true, DisplayOrder = 3, CreatedOnUtc = now, UpdatedOnUtc = now },
                new SalaryComponentConfig { Name = "Conveyance Allowance", ComponentTypeId = (int)SalaryComponentTypeEnum.Earning,   IsPercentage = false, Value = 1600.00m,IsRemainder = false, IsActive = true, DisplayOrder = 4, CreatedOnUtc = now, UpdatedOnUtc = now },
                new SalaryComponentConfig { Name = "Medical Allowance",    ComponentTypeId = (int)SalaryComponentTypeEnum.Earning,   IsPercentage = false, Value = 0m,       IsRemainder = true,  IsActive = true, DisplayOrder = 5, CreatedOnUtc = now, UpdatedOnUtc = now },
                new SalaryComponentConfig { Name = "Professional Tax",     ComponentTypeId = (int)SalaryComponentTypeEnum.Deduction, IsPercentage = false, Value = 200.00m, IsRemainder = false, IsActive = true, DisplayOrder = 1, CreatedOnUtc = now, UpdatedOnUtc = now },
            };
            foreach (var component in defaults)
                await _salaryComponentConfigService.InsertComponentAsync(component);
        }
    }

    public override async Task UninstallAsync()
    {
        await DeleteLocalResourcesAsync();

        await DeleteMessageTemplatesAsync();

        await _settingService.DeleteSettingAsync<AccountManagementSettings>();
        await _settingService.DeleteSettingAsync<ExpenseManagementSettings>();

        await base.UninstallAsync();
    }

    #endregion

    #region Manage SiteMap Methods

    public async Task ManageSiteMapAsync(SiteMapNode siteMapNode)
    {
        var pluginMenuItem = new SiteMapNode()
        {
            Title = await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.MainMenu.Title"),
            Visible = await Authenticate(),
            IconClass = "fa fa-cube"
        };

        var configuration = new SiteMapNode()
        {
            SystemName = "Satyanam.Plugin.Misc.AccountManagement.Admin.Configuration",
            Title = await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.Configuration"),
            ControllerName = "AccountManagement",
            ActionName = "Configure",
            Visible = await Authenticate(),
            IconClass = "far fa-dot-circle",
            RouteValues = new RouteValueDictionary() { { "Satyanam.Plugin.Misc.AccountManagement", null } }
        };
        pluginMenuItem.ChildNodes.Add(configuration);

        var accountGroups = new SiteMapNode()
        {
            SystemName = "Satyanam.Plugin.Misc.AccountManagement.Admin.AccountGroups",
            Title = await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.AccountGroups"),
            ControllerName = "AccountManagement",
            ActionName = "AccountGroups",
            Visible = await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageAccountGroups, PermissionAction.View),
            IconClass = "far fa-dot-circle",
            RouteValues = new RouteValueDictionary() { { "Satyanam.Plugin.Misc.AccountManagement", null } }
        };
        pluginMenuItem.ChildNodes.Add(accountGroups);

        var bankAccounts = new SiteMapNode()
        {
            SystemName = "Satyanam.Plugin.Misc.AccountManagement.Admin.BankAccounts",
            Title = await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.BankAccounts"),
            ControllerName = "AccountManagement",
            ActionName = "BankAccounts",
            Visible = await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageBankAccounts, PermissionAction.View),
            IconClass = "far fa-dot-circle",
            RouteValues = new RouteValueDictionary() { { "Satyanam.Plugin.Misc.AccountManagement", null } }
        };
        pluginMenuItem.ChildNodes.Add(bankAccounts);

        var paymentTerms = new SiteMapNode()
        {
            SystemName = "Satyanam.Plugin.Misc.AccountManagement.Admin.PaymentTerms",
            Title = await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.PaymentTerms"),
            ControllerName = "AccountManagement",
            ActionName = "PaymentTerms",
            Visible = await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManagePaymentTerms, PermissionAction.View),
            IconClass = "far fa-dot-circle",
            RouteValues = new RouteValueDictionary() { { "Satyanam.Plugin.Misc.AccountManagement", null } }
        };
        pluginMenuItem.ChildNodes.Add(paymentTerms);

        var projectBillings = new SiteMapNode()
        {
            SystemName = "Satyanam.Plugin.Misc.AccountManagement.Admin.ProjectBillings",
            Title = await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.ProjectBillings"),
            ControllerName = "AccountManagement",
            ActionName = "ProjectBillings",
            Visible = await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageProjectBillings, PermissionAction.View),
            IconClass = "far fa-dot-circle",
            RouteValues = new RouteValueDictionary() { { "Satyanam.Plugin.Misc.AccountManagement", null } }
        };
        pluginMenuItem.ChildNodes.Add(projectBillings);

        var invoices = new SiteMapNode()
        {
            SystemName = "Satyanam.Plugin.Misc.AccountManagement.Admin.Invoices",
            Title = await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.Invoices"),
            ControllerName = "AccountManagement",
            ActionName = "Invoices",
            Visible = await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageInvoices, PermissionAction.View),
            IconClass = "far fa-dot-circle",
            RouteValues = new RouteValueDictionary() { { "Satyanam.Plugin.Misc.AccountManagement", null } }
        };
        pluginMenuItem.ChildNodes.Add(invoices);

        var accountTransactions = new SiteMapNode()
        {
            SystemName = "Satyanam.Plugin.Misc.AccountManagement.Admin.AccountTransactions",
            Title = await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.AccountTransactions"),
            ControllerName = "AccountManagement",
            ActionName = "AccountTransactions",
            Visible = await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageTransactions, PermissionAction.View),
            IconClass = "far fa-dot-circle",
            RouteValues = new RouteValueDictionary() { { "Satyanam.Plugin.Misc.AccountManagement", null } }
        };
        pluginMenuItem.ChildNodes.Add(accountTransactions);

        var expenseCategories = new SiteMapNode()
        {
            SystemName = "Satyanam.Plugin.Misc.AccountManagement.Admin.ExpenseCategories",
            Title = await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.ExpenseCategories"),
            ControllerName = "ExpenseCategory",
            ActionName = "ExpenseCategories",
            Visible = await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageExpenseCategories, PermissionAction.View),
            IconClass = "far fa-dot-circle",
            RouteValues = new RouteValueDictionary() { { "Satyanam.Plugin.Misc.AccountManagement", null } }
        };
        pluginMenuItem.ChildNodes.Add(expenseCategories);

        var expenses = new SiteMapNode()
        {
            SystemName = "Satyanam.Plugin.Misc.AccountManagement.Admin.Expenses",
            Title = await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.Expenses"),
            ControllerName = "Expense",
            ActionName = "Expenses",
            Visible = await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageExpenses, PermissionAction.View),
            IconClass = "far fa-dot-circle",
            RouteValues = new RouteValueDictionary() { { "Satyanam.Plugin.Misc.AccountManagement", null } }
        };
        pluginMenuItem.ChildNodes.Add(expenses);

        var recurringExpenses = new SiteMapNode()
        {
            SystemName = "Satyanam.Plugin.Misc.AccountManagement.Admin.RecurringExpenses",
            Title = await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.RecurringExpenses"),
            ControllerName = "RecurringExpense",
            ActionName = "RecurringExpenses",
            Visible = await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageRecurringExpenses, PermissionAction.View),
            IconClass = "far fa-dot-circle",
            RouteValues = new RouteValueDictionary() { { "Satyanam.Plugin.Misc.AccountManagement", null } }
        };
        pluginMenuItem.ChildNodes.Add(recurringExpenses);

        var employeeSalaries = new SiteMapNode()
        {
            SystemName = "Satyanam.Plugin.Misc.AccountManagement.Admin.EmployeeSalaries",
            Title = await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.EmployeeSalaries"),
            ControllerName = "EmployeeSalary",
            ActionName = "EmployeeSalaries",
            Visible = await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageSalaryProcessing, PermissionAction.View),
            IconClass = "far fa-dot-circle",
            RouteValues = new RouteValueDictionary() { { "Satyanam.Plugin.Misc.AccountManagement", null } }
        };
        pluginMenuItem.ChildNodes.Add(employeeSalaries);

        var salaryComponentConfigs = new SiteMapNode()
        {
            SystemName = "Satyanam.Plugin.Misc.AccountManagement.Admin.SalaryComponentConfigs",
            Title = await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.SalaryComponentConfigs"),
            ControllerName = "SalaryComponentConfig",
            ActionName = "SalaryComponentConfigs",
            Visible = await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageSalaryProcessing, PermissionAction.View),
            IconClass = "far fa-dot-circle",
            RouteValues = new RouteValueDictionary() { { "Satyanam.Plugin.Misc.AccountManagement", null } }
        };
        pluginMenuItem.ChildNodes.Add(salaryComponentConfigs);

        var executiveDashboard = new SiteMapNode()
        {
            SystemName = "Satyanam.Plugin.Misc.AccountManagement.Admin.ExecutiveDashboard",
            Title = await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.ExecutiveDashboard.Title"),
            ControllerName = "ExecutiveDashboard",
            ActionName = "Index",
            Visible = await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageExecutiveDashboard, PermissionAction.View),
            IconClass = "fas fa-chart-line",
            RouteValues = new RouteValueDictionary() { { "Satyanam.Plugin.Misc.AccountManagement", null } }
        };
        pluginMenuItem.ChildNodes.Add(executiveDashboard);

        var title = await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.MainMenu.Title");
        var targetMenu = siteMapNode.ChildNodes.FirstOrDefault(x => x.Title == title);

        if (targetMenu != null)
            targetMenu.ChildNodes.Add(pluginMenuItem);
        else
            siteMapNode.ChildNodes.Add(pluginMenuItem);
    }

    public async Task<bool> Authenticate()
    {
        bool flag = false;
        if (await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageAccountManagementConfiguration, PermissionAction.View))
            flag = true;

        return flag;
    }

    #endregion
}
