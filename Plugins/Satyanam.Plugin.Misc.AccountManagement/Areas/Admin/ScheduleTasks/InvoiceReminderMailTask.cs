using System;
using System.Linq;
using System.Threading.Tasks;
using App.Core;
using App.Services.Configuration;
using App.Services.ScheduleTasks;
using DocumentFormat.OpenXml.EMMA;
using Satyanam.Nop.Core.Services;
using Satyanam.Plugin.Misc.AccountManagement.Domain;
using Satyanam.Plugin.Misc.AccountManagement.Services;

namespace Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.ScheduleTasks;

public partial class InvoiceReminderMailTask : IScheduleTask
{
	#region Fields

	protected readonly IAccountManagementService _accountManagementService;
    protected readonly ICompanyService _companyService;
    protected readonly IContactsService _contactsService;
    protected readonly IScheduleTaskService _scheduleTaskService;
	protected readonly ISettingService _settingService;
    protected readonly IWorkContext _workContext;

	#endregion

	#region Ctor

	public InvoiceReminderMailTask(IAccountManagementService accountManagementService,
        ICompanyService companyService,
        IContactsService contactsService,
        IScheduleTaskService scheduleTaskService,
        ISettingService settingService,
        IWorkContext workContext)
	{
		_accountManagementService = accountManagementService;
        _companyService = companyService;
        _contactsService = contactsService;
        _scheduleTaskService = scheduleTaskService;
		_settingService = settingService;
        _workContext = workContext;
	}

    #endregion

    #region Methods

    public async Task ExecuteAsync()
	{
        var settings = await _settingService.LoadSettingAsync<AccountManagementSettings>();
        if (!settings.EnablePlugin)
            return;

        var scheduleTask = await _scheduleTaskService.GetTaskByTypeAsync(AccountManagementDefaults.InvoiceReminderScheduleTaskType);
        if (!scheduleTask.Enabled)
            return;

        var dueInvoices = await _accountManagementService.GetAllDueInvoicesAsync(reminderMail: settings.ReminderMail);
        if (dueInvoices == null || !dueInvoices.Any())
            return;

        var workingLanguageId = (await _workContext.GetWorkingLanguageAsync()).Id;

        foreach (var dueInvoice in dueInvoices)
        {
            var existingProjectBilling = await _accountManagementService.GetProjectBillingByIdAsync(dueInvoice.ProjectBillingId);
            if (existingProjectBilling == null)
                continue;

            var existingCompany = await _companyService.GetCompanyByIdAsync(existingProjectBilling.CompanyId);
            if (existingCompany == null)
                continue;

            var existingContacts = await _contactsService.GetContactByCompanyIdAsync(existingProjectBilling.CompanyId, pageIndex: 0, pageSize: int.MaxValue);
            var existingContact = existingContacts?.FirstOrDefault();
            if (existingContact == null)
                continue;

            var contactName = $"{existingContact.FirstName} {existingContact.LastName}".Trim();
            var contactEmail = existingContact.Email;

            await _accountManagementService.SendInvoiceReminderEmailAsync(customerName: contactName, customerEmail: contactEmail, invoiceNumber: dueInvoice.InvoiceNumber, dueDate: dueInvoice.DueDate, languageId: workingLanguageId);
            dueInvoice.ReminderDate = DateTime.UtcNow;
            await _accountManagementService.UpdateInvoiceAsync(dueInvoice);
        }
    }

    #endregion
}
