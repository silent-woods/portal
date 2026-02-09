using App.Core;
using App.Core.Domain.Common;
using App.Core.Domain.Extension.Leaves;
using App.Core.Domain.Extension.PerformanceMeasurements;
using App.Core.Domain.Extension.TimeSheets;
using App.Core.Domain.Holidays;
using App.Core.Domain.Messages;
using App.Core.Domain.TimeSheets;
using App.Core.Events;
using App.Data;
using App.Data.Extensions;
using App.Services.Affiliates;
using App.Services.Common;
using App.Services.Customers;
using App.Services.Designations;
using App.Services.EmployeeAttendances;
using App.Services.Employees;
using App.Services.Helpers;
using App.Services.Holidays;
using App.Services.Leaves;
using App.Services.Localization;
using App.Services.ManageResumes;
using App.Services.Messages;
using App.Services.PerformanceMeasurements;
using App.Services.ProjectEmployeeMappings;
using App.Services.Projects;
using App.Services.ProjectTasks;
using App.Services.Stores;
using App.Services.TimeSheets;
using Microsoft.AspNetCore.Http;
using Satyanam.Nop.Core.Domains;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Satyanam.Nop.Core.Services
{
    /// <summary>
    /// Workflow message service
    /// </summary>
    public partial class WorkflowMessagePluginService : IWorkflowMessagePluginService
    {
        #region Fields

        private readonly CommonSettings _commonSettings;
        private readonly EmailAccountSettings _emailAccountSettings;
        private readonly IAddressService _addressService;
        private readonly IAffiliateService _affiliateService;
        private readonly ICustomerService _customerService;
        private readonly IEmailAccountService _emailAccountService;
        private readonly IEventPublisher _eventPublisher;
        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly IMessageTemplateService _messageTemplateService;
        private readonly IMessageTokenProvider _messageTokenProvider;
        private readonly IQueuedEmailService _queuedEmailService;
        private readonly IStoreContext _storeContext;
        private readonly IStoreService _storeService;
        private readonly ITokenizer _tokenizer;
        private readonly MessagesSettings _messagesSettings;
        private readonly IEmployeeService _employeeService;
        private readonly ICandiatesResumesService _candiatesResumesService;
        private readonly LeaveSettings _leaveSettings;
        private readonly IProjectEmployeeMappingService _projectEmployeeMappingService;
        private readonly ILeaveManagementService _leaveManagementService;
        private readonly ILeaveTypeService _leaveTypeService;
        private readonly IHolidayService _holidayService;
        private TimeSheetSetting _timeSheetSettings;
        private readonly IProjectsService _projectsService;
        private readonly IDesignationService _designationService;
        private readonly ITeamPerformanceMeasurementService _teamPerformanceMeasurementService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IRepository<TimeSheet> _timeSheetRepository;
        private readonly IEmployeeAttendanceService _employeeAttendanceService;
        private readonly TeamPerformanceSettings _teamPerformanceSettings;
        private readonly ITimeSheetsService _timeSheetsService;
        private readonly IProjectTaskService _projectTaskService;
        private readonly MonthlyReportSetting _monthlyReportSetting;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IActivityTrackingService _activityTrackingService;
        private readonly IWorkflowStatusService _workflowStatusService;
        private readonly ICommonPluginService _commonPluginService;
        private readonly IWebHelper _webHelper;

        #endregion

        #region Ctor

        public WorkflowMessagePluginService(CommonSettings commonSettings,
            EmailAccountSettings emailAccountSettings,
            IAddressService addressService,
            IAffiliateService affiliateService,
            ICustomerService customerService,
            IEmailAccountService emailAccountService,
            IEventPublisher eventPublisher,
            ILanguageService languageService,
            ILocalizationService localizationService,
            IMessageTemplateService messageTemplateService,
            IMessageTokenProvider messageTokenProvider,
            IQueuedEmailService queuedEmailService,
            IStoreContext storeContext,
            IStoreService storeService,
            ITokenizer tokenizer,
            MessagesSettings messagesSettings,
            IEmployeeService employeeService,
            ICandiatesResumesService candiatesResumesService,
            LeaveSettings leaveSettings,
            IProjectEmployeeMappingService projectEmployeeMappingService,
            ILeaveManagementService leaveManagementService,
            ILeaveTypeService leaveTypeService,
            IHolidayService holidayService,
            TimeSheetSetting timeSheetSettings,
            IProjectsService projectsService,
            IDesignationService designationService,
            ITeamPerformanceMeasurementService teamPerformanceMeasurementService,
            IDateTimeHelper dateTimeHelper,
            IRepository<TimeSheet> timeSheetRepository,
            IEmployeeAttendanceService employeeAttendanceService,
            TeamPerformanceSettings teamPerformanceSettings,
            ITimeSheetsService timeSheetsService,
            IProjectTaskService projectTaskService
,
            MonthlyReportSetting monthlyReportSetting,
            IHttpContextAccessor httpContextAccessor
,
            IActivityTrackingService activityTrackingService
,
            IWorkflowStatusService workflowStatusService,
            ICommonPluginService commonPluginService,
            IWebHelper webHelper)
        {
            _commonSettings = commonSettings;
            _emailAccountSettings = emailAccountSettings;
            _addressService = addressService;
            _affiliateService = affiliateService;
            _customerService = customerService;
            _emailAccountService = emailAccountService;
            _eventPublisher = eventPublisher;
            _languageService = languageService;
            _localizationService = localizationService;
            _messageTemplateService = messageTemplateService;
            _messageTokenProvider = messageTokenProvider;
            _queuedEmailService = queuedEmailService;
            _storeContext = storeContext;
            _storeService = storeService;
            _tokenizer = tokenizer;
            _messagesSettings = messagesSettings;
            _employeeService = employeeService;
            _candiatesResumesService = candiatesResumesService;
            _leaveSettings = leaveSettings;
            _projectEmployeeMappingService = projectEmployeeMappingService;
            _leaveManagementService = leaveManagementService;
            _leaveTypeService = leaveTypeService;
            _holidayService = holidayService;
            _timeSheetSettings = timeSheetSettings;
            _projectsService = projectsService;
            _designationService = designationService;
            _teamPerformanceMeasurementService = teamPerformanceMeasurementService;
            _dateTimeHelper = dateTimeHelper;
            _timeSheetRepository = timeSheetRepository;
            _employeeAttendanceService = employeeAttendanceService;
            _teamPerformanceSettings = teamPerformanceSettings;
            _timeSheetsService = timeSheetsService;
            _projectTaskService = projectTaskService;
            _monthlyReportSetting = monthlyReportSetting;
            _httpContextAccessor = httpContextAccessor;
            _activityTrackingService = activityTrackingService;
            _workflowStatusService = workflowStatusService;
            _commonPluginService = commonPluginService;
            _webHelper = webHelper;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Get active message templates by the name
        /// </summary>
        /// <param name="messageTemplateName">Message template name</param>
        /// <param name="storeId">Store identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the list of message templates
        /// </returns>
        protected virtual async Task<IList<MessageTemplate>> GetActiveMessageTemplatesAsync(string messageTemplateName, int storeId)
        {
            var messageTemplates = await _messageTemplateService.GetMessageTemplatesByNameAsync(messageTemplateName, storeId);
            if (!messageTemplates?.Any() ?? true)
                return new List<MessageTemplate>();
            messageTemplates = messageTemplates.Where(messageTemplate => messageTemplate.IsActive).ToList();
            return messageTemplates;
        }

        /// <summary>
        /// Get EmailAccount to use with a message templates
        /// </summary>
        /// <param name="messageTemplate">Message template</param>
        /// <param name="languageId">Language identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the emailAccount
        /// </returns>
        public virtual async Task<EmailAccount> GetEmailAccountOfMessageTemplateAsync(MessageTemplate messageTemplate, int languageId)
        {
            var emailAccountId = await _localizationService.GetLocalizedAsync(messageTemplate, mt => mt.EmailAccountId, languageId);
            if (emailAccountId == 0)
                emailAccountId = messageTemplate.EmailAccountId;
            var emailAccount = (await _emailAccountService.GetEmailAccountByIdAsync(emailAccountId) ?? await _emailAccountService.GetEmailAccountByIdAsync(_emailAccountSettings.DefaultEmailAccountId)) ??
                               (await _emailAccountService.GetAllEmailAccountsAsync()).FirstOrDefault();
            return emailAccount;
        }

        /// <summary>
        /// Ensure language is active
        /// </summary>
        /// <param name="languageId">Language identifier</param>
        /// <param name="storeId">Store identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the return a value language identifier
        /// </returns>
        protected virtual async Task<int> EnsureLanguageIsActiveAsync(int languageId, int storeId)
        {
            var language = await _languageService.GetLanguageByIdAsync(languageId);
            if (language == null || !language.Published)
            {
                language = (await _languageService.GetAllLanguagesAsync(storeId: storeId)).FirstOrDefault();
            }
            if (language == null || !language.Published)
            {
                language = (await _languageService.GetAllLanguagesAsync()).FirstOrDefault();
            }
            if (language == null)
                throw new Exception("No active language could be loaded");
            return language.Id;
        }

        /// <summary>
        /// Get email and name to send email for store owner
        /// </summary>
        /// <param name="messageTemplateEmailAccount">Message template email account</param>
        /// <returns>Email address and name to send email fore store owner</returns>
        protected virtual async Task<(string email, string name)> GetStoreOwnerNameAndEmailAsync(EmailAccount messageTemplateEmailAccount)
        {
            var storeOwnerEmailAccount = _messagesSettings.UseDefaultEmailAccountForSendStoreOwnerEmails ? await _emailAccountService.GetEmailAccountByIdAsync(_emailAccountSettings.DefaultEmailAccountId) : null;
            storeOwnerEmailAccount ??= messageTemplateEmailAccount;

            return (storeOwnerEmailAccount.Email, storeOwnerEmailAccount.DisplayName);
        }
        protected virtual async Task<IList<string>> GetTimesheetReminderDatesList(int employeeId, int considerBeforeDays, IList<Holiday> holidayList)
        {
            var startDate = (await _dateTimeHelper.GetUTCAsync()).AddDays(-considerBeforeDays).Date;
            var endDate = (await _dateTimeHelper.GetUTCAsync()).AddDays(-1).Date; 
            var employeeAttendance = await _employeeAttendanceService.GetAllEmployeeAttendanceAsync(employeeId, startDate, endDate, 3);
            var leaveDates = employeeAttendance.Select(a => a.CheckIn.Date).ToList();
            var allDates = Enumerable.Range(0, (endDate - startDate).Days + 1)
                                     .Select(offset => startDate.AddDays(offset))
                                     .ToList();
            var existingDates = await _timeSheetRepository.Table
                .Where(t => t.EmployeeId == employeeId && t.SpentDate >= startDate && t.SpentDate <= endDate)
                .Select(t => t.SpentDate.Date) 
                .Distinct()
                .ToListAsync();
            var excludedDates = allDates
                .Where(date => date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday ||
                               holidayList.Any(h => h.Date.Date == date) ||
                               leaveDates.Contains(date)) 
                .ToList();
            var missingDates = allDates.Except(existingDates.Concat(excludedDates))
                                       .Select(d => d.ToString("d-MMM-yyyy")) 
                                       .ToList();
            return missingDates;
        }
        #endregion

        #region Methods

        public virtual async Task<IList<int>> SendWeeklyReportMessageAsync(int languageId)
        {
            var store = await _storeContext.GetCurrentStoreAsync();
            languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);
            var request = _httpContextAccessor.HttpContext.Request;
            string domain = $"{request.Scheme}://{request.Host}";
            var date = await _dateTimeHelper.GetIndianTimeAsync();
            var allEmployee = await _employeeService.GetAllEmployeeIdsAsync();
            var (from, to) = await _timeSheetsService.GetWeekByDate(date);
            foreach (var empId in allEmployee)
            {
                var managerRole = await _designationService.GetRoleIdProjectManager();
                var leaderRole = await _designationService.GetProjectLeaderRoleId();
                var hrRole = await _designationService.GetHrRoleId();
                var emp = await _employeeService.GetEmployeeByIdAsync(empId);
                var emailBodyBuilder = new StringBuilder();
                emailBodyBuilder.Append(@"
<html>
<head>
<style>
    body {
      font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
      color: #333;
      margin: 0;
      padding: 20px;
      background-color: #fff;
    }

    h2 {
      color: #2f4199;
      margin-top: 0;
    }

    h3 {
      color: #2f4199;
      margin-bottom: 10px;
    }

    p {
      font-size: 14px;
      margin: 5px 0 20px;
    }

    table {
      width: 100%;
      border-collapse: collapse;
      margin-bottom: 25px;
      font-size: 14px;
    }

    th, td {
      padding: 10px 14px;
      border: 1px solid #ddd;
      text-align: left;
    }

    th {
      background-color: #2f4199;
      color: #fff;
    }

    tr:nth-child(even) td {
      background-color: #f4f8fc;
    }

    hr {
      border: none;
      height: 1px;
      background-color: #ccc;
      margin: 30px 0;
    }
  </style></head>
<body>
");
                if (emp != null)
                    if ((emp.DesignationId == managerRole && _monthlyReportSetting.SendReportToManager) || (emp.DesignationId == hrRole && _monthlyReportSetting.SendReportToHR))
                    {
                        emailBodyBuilder.Append(@"
<h2 style='text-align: center;'>Weekly Performance Summary</h2>
<p style='text-align: center;'>Week: " + from.ToString("dd-MMM-yyyy") + " to " + to.ToString("dd-MMM-yyyy") + @"</p>
");
                        foreach (var employeeId in allEmployee)
                        {
                            var employee = await _employeeService.GetEmployeeByIdAsync(employeeId);
                            if (employee == null) continue;
                            var qaId = await _designationService.GetQARoleId();
                            if (employee.DesignationId == qaId) continue;
                            var empTracking = await _activityTrackingService.GetSummaryByEmployeeAndDateRangeAsync(employeeId, from, to);
                            var activeHHMM = await _timeSheetsService.ConvertToHHMMFormat(empTracking.ActiveDuration);
                            var awayHHMM = await _timeSheetsService.ConvertToHHMMFormat(empTracking.AwayDuration);
                            var offlineHHMM = await _timeSheetsService.ConvertToHHMMFormat(empTracking.OfflineDuration);
                            var learningProject = await _commonPluginService.GetLearningTimeOfEmployeeByRange(employeeId, from, to);
                            var manualTimeMinutes = await _commonPluginService.GetManaualTimeOfEmployeeByRange(employeeId, from, to);
                            var manualTime = await _timeSheetsService.ConvertToHHMMFormat(manualTimeMinutes);
                            var totalMinutes = empTracking.TotalDuration + manualTimeMinutes;
                            var totalHHMM = await _timeSheetsService.ConvertToHHMMFormat(totalMinutes);

                            if (employee.DesignationId != managerRole && employee.DesignationId != hrRole)
                            {
                                var totalWorking = await _timeSheetsService.GetTotalWorkingHours(employeeId, from, to);
                                var actualTime = await _timeSheetsService.GetActualWorkingHours(employeeId, from, to);
                                var performanceReport = await _timeSheetsService.GetAllEmployeePerformanceReportAsync(employeeId, from, to, null, 0, int.MaxValue, false, null, _monthlyReportSetting.ShowOnlyNotDOT);
                                var performanceSummary = await _commonPluginService.GetEmployeePerformanceSummaryAsync(employeeId, from, to, null);
                                emailBodyBuilder.Append("<hr/>");
                                emailBodyBuilder.Append($"<h3>{employee.FirstName} {employee.LastName}</h3>");

                                emailBodyBuilder.Append($@"
<table border='1' cellpadding='6' cellspacing='0' style='width:100%; margin-bottom:10px; border-collapse: collapse; font-family: Arial, sans-serif;'>
    <thead style='background-color: #f2f4f8;'>
        <tr>
            <th rowspan='2'>Total Working Hours</th>
            <th rowspan='2'>Actual Logged Hours</th>
            <th rowspan='2'>Total Tasks</th>
            <th rowspan='2'>Overdue Tasks</th>
            <th colspan='3'>Over Estimation (%)</th>
            <th rowspan='2'>Delivered On Time</th>
            <th rowspan='2'>DOT%</th>
            <th rowspan='2'>Work Quality</th>
            

        </tr>
        <tr>
            <th>{performanceSummary.FirstOverDueThreshold}%</th>
            <th>{performanceSummary.SecondOverDueThreshold}%</th>
            <th>{performanceSummary.ThirdOverDueThreshold}%</th>
        </tr>
    </thead>
    <tbody>
");
                                var actualTimeFormat = await _timeSheetsService.ConvertSpentTimeAsync(actualTime.SpentHours, actualTime.SpentMinutes);

                                emailBodyBuilder.Append("<tr>");
                                emailBodyBuilder.Append($"<td style='text-align:center;'>{totalWorking} hrs</td>");
                                emailBodyBuilder.Append($"<td style='text-align:center;'>{actualTimeFormat}</td>");
                                emailBodyBuilder.Append($"<td style='text-align:center;'>{performanceSummary.TotalTask}</td>");
                                emailBodyBuilder.Append($"<td style='text-align:center;'>{performanceSummary.OverDueCount}</td>");
                                emailBodyBuilder.Append($"<td style='text-align:center;'>{performanceSummary.FirstOverDueThresholdCount}</td>");
                                emailBodyBuilder.Append($"<td style='text-align:center;'>{performanceSummary.SecondOverDueThresholdCount}</td>");
                                emailBodyBuilder.Append($"<td style='text-align:center;'>{performanceSummary.ThirdOverDueThresholdCount}</td>");
                                emailBodyBuilder.Append($"<td style='text-align:center;'>{performanceSummary.TotalDeliveredOnTime}</td>");
                                emailBodyBuilder.Append($"<td style='text-align:center;'>{performanceSummary.ResultPercentage}%</td>");
                                emailBodyBuilder.Append($"<td style='text-align:center;'>{performanceSummary.AvgWorkQuality}%</td>");
                                emailBodyBuilder.Append("</tr>");
                                emailBodyBuilder.Append("</tbody></table>");
                                emailBodyBuilder.Append(@"
<table style='width:100%; border-collapse: collapse; margin-bottom: 25px; font-size: 14px; text-align: center;'>
  <tr style='background-color: #f4f8fc;'>
    <th style='padding: 10px; border: 1px solid #ddd;'>
      <span style='display:inline-block; width:10px; height:10px; background-color:#28a745; border-radius:50%; margin-right:6px; vertical-align:middle;'>&nbsp;</span>
      <span>Active</span>
    </th>
    <th style='padding: 10px; border: 1px solid #ddd;'>
      <span style='display:inline-block; width:10px; height:10px; background-color:#ffc107; border-radius:50%; margin-right:6px; vertical-align:middle;'>&nbsp;</span>
      <span>Away</span>
    </th>
    <th style='padding: 10px; border: 1px solid #ddd;'>
      <span style='display:inline-block; width:10px; height:10px; background-color:#dc3545; border-radius:50%; margin-right:6px; vertical-align:middle;'>&nbsp;</span>
      <span>Offline</span>
    </th>
    <th style='padding: 10px; border: 1px solid #ddd;'>Manual</th>

    <th style='padding: 10px; border: 1px solid #ddd;'>Total</th>
    <th style='padding: 10px; border: 1px solid #ddd;'>Learning</th>
  </tr>
  <tr>
    <td style='padding: 10px; border: 1px solid #ddd;'>" + activeHHMM + @"</td>
    <td style='padding: 10px; border: 1px solid #ddd;'>" + awayHHMM + @"</td>
    <td style='padding: 10px; border: 1px solid #ddd;'>" + offlineHHMM + @"</td>
    <td style='padding: 10px; border: 1px solid #ddd;'>" + manualTime + @"</td>

    <td style='padding: 10px; border: 1px solid #ddd;'>" + totalHHMM + @"</td>
    <td style='padding: 10px; border: 1px solid #ddd;'>" + learningProject + @"</td>
  </tr>
</table>");
                                if (performanceReport.Any())
                                {
                                    emailBodyBuilder.Append("<table border='1' cellpadding='5' cellspacing='0' style='width:100%;'>");
                                    emailBodyBuilder.Append("<tr><th>Project</th><th>Task</th><th>Parent Task</th><th>Status</th><th>Due Date</th><th>Estimated Time</th><th>Spent Time</th><th>Extra Time</th><th>DOT</th></tr>");

                                    foreach (var task in performanceReport)
                                    {
                                        var project = await _projectsService.GetProjectsByIdAsync(task.ProjectId);
                                        var taskDetail = await _projectTaskService.GetProjectTasksWithoutCacheByIdAsync(task.TaskId);

                                        if (project != null && taskDetail != null)
                                        {
                                            var spentTime = await _timeSheetsService.ConvertSpentTimeAsync(taskDetail.SpentHours, taskDetail.SpentMinutes);
                                            var estimatedTimeFormat = await _timeSheetsService.ConvertToHHMMFormat(taskDetail.EstimatedTime);
                                            var workflowStatus = await _workflowStatusService.GetWorkflowStatusByIdAsync(taskDetail.StatusId);
                                            int spentTimeInMinutes = (taskDetail.SpentHours * 60) + taskDetail.SpentMinutes;
                                            int estimatedTimeInMinutes = (int)(taskDetail.EstimatedTime * 60);
                                            int extraTimeInMinutes = Math.Max(spentTimeInMinutes - estimatedTimeInMinutes, 0);
                                            int extraHours = extraTimeInMinutes / 60;
                                            int extraMinutes = extraTimeInMinutes % 60;
                                            var extraTime = await _timeSheetsService.ConvertSpentTimeAsync(extraHours, extraMinutes);
                                            string extraPercentage = estimatedTimeInMinutes > 0
                           ? $"{(int)Math.Round((extraTimeInMinutes * 100.0) / estimatedTimeInMinutes, 0)}%"
                           : "--";
                                            if (extraTimeInMinutes > 0)
                                             extraTime = $"{extraTime}({extraPercentage})";
                                            emailBodyBuilder.Append("<tr>");
                                            emailBodyBuilder.Append($"<td>{project.ProjectTitle}</td>");
                                            emailBodyBuilder.Append($@"
<td>
  <a href=""{domain}/ProjectTask/Edit?id={taskDetail.Id}""
     target=""_blank""
     style=""color: #0000EE; text-decoration: underline;"">
     {taskDetail.TaskTitle}
  </a>
</td>");
                                            string parentTaskName = "";
                                            if (taskDetail.ParentTaskId != 0)
                                            {
                                                var parentTask = await _projectTaskService.GetProjectTasksWithoutCacheByIdAsync(taskDetail.ParentTaskId);
                                                if (parentTask != null)
                                                    parentTaskName = $@"
  <a href=""{domain}/ProjectTask/Edit?id={parentTask.Id}""
     target=""_blank""
     style=""color: #0000EE; text-decoration: underline;"">
     {parentTask.TaskTitle}
  </a>";
                                            }
                                            emailBodyBuilder.Append($"<td>{parentTaskName}</td>");
                                            emailBodyBuilder.Append($@"
<td>
  <div style='display: inline-flex; align-items: center; gap: 6px;'>
    <div style='flex-shrink: 0; width: 10px; height: 10px; border-radius: 50%; background-color: {workflowStatus?.ColorCode ?? ""};'></div>
    <span>{workflowStatus?.StatusName ?? ""}</span>
  </div>
</td>");
                                            var isOverdue = await _workflowStatusService.IsTaskOverdue(taskDetail.Id);
                                            var dueDateText = taskDetail.DueDate.HasValue
                                                ? taskDetail.DueDate.Value.ToString("d-MMMM-yyyy")
                                                : "";
                                            var dueDateHtml = isOverdue
                                                ? $"<td style='color: red; font-weight: bold;'>{dueDateText}</td>"
                                                : $"<td>{dueDateText}</td>";
                                            emailBodyBuilder.Append(dueDateHtml);
                                            emailBodyBuilder.Append($"<td>{estimatedTimeFormat}</td>");
                                            emailBodyBuilder.Append($"<td>{spentTime}</td>");
                                            emailBodyBuilder.Append($"<td>{extraTime}</td>");
                                            var deliveryIcon = taskDetail.DeliveryOnTime
    ? "<span style='color: green; font-weight: bold; font-size: 16px;'>&#10004;</span>"
    : "<span style='color:red;'>&#10060;</span>";
                                            emailBodyBuilder.Append($"<td>{deliveryIcon}</td>");
                                            emailBodyBuilder.Append("</tr>");
                                        }
                                    }
                                }
                                emailBodyBuilder.Append("</table>");
                            }
                        }
                        emailBodyBuilder.Append("</body></html>");
                        var emailHtmlBody = emailBodyBuilder.ToString();
                        var messageTemplates = await GetActiveMessageTemplatesAsync(MessageTemplateSystemNames.SendWeeklyReportEmail, store.Id);
                        if (!messageTemplates.Any())
                            return new List<int>();
                        string senderEmail = emp.OfficialEmail;
                        string senderName = emp.FirstName + " " + emp.LastName;
                        var commonTokens = new List<Token>
            {
                new Token("TimeSheetReminder.SenderEmail", senderEmail),
                new Token("TimeSheetReminder.SenderName", senderName)
            };
                        await messageTemplates.SelectAwait(async messageTemplate =>
                        {
                            var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId);
                            var tokens = new List<Token>(commonTokens);
                            await _messageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount);
                            string fromEmail = emailAccount.Email;
                            string fromName = emailAccount.DisplayName;
                            string weeklyReportDate = from.ToString("dd-MMM-yyyy") + " to " + to.ToString("dd-MMM-yyyy");
                            tokens.Add(new Token("Email.Body", emailHtmlBody, true));
                            tokens.Add(new Token("WeeklyReport.Date", weeklyReportDate, true));
                            var cCEmails = "";
                            await _eventPublisher.MessageTokensAddedAsync(messageTemplate, tokens);
                            var toEmail = senderEmail;
                            var toName = senderName;
                            return await SendNotificationWithCCAsync(messageTemplate, emailAccount, languageId, tokens, toEmail, toName,
                                fromEmail: fromEmail,
                                fromName: fromName,
                                replyToEmailAddress: senderEmail,
                                replyToName: senderName,
                                cC: cCEmails);
                        }).ToListAsync();
                    }
                    else
                    {
                        if (_monthlyReportSetting.SendReportToEmployee)
                        {
                            emailBodyBuilder.Append(@"
<h2 style='text-align: center;'>Weekly Performance Summary</h2>
<p style='text-align: center;'>Week: " + from.ToString("dd-MMM-yyyy") + " to " + to.ToString("dd-MMM-yyyy") + @"</p>
");

                            var employee = await _employeeService.GetEmployeeByIdAsync(emp.Id);
                            if (employee == null) continue;
                            var qaId = await _designationService.GetQARoleId();
                            var managerId = await _designationService.GetRoleIdProjectManager();
                            if (employee.DesignationId == qaId || employee.DesignationId == managerId || employee.DesignationId == hrRole) continue;
                            var totalWorking = await _timeSheetsService.GetTotalWorkingHours(emp.Id, from, to);
                            var actualTime = await _timeSheetsService.GetActualWorkingHours(emp.Id, from, to);
                            var performanceReport = await _timeSheetsService.GetAllEmployeePerformanceReportAsync(emp.Id, from, to, null, 0, int.MaxValue, false, null, _monthlyReportSetting.ShowOnlyNotDOT);
                            var performanceSummary = await _commonPluginService.GetEmployeePerformanceSummaryAsync(emp.Id, from, to, null);
                            var empTracking = await _activityTrackingService.GetSummaryByEmployeeAndDateRangeAsync(employee.Id, from, to);
                            var activeHHMM = await _timeSheetsService.ConvertToHHMMFormat(empTracking.ActiveDuration);
                            var awayHHMM = await _timeSheetsService.ConvertToHHMMFormat(empTracking.AwayDuration);
                            var offlineHHMM = await _timeSheetsService.ConvertToHHMMFormat(empTracking.OfflineDuration);
                            var learningProject = await _commonPluginService.GetLearningTimeOfEmployeeByRange(employee.Id, from, to);
                            var manualTimeMinutes = await _commonPluginService.GetManaualTimeOfEmployeeByRange(employee.Id, from, to);
                            var manualTime = await _timeSheetsService.ConvertToHHMMFormat(manualTimeMinutes);
                            var totalMinutes = empTracking.TotalDuration + manualTimeMinutes;
                            var totalHHMM = await _timeSheetsService.ConvertToHHMMFormat(totalMinutes);
                            emailBodyBuilder.Append("<hr/>");
                            emailBodyBuilder.Append($"<h3>{employee.FirstName} {employee.LastName}</h3>");
                            emailBodyBuilder.Append($@"
<table border='1' cellpadding='6' cellspacing='0' style='width:100%; margin-bottom:10px; border-collapse: collapse; font-family: Arial, sans-serif;'>
    <thead style='background-color: #f2f4f8;'>
        <tr>
            <th rowspan='2'>Total Working Hours</th>
            <th rowspan='2'>Actual Logged Hours</th>
            <th rowspan='2'>Total Tasks</th>
            <th rowspan='2'>Overdue Tasks</th>
            <th colspan='3'>Over Estimation (%)</th>
            <th rowspan='2'>Delivered On Time</th>
            <th rowspan='2'>DOT%</th>
           <th rowspan='2'>Work Quality</th>
        </tr>
        <tr>
            <th>{performanceSummary.FirstOverDueThreshold}%</th>
            <th>{performanceSummary.SecondOverDueThreshold}%</th>
            <th>{performanceSummary.ThirdOverDueThreshold}%</th>
        </tr>
    </thead>
    <tbody>
");
                            var actualTimeFormat = await _timeSheetsService.ConvertSpentTimeAsync(actualTime.SpentHours, actualTime.SpentMinutes);

                            emailBodyBuilder.Append("<tr>");
                            emailBodyBuilder.Append($"<td style='text-align:center;'>{totalWorking} hrs</td>");
                            emailBodyBuilder.Append($"<td style='text-align:center;'>{actualTimeFormat}</td>");
                            emailBodyBuilder.Append($"<td style='text-align:center;'>{performanceSummary.TotalTask}</td>");
                            emailBodyBuilder.Append($"<td style='text-align:center;'>{performanceSummary.OverDueCount}</td>");
                            emailBodyBuilder.Append($"<td style='text-align:center;'>{performanceSummary.FirstOverDueThresholdCount}</td>");
                            emailBodyBuilder.Append($"<td style='text-align:center;'>{performanceSummary.SecondOverDueThresholdCount}</td>");
                            emailBodyBuilder.Append($"<td style='text-align:center;'>{performanceSummary.ThirdOverDueThresholdCount}</td>");
                            emailBodyBuilder.Append($"<td style='text-align:center;'>{performanceSummary.TotalDeliveredOnTime}</td>");
                            emailBodyBuilder.Append($"<td style='text-align:center;'>{performanceSummary.ResultPercentage}%</td>");
                            emailBodyBuilder.Append($"<td style='text-align:center;'>{performanceSummary.AvgWorkQuality}%</td>");
                            emailBodyBuilder.Append("</tr>");
                            emailBodyBuilder.Append("</tbody></table>");
                            emailBodyBuilder.Append(@"
<table style='width:100%; border-collapse: collapse; margin-bottom: 25px; font-size: 14px; text-align: center;'>
  <tr style='background-color: #f4f8fc;'>
    <th style='padding: 10px; border: 1px solid #ddd;'>
      <span style='display:inline-block; width:10px; height:10px; background-color:#28a745; border-radius:50%; margin-right:6px; vertical-align:middle;'>&nbsp;</span>
      <span>Active</span>
    </th>
    <th style='padding: 10px; border: 1px solid #ddd;'>
      <span style='display:inline-block; width:10px; height:10px; background-color:#ffc107; border-radius:50%; margin-right:6px; vertical-align:middle;'>&nbsp;</span>
      <span>Away</span>
    </th>
    <th style='padding: 10px; border: 1px solid #ddd;'>
      <span style='display:inline-block; width:10px; height:10px; background-color:#dc3545; border-radius:50%; margin-right:6px; vertical-align:middle;'>&nbsp;</span>
      <span>Offline</span>
    </th>
    <th style='padding: 10px; border: 1px solid #ddd;'>Manual</th>

    <th style='padding: 10px; border: 1px solid #ddd;'>Total</th>
    <th style='padding: 10px; border: 1px solid #ddd;'>Learning</th>
  </tr>
  <tr>                                            
    <td style='padding: 10px; border: 1px solid #ddd;'>" + activeHHMM + @"</td>
    <td style='padding: 10px; border: 1px solid #ddd;'>" + awayHHMM + @"</td>
    <td style='padding: 10px; border: 1px solid #ddd;'>" + offlineHHMM + @"</td>
    <td style='padding: 10px; border: 1px solid #ddd;'>" + manualTime + @"</td>

    <td style='padding: 10px; border: 1px solid #ddd;'>" + totalHHMM + @"</td>
    <td style='padding: 10px; border: 1px solid #ddd;'>" + learningProject + @"</td>
  </tr>
</table>");
                            if (performanceReport.Any())
                            {
                                emailBodyBuilder.Append("<table border='1' cellpadding='5' cellspacing='0' style='width:100%;'>");
                                emailBodyBuilder.Append("<tr><th>Project</th><th>Task</th><th>Parent Task</th><th>Status</th><th>Due Date</th><th>Estimated Time</th><th>Spent Time</th><th>Extra Time</th><th>Delivered On Time</th></tr>");
                                foreach (var task in performanceReport)
                                {
                                    var project = await _projectsService.GetProjectsByIdAsync(task.ProjectId);
                                    var taskDetail = await _projectTaskService.GetProjectTasksByIdAsync(task.TaskId);
                                    if (project != null && taskDetail != null)
                                    {
                                        var developmentTime = await _timeSheetsService.GetDevelopmentTimeByTaskId(task.TaskId);
                                        var spentTime = await _timeSheetsService.ConvertSpentTimeAsync(developmentTime.SpentHours, developmentTime.SpentMinutes);
                                        var estimatedTimeFormat = await _timeSheetsService.ConvertToHHMMFormat(taskDetail.EstimatedTime);
                                        var workflowStatus = await _workflowStatusService.GetWorkflowStatusByIdAsync(taskDetail.StatusId);
                                        int spentTimeInMinutes = (developmentTime.SpentHours * 60) + taskDetail.SpentMinutes;
                                        int estimatedTimeInMinutes = (int)(taskDetail.EstimatedTime * 60);
                                        int extraTimeInMinutes = Math.Max(spentTimeInMinutes - estimatedTimeInMinutes, 0);
                                        int extraHours = extraTimeInMinutes / 60;
                                        int extraMinutes = extraTimeInMinutes % 60;
                                        var extraTime = await _timeSheetsService.ConvertSpentTimeAsync(extraHours, extraMinutes);
                                        string extraPercentage = estimatedTimeInMinutes > 0
                                      ? $"{(int)Math.Round((extraTimeInMinutes * 100.0) / estimatedTimeInMinutes, 0)}%"
                                      : "--";
                                        if (extraTimeInMinutes > 0)
                                            extraTime = $"{extraTime}({extraPercentage})";
                                        emailBodyBuilder.Append("<tr>");
                                        emailBodyBuilder.Append($"<td>{project.ProjectTitle}</td>");
                                        emailBodyBuilder.Append($@"
<td>
  <a href=""{domain}/ProjectTask/Edit?id={taskDetail.Id}""
     target=""_blank""
     style=""color: #0000EE; text-decoration: underline;"">
     {taskDetail.TaskTitle}
  </a>
</td>");
                                        string parentTaskName = "";
                                        if (taskDetail.ParentTaskId != 0)
                                        {
                                            var parentTask = await _projectTaskService.GetProjectTasksWithoutCacheByIdAsync(taskDetail.ParentTaskId);
                                            if (parentTask != null)
                                                parentTaskName = $@"
  <a href=""{domain}/ProjectTask/Edit?id={parentTask.Id}""
     target=""_blank""
     style=""color: #0000EE; text-decoration: underline;"">
     {parentTask.TaskTitle}
  </a>";
                                        }
                                        emailBodyBuilder.Append($"<td>{parentTaskName}</td>");
                                        emailBodyBuilder.Append($@"
<td>
  <div style='display: inline-flex; align-items: center; gap: 6px;'>
    <div style='flex-shrink: 0; width: 10px; height: 10px; border-radius: 50%; background-color: {workflowStatus?.ColorCode ?? ""};'></div>
    <span>{workflowStatus?.StatusName ?? ""}</span>
  </div>
</td>");
                                        var isOverdue = await _workflowStatusService.IsTaskOverdue(taskDetail.Id);
                                        var dueDateText = taskDetail.DueDate.HasValue
                                            ? taskDetail.DueDate.Value.ToString("d-MMMM-yyyy")
                                            : "";
                                        var dueDateHtml = isOverdue
                                            ? $"<td style='color: red; font-weight: bold;'>{dueDateText}</td>"
                                            : $"<td>{dueDateText}</td>";
                                        emailBodyBuilder.Append(dueDateHtml);
                                        emailBodyBuilder.Append($"<td>{estimatedTimeFormat}</td>");
                                        emailBodyBuilder.Append($"<td>{spentTime}</td>");
                                        emailBodyBuilder.Append($"<td>{extraTime}</td>");
                                        var deliveryIcon = taskDetail.DeliveryOnTime
      ? "<span style='color: green; font-weight: bold; font-size: 16px;'>&#10004;</span>"
      : "<span style='color:red;'>&#10060;</span>";

                                        emailBodyBuilder.Append($"<td>{deliveryIcon}</td>");
                                        emailBodyBuilder.Append("</tr>");
                                    }
                                }
                                emailBodyBuilder.Append("</table>");
                            }
                            var projectIds = await _projectEmployeeMappingService.GetProjectIdsManagedOrCoordinateByEmployeeIdAsync(employee.Id);

                            if (projectIds.Count > 0 && _monthlyReportSetting.SendReportToProjectLeader)
                            {
                                var employeeIds = await _timeSheetsService.GetEmployeeIdsWorkOnProjects(projectIds);
                                foreach (var id in employeeIds)
                                {
                                    if (id == emp.Id) continue;
                                    employee = await _employeeService.GetEmployeeByIdAsync(id);
                                    if (employee == null) continue;
                                    if (employee.DesignationId == qaId) continue;
                                    totalWorking = await _timeSheetsService.GetTotalWorkingHours(id, from, to);
                                    actualTime = await _timeSheetsService.GetActualWorkingHours(id, from, to);
                                    performanceReport = await _timeSheetsService.GetAllEmployeePerformanceReportAsync(id, from, to, projectIds, 0, int.MaxValue, false, null, _monthlyReportSetting.ShowOnlyNotDOT);
                                    var projectIdsString = string.Join(",", projectIds);
                                    performanceSummary = await _commonPluginService.GetEmployeePerformanceSummaryAsync(id, from, to, projectIdsString);
                                    empTracking = await _activityTrackingService.GetSummaryByEmployeeAndDateRangeAsync(id, from, to);
                                    activeHHMM = await _timeSheetsService.ConvertToHHMMFormat(empTracking.ActiveDuration);
                                    awayHHMM = await _timeSheetsService.ConvertToHHMMFormat(empTracking.AwayDuration);
                                    offlineHHMM = await _timeSheetsService.ConvertToHHMMFormat(empTracking.OfflineDuration);
                                    learningProject = await _commonPluginService.GetLearningTimeOfEmployeeByRange(id, from, to);
                                    manualTimeMinutes = await _commonPluginService.GetManaualTimeOfEmployeeByRange(employee.Id, from, to);
                                    manualTime = await _timeSheetsService.ConvertToHHMMFormat(manualTimeMinutes);
                                    totalMinutes = empTracking.TotalDuration + manualTimeMinutes;
                                    totalHHMM = await _timeSheetsService.ConvertToHHMMFormat(totalMinutes);
                                    emailBodyBuilder.Append("<hr/>");
                                    emailBodyBuilder.Append($"<h3>{employee.FirstName} {employee.LastName}</h3>");

                                    emailBodyBuilder.Append($@"
<table border='1' cellpadding='6' cellspacing='0' style='width:100%; margin-bottom:10px; border-collapse: collapse; font-family: Arial, sans-serif;'>
    <thead style='background-color: #f2f4f8;'>
        <tr>
            <th rowspan='2'>Total Working Hours</th>
            <th rowspan='2'>Actual Logged Hours</th>
            <th rowspan='2'>Total Tasks</th>
            <th rowspan='2'>Overdue Tasks</th>
            <th colspan='3'>Over Estimation (%)</th>
            <th rowspan='2'>Delivered On Time</th>
            <th rowspan='2'>DOT%</th>

             <th rowspan='2'>Work Quality</th>
        </tr>
        <tr>
            <th>{performanceSummary.FirstOverDueThreshold}%</th>
            <th>{performanceSummary.SecondOverDueThreshold}%</th>
            <th>{performanceSummary.ThirdOverDueThreshold}%</th>
        </tr>
    </thead>
    <tbody>
");

                                    actualTimeFormat = await _timeSheetsService.ConvertSpentTimeAsync(actualTime.SpentHours, actualTime.SpentMinutes);
                                    emailBodyBuilder.Append("<tr>");
                                    emailBodyBuilder.Append($"<td style='text-align:center;'>{totalWorking} hrs</td>");
                                    emailBodyBuilder.Append($"<td style='text-align:center;'>{actualTimeFormat}</td>");
                                    emailBodyBuilder.Append($"<td style='text-align:center;'>{performanceSummary.TotalTask}</td>");
                                    emailBodyBuilder.Append($"<td style='text-align:center;'>{performanceSummary.OverDueCount}</td>");
                                    emailBodyBuilder.Append($"<td style='text-align:center;'>{performanceSummary.FirstOverDueThresholdCount}</td>");
                                    emailBodyBuilder.Append($"<td style='text-align:center;'>{performanceSummary.SecondOverDueThresholdCount}</td>");
                                    emailBodyBuilder.Append($"<td style='text-align:center;'>{performanceSummary.ThirdOverDueThresholdCount}</td>");
                                    emailBodyBuilder.Append($"<td style='text-align:center;'>{performanceSummary.TotalDeliveredOnTime}</td>");
                                    emailBodyBuilder.Append($"<td style='text-align:center;'>{performanceSummary.AvgWorkQuality}%</td>");
                                    emailBodyBuilder.Append($"<td style='text-align:center;'>{performanceSummary.ResultPercentage}%</td>");
                                    emailBodyBuilder.Append("</tr>");
                                    emailBodyBuilder.Append("</tbody></table>");
                                    emailBodyBuilder.Append(@"
<table style='width:100%; border-collapse: collapse; margin-bottom: 25px; font-size: 14px; text-align: center;'>
  <tr style='background-color: #f4f8fc;'>
    <th style='padding: 10px; border: 1px solid #ddd;'>
      <span style='display:inline-block; width:10px; height:10px; background-color:#28a745; border-radius:50%; margin-right:6px; vertical-align:middle;'>&nbsp;</span>
      <span>Active</span>
    </th>
    <th style='padding: 10px; border: 1px solid #ddd;'>
      <span style='display:inline-block; width:10px; height:10px; background-color:#ffc107; border-radius:50%; margin-right:6px; vertical-align:middle;'>&nbsp;</span>
      <span>Away</span>
    </th>
    <th style='padding: 10px; border: 1px solid #ddd;'>
      <span style='display:inline-block; width:10px; height:10px; background-color:#dc3545; border-radius:50%; margin-right:6px; vertical-align:middle;'>&nbsp;</span>
      <span>Offline</span>
    </th>
    <th style='padding: 10px; border: 1px solid #ddd;'>Manual</th>

    <th style='padding: 10px; border: 1px solid #ddd;'>Total</th>
    <th style='padding: 10px; border: 1px solid #ddd;'>Learning</th>
  </tr>
  <tr>
    <td style='padding: 10px; border: 1px solid #ddd;'>" + activeHHMM + @"</td>
    <td style='padding: 10px; border: 1px solid #ddd;'>" + awayHHMM + @"</td>
    <td style='padding: 10px; border: 1px solid #ddd;'>" + offlineHHMM + @"</td>
    <td style='padding: 10px; border: 1px solid #ddd;'>" + manualTime + @"</td>

    <td style='padding: 10px; border: 1px solid #ddd;'>" + totalHHMM + @"</td>
    <td style='padding: 10px; border: 1px solid #ddd;'>" + learningProject + @"</td>
  </tr>
</table>");

                                    if (performanceReport.Any())
                                    {
                                        emailBodyBuilder.Append("<table border='1' cellpadding='5' cellspacing='0' style='width:100%;'>");
                                        emailBodyBuilder.Append("<tr><th>Project</th><th>Task</th><th>Parent Task</th><th>Status</th><th>Due Date</th><th>Estimated Time</th><th>Spent Time</th><th>Extra Time</th><th>DOT</th></tr>");

                                        foreach (var task in performanceReport)
                                        {
                                            var project = await _projectsService.GetProjectsByIdAsync(task.ProjectId);
                                            var taskDetail = await _projectTaskService.GetProjectTasksByIdAsync(task.TaskId);

                                            if (project != null && taskDetail != null)
                                            {
                                                var developmentTime = await _timeSheetsService.GetDevelopmentTimeByTaskId(task.TaskId);
                                                var spentTime = await _timeSheetsService.ConvertSpentTimeAsync(developmentTime.SpentHours, developmentTime.SpentMinutes);
                                                var estimatedTimeFormat = await _timeSheetsService.ConvertToHHMMFormat(taskDetail.EstimatedTime);
                                                int spentTimeInMinutes = (developmentTime.SpentHours * 60) + developmentTime.SpentMinutes;
                                                int estimatedTimeInMinutes = (int)(taskDetail.EstimatedTime * 60);
                                                var workflowStatus = await _workflowStatusService.GetWorkflowStatusByIdAsync(taskDetail.StatusId);
                                                int extraTimeInMinutes = Math.Max(spentTimeInMinutes - estimatedTimeInMinutes, 0);
                                                int extraHours = extraTimeInMinutes / 60;
                                                int extraMinutes = extraTimeInMinutes % 60;
                                                var extraTime = await _timeSheetsService.ConvertSpentTimeAsync(extraHours, extraMinutes);
                                                string extraPercentage = estimatedTimeInMinutes > 0
                                     ? $"{(int)Math.Round((extraTimeInMinutes * 100.0) / estimatedTimeInMinutes, 0)}%"
                                     : "--";
                                                if (extraTimeInMinutes > 0)
                                                    extraTime = $"{extraTime}({extraPercentage})";
                                                emailBodyBuilder.Append("<tr>");
                                                emailBodyBuilder.Append($"<td>{project.ProjectTitle}</td>");
                                                emailBodyBuilder.Append($@"
<td>
  <a href=""{domain}/ProjectTask/Edit?id={taskDetail.Id}""
     target=""_blank""
     style=""color: #0000EE; text-decoration: underline;"">
     {taskDetail.TaskTitle}
  </a>
</td>");

                                                string parentTaskName = "";
                                                if (taskDetail.ParentTaskId != 0)
                                                {
                                                    var parentTask = await _projectTaskService.GetProjectTasksWithoutCacheByIdAsync(taskDetail.ParentTaskId);
                                                    if (parentTask != null)
                                                        parentTaskName = $@"
  <a href=""{domain}/ProjectTask/Edit?id={parentTask.Id}""
     target=""_blank""
     style=""color: #0000EE; text-decoration: underline;"">
     {parentTask.TaskTitle}
  </a>";
                                                }
                                                emailBodyBuilder.Append($"<td>{parentTaskName}</td>");

                                                emailBodyBuilder.Append($@"
<td>
  <div style='display: inline-flex; align-items: center; gap: 6px;'>
    <div style='flex-shrink: 0; width: 10px; height: 10px; border-radius: 50%; background-color: {workflowStatus?.ColorCode ?? ""};'></div>
    <span>{workflowStatus?.StatusName ?? ""}</span>
  </div>
</td>");
                                                var isOverdue = await _workflowStatusService.IsTaskOverdue(taskDetail.Id);
                                                var dueDateText = taskDetail.DueDate.HasValue
                                                    ? taskDetail.DueDate.Value.ToString("d-MMMM-yyyy")
                                                    : "";
                                                var dueDateHtml = isOverdue
                                                    ? $"<td style='color: red; font-weight: bold;'>{dueDateText}</td>"
                                                    : $"<td>{dueDateText}</td>";
                                                emailBodyBuilder.Append(dueDateHtml);
                                                emailBodyBuilder.Append($"<td>{estimatedTimeFormat}</td>");
                                                emailBodyBuilder.Append($"<td>{spentTime}</td>");
                                                emailBodyBuilder.Append($"<td>{extraTime}</td>");
                                                var deliveryIcon = taskDetail.DeliveryOnTime
              ? "<span style='color: green; font-weight: bold; font-size: 16px;'>&#10004;</span>"
              : "<span style='color:red;'>&#10060;</span>";

                                                emailBodyBuilder.Append($"<td>{deliveryIcon}</td>");
                                                emailBodyBuilder.Append("</tr>");
                                            }
                                        }
                                        emailBodyBuilder.Append("</table>");

                                    }
                                }
                            }

                            emailBodyBuilder.Append("</body></html>");
                            var emailHtmlBody = emailBodyBuilder.ToString();
                            var messageTemplates = await GetActiveMessageTemplatesAsync(MessageTemplateSystemNames.SendWeeklyReportEmail, store.Id);
                            if (!messageTemplates.Any())
                                return new List<int>();
                            string senderEmail = emp.OfficialEmail;
                            string senderName = emp.FirstName + " " + emp.LastName;
                            var commonTokens = new List<Token>
            {
                new Token("TimeSheetReminder.SenderEmail", senderEmail),
                new Token("TimeSheetReminder.SenderName", senderName)
            };
                            await messageTemplates.SelectAwait(async messageTemplate =>
                            {
                                var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId);
                                var tokens = new List<Token>(commonTokens);
                                await _messageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount);
                                string fromEmail = emailAccount.Email;
                                string fromName = emailAccount.DisplayName;
                                string weeklyReportDate = from.ToString("dd-MMM-yyyy") + " to " + to.ToString("dd-MMM-yyyy");
                                tokens.Add(new Token("Email.Body", emailHtmlBody, true));
                                tokens.Add(new Token("WeeklyReport.Date", weeklyReportDate, true));
                                var cCEmails = "";
                                await _eventPublisher.MessageTokensAddedAsync(messageTemplate, tokens);
                                var toEmail = senderEmail;
                                var toName = senderName;
                                return await SendNotificationWithCCAsync(messageTemplate, emailAccount, languageId, tokens, toEmail, toName,
                                    fromEmail: fromEmail,
                                    fromName: fromName,
                                    replyToEmailAddress: senderEmail,
                                    replyToName: senderName,
                                    cC: cCEmails);
                            }).ToListAsync();
                        }
                    }
            }
            return new List<int>();
        }

        public virtual async Task<IList<int>> SendOverDueReminderMessageAsync(int languageId)
        {
            var store = await _storeContext.GetCurrentStoreAsync();
            languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);
            var today = await _dateTimeHelper.GetIndianTimeAsync();
            var date = await _dateTimeHelper.GetIndianTimeAsync();
            var request = _httpContextAccessor.HttpContext.Request;
            string domain = $"{request.Scheme}://{request.Host}";
            var allEmployee = await _employeeService.GetAllEmployeeIdsAsync();
            foreach (var empId in allEmployee)
            {
                var managerRole = await _designationService.GetRoleIdProjectManager();
                var leaderRole = await _designationService.GetProjectLeaderRoleId();
                var hrRole = await _designationService.GetHrRoleId();
                var emp = await _employeeService.GetEmployeeByIdAsync(empId);
                bool hasOverdueContent = false;
                var emailBodyBuilder = new StringBuilder();

                emailBodyBuilder.Append(@"
<html>
<head>
<style>
    body {
      font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
      color: #333;
      margin: 0;
      padding: 20px;
      background-color: #fff;
    }

    h2 {
      color: #2f4199;
      margin-top: 0;
    }

    h3 {
      color: #2f4199;
      margin-bottom: 10px;
    }

    p {
      font-size: 14px;
      margin: 5px 0 20px;
    }

    table {
      width: 100%;
      border-collapse: collapse;
      margin-bottom: 25px;
      font-size: 14px;
    }

    th, td {
      padding: 10px 14px;
      border: 1px solid #ddd;
      text-align: left;
    }

    th {
      background-color: #2f4199;
      color: #fff;
    }

    tr:nth-child(even) td {
      background-color: #f4f8fc;
    }

    hr {
      border: none;
      height: 1px;
      background-color: #ccc;
      margin: 30px 0;
    }
  </style></head>
<body>
");
                if (emp != null)
                    if ((emp.DesignationId == managerRole && _monthlyReportSetting.SendOverdueReportToManager) || (emp.DesignationId == hrRole && _monthlyReportSetting.SendOverdueReportToHR))
                    {
                        emailBodyBuilder.Append(@"
<h2 style='text-align: center;'>OverDue Reminder</h2>");

                        foreach (var employeeId in allEmployee)
                        {
                            var employee = await _employeeService.GetEmployeeByIdAsync(employeeId);
                            if (employee == null) continue;
                            if (employee.DesignationId != managerRole && employee.DesignationId != hrRole)
                            {
                                var tasks = await _commonPluginService.GetOverdueTasksByEmployeeIdAsync(employeeId);
                                if (tasks.Any())
                                {
                                    emailBodyBuilder.Append("<hr/>");
                                    emailBodyBuilder.Append($"<h3>{employee.FirstName} {employee.LastName}</h3>");
                                    hasOverdueContent = true;
                                    var overdueToday = tasks.Where(t => t.DueDate?.Date == today.Date).ToList();
                                    var previousOverdue = tasks.Where(t => t.DueDate?.Date < today.Date).ToList();
                                    if (overdueToday.Any())
                                    {
                                        emailBodyBuilder.Append("<table border='1' cellpadding='5' cellspacing='0' style='width:100%;'>");
                                        emailBodyBuilder.Append("<tr><td colspan='10' style='text-align:center; background-color:#f0f4ff; font-weight:bold; font-size:16px;'> Tasks Overdue Today</td></tr><tr><th>Project</th><th>Task</th><th>Status</th><th>Due Date</th><th>Esti. Time</th><th>Spent Time</th><th>Extra Time</th><th>D.O.T</th></tr>");

                                        foreach (var task in overdueToday)
                                        {
                                            var project = await _projectsService.GetProjectsByIdAsync(task.ProjectId);
                                            var taskDetail = await _projectTaskService.GetProjectTasksWithoutCacheByIdAsync(task.Id);

                                            if (project != null && taskDetail != null)
                                            {
                                                var spentTime = await _timeSheetsService.ConvertSpentTimeAsync(taskDetail.SpentHours, taskDetail.SpentMinutes);
                                                var estimatedTimeFormat = await _timeSheetsService.ConvertToHHMMFormat(taskDetail.EstimatedTime);
                                                var workflowStatus = await _workflowStatusService.GetWorkflowStatusByIdAsync(taskDetail.StatusId);
                                                int spentTimeInMinutes = (taskDetail.SpentHours * 60) + taskDetail.SpentMinutes;
                                                int estimatedTimeInMinutes = (int)(taskDetail.EstimatedTime * 60);                 
                                                int extraTimeInMinutes = Math.Max(spentTimeInMinutes - estimatedTimeInMinutes, 0);
                                                int extraHours = extraTimeInMinutes / 60;
                                                int extraMinutes = extraTimeInMinutes % 60;
                                                var extraTime = await _timeSheetsService.ConvertSpentTimeAsync(extraHours, extraMinutes);
                                                string extraPercentage = estimatedTimeInMinutes > 0
                                        ? $"{(int)Math.Round((extraTimeInMinutes * 100.0) / estimatedTimeInMinutes, 0)}%"
                                        : "--";
                                                if (extraTimeInMinutes > 0)
                                                    extraTime = $"{extraTime}({extraPercentage})";
                                                emailBodyBuilder.Append("<tr>");
                                                emailBodyBuilder.Append($"<td>{project.ProjectTitle}</td>");
                                                emailBodyBuilder.Append($@"
<td>
  <a href=""{domain}/ProjectTask/Edit?id={taskDetail.Id}""
     target=""_blank""
     style=""color: #0000EE; text-decoration: underline;"">
     {taskDetail.TaskTitle}
  </a>
</td>");
                                                emailBodyBuilder.Append($@"
<td>
  <div style='display: inline-flex; align-items: center; gap: 6px;'>
    <div style='flex-shrink: 0; width: 10px; height: 10px; border-radius: 50%; background-color: {workflowStatus?.ColorCode ?? ""};'></div>
    <span>{workflowStatus?.StatusName ?? ""}</span>
  </div>
</td>");

                                                var dueDateText = taskDetail.DueDate.HasValue
                                                    ? taskDetail.DueDate.Value.ToString("d-MMMM-yyyy")
                                                    : "";
                                                emailBodyBuilder.Append($"<td>{dueDateText}</td>");
                                                emailBodyBuilder.Append($"<td>{estimatedTimeFormat}</td>");
                                                emailBodyBuilder.Append($"<td>{spentTime}</td>");
                                                emailBodyBuilder.Append($"<td>{extraTime}</td>");
                                                var deliveryIcon = taskDetail.DeliveryOnTime
        ? "<span style='color: green; font-weight: bold; font-size: 16px;'>&#10004;</span>"
        : "<span style='color:red;'>&#10060;</span>";
                                                emailBodyBuilder.Append($"<td>{deliveryIcon}</td>");
                                                emailBodyBuilder.Append("</tr>");
                                            }
                                        }
                                        emailBodyBuilder.Append("</table>");
                                    }
                                    if (previousOverdue.Any())
                                    {
                                        emailBodyBuilder.Append("<table border='1' cellpadding='5' cellspacing='0' style='width:100%;'>");
                                        emailBodyBuilder.Append("<tr><td colspan='10' style='text-align:center; background-color:#fff4f0; font-weight:bold; font-size:16px;'>Previously Overdue Tasks</td></tr><tr><th>Project</th><th>Task</th><th>Status</th><th>Due Date</th><th>Esti. Time</th><th>Spent Time</th><th>Extra Time</th><th>D.O.T</th></tr>");
                                        foreach (var task in previousOverdue)
                                        {
                                            var project = await _projectsService.GetProjectsByIdAsync(task.ProjectId);
                                            var taskDetail = await _projectTaskService.GetProjectTasksWithoutCacheByIdAsync(task.Id);
                                            if (project != null && taskDetail != null)
                                            {
                                                var spentTime = await _timeSheetsService.ConvertSpentTimeAsync(taskDetail.SpentHours, taskDetail.SpentMinutes);
                                                var estimatedTimeFormat = await _timeSheetsService.ConvertToHHMMFormat(taskDetail.EstimatedTime);
                                                var workflowStatus = await _workflowStatusService.GetWorkflowStatusByIdAsync(taskDetail.StatusId);
                                                int spentTimeInMinutes = (taskDetail.SpentHours * 60) + taskDetail.SpentMinutes;
                                                int estimatedTimeInMinutes = (int)(taskDetail.EstimatedTime * 60);
                                                int extraTimeInMinutes = Math.Max(spentTimeInMinutes - estimatedTimeInMinutes, 0);
                                                int extraHours = extraTimeInMinutes / 60;
                                                int extraMinutes = extraTimeInMinutes % 60;
                                                var extraTime = await _timeSheetsService.ConvertSpentTimeAsync(extraHours, extraMinutes);
                                                string extraPercentage = estimatedTimeInMinutes > 0
                                        ? $"{(int)Math.Round((extraTimeInMinutes * 100.0) / estimatedTimeInMinutes, 0)}%"
                                        : "--";
                                                if (extraTimeInMinutes > 0)
                                                    extraTime = $"{extraTime}({extraPercentage})";
                                                emailBodyBuilder.Append("<tr>");
                                                emailBodyBuilder.Append($"<td>{project.ProjectTitle}</td>");
                                                emailBodyBuilder.Append($@"
<td>
  <a href=""{domain}/ProjectTask/Edit?id={taskDetail.Id}""
     target=""_blank""
     style=""color: #0000EE; text-decoration: underline;"">
     {taskDetail.TaskTitle}
  </a>
</td>");

                                                emailBodyBuilder.Append($@"
<td>
  <div style='display: inline-flex; align-items: center; gap: 6px;'>
    <div style='flex-shrink: 0; width: 10px; height: 10px; border-radius: 50%; background-color: {workflowStatus?.ColorCode ?? ""};'></div>
    <span>{workflowStatus?.StatusName ?? ""}</span>
  </div>
</td>");
                                                var dueDateText = taskDetail.DueDate.HasValue
                                                    ? taskDetail.DueDate.Value.ToString("d-MMMM-yyyy")
                                                    : "";
                                                emailBodyBuilder.Append($"<td>{dueDateText}</td>");
                                                emailBodyBuilder.Append($"<td>{estimatedTimeFormat}</td>");
                                                emailBodyBuilder.Append($"<td>{spentTime}</td>");
                                                emailBodyBuilder.Append($"<td>{extraTime}</td>");
                                                var deliveryIcon = taskDetail.DeliveryOnTime
        ? "<span style='color: green; font-weight: bold; font-size: 16px;'>&#10004;</span>"
        : "<span style='color:red;'>&#10060;</span>";
                                                emailBodyBuilder.Append($"<td>{deliveryIcon}</td>");
                                                emailBodyBuilder.Append("</tr>");
                                            }
                                        }
                                        emailBodyBuilder.Append("</table>");
                                    }
                                }
                            }
                        }
                        emailBodyBuilder.Append("</body></html>");
                        var emailHtmlBody = emailBodyBuilder.ToString();
                        if (!hasOverdueContent)
                            continue;
                        var messageTemplates = await GetActiveMessageTemplatesAsync(MessageTemplateSystemNames.SendOverdueReminderEmail, store.Id);
                        if (!messageTemplates.Any())
                            return new List<int>();
                        string senderEmail = emp.OfficialEmail;
                        string senderName = emp.FirstName + " " + emp.LastName;
                        var commonTokens = new List<Token>
            {
                new Token("TimeSheetReminder.SenderEmail", senderEmail),
                new Token("TimeSheetReminder.SenderName", senderName)
            };
                        await messageTemplates.SelectAwait(async messageTemplate =>
                        {
                            var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId);
                            var tokens = new List<Token>(commonTokens);
                            await _messageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount);
                            string fromEmail = emailAccount.Email;
                            string fromName = emailAccount.DisplayName;
                            tokens.Add(new Token("Email.Body", emailHtmlBody, true));
                            var cCEmails = "";
                            await _eventPublisher.MessageTokensAddedAsync(messageTemplate, tokens);
                            var toEmail = senderEmail;
                            var toName = senderName;
                            return await SendNotificationWithCCAsync(messageTemplate, emailAccount, languageId, tokens, toEmail, toName,
                                fromEmail: fromEmail,
                                fromName: fromName,
                                replyToEmailAddress: senderEmail,
                                replyToName: senderName,
                                cC: cCEmails);
                        }).ToListAsync();
                    }
                    else
                    {
                        if (_monthlyReportSetting.SendOverdueEmailToEmployee)
                        {
                            emailBodyBuilder.Append(@"
<h2 style='text-align: center;'>Overdue Reminder</h2>");
                            var employee = await _employeeService.GetEmployeeByIdAsync(emp.Id);
                            if (employee == null) continue;
                            var managerId = await _designationService.GetRoleIdProjectManager();
                            var tasks = await _commonPluginService.GetOverdueTasksByEmployeeIdAsync(emp.Id);
                            if (employee.DesignationId == managerId || employee.DesignationId == hrRole) continue;
                            var overdueToday = tasks.Where(t => t.DueDate?.Date == today.Date).ToList();
                            var previousOverdue = tasks.Where(t => t.DueDate?.Date < today.Date).ToList();
                            if (tasks.Any())
                            {
                                emailBodyBuilder.Append("<hr/>");
                                emailBodyBuilder.Append($"<h3>{employee.FirstName} {employee.LastName}</h3>");
                                hasOverdueContent = true;
                                if (overdueToday.Any())
                                {
                                    emailBodyBuilder.Append("<table border='1' cellpadding='5' cellspacing='0' style='width:100%;'>");
                                    emailBodyBuilder.Append("<tr><td colspan='10' style='text-align:center; background-color:#f0f4ff; font-weight:bold; font-size:16px;'> Tasks Overdue Today</td></tr><tr><th>Project</th><th>Task</th><th>Status</th><th>Due Date</th><th>Esti. Time</th><th>Spent Time</th><th>Extra Time</th><th>D.O.T</th></tr>");
                                    foreach (var task in overdueToday)
                                    {
                                        var project = await _projectsService.GetProjectsByIdAsync(task.ProjectId);
                                        var taskDetail = await _projectTaskService.GetProjectTasksWithoutCacheByIdAsync(task.Id);

                                        if (project != null && taskDetail != null)
                                        {
                                            var spentTime = await _timeSheetsService.ConvertSpentTimeAsync(taskDetail.SpentHours, taskDetail.SpentMinutes);
                                            var estimatedTimeFormat = await _timeSheetsService.ConvertToHHMMFormat(taskDetail.EstimatedTime);
                                            var workflowStatus = await _workflowStatusService.GetWorkflowStatusByIdAsync(taskDetail.StatusId);
                                            int spentTimeInMinutes = (taskDetail.SpentHours * 60) + taskDetail.SpentMinutes;
                                            int estimatedTimeInMinutes = (int)(taskDetail.EstimatedTime * 60);
                                            int extraTimeInMinutes = Math.Max(spentTimeInMinutes - estimatedTimeInMinutes, 0);
                                            int extraHours = extraTimeInMinutes / 60;
                                            int extraMinutes = extraTimeInMinutes % 60;
                                            var extraTime = await _timeSheetsService.ConvertSpentTimeAsync(extraHours, extraMinutes);
                                            string extraPercentage = estimatedTimeInMinutes > 0
                                            ? $"{(int)Math.Round((extraTimeInMinutes * 100.0) / estimatedTimeInMinutes, 0)}%"
                                            : "--";

                                            if (extraTimeInMinutes > 0)
                                                extraTime = $"{extraTime}({extraPercentage})";

                                            emailBodyBuilder.Append("<tr>");
                                            emailBodyBuilder.Append($"<td>{project.ProjectTitle}</td>");
                                            emailBodyBuilder.Append($@"
<td>
  <a href=""{domain}/ProjectTask/Edit?id={taskDetail.Id}""
     target=""_blank""
     style=""color: #0000EE; text-decoration: underline;"">
     {taskDetail.TaskTitle}
  </a>
</td>");

                                            emailBodyBuilder.Append($@"
<td>
  <div style='display: inline-flex; align-items: center; gap: 6px;'>
    <div style='flex-shrink: 0; width: 10px; height: 10px; border-radius: 50%; background-color: {workflowStatus?.ColorCode ?? ""};'></div>
    <span>{workflowStatus?.StatusName ?? ""}</span>
  </div>
</td>");
                                            var dueDateText = taskDetail.DueDate.HasValue
                                                ? taskDetail.DueDate.Value.ToString("d-MMMM-yyyy")
                                                : "";
                                            emailBodyBuilder.Append($"<td>{dueDateText}</td>");
                                            emailBodyBuilder.Append($"<td>{estimatedTimeFormat}</td>");
                                            emailBodyBuilder.Append($"<td>{spentTime}</td>");
                                            emailBodyBuilder.Append($"<td>{extraTime}</td>");
                                            var deliveryIcon = taskDetail.DeliveryOnTime
          ? "<span style='color: green; font-weight: bold; font-size: 16px;'>&#10004;</span>"
          : "<span style='color:red;'>&#10060;</span>";

                                            emailBodyBuilder.Append($"<td>{deliveryIcon}</td>");
                                            emailBodyBuilder.Append("</tr>");
                                        }
                                    }
                                    emailBodyBuilder.Append("</table>");
                                }
                                if (previousOverdue.Any())
                                {
                                    emailBodyBuilder.Append("<table border='1' cellpadding='5' cellspacing='0' style='width:100%;'>");
                                    emailBodyBuilder.Append("<tr><td colspan='10' style='text-align:center; background-color:#fff4f0; font-weight:bold; font-size:16px;'>Previously Overdue Tasks</td></tr><tr><th>Project</th><th>Task</th><th>Status</th><th>Due Date</th><th>Esti. Time</th><th>Spent Time</th><th>Extra Time</th><th>D.O.T</th></tr>");

                                    foreach (var task in previousOverdue)
                                    {
                                        var project = await _projectsService.GetProjectsByIdAsync(task.ProjectId);
                                        var taskDetail = await _projectTaskService.GetProjectTasksWithoutCacheByIdAsync(task.Id);

                                        if (project != null && taskDetail != null)
                                        {
                                            var spentTime = await _timeSheetsService.ConvertSpentTimeAsync(taskDetail.SpentHours, taskDetail.SpentMinutes);
                                            var estimatedTimeFormat = await _timeSheetsService.ConvertToHHMMFormat(taskDetail.EstimatedTime);
                                            var workflowStatus = await _workflowStatusService.GetWorkflowStatusByIdAsync(taskDetail.StatusId);
                                            int spentTimeInMinutes = (taskDetail.SpentHours * 60) + taskDetail.SpentMinutes;
                                            int estimatedTimeInMinutes = (int)(taskDetail.EstimatedTime * 60);
                                            int extraTimeInMinutes = Math.Max(spentTimeInMinutes - estimatedTimeInMinutes, 0);
                                            int extraHours = extraTimeInMinutes / 60;
                                            int extraMinutes = extraTimeInMinutes % 60;
                                            var extraTime = await _timeSheetsService.ConvertSpentTimeAsync(extraHours, extraMinutes);
                                            string extraPercentage = estimatedTimeInMinutes > 0
                                         ? $"{(int)Math.Round((extraTimeInMinutes * 100.0) / estimatedTimeInMinutes, 0)}%"
                                         : "--";
                                            if (extraTimeInMinutes > 0)
                                                extraTime = $"{extraTime}({extraPercentage})";
                                            emailBodyBuilder.Append("<tr>");
                                            emailBodyBuilder.Append($"<td>{project.ProjectTitle}</td>");
                                            emailBodyBuilder.Append($@"
<td>
  <a href=""{domain}/ProjectTask/Edit?id={taskDetail.Id}""
     target=""_blank""
     style=""color: #0000EE; text-decoration: underline;"">
     {taskDetail.TaskTitle}
  </a>
</td>");

                                            emailBodyBuilder.Append($@"
<td>
  <div style='display: inline-flex; align-items: center; gap: 6px;'>
    <div style='flex-shrink: 0; width: 10px; height: 10px; border-radius: 50%; background-color: {workflowStatus?.ColorCode ?? ""};'></div>
    <span>{workflowStatus?.StatusName ?? ""}</span>
  </div>
</td>");

                                            var dueDateText = taskDetail.DueDate.HasValue
                                                ? taskDetail.DueDate.Value.ToString("d-MMMM-yyyy")
                                                : "";
                                            emailBodyBuilder.Append($"<td>{dueDateText}</td>");
                                            emailBodyBuilder.Append($"<td>{estimatedTimeFormat}</td>");
                                            emailBodyBuilder.Append($"<td>{spentTime}</td>");
                                            emailBodyBuilder.Append($"<td>{extraTime}</td>");
                                            var deliveryIcon = taskDetail.DeliveryOnTime
          ? "<span style='color: green; font-weight: bold; font-size: 16px;'>&#10004;</span>"
          : "<span style='color:red;'>&#10060;</span>";
                                            emailBodyBuilder.Append($"<td>{deliveryIcon}</td>");
                                            emailBodyBuilder.Append("</tr>");
                                        }
                                    }
                                    emailBodyBuilder.Append("</table>");
                                }
                            }
                            var projectIds = await _projectEmployeeMappingService.GetProjectIdsManagedOrCoordinateByEmployeeIdAsync(employee.Id);

                            if (projectIds.Count > 0 && _monthlyReportSetting.SendOverdueReportToProjectLeader)
                            {
                                var employeeIds = await _timeSheetsService.GetEmployeeIdsWorkOnProjects(projectIds);
                                foreach (var id in employeeIds)
                                {
                                    if (id == emp.Id) continue;
                                    employee = await _employeeService.GetEmployeeByIdAsync(id);
                                    if (employee == null) continue;
                                    tasks = await _commonPluginService.GetOverdueTasksByEmployeeIdAsync(id);
                                    if (tasks.Any())
                                    {
                                        emailBodyBuilder.Append("<hr/>");
                                        emailBodyBuilder.Append($"<h3>{employee.FirstName} {employee.LastName}</h3>");
                                        hasOverdueContent = true;
                                        overdueToday = tasks.Where(t => t.DueDate?.Date == today.Date).ToList();
                                        previousOverdue = tasks.Where(t => t.DueDate?.Date < today.Date).ToList();
                                        if (overdueToday.Any())
                                        {
                                            emailBodyBuilder.Append("<table border='1' cellpadding='5' cellspacing='0' style='width:100%;'>");
                                            emailBodyBuilder.Append("<tr><td colspan='10' style='text-align:center; background-color:#f0f4ff; font-weight:bold; font-size:16px;'> Tasks Overdue Today</td></tr><tr><th>Project</th><th>Task</th><th>Status</th><th>Due Date</th><th>Esti. Time</th><th>Spent Time</th><th>Extra Time</th><th>D.O.T</th></tr>");
                                            foreach (var task in overdueToday)
                                            {
                                                var project = await _projectsService.GetProjectsByIdAsync(task.ProjectId);
                                                var taskDetail = await _projectTaskService.GetProjectTasksWithoutCacheByIdAsync(task.Id);

                                                if (project != null && taskDetail != null)
                                                {
                                                    var spentTime = await _timeSheetsService.ConvertSpentTimeAsync(taskDetail.SpentHours, taskDetail.SpentMinutes);
                                                    var estimatedTimeFormat = await _timeSheetsService.ConvertToHHMMFormat(taskDetail.EstimatedTime);

                                                    int spentTimeInMinutes = (taskDetail.SpentHours * 60) + taskDetail.SpentMinutes;
                                                    int estimatedTimeInMinutes = (int)(taskDetail.EstimatedTime * 60);
                                                    var workflowStatus = await _workflowStatusService.GetWorkflowStatusByIdAsync(taskDetail.StatusId);
                                                    int extraTimeInMinutes = Math.Max(spentTimeInMinutes - estimatedTimeInMinutes, 0);
                                                    int extraHours = extraTimeInMinutes / 60;
                                                    int extraMinutes = extraTimeInMinutes % 60;
                                                    var extraTime = await _timeSheetsService.ConvertSpentTimeAsync(extraHours, extraMinutes);
                                                    string extraPercentage = estimatedTimeInMinutes > 0
                                      ? $"{(int)Math.Round((extraTimeInMinutes * 100.0) / estimatedTimeInMinutes, 0)}%"
                                      : "--";
                                                    if (extraTimeInMinutes > 0)
                                                        extraTime = $"{extraTime}({extraPercentage})";
                                                    emailBodyBuilder.Append("<tr>");
                                                    emailBodyBuilder.Append($"<td>{project.ProjectTitle}</td>");
                                                    emailBodyBuilder.Append($@"
<td>
  <a href=""{domain}/ProjectTask/Edit?id={taskDetail.Id}""
     target=""_blank""
     style=""color: #0000EE; text-decoration: underline;"">
     {taskDetail.TaskTitle}
  </a>
</td>");

                                                    emailBodyBuilder.Append($@"
<td>
  <div style='display: inline-flex; align-items: center; gap: 6px;'>
    <div style='flex-shrink: 0; width: 10px; height: 10px; border-radius: 50%; background-color: {workflowStatus?.ColorCode ?? ""};'></div>
    <span>{workflowStatus?.StatusName ?? ""}</span>
  </div>
</td>");
                                                    var dueDateText = taskDetail.DueDate.HasValue
                                                        ? taskDetail.DueDate.Value.ToString("d-MMMM-yyyy")
                                                        : "";
                                                    emailBodyBuilder.Append($"<td>{dueDateText}</td>");
                                                    emailBodyBuilder.Append($"<td>{estimatedTimeFormat}</td>");
                                                    emailBodyBuilder.Append($"<td>{spentTime}</td>");
                                                    emailBodyBuilder.Append($"<td>{extraTime}</td>");
                                                    var deliveryIcon = taskDetail.DeliveryOnTime
                  ? "<span style='color: green; font-weight: bold; font-size: 16px;'>&#10004;</span>"
                  : "<span style='color:red;'>&#10060;</span>";

                                                    emailBodyBuilder.Append($"<td>{deliveryIcon}</td>");
                                                    emailBodyBuilder.Append("</tr>");
                                                }
                                            }
                                            emailBodyBuilder.Append("</table>");
                                        }
                                        if (previousOverdue.Any())
                                        {
                                            emailBodyBuilder.Append("<table border='1' cellpadding='5' cellspacing='0' style='width:100%;'>");
                                            emailBodyBuilder.Append("<tr><td colspan='10' style='text-align:center; background-color:#fff4f0; font-weight:bold; font-size:16px;'>Previously Overdue Tasks</td></tr><tr><th>Project</th><th>Task</th><th>Status</th><th>Due Date</th><th>Esti. Time</th><th>Spent Time</th><th>Extra Time</th><th>D.O.T</th></tr>");
                                            foreach (var task in previousOverdue)
                                            {
                                                var project = await _projectsService.GetProjectsByIdAsync(task.ProjectId);
                                                var taskDetail = await _projectTaskService.GetProjectTasksWithoutCacheByIdAsync(task.Id);

                                                if (project != null && taskDetail != null)
                                                {
                                                    var spentTime = await _timeSheetsService.ConvertSpentTimeAsync(taskDetail.SpentHours, taskDetail.SpentMinutes);
                                                    var estimatedTimeFormat = await _timeSheetsService.ConvertToHHMMFormat(taskDetail.EstimatedTime);
                                                    int spentTimeInMinutes = (taskDetail.SpentHours * 60) + taskDetail.SpentMinutes;
                                                    int estimatedTimeInMinutes = (int)(taskDetail.EstimatedTime * 60);
                                                    var workflowStatus = await _workflowStatusService.GetWorkflowStatusByIdAsync(taskDetail.StatusId);
                                                    int extraTimeInMinutes = Math.Max(spentTimeInMinutes - estimatedTimeInMinutes, 0);
                                                    int extraHours = extraTimeInMinutes / 60;
                                                    int extraMinutes = extraTimeInMinutes % 60;
                                                    var extraTime = await _timeSheetsService.ConvertSpentTimeAsync(extraHours, extraMinutes);
                                                    string extraPercentage = estimatedTimeInMinutes > 0
                                 ? $"{(int)Math.Round((extraTimeInMinutes * 100.0) / estimatedTimeInMinutes, 0)}%"
                                 : "--";
                                                    if (extraTimeInMinutes > 0)
                                                        extraTime = $"{extraTime}({extraPercentage})";
                                                    emailBodyBuilder.Append("<tr>");
                                                    emailBodyBuilder.Append($"<td>{project.ProjectTitle}</td>");
                                                    emailBodyBuilder.Append($@"
<td>
  <a href=""{domain}/ProjectTask/Edit?id={taskDetail.Id}""
     target=""_blank""
     style=""color: #0000EE; text-decoration: underline;"">
     {taskDetail.TaskTitle}
  </a>
</td>");

                                                    emailBodyBuilder.Append($@"
<td>
  <div style='display: inline-flex; align-items: center; gap: 6px;'>
    <div style='flex-shrink: 0; width: 10px; height: 10px; border-radius: 50%; background-color: {workflowStatus?.ColorCode ?? ""};'></div>
    <span>{workflowStatus?.StatusName ?? ""}</span>
  </div>
</td>");
                                                    var dueDateText = taskDetail.DueDate.HasValue
                                                        ? taskDetail.DueDate.Value.ToString("d-MMMM-yyyy")
                                                        : "";
                                                    emailBodyBuilder.Append($"<td>{dueDateText}</td>");
                                                    emailBodyBuilder.Append($"<td>{estimatedTimeFormat}</td>");
                                                    emailBodyBuilder.Append($"<td>{spentTime}</td>");
                                                    emailBodyBuilder.Append($"<td>{extraTime}</td>");
                                                    var deliveryIcon = taskDetail.DeliveryOnTime
                  ? "<span style='color: green; font-weight: bold; font-size: 16px;'>&#10004;</span>"
                  : "<span style='color:red;'>&#10060;</span>";
                                                    emailBodyBuilder.Append($"<td>{deliveryIcon}</td>");
                                                    emailBodyBuilder.Append("</tr>");
                                                }
                                            }
                                            emailBodyBuilder.Append("</table>");
                                        }
                                    }
                                }
                            }
                            emailBodyBuilder.Append("</body></html>");
                            var emailHtmlBody = emailBodyBuilder.ToString();
                            if (!hasOverdueContent)
                                continue;
                            var messageTemplates = await GetActiveMessageTemplatesAsync(MessageTemplateSystemNames.SendOverdueReminderEmail, store.Id);
                            if (!messageTemplates.Any())
                                return new List<int>();
                            string senderEmail = emp.OfficialEmail;
                            string senderName = emp.FirstName + " " + emp.LastName;
                            var commonTokens = new List<Token>
            {
                new Token("TimeSheetReminder.SenderEmail", senderEmail),
                new Token("TimeSheetReminder.SenderName", senderName)
            };
                            await messageTemplates.SelectAwait(async messageTemplate =>
                            {
                                var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId);
                                var tokens = new List<Token>(commonTokens);
                                await _messageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount);
                                string fromEmail = emailAccount.Email;
                                string fromName = emailAccount.DisplayName;
                                tokens.Add(new Token("Email.Body", emailHtmlBody, true));
                                var cCEmailsSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                                if (previousOverdue.Count >= _monthlyReportSetting.OverdueCountCCThreshold)
                                {
                                    if (_monthlyReportSetting.IncludeHRInCC)
                                    {
                                        var hrEmployees = await _employeeService.GetAllHREmployees();
                                        foreach (var hr in hrEmployees)
                                        {
                                            cCEmailsSet.Add(hr.OfficialEmail.Trim());
                                        }
                                    }
                                    if (_monthlyReportSetting.IncludeProjectManagerInCC)
                                    {
                                        var projectManagers = await _projectEmployeeMappingService.GetProjectManagersByEmployeeIdAsync(emp.Id);
                                        foreach (var projectManager in projectManagers)
                                        {
                                            var projectManagerEmployee = await _employeeService.GetEmployeeByIdAsync(projectManager);
                                            if (projectManagerEmployee != null)
                                            {
                                                cCEmailsSet.Add(projectManagerEmployee.OfficialEmail.Trim());
                                            }
                                        }
                                    }
                                    if (_monthlyReportSetting.IncludeProjectLeadersInCC)
                                    {
                                        foreach (var overdue in previousOverdue)
                                        {
                                            var projectLeader = await _projectsService.GetProjectLeaderIdByIdAsync(overdue.ProjectId);
                                            var projectLeaderEmployee = await _employeeService.GetEmployeeByIdAsync(projectLeader);
                                            if (projectLeaderEmployee != null)
                                            {
                                                cCEmailsSet.Add(projectLeaderEmployee.OfficialEmail.Trim());
                                            }
                                        }
                                    }            
                                    if (_monthlyReportSetting.IncludeManagementInCC)
                                    {
                                        foreach (var overdue in previousOverdue)
                                        {
                                            var managerId = await _projectsService.GetProjectManagerIdByIdAsync(overdue.ProjectId);
                                            var manager = await _employeeService.GetEmployeeByIdAsync(managerId);
                                            if (manager != null)
                                                cCEmailsSet.Add(manager.OfficialEmail.Trim());
                                        }
                                    }

                                    if (_monthlyReportSetting.IncludeProjectCoordinatorInCC)
                                    {
                                        foreach (var overdue in previousOverdue)
                                        {
                                            var coordinatorId = await _projectsService.GetProjectCoordinatorIdByIdAsync(overdue.ProjectId);
                                            var coordinator = await _employeeService.GetEmployeeByIdAsync(coordinatorId);

                                            if (coordinator != null && !string.IsNullOrEmpty(coordinator.OfficialEmail))
                                            {
                                                cCEmailsSet.Add(coordinator.OfficialEmail.Trim());
                                            }
                                            else
                                            {
                                                var projectLeaderId = await _projectsService.GetProjectLeaderIdByIdAsync(overdue.ProjectId);
                                                var projectLeaderEmployee = await _employeeService.GetEmployeeByIdAsync(projectLeaderId);
                                                if (projectLeaderEmployee != null && !string.IsNullOrEmpty(projectLeaderEmployee.OfficialEmail))
                                                {
                                                    cCEmailsSet.Add(projectLeaderEmployee.OfficialEmail.Trim());
                                                }
                                            }
                                        }
                                    }
                                    if (cCEmailsSet.Contains(senderEmail.Trim(), StringComparer.OrdinalIgnoreCase))
                                    {
                                        cCEmailsSet.Remove(senderEmail.Trim());
                                    }
                                }
                                var cCEmails = string.Join(";", cCEmailsSet);
                                await _eventPublisher.MessageTokensAddedAsync(messageTemplate, tokens);
                                var toEmail = senderEmail;
                                var toName = senderName;

                                return await SendNotificationWithCCAsync(messageTemplate, emailAccount, languageId, tokens, toEmail, toName,
                                    fromEmail: fromEmail,
                                    fromName: fromName,
                                    replyToEmailAddress: senderEmail,
                                    replyToName: senderName,
                                    cC: cCEmails);
                            }).ToListAsync();
                        }
                    }
            }
            return new List<int>();
        }

        public virtual async Task<int> SendNotificationWithCCAsync(MessageTemplate messageTemplate,
          EmailAccount emailAccount, int languageId, IList<Token> tokens,
          string toEmailAddress, string toName,
          string attachmentFilePath = null, string attachmentFileName = null,
          string replyToEmailAddress = null, string replyToName = null, string subject = null,
          string fromEmail = null, string fromName = null, string cC = null, DateTime? dontSendBeforeDateUtc = null)
        {
            if (messageTemplate == null)
                throw new ArgumentNullException(nameof(messageTemplate));

            if (emailAccount == null)
                throw new ArgumentNullException(nameof(emailAccount));
            var bcc = await _localizationService.GetLocalizedAsync(messageTemplate, mt => mt.BccEmailAddresses, languageId);
            subject = await _localizationService.GetLocalizedAsync(messageTemplate, mt => mt.Subject, languageId);
            var body = await _localizationService.GetLocalizedAsync(messageTemplate, mt => mt.Body, languageId);
            var subjectReplaced = _tokenizer.Replace(subject, tokens, false);
            var bodyReplaced = _tokenizer.Replace(body, tokens, true);
            toName = CommonHelper.EnsureMaximumLength(toName, 300);
            var email = new QueuedEmail
            {
                Priority = QueuedEmailPriority.High,
                From = !string.IsNullOrEmpty(fromEmail) ? fromEmail : emailAccount.Email,
                FromName = !string.IsNullOrEmpty(fromName) ? fromName : emailAccount.DisplayName,
                To = toEmailAddress,
                ToName = toName,
                ReplyTo = replyToEmailAddress,
                ReplyToName = replyToName,
                CC = cC,
                Bcc = bcc,
                Subject = subjectReplaced,
                Body = bodyReplaced,
                AttachmentFilePath = attachmentFilePath,
                AttachmentFileName = attachmentFileName,
                AttachedDownloadId = messageTemplate.AttachedDownloadId,
                CreatedOnUtc = await _dateTimeHelper.GetUTCAsync(),
                EmailAccountId = emailAccount.Id,
                DontSendBeforeDateUtc = dontSendBeforeDateUtc
            };
            await _queuedEmailService.InsertQueuedEmailAsync(email);
            return email.Id;
        }

        public virtual async Task<IList<int>> SendAnnouncementMessageAsync(
    Announcement announcement, int languageId, IList<int> employeeIds)
        {
            var store = await _storeContext.GetCurrentStoreAsync();
            languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);
            var messageTemplates = await GetActiveMessageTemplatesAsync(MessageTemplateSystemNames.AnnouncementEmail, store.Id);
            if (!messageTemplates.Any())
                return new List<int>();
            var employees = await _employeeService.GetEmployeesByIdsAsync(employeeIds.ToArray());
            if (!employees.Any())
                return new List<int>();
            var emailIds = new List<int>();
            foreach (var employee in employees)
            {
                foreach (var messageTemplate in messageTemplates)
                {
                    // email accountS
                    var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId);
                    var tokens = new List<Token>();
                    await _messageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount);
                    tokens.Add(new Token("Announcement.Title", announcement.Title));
                    tokens.Add(new Token("Announcement.Body", announcement.Message, true));
                    var likeUrl = $"{_webHelper.GetStoreLocation()}Employee/LikeAnnouncement?id={announcement.Id}&employeeId={employee.Id}";
                    var likeLink = $"<a href=\"{likeUrl}\" target=\"_blank\">Like</a>";
                    tokens.Add(new Token("Announcement.LikeUrl", likeUrl, true));
                    string fromEmail = emailAccount.Email;
                    string fromName = emailAccount.DisplayName;
                    var toEmail = employee.OfficialEmail;
                    var toName = employee.FirstName + " " + employee.LastName;
                    await _eventPublisher.MessageTokensAddedAsync(messageTemplate, tokens);
                    var attachments = new List<Attachment>();
                    if (!string.IsNullOrEmpty(announcement.AttachmentPath) && File.Exists(announcement.AttachmentPath))
                    {
                        var attachment = new Attachment(announcement.AttachmentPath);
                        if (!string.IsNullOrEmpty(announcement.AttachmentName))
                            attachment.Name = announcement.AttachmentName;
                        attachments.Add(attachment);
                    }

                    var emailId = await SendNotificationWithCCAsync(
               messageTemplate,
               emailAccount,
               languageId,
               tokens,
               toEmail,
               toName,
               attachmentFilePath: announcement.AttachmentPath,
               attachmentFileName: announcement.AttachmentName,
               fromEmail: fromEmail,
               fromName: fromName,
               replyToEmailAddress: toEmail,
               replyToName: toName,
               cC: "",
               dontSendBeforeDateUtc: announcement.ScheduledOnUtc
           );

                    if (emailId > 0)
                        emailIds.Add(emailId);
                }
            }
            return emailIds;
        }
        #endregion
    }
}