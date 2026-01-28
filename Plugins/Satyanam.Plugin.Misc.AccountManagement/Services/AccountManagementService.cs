using App.Core;
using App.Core.Domain.Extension.TimeSheets;
using App.Core.Domain.Messages;
using App.Core.Domain.Projects;
using App.Core.Domain.ProjectTasks;
using App.Core.Domain.TimeSheets;
using App.Core.Events;
using App.Core.Infrastructure;
using App.Data;
using App.Data.Extensions;
using App.Services.Helpers;
using App.Services.Localization;
using App.Services.Media;
using App.Services.Messages;
using DocumentFormat.OpenXml.Office2010.Excel;
using Satyanam.Nop.Core.Domains;
using Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Models.Enums;
using Satyanam.Plugin.Misc.AccountManagement.Domain;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace Satyanam.Plugin.Misc.AccountManagement.Services;

public partial class AccountManagementService : IAccountManagementService
{
    #region Fields

    protected readonly IDateTimeHelper _dateTimeHelper;
    protected readonly IDownloadService _downloadService;
    protected readonly IEmailAccountService _emailAccountService;
    protected readonly IEventPublisher _eventPublisher;
    protected readonly ILanguageService _languageService;
    protected readonly ILocalizationService _localizationService;
    protected readonly IMessageTemplateService _messageTemplateService;
    protected readonly IMessageTokenProvider _messageTokenProvider;
    protected readonly INopFileProvider _nopFileProvider;
    protected readonly IRepository<AccountGroup> _accountGroupRepository;
    protected readonly IRepository<AccountTransaction> _accountTransactionRepository;
    protected readonly IRepository<BankAccount> _bankAccountRepository;
    protected readonly IRepository<Invoice> _invoiceRepository;
    protected readonly IRepository<InvoiceItem> _invoiceItemRepository;
    protected readonly IRepository<InvoicePaymentHistory> _invoicePaymentHistoryRepository;
    protected readonly IRepository<Company> _companyRepository;
    protected readonly IRepository<PaymentTerm> _paymentTermRepository;
    protected readonly IRepository<ProjectBilling> _projectBillingRepository;
    protected readonly IRepository<Project> _projectRepository;
    protected readonly IRepository<ProjectTask> _projectTaskRepository;
    protected readonly IRepository<TimeSheet> _timeSheetRepository;
    protected readonly IStoreContext _storeContext;
    protected readonly IWorkflowMessageService _workflowMessageService;
    protected readonly EmailAccountSettings _emailAccountSettings;

    #endregion

    #region Ctor

