using System;
using System.Linq;
using System.Threading.Tasks;
using App.Services.Holidays;
using App.Services.Logging;
using App.Services.ScheduleTasks;
using App.Services.Stores;
using App.Services.Employees;
using App.Core;
using App.Core.Domain.Messages;

namespace App.Services.Messages
{
    /// <summary>
    /// Represents a task for sending weekly update messages to employees.
    /// </summary>
    public partial class WeeklyUpdateMessagesSendTask : IScheduleTask
    {
        #region Fields

        private readonly IEmailAccountService _emailAccountService;
        private readonly IEmailSender _emailSender;
        private readonly ILogger _logger;
        private readonly IHolidayService _holidayService;
        private readonly IStoreContext _storeContext;
        private readonly IEmployeeService _employeeService;

        #endregion

        #region Ctor

        public WeeklyUpdateMessagesSendTask(IEmailAccountService emailAccountService,
            IEmailSender emailSender,
            ILogger logger,
            IHolidayService holidayService,
            IStoreContext storeContext,
            IEmployeeService employeeService)
        {
            _emailAccountService = emailAccountService;
            _emailSender = emailSender;
            _logger = logger;
            _holidayService = holidayService;
            _storeContext = storeContext;
            _employeeService = employeeService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Executes the task.
        /// </summary>
        public virtual async Task ExecuteAsync()
        {
            // Check if today is Monday at 10 AM
            var currentUtcTime = DateTime.UtcNow;
            if (currentUtcTime.DayOfWeek != DayOfWeek.Monday || currentUtcTime.Hour != 10)
                return;

            // Check if today is a holiday
            if (await _holidayService.IsHolidayAsync(currentUtcTime))
                return;

            // Get the current store URL
            var store = await _storeContext.GetCurrentStoreAsync();
            var reportLink = $"{store.Url}WeeklyReporting/WeeklyreportCreate";

            // Fetch the list of employees
            var employees = await _employeeService.GetAllEmployeesAsync();

            // Construct the notification email content
            var subject = "Weekly Report Reminder";
            var body = $"Dear Employee,<br/><br/>Please complete your weekly report by visiting the following link: <a href='{reportLink}'>Weekly Report</a>.<br/><br/>Thank you.";

            // Send emails to employees
            foreach (var employee in employees)
            {
                // Check if the employee's personal email is not null or empty
                if (string.IsNullOrWhiteSpace(employee.PersonalEmail))
                {
                    await _logger.WarningAsync($"Skipping email for {employee.FirstName} {employee.LastName} because the email address is null or empty.");
                    continue;
                }

                try
                {
                  var emailAccount =await _emailAccountService.GetEmailAccountByIdAsync(1);// Use the appropriate email account ID

                    // Ensure credentials are passed correctly
                   await _emailSender.SendEmailAsync(emailAccount,
                        subject,
                        body,
                        emailAccount.Email, // From
                        emailAccount.DisplayName ?? emailAccount.Email, // FromName
                        employee.PersonalEmail, // To
                        $"{employee.FirstName} {employee.LastName}", // ToName
                        null, // ReplyTo
                        null, // ReplyToName
                        null, // Bcc
                        null, // Cc
                        emailAccount.Username, // Username
                        emailAccount.Password  // Password
                    );

                    await _logger.InformationAsync($"Email sent to {employee.PersonalEmail}");
                }
                catch (Exception exc)
                {
                    await _logger.ErrorAsync($"Error sending e-mail to {employee.PersonalEmail}. {exc.Message}", exc);
                }
            }
        }

        #endregion
    }
}
