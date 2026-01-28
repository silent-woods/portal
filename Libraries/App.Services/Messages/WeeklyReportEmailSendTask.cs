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

namespace App.Services.Messages
{
    /// <summary>
    /// Represents a task for sending queued message 
    /// </summary>
    public partial class WeeklyReportEmailSendTask : IScheduleTask
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
        private readonly MonthlyReportSetting _monthlyReportSetting;

        #endregion

        #region Ctor

        public WeeklyReportEmailSendTask(IEmailAccountService emailAccountService,
            IEmailSender emailSender,
            ILogger logger,
            IQueuedEmailService queuedEmailService, IEmployeeService employeeService, IStoreContext storeContext, IWorkflowMessageService workflowMessageService,
            IWorkContext workContext, TimeSheetSetting timeSheetSettings, ITimeSheetsService timeSheetsService, IHolidayService holidayService, ILeaveManagementService leaveManagementService,
            IDateTimeHelper dateTimeHelper, MonthlyReportSetting monthlyReportSetting)
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
            _dateTimeHelper = dateTimeHelper;
            _monthlyReportSetting = monthlyReportSetting;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Executes a task
        /// </summary>
        public virtual async System.Threading.Tasks.Task ExecuteAsync()
        {


            var time = await _dateTimeHelper.GetIndianTimeAsync();

            if ((int)time.DayOfWeek == _monthlyReportSetting.WeekDay)
            {
                var currentTime = time.TimeOfDay;

                if (_monthlyReportSetting.DayTime_From.HasValue && _monthlyReportSetting.DayTime_To.HasValue)
                {
                    var fromTime = _monthlyReportSetting.DayTime_From.Value.TimeOfDay;
                    var toTime = _monthlyReportSetting.DayTime_To.Value.TimeOfDay;

                    if (currentTime >= fromTime && currentTime <= toTime)
                    {
                        await _workflowMessageService.SendWeeklyReportMessageAsync((await _workContext.GetWorkingLanguageAsync()).Id);
                    }
                }
            }



        }


        #endregion
    }
}