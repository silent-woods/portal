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
using App.Services.Messages;

namespace Satyanam.Nop.Core.Services
{
    /// <summary>
    /// Represents a task for sending queued message 
    /// </summary>
    public partial class OverdueEmailSendPluginTask : IScheduleTask
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
        private readonly IWorkflowMessagePluginService _workflowMessagePluginService;

        #endregion

        #region Ctor

        public OverdueEmailSendPluginTask(IEmailAccountService emailAccountService,
            IEmailSender emailSender,
            ILogger logger,
            IQueuedEmailService queuedEmailService, IEmployeeService employeeService, IStoreContext storeContext, IWorkflowMessageService workflowMessageService,
            IWorkContext workContext, TimeSheetSetting timeSheetSettings, ITimeSheetsService timeSheetsService, IHolidayService holidayService, ILeaveManagementService leaveManagementService,
            IDateTimeHelper dateTimeHelper, MonthlyReportSetting monthlyReportSetting, IWorkflowMessagePluginService workflowMessagePluginService)
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
            _workflowMessagePluginService = workflowMessagePluginService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Executes a task
        /// </summary>
        public virtual async System.Threading.Tasks.Task ExecuteAsync()
        {


            var time = await _dateTimeHelper.GetIndianTimeAsync();

          
                var currentTime = time.TimeOfDay;

                if (_monthlyReportSetting.OverDue_From.HasValue && _monthlyReportSetting.OverDue_To.HasValue)
                {
                    var fromTime = _monthlyReportSetting.OverDue_From.Value.TimeOfDay;
                    var toTime = _monthlyReportSetting.OverDue_To.Value.TimeOfDay;

                    if (currentTime >= fromTime && currentTime <= toTime)
                    {
                        await _workflowMessagePluginService.SendOverDueReminderMessageAsync((await _workContext.GetWorkingLanguageAsync()).Id);
                    }
                }
            



        }


        #endregion
    }
}