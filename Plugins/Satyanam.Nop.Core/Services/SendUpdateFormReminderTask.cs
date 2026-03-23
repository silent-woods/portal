using App.Core;
using App.Core.Domain.Employees;
using App.Core.Domain.Messages;
using App.Core.Domain.Stores;
using App.Data;
using App.Data.Extensions;
using App.Services.Customers;
using App.Services.Employees;
using App.Services.Messages;
using App.Services.ScheduleTasks;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Satyanam.Nop.Core.Domains;
using StackExchange.Profiling.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Satyanam.Nop.Core.Services
{

    public partial class SendUpdateFormReminderTask : IScheduleTask
    {
        #region Fields
        private readonly IRepository<UpdateTemplate> _templateRepo;
        private readonly IRepository<Employee> _employeeRepo;
        private readonly IRepository<UpdateTemplatePeriod> _periodRepo;
        private readonly ICustomerService _customerService;
        private readonly IStoreContext _storeContext;
        private readonly IQueuedEmailService _queuedEmailService;
        private readonly IEmailAccountService _emailAccountService;
        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly IRepository<UpdateSubmission> _submissionRepo;
        private readonly INotificationService _notificationService;
        private readonly IEmployeeService _employeeService;
        private readonly IRepository<QueuedEmail> _queuedEmailRepo;
        #endregion

        #region Ctor

        public SendUpdateFormReminderTask(IRepository<UpdateTemplate> templateRepo,
            IRepository<Employee> employeeRepo,
            IRepository<UpdateTemplatePeriod> periodRepo,
            ICustomerService customerService,
            IStoreContext storeContext,
            IQueuedEmailService queuedEmailService,
            IEmailAccountService emailAccountService,
            IUrlHelperFactory urlHelperFactory,
            IActionContextAccessor actionContextAccessor,
            IRepository<UpdateSubmission> submissionRepo,
            INotificationService notificationService,
            IEmployeeService employeeService,
            IRepository<QueuedEmail> queuedEmailRepo)
        {
            _templateRepo = templateRepo;
            _employeeRepo = employeeRepo;
            _periodRepo = periodRepo;
            _customerService = customerService;
            _storeContext = storeContext;
            _queuedEmailService = queuedEmailService;
            _emailAccountService = emailAccountService;
            _urlHelperFactory = urlHelperFactory;
            _actionContextAccessor = actionContextAccessor;
            _submissionRepo = submissionRepo;
            _notificationService = notificationService;
            _employeeService = employeeService;
            _queuedEmailRepo = queuedEmailRepo;
        }

        #endregion

        #region Methods

        #region Update Form Reminder

        public async Task ExecuteAsync()
        {
            var nowUtc = DateTime.UtcNow;
            var store = await _storeContext.GetCurrentStoreAsync();

            // Get valid email account (fallback if not set)
            var emailAccount = (await _emailAccountService.GetAllEmailAccountsAsync()).FirstOrDefault();

            if (emailAccount == null)
            {
                _notificationService.ErrorNotification("EmailAccount is null. Reminder emails cannot be sent.");
                return;
            }

            var templates = await _templateRepo.Table
                .Where(t => t.IsActive && !string.IsNullOrWhiteSpace(t.SubmitterUserIds))
                .ToListAsync();

            foreach (var template in templates)
            {
                //Always calculate scheduledUtc first
                var indiaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
                if (!TimeSpan.TryParse(template.DueTime, out var dueTime))
                    dueTime = new TimeSpan(10, 0, 0);

                var indiaNow = TimeZoneInfo.ConvertTimeFromUtc(nowUtc, indiaTimeZone);

                DateTime lastScheduledIndia;

                // WEEKLY
                if (template.FrequencyId == 3 && !string.IsNullOrEmpty(template.SelectedWeekDays))
                {
                    var days = template.SelectedWeekDays
                        .Split(',')
                        .Select(int.Parse)
                        .ToList();

                    lastScheduledIndia = indiaNow.Date;

                    while (!days.Contains((int)lastScheduledIndia.DayOfWeek))
                        lastScheduledIndia = lastScheduledIndia.AddDays(-1);
                }

                // MONTHLY
                else if (template.FrequencyId == 4 && template.OnDay.HasValue)
                {
                    var day = template.OnDay.Value;

                    lastScheduledIndia = new DateTime(indiaNow.Year, indiaNow.Month, day);

                    if (lastScheduledIndia > indiaNow)
                        lastScheduledIndia = lastScheduledIndia.AddMonths(-1);
                }

                // DAILY or fallback
                else if (template.FrequencyId == 2)
                {
                    // Get last processed date
                    var lastSentIndia = template.LastReminderSentUtc.HasValue
                        ? TimeZoneInfo.ConvertTimeFromUtc(template.LastReminderSentUtc.Value, indiaTimeZone).Date
                        : indiaNow.Date.AddDays(-1);

                    // Loop from last processed date till yesterday
                    for (var date = lastSentIndia; date < indiaNow.Date; date = date.AddDays(1))
                    {
                        var lastScheduledIndiaLoop = date;

                        var scheduledIstLoop = DateTime.SpecifyKind(lastScheduledIndiaLoop.Date + dueTime, DateTimeKind.Unspecified);
                        var scheduledUtcLoop = TimeZoneInfo.ConvertTimeToUtc(scheduledIstLoop, indiaTimeZone);

                        var overdueDaysLoop = (indiaNow.Date - lastScheduledIndiaLoop.Date).Days;

                        await ProcessTemplateForDate(template, lastScheduledIndiaLoop, scheduledIstLoop, scheduledUtcLoop, overdueDaysLoop, indiaNow, nowUtc, store, emailAccount);
                    }

                    // ALSO process TODAY separately (for reminder)
                    lastScheduledIndia = indiaNow.Date;
                }
                else
                {
                    lastScheduledIndia = indiaNow.Date;
                }

                var scheduledIst = DateTime.SpecifyKind(lastScheduledIndia.Date + dueTime, DateTimeKind.Unspecified);

                var scheduledUtc = TimeZoneInfo.ConvertTimeToUtc(scheduledIst, indiaTimeZone);

                var overdueDays = (indiaNow.Date - lastScheduledIndia.Date).Days;
                // Decide overdue/due/future 
                bool shouldSend = false;

                if (indiaNow.Date > lastScheduledIndia.Date) // overdue
                {
                    if (template.LastReminderSentUtc == null)
                    {
                        shouldSend = true;
                    }
                    else
                    {
                        var lastSentIndia = TimeZoneInfo.ConvertTimeFromUtc(template.LastReminderSentUtc.Value, indiaTimeZone);

                        var daysSinceLast = (indiaNow.Date - lastSentIndia.Date).Days;

                        if (daysSinceLast >= template.RepeatEvery || template.LastReminderSentUtc == null)
                            shouldSend = true;
                    }
                }
                else if (indiaNow.Date == lastScheduledIndia.Date) // due today
                {
                    // Send main mail exactly when scheduled time hits
                    if (indiaNow >= scheduledIst &&
                        (template.LastReminderSentUtc == null ||
         TimeZoneInfo.ConvertTimeFromUtc(template.LastReminderSentUtc.Value, indiaTimeZone) < scheduledIst))
                    {
                        shouldSend = true;
                    }
                    else
                    {
                        if (template.ReminderBeforeMinutes > 0)
                        {
                            shouldSend = IsReminderDueToday(template, nowUtc);
                        }
                        else
                        {
                            shouldSend = false;
                        }
                    }
                }
                else // future
                {
                    shouldSend = IsReminderDueToday(template, nowUtc);
                }

                var viewerEmails = new List<string>();

                if (!string.IsNullOrWhiteSpace(template.ViewerUserIds))
                {
                    var viewerIds = template.ViewerUserIds
                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(int.Parse)
                        .ToList();

                    foreach (var viewerId in viewerIds)
                    {
                        var viewer = await _employeeService.GetEmployeeByIdAsync(viewerId);
                        if (viewer != null && !string.IsNullOrWhiteSpace(viewer.OfficialEmail))
                            viewerEmails.Add(viewer.OfficialEmail);
                    }
                }

                var submitterIds = template.SubmitterUserIds.Split(',').Select(int.Parse).ToList();
                var anyEmailQueued = false;
                // Check existing submission
                var currentPeriod = await _periodRepo.Table
.Where(p =>
    p.UpdateTemplateId == template.Id &&
    p.PeriodStart <= indiaNow &&
    p.PeriodEnd >= indiaNow)
.FirstOrDefaultAsync();

                foreach (var employeeId in submitterIds)
                {
                    var employee = await _employeeService.GetEmployeeByIdAsync(employeeId);
                    if (employee == null || string.IsNullOrWhiteSpace(employee.OfficialEmail))
                        continue;
                    var customerId = employee.Customer_Id;
                    if (currentPeriod == null)
                        continue;
                    // Check submission using PeriodId
                    var existingSubmission = await _submissionRepo.Table
                        .Where(s =>
                            s.UpdateTemplateId == template.Id &&
                            s.SubmittedByCustomerId == customerId &&
                            s.PeriodId == currentPeriod.Id)
                        .FirstOrDefaultAsync();

                    bool alreadySubmitted = existingSubmission != null;

                    if (alreadySubmitted)
                        continue;

                    //Subject & body messages
                    string subject;
                    string bodyMessage;
                    bool addViewerInCc = false;

                    if (indiaNow >= scheduledIst && indiaNow.Date == scheduledIst.Date)
                    {
                        // MAIN MAIL (exact time reached)

                        subject = $"Action Required ({lastScheduledIndia:MMM dd}): Please submit '{template.Title}' form";

                        bodyMessage = $@"
    <p>The form <strong>{template.Title}</strong> is now available for submission.</p>

    <p>
        Please complete and submit the form as soon as possible.
    </p>

    <p>
        <strong>Submission time:</strong> {scheduledUtc.ToLocalTime():hh:mm tt}
    </p>";
                    }
                    else if (indiaNow.Date == scheduledIst.Date && template.ReminderBeforeMinutes > 0)
                    {
                        // REMINDER BEFORE DUE TIME

                        subject = $"Reminder: '{template.Title}' form is due today";

                        bodyMessage = $@"
    <p>This is a reminder that your form <strong>{template.Title}</strong> 
    will be due today.</p>

    <p>Please ensure it is completed before the deadline.</p>";
                    }
                    else if (indiaNow.Date > scheduledIst.Date)
                    {
                        if (overdueDays >= 3)
                        {
                            addViewerInCc = true;

                            subject = $"⚠️ Escalation ({lastScheduledIndia:MMM dd}): '{template.Title}' overdue for {overdueDays} days";

                            bodyMessage = $@"
<p>
The form <strong>{template.Title}</strong> for 
<strong>{lastScheduledIndia:MMM dd, yyyy}</strong> has been 
<strong style='color:red;'>overdue for {overdueDays} days</strong>.
</p>

<p>The assigned submitter has not completed the form.</p>

<p><strong>Viewers have been copied for awareness and follow-up.</strong></p>";
                        }
                        else
                        {
                            subject = $"Overdue ({lastScheduledIndia:MMM dd}): '{template.Title}' form still pending";

                            bodyMessage = $@"
<p>
You did not submit the <strong>{template.Title}</strong> form for 
<strong>{lastScheduledIndia:MMM dd, yyyy}</strong>.
</p>

<p>Please submit it as soon as possible.</p>";
                        }
                    }
                    else
                    {
                        if (template.ReminderBeforeMinutes <= 0)
                            continue;
                        subject = $"Reminder: Please fill the '{template.Title}' form";

                        bodyMessage = $@"
    <p>This is a reminder to complete the 
    <strong>{template.Title}</strong> form before the deadline.</p>";
                    }
                    var emailAlreadySent = await _queuedEmailRepo.Table.AnyAsync(e =>
    e.To == employee.OfficialEmail &&
    e.Subject == subject &&
    e.CreatedOnUtc.Date == nowUtc.Date);

                    if (emailAlreadySent)
                        continue;
                    // Build form link
                    var formLink = $"{store.Url.TrimEnd('/')}/UpdateSubmission/Submit/{template.Id}";
                    var periodId = existingSubmission?.PeriodId ?? 0;
                    if (periodId > 0)
                        formLink += $"?periodId={periodId}";

                    var body = $@"
                        <p>Dear {employee.FirstName} {employee.LastName},</p>

                        {bodyMessage}

                        <p style='margin-top:20px;'>
                        <table cellspacing='0' cellpadding='0' border='0'>
                          <tr>
                            <td align='center' bgcolor='#007bff' style='border-radius:6px;'>
                              <a href='{formLink}'
                                 target='_blank'
                                 style='
                                    display:inline-block;
                                    padding:12px 24px;
                                    font-size:14px;
                                    font-family:Arial, sans-serif;
                                    color:#ffffff;
                                    text-decoration:none;
                                    font-weight:bold;
                                    border-radius:6px;'>
                                Fill The Form
                              </a>
                            </td>
                          </tr>
                        </table>
                        </p>";

                    var email = new QueuedEmail
                    {
                        From = emailAccount.Email,
                        FromName = emailAccount.DisplayName,
                        To = employee.OfficialEmail,
                        CC = addViewerInCc && viewerEmails.Any() ? string.Join(";", viewerEmails) : null,
                        Subject = subject,
                        Body = body,
                        Priority = QueuedEmailPriority.High,
                        CreatedOnUtc = nowUtc,
                        EmailAccountId = emailAccount.Id
                    };

                    await _queuedEmailService.InsertQueuedEmailAsync(email);
                    anyEmailQueued = true;
                }

                if (anyEmailQueued)
                {
                    template.LastReminderSentUtc = nowUtc;
                    await _templateRepo.UpdateAsync(template);
                }
            }
        }
        private async Task ProcessTemplateForDate(
    UpdateTemplate template,
    DateTime lastScheduledIndia,
    DateTime scheduledIst,
    DateTime scheduledUtc,
    int overdueDays,
    DateTime indiaNow,
    DateTime nowUtc,
    Store store,
    EmailAccount emailAccount)
        {
            var viewerEmails = new List<string>();

            if (!string.IsNullOrWhiteSpace(template.ViewerUserIds))
            {
                var viewerIds = template.ViewerUserIds
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(int.Parse)
                    .ToList();

                foreach (var viewerId in viewerIds)
                {
                    var viewer = await _employeeService.GetEmployeeByIdAsync(viewerId);
                    if (viewer != null && !string.IsNullOrWhiteSpace(viewer.OfficialEmail))
                        viewerEmails.Add(viewer.OfficialEmail);
                }
            }

            var submitterIds = template.SubmitterUserIds.Split(',').Select(int.Parse).ToList();
            var anyEmailQueued = false;

            // ✅ FIX: get correct period based on scheduled date (NOT indiaNow)
            var currentPeriod = await _periodRepo.Table
                .Where(p =>
                    p.UpdateTemplateId == template.Id &&
                    p.PeriodStart <= lastScheduledIndia &&
                    p.PeriodEnd >= lastScheduledIndia)
                .FirstOrDefaultAsync();

            if (currentPeriod == null)
                return;

            foreach (var employeeId in submitterIds)
            {
                var employee = await _employeeService.GetEmployeeByIdAsync(employeeId);
                if (employee == null || string.IsNullOrWhiteSpace(employee.OfficialEmail))
                    continue;

                var customerId = employee.Customer_Id;

                // ✅ FIX: check submission for THAT specific date period
                var existingSubmission = await _submissionRepo.Table
                    .Where(s =>
                        s.UpdateTemplateId == template.Id &&
                        s.SubmittedByCustomerId == customerId &&
                        s.PeriodId == currentPeriod.Id)
                    .FirstOrDefaultAsync();

                bool alreadySubmitted = existingSubmission != null;

                if (alreadySubmitted)
                    continue;

                string subject;
                string bodyMessage;
                bool addViewerInCc = false;

                // ✅ MAIN MAIL
                if (indiaNow >= scheduledIst && indiaNow.Date == scheduledIst.Date)
                {
                    subject = $"Action Required ({lastScheduledIndia:MMM dd}): Please submit '{template.Title}' form";

                    bodyMessage = $@"
<p>The form <strong>{template.Title}</strong> is now available for submission.</p>
<p>Please complete and submit the form as soon as possible.</p>
<p><strong>Submission time:</strong> {scheduledUtc.ToLocalTime():hh:mm tt}</p>";
                }
                // ✅ REMINDER
                else if (indiaNow.Date == scheduledIst.Date && template.ReminderBeforeMinutes > 0)
                {
                    subject = $"Reminder: '{template.Title}' form is due today";

                    bodyMessage = $@"
<p>This is a reminder that your form <strong>{template.Title}</strong> 
will be due today.</p>
<p>Please ensure it is completed before the deadline.</p>";
                }
                // ✅ OVERDUE
                else if (indiaNow.Date > scheduledIst.Date)
                {
                    if (overdueDays >= 3)
                    {
                        addViewerInCc = true;

                        subject = $"⚠️ Escalation ({lastScheduledIndia:MMM dd}): '{template.Title}' overdue for {overdueDays} days";

                        bodyMessage = $@"
<p>
The form <strong>{template.Title}</strong> for 
<strong>{lastScheduledIndia:MMM dd, yyyy}</strong> has been 
<strong style='color:red;'>overdue for {overdueDays} days</strong>.
</p>
<p>The assigned submitter has not completed the form.</p>
<p><strong>Viewers have been copied for awareness and follow-up.</strong></p>";
                    }
                    else
                    {
                        subject = $"Overdue ({lastScheduledIndia:MMM dd}): '{template.Title}' form still pending";

                        bodyMessage = $@"
<p>
You did not submit the <strong>{template.Title}</strong> form for 
<strong>{lastScheduledIndia:MMM dd, yyyy}</strong>.
</p>
<p>Please submit it as soon as possible.</p>";
                    }
                }
                else
                {
                    if (template.ReminderBeforeMinutes <= 0)
                        continue;

                    subject = $"Reminder: Please fill the '{template.Title}' form";

                    bodyMessage = $@"
<p>This is a reminder to complete the 
<strong>{template.Title}</strong> form before the deadline.</p>";
                }

                // ✅ FIX: allow multiple overdue emails (date-based)
                var emailAlreadySent = await _queuedEmailRepo.Table.AnyAsync(e =>
                    e.To == employee.OfficialEmail &&
                    e.Subject.Contains(template.Title) &&
                    e.CreatedOnUtc.Date == nowUtc.Date &&
                    e.Body.Contains(lastScheduledIndia.ToString("MMM dd"))
                );

                if (emailAlreadySent)
                    continue;

                var formLink = $"{store.Url.TrimEnd('/')}/UpdateSubmission/Submit/{template.Id}";
                var periodId = currentPeriod.Id;

                if (periodId > 0)
                    formLink += $"?periodId={periodId}";

                var body = $@"
<p>Dear {employee.FirstName} {employee.LastName},</p>

{bodyMessage}

<p style='margin-top:20px;'>
<table cellspacing='0' cellpadding='0' border='0'>
<tr>
<td align='center' bgcolor='#007bff' style='border-radius:6px;'>
<a href='{formLink}'
target='_blank'
style='
display:inline-block;
padding:12px 24px;
font-size:14px;
font-family:Arial, sans-serif;
color:#ffffff;
text-decoration:none;
font-weight:bold;
border-radius:6px;'>
Fill The Form
</a>
</td>
</tr>
</table>
</p>";

                var email = new QueuedEmail
                {
                    From = emailAccount.Email,
                    FromName = emailAccount.DisplayName,
                    To = employee.OfficialEmail,
                    CC = addViewerInCc && viewerEmails.Any() ? string.Join(";", viewerEmails) : null,
                    Subject = subject,
                    Body = body,
                    Priority = QueuedEmailPriority.High,
                    CreatedOnUtc = nowUtc,
                    EmailAccountId = emailAccount.Id
                };

                await _queuedEmailService.InsertQueuedEmailAsync(email);
                anyEmailQueued = true;
            }

            // ✅ update last reminder
            if (anyEmailQueued)
            {
                template.LastReminderSentUtc = nowUtc;
                await _templateRepo.UpdateAsync(template);
            }
        }
        private bool IsReminderDueToday(UpdateTemplate template, DateTime nowUtc)
        {
            // Set up IST time zone
            var indiaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");

            // Convert nowUtc to IST for calculating today's date in IST
            var nowIndia = TimeZoneInfo.ConvertTimeFromUtc(nowUtc, indiaTimeZone);
            var today = nowIndia.Date;

            // Parse the due time (like "10:00")
            if (!TimeSpan.TryParse(template.DueTime, out var dueTime))
                dueTime = new TimeSpan(10, 0, 0); // Default to 10:00 AM IST

            // Calculate full due time in IST
            var scheduledIst = today + dueTime;

            // Convert scheduled IST time to UTC
            var scheduledUtc = TimeZoneInfo.ConvertTimeToUtc(scheduledIst, indiaTimeZone);

            // Calculate reminder offset
            var reminderMinutes = template.ReminderBeforeMinutes;

            if (reminderMinutes <= 0)
                return false;

            var reminderTimeUtc = scheduledUtc.AddMinutes(-reminderMinutes);

            //  Skip if current time is earlier than reminder time
            if (nowUtc < reminderTimeUtc)
                return false;

            var repeatEvery = template.RepeatEvery > 0 ? template.RepeatEvery : 1;

            switch (template.FrequencyId)
            {
                case 1: // One time
                    return template.DueDate?.Date == today;

                case 2: // Daily

                    if (template.RepeatType?.ToLower() == "week")
                    {
                        //  Weekly-style logic inside Daily
                        if (string.IsNullOrWhiteSpace(template.SelectedWeekDays))
                            return false;

                        var currentDayName = nowUtc.DayOfWeek.ToString();
                        var selectedWeekdays = template.SelectedWeekDays
                            .Split(',')
                            .Select(d => d.Trim())
                            .ToList();

                        if (!selectedWeekdays.Contains(currentDayName, StringComparer.OrdinalIgnoreCase))
                            return false;

                        if (template.LastReminderSentUtc == null)
                            return true;

                        var weeksSinceLast = (nowUtc.Date - template.LastReminderSentUtc.Value.Date).Days / 7;
                        return weeksSinceLast >= repeatEvery;
                    }
                    else if (template.RepeatType?.ToLower() == "month")
                    {
                        if (template.OnDay == null || template.OnDay < 1 || template.OnDay > 31)
                            return false;

                        if (today.Day != template.OnDay)
                            return false;

                        if (template.LastReminderSentUtc == null)
                            return true;

                        var monthsSinceLast = ((nowUtc.Year - template.LastReminderSentUtc.Value.Year) * 12) +
                                              nowUtc.Month - template.LastReminderSentUtc.Value.Month;

                        return monthsSinceLast >= repeatEvery;
                    }
                    else
                    {
                        // Normal daily logic
                        if (template.LastReminderSentUtc == null)
                            return true;

                        var daysSinceLast = (nowUtc.Date - template.LastReminderSentUtc.Value.Date).Days;
                        return daysSinceLast >= repeatEvery;
                    }

                case 3: // Weekly

                    if (template.RepeatType?.ToLower() == "day")
                    {
                        // Repeat every N days
                        if (template.LastReminderSentUtc == null)
                            return true;

                        var daysSinceLast = (nowUtc.Date - template.LastReminderSentUtc.Value.Date).Days;
                        return daysSinceLast >= repeatEvery;
                    }
                    else if (template.RepeatType?.ToLower() == "week")
                    {
                        if (string.IsNullOrWhiteSpace(template.SelectedWeekDays))
                            return false;

                        var currentDayName = nowUtc.DayOfWeek.ToString();

                        var dayNumToName = new Dictionary<string, string>
                        {
                            { "0", "Sunday" },
                            { "1", "Monday" },
                            { "2", "Tuesday" },
                            { "3", "Wednesday" },
                            { "4", "Thursday" },
                            { "5", "Friday" },
                            { "6", "Saturday" }
                        };

                        var selectedWeekdays = template.SelectedWeekDays
                            .Split(',')
                            .Select(d => d.Trim())
                            .Select(d => dayNumToName.ContainsKey(d) ? dayNumToName[d] : d)
                            .ToList();

                        if (!selectedWeekdays.Contains(currentDayName, StringComparer.OrdinalIgnoreCase))
                            return false;

                        if (template.LastReminderSentUtc == null)
                            return true;

                        var weeksSinceLast = (nowUtc.Date - template.LastReminderSentUtc.Value.Date).Days / 7;
                        return weeksSinceLast >= repeatEvery;
                    }
                    else if (template.RepeatType?.ToLower() == "month")
                    {
                        if (template.OnDay == null || template.OnDay < 1 || template.OnDay > 31)
                            return false;

                        if (today.Day != template.OnDay)
                            return false;

                        if (template.LastReminderSentUtc == null)
                            return true;

                        var monthsSinceLast = ((nowUtc.Year - template.LastReminderSentUtc.Value.Year) * 12) +
                                              nowUtc.Month - template.LastReminderSentUtc.Value.Month;

                        return monthsSinceLast >= repeatEvery;
                    }
                    else
                    {
                        // Default fallback to weekly every N weeks
                        if (template.LastReminderSentUtc == null)
                            return true;

                        var weeksSinceLast = (nowUtc.Date - template.LastReminderSentUtc.Value.Date).Days / 7;
                        return weeksSinceLast >= repeatEvery;
                    }


                case 4: // Monthly
                    {

                        if (template.RepeatType?.ToLower() == "month")
                        {
                            // Repeat every X months on a specific day (e.g., day 15)
                            if (template.OnDay == null || template.OnDay < 1 || template.OnDay > 31)
                                return false;

                            if (today.Day != template.OnDay)
                                return false;

                            if (template.LastReminderSentUtc == null)
                                return true;

                            var monthsSinceLast = ((nowUtc.Year - template.LastReminderSentUtc.Value.Year) * 12) +
                                                  nowUtc.Month - template.LastReminderSentUtc.Value.Month;

                            return monthsSinceLast >= repeatEvery;
                        }
                        else if (template.RepeatType?.ToLower() == "week")
                        {
                            // Repeat every X weeks on specific weekdays
                            if (string.IsNullOrWhiteSpace(template.SelectedWeekDays))
                                return false;

                            var currentDayName = nowUtc.DayOfWeek.ToString();
                            var dayNumToName = new Dictionary<string, string>
                            {
                                { "0", "Sunday" },
                                { "1", "Monday" },
                                { "2", "Tuesday" },
                                { "3", "Wednesday" },
                                { "4", "Thursday" },
                                { "5", "Friday" },
                                { "6", "Saturday" }
                            };
                            var selectedWeekdays = template.SelectedWeekDays
                            .Split(',')
                            .Select(d => d.Trim())
                            .Select(d => dayNumToName.ContainsKey(d) ? dayNumToName[d] : d)
                            .ToList();

                            if (!selectedWeekdays.Contains(currentDayName, StringComparer.OrdinalIgnoreCase))
                                return false;

                            if (template.LastReminderSentUtc == null)
                                return true;

                            var weeksSinceLast = (nowUtc.Date - template.LastReminderSentUtc.Value.Date).Days / 7;
                            return weeksSinceLast >= repeatEvery;
                        }
                        else if (template.RepeatType?.ToLower() == "day")
                        {
                            // Repeat every X days
                            if (template.LastReminderSentUtc == null)
                                return true;

                            var daysSinceLast = (nowUtc.Date - template.LastReminderSentUtc.Value.Date).Days;
                            return daysSinceLast >= repeatEvery;
                        }

                        return false;
                    }


                default:
                    return false;
            }
        }

        #endregion

        #endregion
    }
}
