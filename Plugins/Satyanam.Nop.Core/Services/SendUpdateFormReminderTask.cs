using App.Core;
using App.Core.Domain.Employees;
using App.Core.Domain.Messages;
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
            IEmployeeService employeeService)
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
                // --- Always calculate scheduledUtc first ---
                var indiaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
                if (!TimeSpan.TryParse(template.DueTime, out var dueTime))
                    dueTime = new TimeSpan(10, 0, 0);

                var dueDate = template.DueDate ?? nowUtc.AddDays(-1);
                var scheduledIst = DateTime.SpecifyKind(dueDate.Date + dueTime, DateTimeKind.Unspecified);
                var scheduledUtc = TimeZoneInfo.ConvertTimeToUtc(scheduledIst, indiaTimeZone);

                var overdueDays = 0;
                if (nowUtc.Date > scheduledUtc.Date)
                    overdueDays = (nowUtc.Date - scheduledUtc.Date).Days;
                // --- Decide overdue/due/future ---
                bool shouldSend = false;

                if (nowUtc.Date > scheduledUtc.Date) // overdue
                {
                    if (template.LastReminderSentUtc == null)
                    {
                            shouldSend = true;
                    }
                    else
                    {
                        var daysSinceLast = (nowUtc.Date - template.LastReminderSentUtc.Value.Date).Days;
                        if (daysSinceLast >= template.RepeatEvery)
                            shouldSend = true;
                    }
                }
                else if (nowUtc.Date == scheduledUtc.Date) // due today
                {
                    if (template.LastReminderSentUtc == null ||template.LastReminderSentUtc.Value.Date < nowUtc.Date)
                    {
                        shouldSend = IsReminderDueToday(template, nowUtc);
                    }
                }
                else // future
                {
                    shouldSend = IsReminderDueToday(template, nowUtc);
                }

                if (!shouldSend)
                    continue;

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

                foreach (var employeeId in submitterIds)
                {
                    var employee = await _employeeService.GetEmployeeByIdAsync(employeeId);
                    if (employee == null || string.IsNullOrWhiteSpace(employee.OfficialEmail))
                        continue;
                    var customerId = employee.Customer_Id;

                    // Check existing submission
                    var existingSubmission = await _submissionRepo.Table
                        .Where(s => s.UpdateTemplateId == template.Id && s.SubmittedByCustomerId == customerId)
                        .OrderByDescending(s => s.Id)
                        .FirstOrDefaultAsync();

                    bool alreadySubmitted = existingSubmission != null;
                   

                    if (alreadySubmitted)
                        continue;

                    // --- Subject & body messages ---
                    string subject;
                    string bodyMessage;
                    bool addViewerInCc = false;

                    if (nowUtc.Date == scheduledUtc.Date)
                    {
                        subject = $"Reminder: '{template.Title}' form is due today";
                        bodyMessage = $"<p>This is a reminder that your form <strong>{template.Title}</strong> is due today. Please fill it up.</p>";
                    }
                    else if (nowUtc.Date > scheduledUtc.Date)
                    {
                        if (overdueDays >= 4)
                        {
                            addViewerInCc = true;

                            subject = $"⚠️ Escalation: '{template.Title}' overdue for {overdueDays} days";

                            bodyMessage = $@"
                        <p>
                            The form <strong>{template.Title}</strong> has been
                            <strong style='color:red;'>overdue for {overdueDays} days</strong>.
                        </p>
                        <p>
                            The assigned submitter has not completed the form.
                        </p>
                        <p>
                            <strong>Viewers have been copied for awareness and follow-up.</strong>
                        </p>";
                        }
                        else
                        {
                            subject = $"Overdue: '{template.Title}' form still pending";
                            bodyMessage = $"<p>This is a reminder that you are <strong>overdue</strong> on the form <strong>{template.Title}</strong>. Please submit it as soon as possible.</p>";
                        }
                    }
                    else
                    {
                        subject = $"Reminder: Please fill the '{template.Title}' form";
                        bodyMessage = $"<p>This is a reminder to complete the <strong>{template.Title}</strong> form before the deadline.</p>";
                    }

                    // --- Build form link ---
                    var formLink = $"{store.Url.TrimEnd('/')}/UpdateSubmission/Submit/{template.Id}";
                    var periodId = existingSubmission?.PeriodId ?? 0;
                    if (periodId > 0)
                        formLink += $"?periodId={periodId}";

                    var body = $"<p>Dear {employee.FirstName} {employee.LastName},</p>" +
                               bodyMessage +
                               $"<p><a href=\"{formLink}\" target=\"_blank\">Click here to fill the form</a></p>";

                    var email = new QueuedEmail
                    {
                        From = emailAccount.Email,
                        FromName = emailAccount.DisplayName,
                        To = employee.OfficialEmail,
                        CC = addViewerInCc && viewerEmails.Any()? string.Join(",", viewerEmails): null,
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
            var reminderMinutes = template.ReminderBeforeMinutes > 0
                ? template.ReminderBeforeMinutes
                : 60;

            var reminderTimeUtc = scheduledUtc.AddMinutes(-reminderMinutes);

            // ✅ Skip if current time is earlier than reminder time
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
                        // ✅ Weekly-style logic inside Daily
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
