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
using static System.Runtime.InteropServices.JavaScript.JSType;
using App.Services.Localization;
using App.Core.Domain.Extension.Leaves;
using App.Services.Helpers;
using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.Spreadsheet;

namespace App.Services.Messages
{
    /// <summary>
    /// Represents a task for sending queued message 
    /// </summary>
    public partial class AddMonthlyLeaveTask : IScheduleTask
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
        private readonly ILeaveTransactionLogService _leaveTransactionLogService;
        private readonly ILocalizationService _localizationService;
        private readonly LeaveSettings _leaveSettings;
        private readonly IDateTimeHelper _dateTimeHelper;


        #endregion

        #region Ctor

        public AddMonthlyLeaveTask(IEmailAccountService emailAccountService,
            IEmailSender emailSender,
            ILogger logger,
            IQueuedEmailService queuedEmailService, IEmployeeService employeeService, IStoreContext storeContext, IWorkflowMessageService workflowMessageService,
            IWorkContext workContext, TimeSheetSetting timeSheetSettings, ITimeSheetsService timeSheetsService, IHolidayService holidayService, ILeaveManagementService leaveManagementService, ILeaveTransactionLogService leaveTransactionLogService, ILocalizationService localizationService, LeaveSettings leaveSettings, IDateTimeHelper dateTimeHelper)
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
            _leaveManagementService = leaveManagementService;
            _leaveTransactionLogService = leaveTransactionLogService;
            _localizationService = localizationService;
            _leaveSettings = leaveSettings;
            _dateTimeHelper = dateTimeHelper;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Executes a task
        /// </summary>
        public virtual async System.Threading.Tasks.Task ExecuteAsync()
        {
            var Date = await _dateTimeHelper.GetIndianTimeAsync();
            if (Date.Day == _leaveSettings.AddMonthlyLeaveDay)
            {
                await _leaveTransactionLogService.AddMonthlyLeave(Date.Month, Date.Year);
            }

            #endregion
        }
    }
}