    public AccountManagementService(IDateTimeHelper dateTimeHelper,
        IDownloadService downloadService,
        IEmailAccountService emailAccountService,
        IEventPublisher eventPublisher,
        ILanguageService languageService,
        ILocalizationService localizationService,
        IMessageTemplateService messageTemplateService,
        IMessageTokenProvider messageTokenProvider,
        INopFileProvider nopFileProvider,
        IRepository<AccountGroup> accountGroupRepository,
        IRepository<AccountTransaction> accountTransactionRepository,
        IRepository<BankAccount> bankAccountRepository,
        IRepository<Invoice> invoiceRepository,
        IRepository<InvoiceItem> invoiceItemRepository,
        IRepository<InvoicePaymentHistory> invoicePaymentHistoryRepository,
        IRepository<Company> companyRepository,
        IRepository<PaymentTerm> paymentTermRepository,
        IRepository<ProjectBilling> projectBillingRepository,
        IRepository<Project> projectRepository,
        IRepository<ProjectTask> projectTaskRepository,
        IRepository<TimeSheet> timeSheetRepository,
        IStoreContext storeContext,
        IWorkflowMessageService workflowMessageService,
        EmailAccountSettings emailAccountSettings)
    {
        _dateTimeHelper = dateTimeHelper;
        _downloadService = downloadService;
        _emailAccountService = emailAccountService;
        _eventPublisher = eventPublisher;
        _languageService = languageService;
        _localizationService = localizationService;
        _messageTemplateService = messageTemplateService;
        _messageTokenProvider = messageTokenProvider;
        _nopFileProvider = nopFileProvider;
        _accountGroupRepository = accountGroupRepository;
        _accountTransactionRepository = accountTransactionRepository;
        _bankAccountRepository = bankAccountRepository;
        _invoiceRepository = invoiceRepository;
        _invoiceItemRepository = invoiceItemRepository;
        _invoicePaymentHistoryRepository = invoicePaymentHistoryRepository;
        _companyRepository = companyRepository;
        _paymentTermRepository = paymentTermRepository;
        _projectBillingRepository = projectBillingRepository;
        _projectRepository = projectRepository;
        _projectTaskRepository = projectTaskRepository;
        _timeSheetRepository = timeSheetRepository;
        _storeContext = storeContext;
        _workflowMessageService = workflowMessageService;
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

        File.WriteAllBytes(fullFilePath, fileBytes);

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

    protected virtual async Task<IList<MessageTemplate>> GetActiveMessageTemplatesAsync(string messageTemplateName, int storeId)
    {
        var messageTemplates = await _messageTemplateService.GetMessageTemplatesByNameAsync(messageTemplateName, storeId);

        if (!messageTemplates?.Any() ?? true)
            return new List<MessageTemplate>();

        messageTemplates = messageTemplates.Where(messageTemplate => messageTemplate.IsActive).ToList();

        return messageTemplates;
    }

    protected virtual async Task<EmailAccount> GetEmailAccountOfMessageTemplateAsync(MessageTemplate messageTemplate, int languageId)
    {
        var emailAccountId = await _localizationService.GetLocalizedAsync(messageTemplate, mt => mt.EmailAccountId, languageId);
        if (emailAccountId == 0)
            emailAccountId = messageTemplate.EmailAccountId;

        var emailAccount = (await _emailAccountService.GetEmailAccountByIdAsync(emailAccountId) ?? await _emailAccountService.GetEmailAccountByIdAsync(_emailAccountSettings.DefaultEmailAccountId)) ??
                           (await _emailAccountService.GetAllEmailAccountsAsync()).FirstOrDefault();
        return emailAccount;
    }

    protected virtual async Task<int> EnsureLanguageIsActiveAsync(int languageId, int storeId)
    {
        var language = await _languageService.GetLanguageByIdAsync(languageId);

        if (language == null || !language.Published)
            language = (await _languageService.GetAllLanguagesAsync(storeId: storeId)).FirstOrDefault();

        if (language == null || !language.Published)
            language = (await _languageService.GetAllLanguagesAsync()).FirstOrDefault();

        if (language == null)
            throw new Exception("No active language could be loaded");

        return language.Id;
    }

    #endregion

    #region Account Groups Methods

    public virtual async Task InsertAccountGroupAsync(AccountGroup accountGroup)
    {
        ArgumentNullException.ThrowIfNull(nameof(accountGroup));

        await _accountGroupRepository.InsertAsync(accountGroup);
    }

    public virtual async Task UpdateAccountGroupAsync(AccountGroup accountGroup)
    {
        ArgumentNullException.ThrowIfNull(nameof(accountGroup));

        await _accountGroupRepository.UpdateAsync(accountGroup);
    }

    public virtual async Task DeleteAccountGroupAsync(AccountGroup accountGroup)
    {
        ArgumentNullException.ThrowIfNull(nameof(accountGroup));

        await _accountGroupRepository.DeleteAsync(accountGroup);
    }

    public virtual async Task<AccountGroup> GetAccountGroupByIdAsync(int id = 0)
    {
        ArgumentNullException.ThrowIfNull(nameof(id));

        return await _accountGroupRepository.GetByIdAsync(id);
    }

    public virtual async Task<IPagedList<AccountGroup>> GetAllAccountGroupsAsync(int accountCategoryId = 0, bool showHidden = false, int pageIndex = 0, int pageSize = int.MaxValue)
    {
        var accountGroups = from ac in _accountGroupRepository.Table
                            where !ac.Deleted
                            select ac;

        if (accountCategoryId == (int)AccountCategoryEnum.Income)
            accountGroups = accountGroups.Where(ac => ac.AccountCategoryId == accountCategoryId);

        if (accountCategoryId == (int)AccountCategoryEnum.Expense)
            accountGroups = accountGroups.Where(ac => ac.AccountCategoryId == accountCategoryId);

        if (showHidden)
            accountGroups = accountGroups.Where(ac => ac.IsActive);

        accountGroups = accountGroups.OrderBy(ac => ac.DisplayOrder);

        return await accountGroups.ToPagedListAsync(pageIndex, pageSize);
    }

    #endregion

    #region Bank Account Methods

    public virtual async Task InsertBankAccountAsync(BankAccount bankAccount)
    {
        ArgumentNullException.ThrowIfNull(nameof(bankAccount));

        await _bankAccountRepository.InsertAsync(bankAccount);
    }

    public virtual async Task UpdateBankAccountAsync(BankAccount bankAccount)
    {
        ArgumentNullException.ThrowIfNull(nameof(bankAccount));

        await _bankAccountRepository.UpdateAsync(bankAccount);
    }

    public virtual async Task DeleteBankAccountAsync(BankAccount bankAccount)
    {
        ArgumentNullException.ThrowIfNull(nameof(bankAccount));

        await _bankAccountRepository.DeleteAsync(bankAccount);
    }

    public virtual async Task<BankAccount> GetBankAccountByIdAsync(int id = 0)
    {
        ArgumentNullException.ThrowIfNull(nameof(id));

        return await _bankAccountRepository.GetByIdAsync(id);
    }

    public virtual async Task<BankAccount> GetDefaultBankAccountAsync()
    {
        return await _bankAccountRepository.Table.Where(ba => ba.IsDefault).FirstOrDefaultAsync();

    }

    public virtual async Task<IPagedList<BankAccount>> GetAllBankAccountsAsync(bool showHidden = false, int pageIndex = 0, int pageSize = int.MaxValue)
    {
        var bankAccounts = from ba in _bankAccountRepository.Table
                           where !ba.Deleted
                           select ba;

        if (showHidden)
            bankAccounts = bankAccounts.Where(ac => ac.IsActive);

        bankAccounts = bankAccounts.OrderBy(ac => ac.DisplayOrder);

        return await bankAccounts.ToPagedListAsync(pageIndex, pageSize);
    }

    #endregion

    #region Payment Terms Methods

    public virtual async Task InsertPaymentTermAsync(PaymentTerm paymentTerm)
    {
        ArgumentNullException.ThrowIfNull(nameof(paymentTerm));

        await _paymentTermRepository.InsertAsync(paymentTerm);
    }

    public virtual async Task UpdatePaymentTermAsync(PaymentTerm paymentTerm)
    {
        ArgumentNullException.ThrowIfNull(nameof(paymentTerm));

        await _paymentTermRepository.UpdateAsync(paymentTerm);
    }

    public virtual async Task DeletePaymentTermAsync(PaymentTerm paymentTerm)
    {
        ArgumentNullException.ThrowIfNull(nameof(paymentTerm));

        await _paymentTermRepository.DeleteAsync(paymentTerm);
    }

    public virtual async Task<PaymentTerm> GetPaymentTermByIdAsync(int id = 0)
    {
        ArgumentNullException.ThrowIfNull(nameof(id));

        return await _paymentTermRepository.GetByIdAsync(id);
    }

    public virtual async Task<IPagedList<PaymentTerm>> GetAllPaymentTermsAsync(bool showHidden = false, int pageIndex = 0, int pageSize = int.MaxValue)
    {
        var paymentTerms = from pt in _paymentTermRepository.Table
                           where !pt.Deleted
                           select pt;

        if (showHidden)
            paymentTerms = paymentTerms.Where(pt => pt.IsActive && pt.NumberOfDays > 0);

        paymentTerms = paymentTerms.OrderBy(pt => pt.DisplayOrder);

        return await paymentTerms.ToPagedListAsync(pageIndex, pageSize);
    }

    #endregion

    #region Project Billing Methods

    public virtual async Task InsertProjectBillingAsync(ProjectBilling projectBilling)
    {
        ArgumentNullException.ThrowIfNull(nameof(projectBilling));

        await _projectBillingRepository.InsertAsync(projectBilling);
    }

    public virtual async Task UpdateProjectBillingAsync(ProjectBilling projectBilling)
    {
        ArgumentNullException.ThrowIfNull(nameof(projectBilling));

        await _projectBillingRepository.UpdateAsync(projectBilling);
    }

    public virtual async Task DeleteProjectBillingAsync(ProjectBilling projectBilling)
    {
        ArgumentNullException.ThrowIfNull(nameof(projectBilling));

        await _projectBillingRepository.DeleteAsync(projectBilling);
    }

    public virtual async Task<ProjectBilling> GetProjectBillingByIdAsync(int id = 0)
    {
        ArgumentNullException.ThrowIfNull(nameof(id));

        return await _projectBillingRepository.GetByIdAsync(id);
    }

    public virtual async Task<ProjectBilling> GetProjectBillingByProjectIdAsync(int projectId = 0)
    {
        ArgumentNullException.ThrowIfNull(nameof(projectId));

        return await _projectBillingRepository.Table.Where(pb => pb.ProjectId == projectId).FirstOrDefaultAsync();
    }

    public virtual async Task<IPagedList<ProjectBilling>> GetAllProjectBillingsAsync(string billingName = null, int projectId = 0, int companyId = 0,
        int billingTypeId = 0, int paymentTermId = 0, bool showHidden = false, int pageIndex = 0, int pageSize = int.MaxValue)
    {
        var projectBillings = from pb in _projectBillingRepository.Table
                              join p in _projectRepository.Table on pb.ProjectId equals p.Id
                              join c in _companyRepository.Table on pb.CompanyId equals c.Id
                              join pt in _paymentTermRepository.Table on pb.PaymentTermId equals pt.Id
                              where !pb.Deleted && !p.IsDeleted && !pt.Deleted
                              select pb;

        if (!string.IsNullOrWhiteSpace(billingName))
            projectBillings = projectBillings.Where(pb => pb.BillingName.Contains(billingName));

        if (projectId > 0)
            projectBillings = projectBillings.Where(pb => pb.ProjectId == projectId);

        if (companyId > 0)
            projectBillings = projectBillings.Where(pb => pb.CompanyId == companyId);

        if (billingTypeId > 0)
            projectBillings = projectBillings.Where(pb => pb.BillingTypeId == billingTypeId);

        if (paymentTermId > 0)
            projectBillings = projectBillings.Where(pb => pb.PaymentTermId == paymentTermId);

        if (showHidden)
            projectBillings = projectBillings.Where(pt => pt.IsActive);

        projectBillings = projectBillings.OrderBy(pb => pb.DisplayOrder);

        return await projectBillings.ToPagedListAsync(pageIndex, pageSize);
    }

    #endregion

    #region Invoice Methods

    public virtual async Task InsertInvoiceAsync(Invoice invoice)
    {
        ArgumentNullException.ThrowIfNull(nameof(invoice));

        await _invoiceRepository.InsertAsync(invoice);
    }

    public virtual async Task UpdateInvoiceAsync(Invoice invoice)
    {
        ArgumentNullException.ThrowIfNull(nameof(invoice));

        await _invoiceRepository.UpdateAsync(invoice);
    }

    public virtual async Task DeleteInvoiceAsync(Invoice invoice)
    {
        ArgumentNullException.ThrowIfNull(nameof(invoice));

        var invoiceItems = await GetAllInvoiceItemsAsync(invoice.Id);
        foreach (var invoiceItem in invoiceItems)
            await DeleteInvoiceItemAsync(invoiceItem);

        await _invoiceRepository.DeleteAsync(invoice);
    }

    public virtual async Task<Invoice> GetInvoiceByIdAsync(int id = 0)
    {
        ArgumentNullException.ThrowIfNull(nameof(id));

        return await _invoiceRepository.GetByIdAsync(id);
    }

    public virtual async Task<IPagedList<Invoice>> GetAllInvoicesAsync(DateTime? createdFromUTC = null, DateTime? createdToUTC = null, int companyId = 0, 
        int statusId = 0, int monthId = 0, int yearId = 0, int invoiceNumber = 0, bool showHidden = false, int pageIndex = 0, int pageSize = int.MaxValue)
    {
        var invoices = from i in _invoiceRepository.Table
                       join pb in _projectBillingRepository.Table on i.ProjectBillingId equals pb.Id
                       join ag in _accountGroupRepository.Table on i.AccountGroupId equals ag.Id
                       where !i.Deleted && !pb.Deleted && !ag.Deleted
                       select i;

        if (companyId > 0)
        {
            invoices = from i in invoices
                       join pb in _projectBillingRepository.Table on i.ProjectBillingId equals pb.Id
                       join c in _companyRepository.Table on pb.CompanyId equals c.Id
                       where c.Id == companyId
                       select i;
        }

        if (createdFromUTC.HasValue)
            invoices = invoices.Where(i => createdFromUTC.Value <= i.CreatedOnUtc);

        if (createdToUTC.HasValue)
            invoices = invoices.Where(i => createdToUTC.Value >= i.CreatedOnUtc);

        if (statusId > 0)
            invoices = invoices.Where(i => i.StatusId == statusId);

        if (monthId > 0)
            invoices = invoices.Where(i => i.MonthId == monthId);

        if (yearId > 0)
            invoices = invoices.Where(i => i.YearId == yearId);

        if (invoiceNumber > 0)
            invoices = invoices.Where(i => i.InvoiceNumber == invoiceNumber);

        if (showHidden)
            invoices = invoices.Where(i => i.IsActive);

        invoices = invoices.OrderBy(i => i.DisplayOrder);

        return await invoices.ToPagedListAsync(pageIndex, pageSize);
    }

    #endregion

    #region Invoice Item Methods

    public virtual async Task InsertInvoiceItemAsync(InvoiceItem invoiceItem)
    {
        ArgumentNullException.ThrowIfNull(nameof(invoiceItem));

        await _invoiceItemRepository.InsertAsync(invoiceItem);
    }

    public virtual async Task UpdateInvoiceItemAsync(InvoiceItem invoiceItem)
    {
        ArgumentNullException.ThrowIfNull(nameof(invoiceItem));

        await _invoiceItemRepository.UpdateAsync(invoiceItem);
    }

    public virtual async Task DeleteInvoiceItemAsync(InvoiceItem invoiceItem)
    {
        ArgumentNullException.ThrowIfNull(nameof(invoiceItem));

        await _invoiceItemRepository.DeleteAsync(invoiceItem);
    }

    public virtual async Task<InvoiceItem> GetInvoiceItemByIdAsync(int id = 0)
    {
        ArgumentNullException.ThrowIfNull(nameof(id));

        return await _invoiceItemRepository.GetByIdAsync(id);
    }

    public virtual async Task<IPagedList<InvoiceItem>> GetAllInvoiceItemsAsync(int invoiceId = 0, int pageIndex = 0,
        int pageSize = int.MaxValue)
    {
        var invoiceItems = from ii in _invoiceItemRepository.Table
                           join i in _invoiceRepository.Table on ii.InvoiceId equals i.Id
                           where !ii.Deleted && ii.InvoiceId == invoiceId
                           select ii;

        invoiceItems = invoiceItems.OrderBy(aci => aci.Id);

        return await invoiceItems.ToPagedListAsync(pageIndex, pageSize);
    }

    #endregion

    #region Invoice Payment Histories Methods

    public virtual async Task InsertInvoicePaymentHistoryAsync(InvoicePaymentHistory invoicePaymentHistory)
    {
        ArgumentNullException.ThrowIfNull(nameof(invoicePaymentHistory));

        await _invoicePaymentHistoryRepository.InsertAsync(invoicePaymentHistory);
    }

    public virtual async Task UpdateInvoicePaymentHistoryAsync(InvoicePaymentHistory invoicePaymentHistory)
    {
        ArgumentNullException.ThrowIfNull(nameof(invoicePaymentHistory));

        await _invoicePaymentHistoryRepository.UpdateAsync(invoicePaymentHistory);
    }

    public virtual async Task DeleteInvoicePaymentHistoryAsync(InvoicePaymentHistory invoicePaymentHistory)
    {
        ArgumentNullException.ThrowIfNull(nameof(invoicePaymentHistory));

        await _invoicePaymentHistoryRepository.DeleteAsync(invoicePaymentHistory);
    }

    public virtual async Task<InvoicePaymentHistory> GetInvoicePaymentHistoryByIdAsync(int id = 0)
    {
        ArgumentNullException.ThrowIfNull(nameof(id));

        return await _invoicePaymentHistoryRepository.GetByIdAsync(id);
    }

    public virtual async Task<IPagedList<InvoicePaymentHistory>> GetAllInvoicePaymentHistoriesAsync(int invoiceId = 0, int pageIndex = 0,
        int pageSize = int.MaxValue)
    {
        var invoicePaymentHistories = from iph in _invoicePaymentHistoryRepository.Table
                                      join i in _invoiceRepository.Table on iph.InvoiceId equals i.Id
                                      where iph.InvoiceId == invoiceId && !iph.Deleted
                                      select iph;

        invoicePaymentHistories = invoicePaymentHistories.OrderBy(aci => aci.Id);

        return await invoicePaymentHistories.ToPagedListAsync(pageIndex, pageSize);
    }

    #endregion

    #region Account Transaction Methods

    public virtual async Task InsertAccountTransactionAsync(AccountTransaction accountTransaction)
    {
        ArgumentNullException.ThrowIfNull(nameof(accountTransaction));

        await _accountTransactionRepository.InsertAsync(accountTransaction);
    }

    public virtual async Task UpdateAccountTransactionAsync(AccountTransaction accountTransaction)
    {
        ArgumentNullException.ThrowIfNull(nameof(accountTransaction));

        await _accountTransactionRepository.UpdateAsync(accountTransaction);
    }

    public virtual async Task DeleteAccountTransactionAsync(AccountTransaction accountTransaction)
    {
        ArgumentNullException.ThrowIfNull(nameof(accountTransaction));

        await _accountTransactionRepository.DeleteAsync(accountTransaction);
    }

    public virtual async Task<AccountTransaction> GetAccountTransactionByIdAsync(int id = 0)
    {
        ArgumentNullException.ThrowIfNull(nameof(id));

        return await _accountTransactionRepository.GetByIdAsync(id);
    }

    public virtual async Task<AccountTransaction> GetAccountTransactionByInvoicePaymentHistoryIdAsync(int invoicePaymentHistoryId = 0)
    {
        ArgumentNullException.ThrowIfNull(nameof(invoicePaymentHistoryId));

        return await _accountTransactionRepository.Table.Where(at => at.InvoicePaymentHistoryId == invoicePaymentHistoryId).FirstOrDefaultAsync();
    }

    public virtual async Task<IPagedList<AccountTransaction>> GetAllAccountTransactionsAsync(int transactionTypeId = 0, int paymentMethodId = 0,
        int pageIndex = 0, int pageSize = int.MaxValue)
    {
        var accountTransactions = from at in _accountTransactionRepository.Table
                                  where !at.Deleted
                                  select at;

        if (transactionTypeId > 0)
            accountTransactions = accountTransactions.Where(at => at.TransactionTypeId == transactionTypeId);

        if (paymentMethodId > 0)
            accountTransactions = accountTransactions.Where(at => at.PaymentMethodId == paymentMethodId);

        accountTransactions = accountTransactions.OrderByDescending(ac => ac.CreatedOnUtc);

        return await accountTransactions.ToPagedListAsync(pageIndex, pageSize);
    }

    #endregion

    #region Send Invoice Mail Methods

    public virtual async Task<IList<int>> SendInvoiceEmailAsync(IList<string> contactEmails, string companyName, int invoiceFileId, int timesheetFileId,
        int languageId)
    {
        var store = await _storeContext.GetCurrentStoreAsync();
        languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);

        var messageTemplates = await GetActiveMessageTemplatesAsync(AccountManagementDefaults.SendInvoiceNotification, store.Id);
        if (!messageTemplates.Any())
            return new List<int>();

        var attachedEmails = string.Join(";", contactEmails);

        var invoiceDownload = invoiceFileId > 0 ? await _downloadService.GetDownloadByIdAsync(invoiceFileId) : null;
        var timesheetDownload = timesheetFileId > 0 ? await _downloadService.GetDownloadByIdAsync(timesheetFileId) : null;

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

        var commonTokens = new List<Token>();
        var results = new List<int>();

        return await messageTemplates.SelectAwait(async messageTemplate =>
        {
            var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId);

            var tokens = new List<Token>(commonTokens);
            await _messageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount);
            await _eventPublisher.MessageTokensAddedAsync(messageTemplate, tokens);

            var toEmail = attachedEmails;
            var toName = companyName;

            return await _workflowMessageService.SendNotificationAsync(messageTemplate, emailAccount, languageId, tokens, toEmail, toName,
                attachmentFilePath, attachmentFileName, fromEmail: emailAccount.Email, fromName: emailAccount.DisplayName);
        }).ToListAsync();
    }

    #endregion

    #region Time Summary Report Methods

    public virtual async Task<IList<TimeSheetReport>> GetReportByEmployeeListWithProjectsAsync(DateTime? from, DateTime? to, int employeeId = 0,
        int projectId = 0, int periodId = 0, int hoursId = 0)
    {
        var query = await _timeSheetRepository.GetAllAsync(async query =>
        {
            query = query.Where(c => c.EmployeeId == employeeId && c.SpentDate >= from && c.SpentDate <= to).Where(c => !_projectTaskRepository.Table
                .Any(t => t.Id == c.TaskId && t.IsDeleted)); ;

            if (projectId > 0)
                query = query.Where(c => c.ProjectId == projectId);

            if (hoursId == 2)
                query = query.Where(c => c.Billable == true);

            if (hoursId == 3)
                query = query.Where(c => c.Billable == false);

            return query.OrderBy(c => c.SpentDate);
        });

        query = query.ToList();

        var timeSheetReports = query.GroupBy(c => c.SpentDate).Select(g => new TimeSheetReport
        {
            EmployeeId = g.First().EmployeeId,
            SpentDate = g.Key,
            TaskId = g.First().TaskId,
            CreateOnUtc = g.First().CreateOnUtc,
            UpdateOnUtc = g.First().UpdateOnUtc,
            TotalMinutes = g.Sum(c => (c.SpentHours * 60) + c.SpentMinutes),
            SpentHours = g.Sum(c => (c.SpentHours * 60) + c.SpentMinutes) / 60,
            SpentMinutes = g.Sum(c => (c.SpentHours * 60) + c.SpentMinutes) % 60,
            BillableMinutes = g.Where(c => c.Billable)
                    .Sum(c => (c.SpentHours * 60) + c.SpentMinutes),
        }).ToList();

        for (var date = from.Value; date <= to.Value; date = date.AddDays(1))
        {
            if (!timeSheetReports.Any(r => r.SpentDate == date))
            {
                timeSheetReports.Add(new TimeSheetReport
                {
                    EmployeeId = employeeId,
                    SpentDate = date,
                    SpentHours = 0,
                    SpentMinutes = 0,
                    TotalMinutes = 0,
                    BillableMinutes = 0,
                    CreateOnUtc = await _dateTimeHelper.GetUTCAsync(),
                    UpdateOnUtc = await _dateTimeHelper.GetUTCAsync()
                });
            }
        }

        timeSheetReports = timeSheetReports.OrderBy(c => c.SpentDate).ToList();

        if (timeSheetReports.Any())
        {
            int totalMinutes = timeSheetReports.Sum(x => x.TotalMinutes);
            timeSheetReports.First().TotalSpentHours = totalMinutes / 60;
            timeSheetReports.First().TotalSpentMinutes = totalMinutes % 60;
        }

        return timeSheetReports;
    }

    #endregion
}
