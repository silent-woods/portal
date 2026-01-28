using System;
using App.Core.Domain.Employees;
using System.Collections.Generic;
using System.Threading.Tasks;
using App.Services.Logging;
using App.Services.ScheduleTasks;
using App.Services.Employees;
using App.Core.Domain.Messages;
using App.Core;
using DocumentFormat.OpenXml.Wordprocessing;
using App.Core.Domain.Extension.TimeSheets;
using App.Services.TimeSheets;
using App.Services.Holidays;
using System.Linq;
using App.Services.Leaves;
using App.Services.Helpers;
using App.Core.Domain.Extension.Leaves;
using DocumentFormat.OpenXml.EMMA;
using App.Services.Configuration;
using App.Services.Localization;

namespace App.Services.Messages
{
    /// <summary>
    /// Represents a task for sending queued message 
    /// </summary>
    public partial class LeaveBalanceCalculationTask : IScheduleTask
    {
        #region Fields

        private readonly IEmailAccountService _emailAccountService;
        private readonly IEmailSender _emailSender;
        private readonly ILogger _logger;
        private readonly IQueuedEmailService _queuedEmailService;
        private readonly IEmployeeService _employeeService;
        private readonly IStoreContext _storeContext;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly IWorkContext _workContext;
        private readonly TimeSheetSetting _timeSheetSettings;
        private readonly ITimeSheetsService _timeSheetsService;
        private readonly IHolidayService _holidayService;
        private readonly ILeaveManagementService _leaveManagementService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly LeaveSettings _leaveSettings;
        private readonly ISettingService _settingService;
        private readonly IScheduleTaskService _scheduleTaskService;
        private readonly INotificationService _notificationService;
        private readonly ILocalizationService _localizationService;

        #endregion

        #region Ctor

        public LeaveBalanceCalculationTask(IEmailAccountService emailAccountService,
            IEmailSender emailSender,
            ILogger logger,
            IQueuedEmailService queuedEmailService, IEmployeeService employeeService, IStoreContext storeContext,IWorkflowMessageService workflowMessageService,
            IWorkContext workContext, TimeSheetSetting timeSheetSettings,ITimeSheetsService timeSheetsService,IHolidayService holidayService,ILeaveManagementService leaveManagementService,
            IDateTimeHelper dateTimeHelper,LeaveSettings leaveSettings, ISettingService settingService, IScheduleTaskService scheduleTaskService,
            INotificationService notificationService,ILocalizationService localizationService)
        {
            _emailAccountService = emailAccountService;
            _emailSender = emailSender;
            _logger = logger;
            _queuedEmailService = queuedEmailService;
            _employeeService = employeeService;
            _storeContext = storeContext;
            _workflowMessageService = workflowMessageService;
            _workContext = workContext;
            _timeSheetSettings = timeSheetSettings;
            _timeSheetsService = timeSheetsService;
            _holidayService = holidayService;
            _leaveManagementService =leaveManagementService;
            _dateTimeHelper = dateTimeHelper;
            _leaveSettings = leaveSettings;
            _settingService = settingService;
            _scheduleTaskService = scheduleTaskService;
            _notificationService = notificationService;
            _localizationService = localizationService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Executes a task
        /// </summary>
        public virtual async System.Threading.Tasks.Task ExecuteAsync()
        {
            var currTime = await _dateTimeHelper.GetIndianTimeAsync();
            if (_leaveSettings.LeaveTestDate != DateTime.MinValue)
            {
                currTime = _leaveSettings.LeaveTestDate;
            }

            DateTime? lastUpdateOn = null;
            if (!string.IsNullOrEmpty(_leaveSettings.LastUpdateBalance))
            {
                if (DateTime.TryParseExact(_leaveSettings.LastUpdateBalance, "d-MMM-yyyy HH:mm",
                                           System.Globalization.CultureInfo.InvariantCulture,
                                           System.Globalization.DateTimeStyles.None, out DateTime parsedDate))
                {
                    lastUpdateOn = parsedDate;
                }
            }
           
            if (currTime.Day == 1)
            {
               
                await _leaveManagementService.ExecuteLeaveBalanceCalculation();

                _leaveSettings.LastUpdateBalance = currTime.ToString("d-MMM-yyyy HH:mm");

                await _settingService.SaveSettingAsync(_leaveSettings);
            }
        }

        #endregion
    }
}