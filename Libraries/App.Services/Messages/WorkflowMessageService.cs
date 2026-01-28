using App.Core;
using App.Core.Domain.Blogs;
using App.Core.Domain.Common;
using App.Core.Domain.Customers;
using App.Core.Domain.Employees;
using App.Core.Domain.Extension.Leaves;
using App.Core.Domain.Extension.PerformanceMeasurements;
using App.Core.Domain.Extension.TimeSheets;
using App.Core.Domain.Extension.WeeklyQuestions;
using App.Core.Domain.Forums;
using App.Core.Domain.Holidays;
using App.Core.Domain.ManageResumes;
using App.Core.Domain.Messages;
using App.Core.Domain.News;
using App.Core.Domain.result;
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
using App.Services.PerformanceMeasurements;
using App.Services.ProjectEmployeeMappings;
using App.Services.Projects;
using App.Services.ProjectTasks;
using App.Services.Stores;
using App.Services.TimeSheets;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace App.Services.Messages
{
    /// <summary>
    /// Workflow message service
    /// </summary>
    public partial class WorkflowMessageService : IWorkflowMessageService
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
 

        #endregion

        #region Ctor

        public WorkflowMessageService(CommonSettings commonSettings,
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

            )
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
            //get message templates by the name
            var messageTemplates = await _messageTemplateService.GetMessageTemplatesByNameAsync(messageTemplateName, storeId);

            //no template found
            if (!messageTemplates?.Any() ?? true)
                return new List<MessageTemplate>();

            //filter active templates
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
        protected virtual async Task<EmailAccount> GetEmailAccountOfMessageTemplateAsync(MessageTemplate messageTemplate, int languageId)
        {
            var emailAccountId = await _localizationService.GetLocalizedAsync(messageTemplate, mt => mt.EmailAccountId, languageId);
            //some 0 validation (for localizable "Email account" dropdownlist which saves 0 if "Standard" value is chosen)
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
            //load language by specified ID
            var language = await _languageService.GetLanguageByIdAsync(languageId);

            if (language == null || !language.Published)
            {
                //load any language from the specified store
                language = (await _languageService.GetAllLanguagesAsync(storeId: storeId)).FirstOrDefault();
            }

            if (language == null || !language.Published)
            {
                //load any language
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
            var employee = await _employeeService.GetEmployeeByIdAsync(employeeId);
            if (employee == null)
                return new List<string>();

            // Calculate the start and end dates
            var potentialStartDate = (await _dateTimeHelper.GetUTCAsync())
                                        .AddDays(-considerBeforeDays).Date;

            //  Ensure startDate is not before employee joined
            var startDate = employee.DateofJoining.Date > potentialStartDate
                ? employee.DateofJoining.Date
                : potentialStartDate;

            var endDate = (await _dateTimeHelper.GetUTCAsync()).AddDays(-1).Date;

            // Get leaves for the given employee within the range
            var employeeAttendance = await _employeeAttendanceService.GetAllEmployeeAttendanceAsync(employeeId, startDate, endDate, 3);
            var leaveDates = employeeAttendance.Select(a => a.CheckIn.Date).ToList();

            // Generate the range of dates
            var allDates = Enumerable.Range(0, (endDate - startDate).Days + 1)
                                     .Select(offset => startDate.AddDays(offset))
                                     .ToList();

            // Get all spent dates for the given employee within the range
            var existingDates = await _timeSheetRepository.Table
                .Where(t => t.EmployeeId == employeeId && t.SpentDate >= startDate && t.SpentDate <= endDate)
                .Select(t => t.SpentDate.Date) // Extract only date part
                .Distinct()
                .ToListAsync();

            // Exclude weekends, holidays, and leave dates
            var excludedDates = allDates
                .Where(date => date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday ||
                               holidayList.Any(h => h.Date.Date == date) ||
                               leaveDates.Contains(date)) // Exclude leave dates
                .ToList();

            // Find missing dates
            var missingDates = allDates.Except(existingDates.Concat(excludedDates))
                                       .Select(d => d.ToString("d-MMM-yyyy")) // Format dates to string
                                       .ToList();

            return missingDates;
        }


        #endregion

        #region Methods

        #region Customer workflow

        /// <summary>
        /// Sends 'New customer' notification message to a store owner
        /// </summary>
        /// <param name="customer">Customer instance</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the queued email identifier
        /// </returns>
        public virtual async Task<IList<int>> SendCustomerRegisteredStoreOwnerNotificationMessageAsync(Customer customer, int languageId)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            var store = await _storeContext.GetCurrentStoreAsync();
            languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);

            var messageTemplates = await GetActiveMessageTemplatesAsync(MessageTemplateSystemNames.CustomerRegisteredStoreOwnerNotification, store.Id);
            if (!messageTemplates.Any())
                return new List<int>();

            //tokens
            var commonTokens = new List<Token>();
            await _messageTokenProvider.AddCustomerTokensAsync(commonTokens, customer);

            return await messageTemplates.SelectAwait(async messageTemplate =>
            {
                //email account
                var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId);

                var tokens = new List<Token>(commonTokens);
                await _messageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount);

                //event notification
                await _eventPublisher.MessageTokensAddedAsync(messageTemplate, tokens);

                var (toEmail, toName) = await GetStoreOwnerNameAndEmailAsync(emailAccount);

                return await SendNotificationAsync(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
            }).ToListAsync();
        }

        /// <summary>
        /// Sends a welcome message to a customer
        /// </summary>
        /// <param name="customer">Customer instance</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the queued email identifier
        /// </returns>
        public virtual async Task<IList<int>> SendCustomerWelcomeMessageAsync(Customer customer, int languageId)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            var store = await _storeContext.GetCurrentStoreAsync();
            languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);

            var messageTemplates = await GetActiveMessageTemplatesAsync(MessageTemplateSystemNames.CustomerWelcomeMessage, store.Id);
            if (!messageTemplates.Any())
                return new List<int>();

            //tokens
            var commonTokens = new List<Token>();
            await _messageTokenProvider.AddCustomerTokensAsync(commonTokens, customer);

            return await messageTemplates.SelectAwait(async messageTemplate =>
            {
                //email account
                var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId);

                var tokens = new List<Token>(commonTokens);
                await _messageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount);

                //event notification
                await _eventPublisher.MessageTokensAddedAsync(messageTemplate, tokens);

                var toEmail = customer.Email;
                var toName = await _customerService.GetCustomerFullNameAsync(customer);

                return await SendNotificationAsync(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
            }).ToListAsync();
        }

        /// <summary>
        /// Sends an email validation message to a customer
        /// </summary>
        /// <param name="customer">Customer instance</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the queued email identifier
        /// </returns>
        public virtual async Task<IList<int>> SendCustomerEmailValidationMessageAsync(Customer customer, int languageId)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            var store = await _storeContext.GetCurrentStoreAsync();
            languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);

            var messageTemplates = await GetActiveMessageTemplatesAsync(MessageTemplateSystemNames.CustomerEmailValidationMessage, store.Id);
            if (!messageTemplates.Any())
                return new List<int>();

            //tokens
            var commonTokens = new List<Token>();
            await _messageTokenProvider.AddCustomerTokensAsync(commonTokens, customer);

            return await messageTemplates.SelectAwait(async messageTemplate =>
            {
                //email account
                var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId);

                var tokens = new List<Token>(commonTokens);
                await _messageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount);

                //event notification
                await _eventPublisher.MessageTokensAddedAsync(messageTemplate, tokens);

                var toEmail = customer.Email;
                var toName = await _customerService.GetCustomerFullNameAsync(customer);

                return await SendNotificationAsync(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
            }).ToListAsync();
        }

        /// <summary>
        /// Sends an email re-validation message to a customer
        /// </summary>
        /// <param name="customer">Customer instance</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the queued email identifier
        /// </returns>
        public virtual async Task<IList<int>> SendCustomerEmailRevalidationMessageAsync(Customer customer, int languageId)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            var store = await _storeContext.GetCurrentStoreAsync();
            languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);

            var messageTemplates = await GetActiveMessageTemplatesAsync(MessageTemplateSystemNames.CustomerEmailRevalidationMessage, store.Id);
            if (!messageTemplates.Any())
                return new List<int>();

            //tokens
            var commonTokens = new List<Token>();
            await _messageTokenProvider.AddCustomerTokensAsync(commonTokens, customer);

            return await messageTemplates.SelectAwait(async messageTemplate =>
            {
                //email account
                var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId);

                var tokens = new List<Token>(commonTokens);
                await _messageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount);

                //event notification
                await _eventPublisher.MessageTokensAddedAsync(messageTemplate, tokens);

                //email to re-validate
                var toEmail = customer.EmailToRevalidate;
                var toName = await _customerService.GetCustomerFullNameAsync(customer);

                return await SendNotificationAsync(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
            }).ToListAsync();
        }

        /// <summary>
        /// Sends password recovery message to a customer
        /// </summary>
        /// <param name="customer">Customer instance</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the queued email identifier
        /// </returns>
        public virtual async Task<IList<int>> SendCustomerPasswordRecoveryMessageAsync(Customer customer, int languageId)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            var store = await _storeContext.GetCurrentStoreAsync();
            languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);

            var messageTemplates = await GetActiveMessageTemplatesAsync(MessageTemplateSystemNames.CustomerPasswordRecoveryMessage, store.Id);
            if (!messageTemplates.Any())
                return new List<int>();

            //tokens
            var commonTokens = new List<Token>();
            await _messageTokenProvider.AddCustomerTokensAsync(commonTokens, customer);

            return await messageTemplates.SelectAwait(async messageTemplate =>
            {
                //email account
                var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId);

                var tokens = new List<Token>(commonTokens);
                await _messageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount);

                //event notification
                await _eventPublisher.MessageTokensAddedAsync(messageTemplate, tokens);

                var toEmail = customer.Email;
                var toName = await _customerService.GetCustomerFullNameAsync(customer);

                return await SendNotificationAsync(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
            }).ToListAsync();
        }

        #endregion

        #region Newsletter workflow

        /// <summary>
        /// Sends a newsletter subscription activation message
        /// </summary>
        /// <param name="subscription">Newsletter subscription</param>
        /// <param name="languageId">Language identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the queued email identifier
        /// </returns>
        public virtual async Task<IList<int>> SendNewsLetterSubscriptionActivationMessageAsync(NewsLetterSubscription subscription, int languageId)
        {
            if (subscription == null)
                throw new ArgumentNullException(nameof(subscription));

            var store = await _storeContext.GetCurrentStoreAsync();
            languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);

            var messageTemplates = await GetActiveMessageTemplatesAsync(MessageTemplateSystemNames.NewsletterSubscriptionActivationMessage, store.Id);
            if (!messageTemplates.Any())
                return new List<int>();

            //tokens
            var commonTokens = new List<Token>();
            await _messageTokenProvider.AddNewsLetterSubscriptionTokensAsync(commonTokens, subscription);

            return await messageTemplates.SelectAwait(async messageTemplate =>
            {
                //email account
                var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId);

                var tokens = new List<Token>(commonTokens);
                await _messageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount);

                //event notification
                await _eventPublisher.MessageTokensAddedAsync(messageTemplate, tokens);

                return await SendNotificationAsync(messageTemplate, emailAccount, languageId, tokens, subscription.Email, string.Empty);
            }).ToListAsync();
        }

        /// <summary>
        /// Sends a newsletter subscription deactivation message
        /// </summary>
        /// <param name="subscription">Newsletter subscription</param>
        /// <param name="languageId">Language identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the queued email identifier
        /// </returns>
        public virtual async Task<IList<int>> SendNewsLetterSubscriptionDeactivationMessageAsync(NewsLetterSubscription subscription, int languageId)
        {
            if (subscription == null)
                throw new ArgumentNullException(nameof(subscription));

            var store = await _storeContext.GetCurrentStoreAsync();
            languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);

            var messageTemplates = await GetActiveMessageTemplatesAsync(MessageTemplateSystemNames.NewsletterSubscriptionDeactivationMessage, store.Id);
            if (!messageTemplates.Any())
                return new List<int>();

            //tokens
            var commonTokens = new List<Token>();
            await _messageTokenProvider.AddNewsLetterSubscriptionTokensAsync(commonTokens, subscription);

            return await messageTemplates.SelectAwait(async messageTemplate =>
            {
                //email account
                var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId);

                var tokens = new List<Token>(commonTokens);
                await _messageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount);

                //event notification
                await _eventPublisher.MessageTokensAddedAsync(messageTemplate, tokens);

                return await SendNotificationAsync(messageTemplate, emailAccount, languageId, tokens, subscription.Email, string.Empty);
            }).ToListAsync();
        }

        #endregion

        #region Send a message to a friend

        /// <summary>
        /// Sends wishlist "email a friend" message
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="languageId">Message language identifier</param>
        /// <param name="customerEmail">Customer's email</param>
        /// <param name="friendsEmail">Friend's email</param>
        /// <param name="personalMessage">Personal message</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the queued email identifier
        /// </returns>
        public virtual async Task<IList<int>> SendWishlistEmailAFriendMessageAsync(Customer customer, int languageId,
             string customerEmail, string friendsEmail, string personalMessage)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            var store = await _storeContext.GetCurrentStoreAsync();
            languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);

            var messageTemplates = await GetActiveMessageTemplatesAsync(MessageTemplateSystemNames.WishlistToFriendMessage, store.Id);
            if (!messageTemplates.Any())
                return new List<int>();

            //tokens
            var commonTokens = new List<Token>();
            await _messageTokenProvider.AddCustomerTokensAsync(commonTokens, customer);
            commonTokens.Add(new Token("Wishlist.PersonalMessage", personalMessage, true));
            commonTokens.Add(new Token("Wishlist.Email", customerEmail));

            return await messageTemplates.SelectAwait(async messageTemplate =>
            {
                //email account
                var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId);

                var tokens = new List<Token>(commonTokens);
                await _messageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount);

                //event notification
                await _eventPublisher.MessageTokensAddedAsync(messageTemplate, tokens);

                return await SendNotificationAsync(messageTemplate, emailAccount, languageId, tokens, friendsEmail, string.Empty);
            }).ToListAsync();
        }

        #endregion

        #region Forum Notifications

        /// <summary>
        /// Sends a forum subscription message to a customer
        /// </summary>
        /// <param name="customer">Customer instance</param>
        /// <param name="forumTopic">Forum Topic</param>
        /// <param name="forum">Forum</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the queued email identifier
        /// </returns>
        public virtual async Task<IList<int>> SendNewForumTopicMessageAsync(Customer customer, ForumTopic forumTopic, Forum forum, int languageId)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            var store = await _storeContext.GetCurrentStoreAsync();

            var messageTemplates = await GetActiveMessageTemplatesAsync(MessageTemplateSystemNames.NewForumTopicMessage, store.Id);
            if (!messageTemplates.Any())
                return new List<int>();

            //tokens
            var commonTokens = new List<Token>();
            await _messageTokenProvider.AddForumTopicTokensAsync(commonTokens, forumTopic);
            await _messageTokenProvider.AddForumTokensAsync(commonTokens, forum);
            await _messageTokenProvider.AddCustomerTokensAsync(commonTokens, customer);

            return await messageTemplates.SelectAwait(async messageTemplate =>
            {
                //email account
                var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId);

                var tokens = new List<Token>(commonTokens);
                await _messageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount);

                //event notification
                await _eventPublisher.MessageTokensAddedAsync(messageTemplate, tokens);

                var toEmail = customer.Email;
                var toName = await _customerService.GetCustomerFullNameAsync(customer);

                return await SendNotificationAsync(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
            }).ToListAsync();
        }

        /// <summary>
        /// Sends a forum subscription message to a customer
        /// </summary>
        /// <param name="customer">Customer instance</param>
        /// <param name="forumPost">Forum post</param>
        /// <param name="forumTopic">Forum Topic</param>
        /// <param name="forum">Forum</param>
        /// <param name="friendlyForumTopicPageIndex">Friendly (starts with 1) forum topic page to use for URL generation</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the queued email identifier
        /// </returns>
        public virtual async Task<IList<int>> SendNewForumPostMessageAsync(Customer customer, ForumPost forumPost, ForumTopic forumTopic,
            Forum forum, int friendlyForumTopicPageIndex, int languageId)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            var store = await _storeContext.GetCurrentStoreAsync();

            var messageTemplates = await GetActiveMessageTemplatesAsync(MessageTemplateSystemNames.NewForumPostMessage, store.Id);
            if (!messageTemplates.Any())
                return new List<int>();

            //tokens
            var commonTokens = new List<Token>();
            await _messageTokenProvider.AddForumPostTokensAsync(commonTokens, forumPost);
            await _messageTokenProvider.AddForumTopicTokensAsync(commonTokens, forumTopic, friendlyForumTopicPageIndex, forumPost.Id);
            await _messageTokenProvider.AddForumTokensAsync(commonTokens, forum);
            await _messageTokenProvider.AddCustomerTokensAsync(commonTokens, customer);

            return await messageTemplates.SelectAwait(async messageTemplate =>
            {
                //email account
                var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId);

                var tokens = new List<Token>(commonTokens);
                await _messageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount);

                //event notification
                await _eventPublisher.MessageTokensAddedAsync(messageTemplate, tokens);

                var toEmail = customer.Email;
                var toName = await _customerService.GetCustomerFullNameAsync(customer);

                return await SendNotificationAsync(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
            }).ToListAsync();
        }

        /// <summary>
        /// Sends a private message notification
        /// </summary>
        /// <param name="privateMessage">Private message</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the queued email identifier
        /// </returns>
        public virtual async Task<IList<int>> SendPrivateMessageNotificationAsync(PrivateMessage privateMessage, int languageId)
        {
            if (privateMessage == null)
                throw new ArgumentNullException(nameof(privateMessage));

            var store = await _storeService.GetStoreByIdAsync(privateMessage.StoreId) ?? await _storeContext.GetCurrentStoreAsync();

            var messageTemplates = await GetActiveMessageTemplatesAsync(MessageTemplateSystemNames.PrivateMessageNotification, store.Id);
            if (!messageTemplates.Any())
                return new List<int>();

            //tokens
            var commonTokens = new List<Token>();
            await _messageTokenProvider.AddPrivateMessageTokensAsync(commonTokens, privateMessage);
            await _messageTokenProvider.AddCustomerTokensAsync(commonTokens, privateMessage.ToCustomerId);

            return await messageTemplates.SelectAwait(async messageTemplate =>
            {
                //email account
                var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId);

                var tokens = new List<Token>(commonTokens);
                await _messageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount);

                //event notification
                await _eventPublisher.MessageTokensAddedAsync(messageTemplate, tokens);

                var customer = await _customerService.GetCustomerByIdAsync(privateMessage.ToCustomerId);
                var toEmail = customer.Email;
                var toName = await _customerService.GetCustomerFullNameAsync(customer);

                return await SendNotificationAsync(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
            }).ToListAsync();
        }

        #endregion

        #region Misc

        /// <summary>
        /// Sends a blog comment notification message to a store owner
        /// </summary>
        /// <param name="blogComment">Blog comment</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the list of queued email identifiers
        /// </returns>
        public virtual async Task<IList<int>> SendBlogCommentStoreOwnerNotificationMessageAsync(BlogComment blogComment, int languageId)
        {
            if (blogComment == null)
                throw new ArgumentNullException(nameof(blogComment));

            var store = await _storeContext.GetCurrentStoreAsync();
            languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);

            var messageTemplates = await GetActiveMessageTemplatesAsync(MessageTemplateSystemNames.BlogCommentStoreOwnerNotification, store.Id);
            if (!messageTemplates.Any())
                return new List<int>();

            //tokens
            var commonTokens = new List<Token>();
            await _messageTokenProvider.AddBlogCommentTokensAsync(commonTokens, blogComment);
            await _messageTokenProvider.AddCustomerTokensAsync(commonTokens, blogComment.CustomerId);

            return await messageTemplates.SelectAwait(async messageTemplate =>
            {
                //email account
                var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId);

                var tokens = new List<Token>(commonTokens);
                await _messageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount);

                //event notification
                await _eventPublisher.MessageTokensAddedAsync(messageTemplate, tokens);

                var (toEmail, toName) = await GetStoreOwnerNameAndEmailAsync(emailAccount);

                return await SendNotificationAsync(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
            }).ToListAsync();
        }

        /// <summary>
        /// Sends a news comment notification message to a store owner
        /// </summary>
        /// <param name="newsComment">News comment</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the queued email identifier
        /// </returns>
        public virtual async Task<IList<int>> SendNewsCommentStoreOwnerNotificationMessageAsync(NewsComment newsComment, int languageId)
        {
            if (newsComment == null)
                throw new ArgumentNullException(nameof(newsComment));

            var store = await _storeContext.GetCurrentStoreAsync();
            languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);

            var messageTemplates = await GetActiveMessageTemplatesAsync(MessageTemplateSystemNames.NewsCommentStoreOwnerNotification, store.Id);
            if (!messageTemplates.Any())
                return new List<int>();

            //tokens
            var commonTokens = new List<Token>();
            await _messageTokenProvider.AddNewsCommentTokensAsync(commonTokens, newsComment);
            await _messageTokenProvider.AddCustomerTokensAsync(commonTokens, newsComment.CustomerId);

            return await messageTemplates.SelectAwait(async messageTemplate =>
            {
                //email account
                var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId);

                var tokens = new List<Token>(commonTokens);
                await _messageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount);

                //event notification
                await _eventPublisher.MessageTokensAddedAsync(messageTemplate, tokens);

                var (toEmail, toName) = await GetStoreOwnerNameAndEmailAsync(emailAccount);

                return await SendNotificationAsync(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
            }).ToListAsync();
        }

        /// <summary>
        /// Sends "contact us" message
        /// </summary>
        /// <param name="languageId">Message language identifier</param>
        /// <param name="senderEmail">Sender email</param>
        /// <param name="senderName">Sender name</param>
        /// <param name="subject">Email subject. Pass null if you want a message template subject to be used.</param>
        /// <param name="body">Email body</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the queued email identifier
        /// </returns>
        public virtual async Task<IList<int>> SendContactUsMessageAsync(int languageId, string senderEmail,
            string senderName, string subject, string body)
        {
            var store = await _storeContext.GetCurrentStoreAsync();
            languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);

            var messageTemplates = await GetActiveMessageTemplatesAsync(MessageTemplateSystemNames.ContactUsMessage, store.Id);
            if (!messageTemplates.Any())
                return new List<int>();

            //tokens
            var commonTokens = new List<Token>
            {
                new Token("ContactUs.SenderEmail", senderEmail),
                new Token("ContactUs.SenderName", senderName)
            };

            return await messageTemplates.SelectAwait(async messageTemplate =>
            {
                //email account
                var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId);

                var tokens = new List<Token>(commonTokens);
                await _messageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount);

                string fromEmail;
                string fromName;
                //required for some SMTP servers
                if (_commonSettings.UseSystemEmailForContactUsForm)
                {
                    fromEmail = emailAccount.Email;
                    fromName = emailAccount.DisplayName;
                    body = $"<strong>From</strong>: {WebUtility.HtmlEncode(senderName)} - {WebUtility.HtmlEncode(senderEmail)}<br /><br />{body}";
                }
                else
                {
                    fromEmail = senderEmail;
                    fromName = senderName;
                }

                tokens.Add(new Token("ContactUs.Body", body, true));

                //event notification
                await _eventPublisher.MessageTokensAddedAsync(messageTemplate, tokens);

                var toEmail = emailAccount.Email;
                var toName = emailAccount.DisplayName;

                return await SendNotificationAsync(messageTemplate, emailAccount, languageId, tokens, toEmail, toName,
                    fromEmail: fromEmail,
                    fromName: fromName,
                    subject: subject,
                    replyToEmailAddress: senderEmail,
                    replyToName: senderName);
            }).ToListAsync();
        }

        /// <summary>
        /// Sends a test email
        /// </summary>
        /// <param name="messageTemplateId">Message template identifier</param>
        /// <param name="sendToEmail">Send to email</param>
        /// <param name="tokens">Tokens</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the queued email identifier
        /// </returns>
        public virtual async Task<int> SendTestEmailAsync(int messageTemplateId, string sendToEmail, List<Token> tokens, int languageId)
        {
            var messageTemplate = await _messageTemplateService.GetMessageTemplateByIdAsync(messageTemplateId);
            if (messageTemplate == null)
                throw new ArgumentException("Template cannot be loaded");

            //email account
            var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId);

            //event notification
            await _eventPublisher.MessageTokensAddedAsync(messageTemplate, tokens);

            //force sending
            messageTemplate.DelayBeforeSend = null;

            return await SendNotificationAsync(messageTemplate, emailAccount, languageId, tokens, sendToEmail, null);
        }

        public virtual async Task<IList<int>> SendWelcomeMessageAsync(int languageId, string senderEmail, string senderName, int employeeId)
        {
            var store = await _storeContext.GetCurrentStoreAsync();
            languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);

            var messageTemplates = await GetActiveMessageTemplatesAsync(MessageTemplateSystemNames.SendWelcomeEmployeeMail, store.Id);
            if (!messageTemplates.Any())
                return new List<int>();

            var employee = await _employeeService.GetEmployeeByIdAsync(employeeId);
            string employeeName = "";
            if (employee != null)
                employeeName = employee.FirstName + " " + employee.LastName;



            //tokens
            var commonTokens = new List<Token>
    {
        new Token("Welcome.SenderEmail", senderEmail),
        new Token("Welcome.SenderName", senderName)
    };

            var cCEmailsSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // Add HR email
            if (!string.IsNullOrEmpty(_leaveSettings.HrEmail))
            {
                cCEmailsSet.Add(_leaveSettings.HrEmail.Trim());
            }

            var cCEmails = string.Join(";", cCEmailsSet);

            return await messageTemplates.SelectAwait(async messageTemplate =>
            {
                // email account
                var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId);

                var tokens = new List<Token>(commonTokens);
                await _messageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount);

                string fromEmail = emailAccount.Email;
                string fromName = emailAccount.DisplayName;

                tokens.Add(new Token("Employee.Name", employeeName, true));



                // event notification
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

        public virtual async Task<IList<int>> SendLeaveApprovedMessageAsync(int languageId, string senderEmail, string senderName, int employeeId, int leaveId, IList<int> selectedEmployee = null)
        {
            var store = await _storeContext.GetCurrentStoreAsync();
            languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);

            var messageTemplates = await GetActiveMessageTemplatesAsync(MessageTemplateSystemNames.SendLeaveapprovalMail, store.Id);
            if (!messageTemplates.Any())
                return new List<int>();

            var employee = await _employeeService.GetEmployeeByIdAsync(employeeId);
            var leave = await _leaveManagementService.GetLeaveManagementByIdAsync(leaveId);
            string employeeName = employee.FirstName + " " + employee.LastName;
            string NoOfDays = leave.NoOfDays.ToString("0.0");
            string FromDate = leave.From.ToString("dd MMMM yyyy");
            string ToDate = leave.To.ToString("dd MMMM yyyy");
            string leaveReason = leave.ReasonForLeave.ToString();
            var leaveType = await _leaveTypeService.GetLeaveTypeByIdAsync(leave.LeaveTypeId);
            string leaveTypeName = leaveType.Type.ToString();

            //tokens
            var commonTokens = new List<Token>
    {
        new Token("LeaveManagement.SenderEmail", senderEmail),
        new Token("LeaveManagement.SenderName", senderName)
    };

            var cCEmailsSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (selectedEmployee != null)
                foreach (var empId in selectedEmployee)
                {
                    var emp = await _employeeService.GetEmployeeByIdAsync(empId);
                    if (emp != null)
                        cCEmailsSet.Add(emp.OfficialEmail);

                }

            // Add HR email
            if (!string.IsNullOrEmpty(_leaveSettings.HrEmail))
            {
                cCEmailsSet.Add(_leaveSettings.HrEmail.Trim());
            }

            // Add common emails
            if (!string.IsNullOrEmpty(_leaveSettings.CommonEmails))
            {
                foreach (var email in _leaveSettings.CommonEmails.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    cCEmailsSet.Add(email.Trim());
                }
            }

            //// Add project leaders' emails
            //if (_leaveSettings.SendEmailToAllProjectLeaders)
            //{
            //    var projectLeaders = await _projectEmployeeMappingService.GetProjectLeadersByEmployeeIdAsync(employeeId);
            //    foreach (var projectLeader in projectLeaders)
            //    {
            //        var projectLeaderEmployee = await _employeeService.GetEmployeeByIdAsync(projectLeader);
            //        if (projectLeaderEmployee != null)
            //        {
            //            cCEmailsSet.Add(projectLeaderEmployee.OfficialEmail.Trim());
            //        }
            //    }
            //}

            //// Add project managers' emails
            //if (_leaveSettings.SendEmailToAllProjectManager)
            //{
            //    var projectManagers = await _projectEmployeeMappingService.GetProjectManagersByEmployeeIdAsync(employeeId);
            //    foreach (var projectManager in projectManagers)
            //    {
            //        var projectManagerEmployee = await _employeeService.GetEmployeeByIdAsync(projectManager);
            //        if (projectManagerEmployee != null)
            //        {
            //            cCEmailsSet.Add(projectManagerEmployee.OfficialEmail.Trim());
            //        }
            //    }
            //}

            //// Add employee manager's email
            //if (_leaveSettings.SendEmailToEmployeeManager)
            //{
            //    var employeeManager = await _employeeService.GetEmployeeByIdAsync(employee.ManagerId);
            //    if (employeeManager != null)
            //    {
            //        cCEmailsSet.Add(employeeManager.OfficialEmail.Trim());
            //    }
            //}

            // Ensure sender email is not included in CC
            if (cCEmailsSet.Contains(senderEmail.Trim(), StringComparer.OrdinalIgnoreCase))
            {
                cCEmailsSet.Remove(senderEmail.Trim());
            }


            var cCEmails = string.Join(";", cCEmailsSet);

            return await messageTemplates.SelectAwait(async messageTemplate =>
            {
                // email account
                var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId);

                var tokens = new List<Token>(commonTokens);
                await _messageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount);

                string fromEmail = emailAccount.Email;
                string fromName = emailAccount.DisplayName;

                tokens.Add(new Token("Employee.Name", employeeName, true));
                tokens.Add(new Token("Leave.NoOfDays", NoOfDays, true));
                tokens.Add(new Token("Leave.From", FromDate, true));
                tokens.Add(new Token("Leave.To", ToDate, true));
                tokens.Add(new Token("Leave.Type", leaveTypeName, true));
                tokens.Add(new Token("Leave.Reason", leaveReason, true));


                // event notification
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

        //    public virtual async Task<IList<int>> SendLeaveRequestMessageAsync(int languageId, string senderEmail,
        //string senderName, int employeeId, int leaveId)
        //    {
        //        var store = await _storeContext.GetCurrentStoreAsync();
        //        languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);

        //        var messageTemplates = await GetActiveMessageTemplatesAsync(MessageTemplateSystemNames.SendLeaveRequestMail, store.Id);
        //        if (!messageTemplates.Any())
        //            return new List<int>();

        //        var employee = await _employeeService.GetEmployeeByIdAsync(employeeId);
        //        var leave = await _leaveManagementService.GetLeaveManagementByIdAsync(leaveId);
        //        string employeeName = employee.FirstName + " " + employee.LastName;
        //        string noOfDays = leave.NoOfDays.ToString("0.0");
        //        string fromDate = leave.From.ToString("dd MMMM yyyy");
        //        string toDate = leave.To.ToString("dd MMMM yyyy");
        //        string leaveReason = leave.ReasonForLeave.ToString();
        //        var leaveType = await _leaveTypeService.GetLeaveTypeByIdAsync(leave.LeaveTypeId);
        //        string leaveTypeName = leaveType.Type.ToString();

        //        // Tokens
        //        var commonTokens = new List<Token>
        //{
        //    new Token("LeaveManagement.SenderEmail", senderEmail),
        //    new Token("LeaveManagement.SenderName", senderName)
        //};

        //        var cCEmailsSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        //        // Add HR email
        //        if (!string.IsNullOrEmpty(_leaveSettings.HrEmail))
        //        {
        //            cCEmailsSet.Add(_leaveSettings.HrEmail.Trim());
        //        }

        //        // Add common emails
        //        if (!string.IsNullOrEmpty(_leaveSettings.CommonEmails))
        //        {
        //            foreach (var email in _leaveSettings.CommonEmails.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
        //            {
        //                cCEmailsSet.Add(email.Trim());
        //            }
        //        }

        //        // Add project managers' emails
        //        if (_leaveSettings.SendEmailToAllProjectManager)
        //        {
        //            var projectManagers = await _projectEmployeeMappingService.GetProjectManagersByEmployeeIdAsync(employeeId);
        //            foreach (var projectManager in projectManagers)
        //            {
        //                var projectManagerEmployee = await _employeeService.GetEmployeeByIdAsync(projectManager);
        //                if (projectManagerEmployee != null)
        //                {
        //                    cCEmailsSet.Add(projectManagerEmployee.OfficialEmail.Trim());
        //                }
        //            }
        //        }

        //        // Add project leaders' emails
        //        if (_leaveSettings.SendEmailToAllProjectLeaders)
        //        {
        //            var projectLeaders = await _projectEmployeeMappingService.GetProjectLeadersByEmployeeIdAsync(employeeId);
        //            foreach (var projectLeader in projectLeaders)
        //            {
        //                var projectLeaderEmployee = await _employeeService.GetEmployeeByIdAsync(projectLeader);
        //                if (projectLeaderEmployee != null)
        //                {
        //                    cCEmailsSet.Add(projectLeaderEmployee.OfficialEmail.Trim());
        //                }
        //            }
        //        }

        //        // Add employee manager's email
        //        if (_leaveSettings.SendEmailToEmployeeManager)
        //        {
        //            var employeeManager = await _employeeService.GetEmployeeByIdAsync(employee.ManagerId);
        //            if (employeeManager != null)
        //            {
        //                cCEmailsSet.Add(employeeManager.OfficialEmail.Trim());
        //            }
        //        }

        //        // Ensure sender email is not included in CC
        //        if (cCEmailsSet.Contains(senderEmail.Trim(), StringComparer.OrdinalIgnoreCase))
        //        {
        //            cCEmailsSet.Remove(senderEmail.Trim());
        //        }

        //        var cCEmails = string.Join(";", cCEmailsSet);  // Join the emails into a string

        //        return await messageTemplates.SelectAwait(async messageTemplate =>
        //        {
        //            // Email account
        //            var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId);

        //            var tokens = new List<Token>(commonTokens);
        //            await _messageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount);

        //            string fromEmail = emailAccount.Email;
        //            string fromName = emailAccount.DisplayName;

        //            tokens.Add(new Token("Employee.Name", employeeName, true));
        //            tokens.Add(new Token("Leave.NoOfDays", noOfDays, true));
        //            tokens.Add(new Token("Leave.From", fromDate, true));
        //            tokens.Add(new Token("Leave.To", toDate, true));
        //            tokens.Add(new Token("Leave.Type", leaveTypeName, true));
        //            tokens.Add(new Token("Leave.Reason", leaveReason, true));

        //            // Event notification
        //            await _eventPublisher.MessageTokensAddedAsync(messageTemplate, tokens);

        //            var toEmail = senderEmail;
        //            var toName = senderName;

        //            return await SendNotificationWithCCAsync(messageTemplate, emailAccount, languageId, tokens, toEmail, toName,
        //                fromEmail: fromEmail,
        //                fromName: fromName,
        //                replyToEmailAddress: senderEmail,
        //                replyToName: senderName,
        //                cC: cCEmails);  // Pass the CC emails string
        //        }).ToListAsync();
        //    }


        public virtual async Task<IList<int>> SendLeaveRequestMessageAsync(int languageId, string senderEmail,
   string senderName, int employeeId, int leaveId, IList<int> selectedEmployee = null)
        {
            var store = await _storeContext.GetCurrentStoreAsync();
            languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);

            var messageTemplates = await GetActiveMessageTemplatesAsync(MessageTemplateSystemNames.SendLeaveRequestMail, store.Id);
            if (!messageTemplates.Any())
                return new List<int>();

            var employee = await _employeeService.GetEmployeeByIdAsync(employeeId);
            var leave = await _leaveManagementService.GetLeaveManagementByIdAsync(leaveId);
            string employeeName = employee.FirstName + " " + employee.LastName;
            string noOfDays = leave.NoOfDays.ToString("0.0");
            string fromDate = leave.From.ToString("dd MMMM yyyy");
            string toDate = leave.To.ToString("dd MMMM yyyy");
            string leaveReason = leave.ReasonForLeave.ToString();
            var leaveType = await _leaveTypeService.GetLeaveTypeByIdAsync(leave.LeaveTypeId);
            string leaveTypeName = leaveType.Type.ToString();

            // Tokens
            var commonTokens = new List<Token>
    {
        new Token("LeaveManagement.SenderEmail", senderEmail),
        new Token("LeaveManagement.SenderName", senderName)
    };

            var cCEmailsSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);


            if (selectedEmployee != null)
                foreach (var empId in selectedEmployee)
                {
                    var emp = await _employeeService.GetEmployeeByIdAsync(empId);
                    if (emp != null)
                        cCEmailsSet.Add(emp.OfficialEmail);

                }
            // Add HR email
            if (!string.IsNullOrEmpty(_leaveSettings.HrEmail))
            {
                cCEmailsSet.Add(_leaveSettings.HrEmail.Trim());
            }

            // Add common emails
            if (!string.IsNullOrEmpty(_leaveSettings.CommonEmails))
            {
                foreach (var email in _leaveSettings.CommonEmails.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    cCEmailsSet.Add(email.Trim());
                }
            }

          

            // Ensure sender email is not included in CC
            if (cCEmailsSet.Contains(senderEmail.Trim(), StringComparer.OrdinalIgnoreCase))
            {
                cCEmailsSet.Remove(senderEmail.Trim());
            }

            var cCEmails = string.Join(";", cCEmailsSet);  // Join the emails into a string

            return await messageTemplates.SelectAwait(async messageTemplate =>
            {
                // Email account
                var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId);

                var tokens = new List<Token>(commonTokens);
                await _messageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount);

                string fromEmail = emailAccount.Email;
                string fromName = emailAccount.DisplayName;

                tokens.Add(new Token("Employee.Name", employeeName, true));
                tokens.Add(new Token("Leave.NoOfDays", noOfDays, true));
                tokens.Add(new Token("Leave.From", fromDate, true));
                tokens.Add(new Token("Leave.To", toDate, true));
                tokens.Add(new Token("Leave.Type", leaveTypeName, true));
                tokens.Add(new Token("Leave.Reason", leaveReason, true));

                // Event notification
                await _eventPublisher.MessageTokensAddedAsync(messageTemplate, tokens);

                var toEmail = senderEmail;
                var toName = senderName;

                return await SendNotificationWithCCAsync(messageTemplate, emailAccount, languageId, tokens, toEmail, toName,
                    fromEmail: fromEmail,
                    fromName: fromName,
                    replyToEmailAddress: senderEmail,
                    replyToName: senderName,
                    cC: cCEmails);  // Pass the CC emails string
            }).ToListAsync();
        }




        public virtual async Task<IList<int>> SendLeaveRejectedMessageAsync(int languageId, string senderEmail,
            string senderName, int employeeId, int leaveId, IList<int> selectedEmployee = null)
        {
            var store = await _storeContext.GetCurrentStoreAsync();
            languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);

            var messageTemplates = await GetActiveMessageTemplatesAsync(MessageTemplateSystemNames.SendLeaveRejectMail, store.Id);
            if (!messageTemplates.Any())
                return new List<int>();

            var employee = await _employeeService.GetEmployeeByIdAsync(employeeId);
            var leave = await _leaveManagementService.GetLeaveManagementByIdAsync(leaveId);
            string employeeName = employee.FirstName + " " + employee.LastName;
            string FromDate = leave.From.ToString("dd MMMM yyyy");
            string ToDate = leave.To.ToString("dd MMMM yyyy");
            string NoOfDays = leave.NoOfDays.ToString("0.0");
            string leaveReason = leave.ReasonForLeave.ToString();
            var leaveType = await _leaveTypeService.GetLeaveTypeByIdAsync(leave.LeaveTypeId);
            string leaveTypeName = leaveType.Type.ToString();
            // Tokens
            var commonTokens = new List<Token>
    {
        new Token("LeaveManagement.SenderEmail", senderEmail),
        new Token("LeaveManagement.SenderName", senderName)
    };

            var cCEmailsSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);


            if (selectedEmployee != null)
                foreach (var empId in selectedEmployee)
                {
                    var emp = await _employeeService.GetEmployeeByIdAsync(empId);
                    if (emp != null)
                        cCEmailsSet.Add(emp.OfficialEmail);

                }

            // Add HR email
            if (!string.IsNullOrEmpty(_leaveSettings.HrEmail))
            {
                cCEmailsSet.Add(_leaveSettings.HrEmail.Trim());
            }

            // Add common emails
            if (!string.IsNullOrEmpty(_leaveSettings.CommonEmails))
            {
                foreach (var email in _leaveSettings.CommonEmails.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    cCEmailsSet.Add(email.Trim());
                }
            }



            // Ensure sender email is not included in CC
            if (cCEmailsSet.Contains(senderEmail.Trim(), StringComparer.OrdinalIgnoreCase))
            {
                cCEmailsSet.Remove(senderEmail.Trim());
            }


            var cCEmails = string.Join(";", cCEmailsSet);

            return await messageTemplates.SelectAwait(async messageTemplate =>
            {
                // Email account
                var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId);

                var tokens = new List<Token>(commonTokens);
                await _messageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount);

                string fromEmail = emailAccount.Email;
                string fromName = emailAccount.DisplayName;

                tokens.Add(new Token("Employee.Name", employeeName, true));
                tokens.Add(new Token("Leave.To", ToDate, true));
                tokens.Add(new Token("Leave.From", FromDate, true));
                tokens.Add(new Token("Leave.NoOfDays", NoOfDays, true));
                tokens.Add(new Token("Leave.Type", leaveTypeName, true));
                tokens.Add(new Token("Leave.Reason", leaveReason, true));

                // Event notification
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


        public virtual async Task<IList<int>> SendLeaveCancelledMessageAsync(int languageId, string senderEmail,
            string senderName, int employeeId, int leaveId, IList<int> selectedEmployee = null)
        {
            var store = await _storeContext.GetCurrentStoreAsync();
            languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);

            var messageTemplates = await GetActiveMessageTemplatesAsync(MessageTemplateSystemNames.SendLeaveCancelMail, store.Id);
            if (!messageTemplates.Any())
                return new List<int>();

            var employee = await _employeeService.GetEmployeeByIdAsync(employeeId);
            var leave = await _leaveManagementService.GetLeaveManagementByIdAsync(leaveId);
            string employeeName = employee.FirstName + " " + employee.LastName;
            string FromDate = leave.From.ToString("dd MMMM yyyy");
            string ToDate = leave.To.ToString("dd MMMM yyyy");
            string NoOfDays = leave.NoOfDays.ToString("0.0");
            string leaveReason = leave.ReasonForLeave.ToString();
            var leaveType = await _leaveTypeService.GetLeaveTypeByIdAsync(leave.LeaveTypeId);
            string leaveTypeName = leaveType.Type.ToString();
            // Tokens
            var commonTokens = new List<Token>
    {
        new Token("LeaveManagement.SenderEmail", senderEmail),
        new Token("LeaveManagement.SenderName", senderName)
    };

            var cCEmailsSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);


            if (selectedEmployee != null)
                foreach (var empId in selectedEmployee)
                {
                    var emp = await _employeeService.GetEmployeeByIdAsync(empId);
                    if (emp != null)
                        cCEmailsSet.Add(emp.OfficialEmail);

                }

            //Add HR email
            if (!string.IsNullOrEmpty(_leaveSettings.HrEmail))
            {
                cCEmailsSet.Add(_leaveSettings.HrEmail.Trim());
            }

            // Add common emails
            if (!string.IsNullOrEmpty(_leaveSettings.CommonEmails))
            {
                foreach (var email in _leaveSettings.CommonEmails.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    cCEmailsSet.Add(email.Trim());
                }
            }

            //// Add project managers' emails
            //if (_leaveSettings.SendEmailToEmployeeManager)
            //{
            //    var projectManagers = await _projectEmployeeMappingService.GetProjectManagersByEmployeeIdAsync(employeeId);
            //    foreach (var projectManager in projectManagers)
            //    {
            //        var projectManagerEmployee = await _employeeService.GetEmployeeByIdAsync(projectManager);
            //        if (projectManagerEmployee != null)
            //        {
            //            cCEmailsSet.Add(projectManagerEmployee.OfficialEmail.Trim());
            //        }
            //    }
            //}
            //if (_leaveSettings.SendEmailToAllProjectLeaders)
            //{
            //    var projectLeaders = await _projectEmployeeMappingService.GetProjectLeadersByEmployeeIdAsync(employeeId);
            //    foreach (var projectLeader in projectLeaders)
            //    {
            //        var projectLeaderEmployee = await _employeeService.GetEmployeeByIdAsync(projectLeader);
            //        if (projectLeaderEmployee != null)
            //        {
            //            cCEmailsSet.Add(projectLeaderEmployee.OfficialEmail.Trim());
            //        }
            //    }
            //}
            //// Add employee manager's email
            //if (_leaveSettings.SendEmailToEmployeeManager)
            //{
            //    var employeeManager = await _employeeService.GetEmployeeByIdAsync(employee.ManagerId);
            //    if (employeeManager != null)
            //    {
            //        cCEmailsSet.Add(employeeManager.OfficialEmail.Trim());
            //    }
            //}

            // Ensure sender email is not included in CC
            if (cCEmailsSet.Contains(senderEmail.Trim(), StringComparer.OrdinalIgnoreCase))
            {
                cCEmailsSet.Remove(senderEmail.Trim());
            }


            var cCEmails = string.Join(";", cCEmailsSet);

            return await messageTemplates.SelectAwait(async messageTemplate =>
            {
                // Email account
                var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId);

                var tokens = new List<Token>(commonTokens);
                await _messageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount);

                string fromEmail = emailAccount.Email;
                string fromName = emailAccount.DisplayName;

                tokens.Add(new Token("Employee.Name", employeeName, true));
                tokens.Add(new Token("Leave.To", ToDate, true));
                tokens.Add(new Token("Leave.From", FromDate, true));
                tokens.Add(new Token("Leave.NoOfDays", NoOfDays, true));
                tokens.Add(new Token("Leave.Type", leaveTypeName, true));
                tokens.Add(new Token("Leave.Reason", leaveReason, true));
                // Event notification
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

        public virtual async Task<IList<int>> SendEmployeeOnBordingMessageAsync(int languageId, int employeeId, string onBordingBody)
        {
            var store = await _storeContext.GetCurrentStoreAsync();
            languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);

            var messageTemplates = await GetActiveMessageTemplatesAsync(MessageTemplateSystemNames.SendEmployeeOnBordingEmail, store.Id);
            if (!messageTemplates.Any())
                return new List<int>();

            var employee = await _employeeService.GetEmployeeByIdAsync(employeeId);
            string employeeName = "";
            string senderEmail = "";
            string senderName = "";
            if (employee != null)
            {
                employeeName = employee.FirstName + " " + employee.LastName;
                senderEmail = employee.OfficialEmail;
                senderName = employeeName;
                //tokens

            }
            var commonTokens = new List<Token>
    {
        new Token("TimeSheetReminder.SenderEmail", senderEmail),
        new Token("TimeSheetReminder.SenderName", senderName)
    };
            await messageTemplates.SelectAwait(async messageTemplate =>
                {
                    // email account
                    var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId);

                    var tokens = new List<Token>(commonTokens);
                    await _messageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount);

                    string fromEmail = emailAccount.Email;
                    string fromName = emailAccount.DisplayName;

                    tokens.Add(new Token("EmployeeOnBording.Body", System.Net.WebUtility.HtmlDecode(onBordingBody), true));

                    var cCEmails = "";

                    // event notification
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

            return new List<int>();
        }


        public virtual async Task<IList<int>> SendEmployeeMentionMessageAsync(int languageId, int employeeId, int taskId, string comments)
        {
            var store = await _storeContext.GetCurrentStoreAsync();
            languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);

            var mentionByEmployee = await _employeeService.GetEmployeeByIdAsync(employeeId);
            if (mentionByEmployee == null)
                return new List<int>();

            var mentionEmployees = await _employeeService.GetEmployeesByMention(comments);

            foreach (var mentionEmployee in mentionEmployees)
            {
                if (mentionByEmployee == null)
                    continue;

                var messageTemplates = await GetActiveMessageTemplatesAsync(MessageTemplateSystemNames.SendMentionEmail, store.Id);
                if (!messageTemplates.Any())
                    return new List<int>();

                string mentionBy = mentionByEmployee.FirstName + " " + mentionByEmployee.LastName;
                string employeeName = mentionEmployee.FirstName + " " + mentionEmployee.LastName;
                string senderEmail = mentionEmployee.OfficialEmail;
                string senderName = employeeName;

                // tokens
                var commonTokens = new List<Token>
        {
            new Token("MentionEmail.SenderEmail", senderEmail),
            new Token("MentionEmail.SenderName", senderName)
        };

                await messageTemplates.SelectAwait(async messageTemplate =>
                {
                    // email account
                    var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId);

                    var tokens = new List<Token>(commonTokens);
                    await _messageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount);

                    string fromEmail = emailAccount.Email;
                    string fromName = emailAccount.DisplayName;
                    var request = _httpContextAccessor.HttpContext.Request;
                    string domain = $"{request.Scheme}://{request.Host}";
                    string link = $"<a href='{domain}/ProjectTask/Edit?Id={taskId}'>click here</a>";

                    var task = await _projectTaskService.GetProjectTasksByIdAsync(taskId);

                    tokens.Add(new Token("Task.Id", taskId, true));
                    if (task != null)
                    {
                        tokens.Add(new Token("Task.Title", task.TaskTitle, true));
                        var project = await _projectsService.GetProjectsByIdAsync(task.ProjectId);
                        if (project != null)
                            tokens.Add(new Token("Project.Name", project.ProjectTitle, true));
                    }

                    tokens.Add(new Token("Employee.Name", employeeName, true));

                    //  Process comments safely
                    var formattedComments = comments;

                    // 1. Highlight mentions @<Name>
                    var mentionPattern = @"@<([^>]+)>";
                    formattedComments = Regex.Replace(
                        formattedComments,
                        mentionPattern,
                        m => $"<span style='color:#0366d6; font-weight:bold;'>@{m.Groups[1].Value}</span>"
                    );

                    // 2. Make URLs clickable
                    // 2. Make URLs clickable safely (ignore ones already in <a> tags)
                    var urlPattern = @"(?<!href=['""])(https?:\/\/[^\s<]+)";
                    formattedComments = Regex.Replace(
                        formattedComments,
                        urlPattern,
                        m =>
                        {
                            var url = m.Value;
                            return $"<a href='{url}' target='_blank' style='color:#0366d6; text-decoration:underline;'>{url}</a>";
                        },
                        RegexOptions.IgnoreCase
                    );


                    // 3. Allow only safe <img> tags 
                    formattedComments = Regex.Replace(
                        formattedComments,
                        @"<img[^>]+src=['""](?<src>/content/images/taskcomments/[^'""]+)['""][^>]*>",
                        m =>
                        {
                            var relativeUrl = m.Groups["src"].Value;
                            var absoluteUrl = domain + relativeUrl;

                            // Add safe size styling (max width/height)
                            return $"<img src=\"{absoluteUrl}\" style=\"max-width:400px; height:auto; display:block; margin:5px 0;\" />";
                        },
                        RegexOptions.IgnoreCase
                    );


                    tokens.Add(new Token("Mention.Comments", formattedComments, true));
                    tokens.Add(new Token("Employee.MentionBy", mentionBy, true));
                    tokens.Add(new Token("Mention.Link", link, true));

                    var cCEmails = "";

                    // event notification
                    await _eventPublisher.MessageTokensAddedAsync(messageTemplate, tokens);

                    var toEmail = senderEmail;
                    var toName = senderName;

                    return await SendNotificationWithCCAsync(
                        messageTemplate, emailAccount, languageId, tokens, toEmail, toName,
                        fromEmail: fromEmail,
                        fromName: fromName,
                        replyToEmailAddress: senderEmail,
                        replyToName: senderName,
                        cC: cCEmails
                    );
                }).ToListAsync();
            }

            return new List<int>();
        }


        public virtual async Task<IList<int>> SendTimesheetReminderMessageAsync(int languageId, IList<Employee> employeeList, DateTime date)
        {
            var store = await _storeContext.GetCurrentStoreAsync();
            languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);

            var messageTemplates = await GetActiveMessageTemplatesAsync(MessageTemplateSystemNames.SendTimeSheetReminderMail, store.Id);
            if (!messageTemplates.Any())
                return new List<int>();
            foreach (var employee in employeeList)
            {
                string employeeName = employee.FirstName + " " + employee.LastName;
                string senderEmail = employee.OfficialEmail;
                string senderName = employeeName;
                //tokens
                var commonTokens = new List<Token>
    {
        new Token("TimeSheetReminder.SenderEmail", senderEmail),
        new Token("TimeSheetReminder.SenderName", senderName)
    };
                await messageTemplates.SelectAwait(async messageTemplate =>
                 {
                     // email account
                     var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId);

                     var tokens = new List<Token>(commonTokens);
                     await _messageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount);

                     string fromEmail = emailAccount.Email;
                     string fromName = emailAccount.DisplayName;
                     string link = "<a href='https://portal.satyanamsoft.com/TimeSheet/UpdateTimeSheet'>click here</a>";

                     tokens.Add(new Token("Employee.Name", employeeName, true));
                     tokens.Add(new Token("Timesheet.Date", date.ToString("d-MMM-yyyy"), true));
                     tokens.Add(new Token("TimeSheet.Link", link, true));

                     var cCEmails = "";


                     // event notification
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
            return new List<int>();
        }


        public virtual async Task<IList<int>> SendWeeklyReportMessageAsync(int languageId)
        {
            var store = await _storeContext.GetCurrentStoreAsync();
            languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);

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

                            if (employee.DesignationId != managerRole && employee.DesignationId != hrRole)
                            {
                                var totalWorking = await _timeSheetsService.GetTotalWorkingHours(employeeId, from, to);
                                var actualTime = await _timeSheetsService.GetActualWorkingHours(employeeId, from, to);

                                var performanceReport = await _timeSheetsService.GetAllEmployeePerformanceReportAsync(employeeId, from, to, null, 0, int.MaxValue, false, null, _monthlyReportSetting.ShowOnlyNotDOT);
                                var performanceSummary = await _timeSheetsService.GetEmployeePerformanceSummaryAsync(employeeId, from, to, null);

                                

                                emailBodyBuilder.Append("<hr/>");
                                emailBodyBuilder.Append($"<h3>{employee.FirstName} {employee.LastName}</h3>");

                                // Combined summary table
                                emailBodyBuilder.Append("<table border='1' cellpadding='5' cellspacing='0' style='width:100%; margin-bottom:10px;'>");
                                emailBodyBuilder.Append("<tr>");
                                emailBodyBuilder.Append("<th>Total Working Hours</th>");
                                emailBodyBuilder.Append("<th>Actual Logged Hours</th>");
                                emailBodyBuilder.Append("<th>Total Tasks</th>");
                                emailBodyBuilder.Append("<th>Delivered On Time</th>");
                                emailBodyBuilder.Append("<th>Efficiency (%)</th>");
                                emailBodyBuilder.Append("</tr>");

                                var actualTimeFormat = await _timeSheetsService.ConvertSpentTimeAsync(actualTime.SpentHours, actualTime.SpentMinutes);

                                emailBodyBuilder.Append("<tr>");
                                emailBodyBuilder.Append($"<td>{totalWorking} hrs</td>");
                                emailBodyBuilder.Append($"<td>{actualTimeFormat}</td>");
                                emailBodyBuilder.Append($"<td>{performanceSummary.TotalTask}</td>");
                                emailBodyBuilder.Append($"<td>{performanceSummary.TotalDeliveredOnTime}</td>");
                                emailBodyBuilder.Append($"<td>{performanceSummary.ResultPercentage}%</td>");
                                emailBodyBuilder.Append("</tr>");
                                emailBodyBuilder.Append("</table>");

                                // Detailed task breakdown
                                if (performanceReport.Any())
                                {
                                    emailBodyBuilder.Append("<table border='1' cellpadding='5' cellspacing='0' style='width:100%;'>");
                                    emailBodyBuilder.Append("<tr><th>Project</th><th>Task</th><th>Estimated Time</th><th>Spent Time</th><th>Extra Time</th><th>Delivered On Time</th></tr>");

                                    foreach (var task in performanceReport)
                                    {
                                        var project = await _projectsService.GetProjectsByIdAsync(task.ProjectId);
                                        var taskDetail = await _projectTaskService.GetProjectTasksByIdAsync(task.TaskId);

                                        if (project != null && taskDetail != null)
                                        {
                                            var spentTime = await _timeSheetsService.ConvertSpentTimeAsync(taskDetail.SpentHours, taskDetail.SpentMinutes);
                                            var estimatedTimeFormat = await _timeSheetsService.ConvertToHHMMFormat(taskDetail.EstimatedTime);

                                            int spentTimeInMinutes = (taskDetail.SpentHours * 60) + taskDetail.SpentMinutes;
                                            int estimatedTimeInMinutes = (int)(taskDetail.EstimatedTime * 60);

                                            // Calculate ExtraTime in minutes (Ensure it's non-negative)
                                            int extraTimeInMinutes = Math.Max(spentTimeInMinutes - estimatedTimeInMinutes, 0);

                                            // Convert ExtraTime to HH:MM format
                                            int extraHours = extraTimeInMinutes / 60;
                                            int extraMinutes = extraTimeInMinutes % 60;
                                            var extraTime = await _timeSheetsService.ConvertSpentTimeAsync(extraHours, extraMinutes);

                                            emailBodyBuilder.Append("<tr>");
                                            emailBodyBuilder.Append($"<td>{project.ProjectTitle}</td>");
                                            emailBodyBuilder.Append($"<td>{taskDetail.TaskTitle}</td>");
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
                        //tokens
                        var commonTokens = new List<Token>
            {
                new Token("TimeSheetReminder.SenderEmail", senderEmail),
                new Token("TimeSheetReminder.SenderName", senderName)
            };
                        await messageTemplates.SelectAwait(async messageTemplate =>
                        {
                            // email account
                            var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId);

                            var tokens = new List<Token>(commonTokens);
                            await _messageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount);

                            string fromEmail = emailAccount.Email;
                            string fromName = emailAccount.DisplayName;

                            string weeklyReportDate = from.ToString("dd-MMM-yyyy") + " to " + to.ToString("dd-MMM-yyyy");
                            tokens.Add(new Token("Email.Body", emailHtmlBody, true));
                            tokens.Add(new Token("WeeklyReport.Date", weeklyReportDate, true));



                            var cCEmails = "";


                            // event notification
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
                            var performanceSummary = await _timeSheetsService.GetEmployeePerformanceSummaryAsync(emp.Id, from, to, null);

                            emailBodyBuilder.Append("<hr/>");
                            emailBodyBuilder.Append($"<h3>{employee.FirstName} {employee.LastName}</h3>");

                            // Combined summary table
                            emailBodyBuilder.Append("<table border='1' cellpadding='5' cellspacing='0' style='width:100%; margin-bottom:10px;'>");
                            emailBodyBuilder.Append("<tr>");
                            emailBodyBuilder.Append("<th>Total Working Hours</th>");
                            emailBodyBuilder.Append("<th>Actual Logged Hours</th>");
                            emailBodyBuilder.Append("<th>Total Tasks</th>");
                            emailBodyBuilder.Append("<th>Delivered On Time</th>");
                            emailBodyBuilder.Append("<th>Efficiency (%)</th>");
                            emailBodyBuilder.Append("</tr>");

                            var actualTimeFormat = await _timeSheetsService.ConvertSpentTimeAsync(actualTime.SpentHours, actualTime.SpentMinutes);

                            emailBodyBuilder.Append("<tr>");
                            emailBodyBuilder.Append($"<td>{totalWorking} hrs</td>");
                            emailBodyBuilder.Append($"<td>{actualTimeFormat}</td>");
                            emailBodyBuilder.Append($"<td>{performanceSummary.TotalTask}</td>");
                            emailBodyBuilder.Append($"<td>{performanceSummary.TotalDeliveredOnTime}</td>");
                            emailBodyBuilder.Append($"<td>{performanceSummary.ResultPercentage}%</td>");
                            emailBodyBuilder.Append("</tr>");
                            emailBodyBuilder.Append("</table>");

                            // Detailed task breakdown
                            if (performanceReport.Any())
                            {
                                emailBodyBuilder.Append("<table border='1' cellpadding='5' cellspacing='0' style='width:100%;'>");
                                emailBodyBuilder.Append("<tr><th>Project</th><th>Task</th><th>Estimated Time</th><th>Spent Time</th><th>Extra Time</th><th>Delivered On Time</th></tr>");

                                foreach (var task in performanceReport)
                                {
                                    var project = await _projectsService.GetProjectsByIdAsync(task.ProjectId);
                                    var taskDetail = await _projectTaskService.GetProjectTasksByIdAsync(task.TaskId);

                                    if (project != null && taskDetail != null)
                                    {
                                        var spentTime = await _timeSheetsService.ConvertSpentTimeAsync(taskDetail.SpentHours, taskDetail.SpentMinutes);
                                        var estimatedTimeFormat = await _timeSheetsService.ConvertToHHMMFormat(taskDetail.EstimatedTime);

                                        int spentTimeInMinutes = (taskDetail.SpentHours * 60) + taskDetail.SpentMinutes;
                                        int estimatedTimeInMinutes = (int)(taskDetail.EstimatedTime * 60);

                                        // Calculate ExtraTime in minutes (Ensure it's non-negative)
                                        int extraTimeInMinutes = Math.Max(spentTimeInMinutes - estimatedTimeInMinutes, 0);

                                        // Convert ExtraTime to HH:MM format
                                        int extraHours = extraTimeInMinutes / 60;
                                        int extraMinutes = extraTimeInMinutes % 60;
                                        var extraTime = await _timeSheetsService.ConvertSpentTimeAsync(extraHours, extraMinutes);

                                        emailBodyBuilder.Append("<tr>");
                                        emailBodyBuilder.Append($"<td>{project.ProjectTitle}</td>");
                                        emailBodyBuilder.Append($"<td>{taskDetail.TaskTitle}</td>");
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
                            var projectIds = await _projectEmployeeMappingService.GetProjectIdsManagedByEmployeeIdAsync(employee.Id);

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
                                    performanceSummary = await _timeSheetsService.GetEmployeePerformanceSummaryAsync(id, from, to, projectIdsString);

                                    emailBodyBuilder.Append("<hr/>");
                                    emailBodyBuilder.Append($"<h3>{employee.FirstName} {employee.LastName}</h3>");

                                    // Combined summary table
                                    emailBodyBuilder.Append("<table border='1' cellpadding='5' cellspacing='0' style='width:100%; margin-bottom:10px;'>");
                                    emailBodyBuilder.Append("<tr>");
                                    emailBodyBuilder.Append("<th>Total Working Hours</th>");
                                    emailBodyBuilder.Append("<th>Actual Logged Hours</th>");
                                    emailBodyBuilder.Append("<th>Total Tasks</th>");
                                    emailBodyBuilder.Append("<th>Delivered On Time</th>");
                                    emailBodyBuilder.Append("<th>Efficiency (%)</th>");
                                    emailBodyBuilder.Append("</tr>");

                                    actualTimeFormat = await _timeSheetsService.ConvertSpentTimeAsync(actualTime.SpentHours, actualTime.SpentMinutes);

                                    emailBodyBuilder.Append("<tr>");
                                    emailBodyBuilder.Append($"<td>{totalWorking} hrs</td>");
                                    emailBodyBuilder.Append($"<td>{actualTimeFormat}</td>");
                                    emailBodyBuilder.Append($"<td>{performanceSummary.TotalTask}</td>");
                                    emailBodyBuilder.Append($"<td>{performanceSummary.TotalDeliveredOnTime}</td>");
                                    emailBodyBuilder.Append($"<td>{performanceSummary.ResultPercentage}%</td>");
                                    emailBodyBuilder.Append("</tr>");
                                    emailBodyBuilder.Append("</table>");

                                    // Detailed task breakdown
                                    if (performanceReport.Any())
                                    {
                                        emailBodyBuilder.Append("<table border='1' cellpadding='5' cellspacing='0' style='width:100%;'>");
                                        emailBodyBuilder.Append("<tr><th>Project</th><th>Task</th><th>Estimated Time</th><th>Spent Time</th><th>Extra Time</th><th>Delivered On Time</th></tr>");

                                        foreach (var task in performanceReport)
                                        {
                                            var project = await _projectsService.GetProjectsByIdAsync(task.ProjectId);
                                            var taskDetail = await _projectTaskService.GetProjectTasksByIdAsync(task.TaskId);

                                            if (project != null && taskDetail != null)
                                            {
                                                var spentTime = await _timeSheetsService.ConvertSpentTimeAsync(taskDetail.SpentHours, taskDetail.SpentMinutes);
                                                var estimatedTimeFormat = await _timeSheetsService.ConvertToHHMMFormat(taskDetail.EstimatedTime);

                                                int spentTimeInMinutes = (taskDetail.SpentHours * 60) + taskDetail.SpentMinutes;
                                                int estimatedTimeInMinutes = (int)(taskDetail.EstimatedTime * 60);

                                                // Calculate ExtraTime in minutes (Ensure it's non-negative)
                                                int extraTimeInMinutes = Math.Max(spentTimeInMinutes - estimatedTimeInMinutes, 0);

                                                // Convert ExtraTime to HH:MM format
                                                int extraHours = extraTimeInMinutes / 60;
                                                int extraMinutes = extraTimeInMinutes % 60;
                                                var extraTime = await _timeSheetsService.ConvertSpentTimeAsync(extraHours, extraMinutes);

                                                emailBodyBuilder.Append("<tr>");
                                                emailBodyBuilder.Append($"<td>{project.ProjectTitle}</td>");
                                                emailBodyBuilder.Append($"<td>{taskDetail.TaskTitle}</td>");
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
                            //tokens
                            var commonTokens = new List<Token>
            {
                new Token("TimeSheetReminder.SenderEmail", senderEmail),
                new Token("TimeSheetReminder.SenderName", senderName)
            };
                            await messageTemplates.SelectAwait(async messageTemplate =>
                            {
                                // email account
                                var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId);

                                var tokens = new List<Token>(commonTokens);
                                await _messageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount);

                                string fromEmail = emailAccount.Email;
                                string fromName = emailAccount.DisplayName;

                                string weeklyReportDate = from.ToString("dd-MMM-yyyy") + " to " + to.ToString("dd-MMM-yyyy");

                                tokens.Add(new Token("Email.Body", emailHtmlBody, true));
                                tokens.Add(new Token("WeeklyReport.Date", weeklyReportDate, true));



                                var cCEmails = "";


                                // event notification
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


        public virtual async Task<IList<int>> SendTimesheetReminder2MessageAsync(int languageId, IList<Employee> employeeList, DateTime date)
        {
            var store = await _storeContext.GetCurrentStoreAsync();
            languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);

            var messageTemplates = await GetActiveMessageTemplatesAsync(MessageTemplateSystemNames.SendTimeSheetReminder2Mail, store.Id);
            if (!messageTemplates.Any())
                return new List<int>();

            var holidayList = await _holidayService.GetAllHolidaysAsync();
            foreach (var employee in employeeList)
            {


                var dateList = await GetTimesheetReminderDatesList(employee.Id, _timeSheetSettings.ConsiderBeforeDay, holidayList);
                if (dateList.Any())
                {
                    string employeeName = employee.FirstName + " " + employee.LastName;
                    string senderEmail = employee.OfficialEmail;
                    string senderName = employeeName;
                    //tokens
                    var commonTokens = new List<Token>
    {
        new Token("TimeSheetReminder.SenderEmail", senderEmail),
        new Token("TimeSheetReminder.SenderName", senderName)
    };
                    await messageTemplates.SelectAwait(async messageTemplate =>
                    {
                        // email account
                        var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId);

                        var tokens = new List<Token>(commonTokens);
                        await _messageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount);


                        // Build the HTML table with employee names
                        string dateTable = "<table style='width: 100%; border-collapse: collapse;'>";
                        dateTable += "<tr><th style='text-align: left; border: 1px solid #ddd; padding: 8px;'>Pending Dates</th></tr>";

                        foreach (var date in dateList)
                        {
                            dateTable += $"<tr><td style='border: 1px solid #ddd; padding: 8px;'>{date}</td></tr>";
                        }

                        dateTable += "</table>";

                        string fromEmail = emailAccount.Email;
                        string fromName = emailAccount.DisplayName;
                        string link = "<a href='https://portal.satyanamsoft.com/TimeSheet/UpdateTimeSheet'>click here</a>";
                        tokens.Add(new Token("Employee.Name", employeeName, true));

                        tokens.Add(new Token("TimeSheet.Link", link, true));
                        tokens.Add(new Token("TimeSheet.PendingDates", dateTable, true));



                        var cCEmailsSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                        if (dateList.Count >= _timeSheetSettings.SendWithCCAfterDay)
                        {

                            // Add HR email
                            if (!string.IsNullOrEmpty(_leaveSettings.HrEmail))
                            {
                                cCEmailsSet.Add(_leaveSettings.HrEmail.Trim());
                            }

                            // Add common emails
                            if (!string.IsNullOrEmpty(_timeSheetSettings.CommonEmails))
                            {
                                foreach (var email in _timeSheetSettings.CommonEmails.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
                                {
                                    cCEmailsSet.Add(email.Trim());
                                }
                            }

                            // Add project managers' emails
                            if (_timeSheetSettings.SendEmailToAllProjectManager)
                            {
                                var projectManagers = await _projectEmployeeMappingService.GetProjectManagersByEmployeeIdAsync(employee.Id);
                                foreach (var projectManager in projectManagers)
                                {
                                    var projectManagerEmployee = await _employeeService.GetEmployeeByIdAsync(projectManager);
                                    if (projectManagerEmployee != null)
                                    {
                                        cCEmailsSet.Add(projectManagerEmployee.OfficialEmail.Trim());
                                    }
                                }
                            }

                            // Add project leaders' emails
                            if (_timeSheetSettings.SendEmailToAllProjectLeaders)
                            {
                                var projectLeaders = await _projectEmployeeMappingService.GetProjectLeadersByEmployeeIdAsync(employee.Id);
                                foreach (var projectLeader in projectLeaders)
                                {
                                    var projectLeaderEmployee = await _employeeService.GetEmployeeByIdAsync(projectLeader);
                                    if (projectLeaderEmployee != null)
                                    {
                                        cCEmailsSet.Add(projectLeaderEmployee.OfficialEmail.Trim());
                                    }
                                }
                            }

                            // Add employee manager's email
                            if (_timeSheetSettings.SendEmailToEmployeeManager)
                            {
                                var employeeManager = await _employeeService.GetEmployeeByIdAsync(employee.ManagerId);
                                if (employeeManager != null)
                                {
                                    cCEmailsSet.Add(employeeManager.OfficialEmail.Trim());
                                }
                            }

                            // Ensure sender email is not included in CC
                            if (cCEmailsSet.Contains(senderEmail.Trim(), StringComparer.OrdinalIgnoreCase))
                            {
                                cCEmailsSet.Remove(senderEmail.Trim());
                            }


                        }
                        var cCEmails = string.Join(";", cCEmailsSet);  // Join the emails into a string

                        // event notification
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
            return new List<int>();
        }

        public virtual async Task<IList<int>> SendTeamMemberAddedMessageAsync(int languageId, int employeeId, int designationId, int projectId)
        {
            var store = await _storeContext.GetCurrentStoreAsync();
            languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);

            var messageTemplates = await GetActiveMessageTemplatesAsync(MessageTemplateSystemNames.SendTeamMemberAddedMail, store.Id);
            if (!messageTemplates.Any())
                return new List<int>();

            string employeeName = "";
            string senderEmail = "";

            var employee = await _employeeService.GetEmployeeByIdAsync(employeeId);
            if (employee != null)
            {
                employeeName = employee.FirstName + " " + employee.LastName;
                senderEmail = employee.OfficialEmail;
            }
            string senderName = employeeName;
            //tokens
            var commonTokens = new List<Token>
    {
        new Token("TeamMemberAdded.SenderEmail", senderEmail),
        new Token("TeamMemberAdded.SenderName", senderName)
    };
            await messageTemplates.SelectAwait(async messageTemplate =>
            {
                // email account
                var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId);

                var tokens = new List<Token>(commonTokens);
                await _messageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount);

                string fromEmail = emailAccount.Email;
                string fromName = emailAccount.DisplayName;

                var project = await _projectsService.GetProjectsByIdAsync(projectId);
                string projectName = "";
                if (project != null)
                    projectName = project.ProjectTitle;

                var designation = await _designationService.GetDesignationByIdAsync(designationId);
                string designationName = "";
                if (designation != null)
                    designationName = designation.Name;

                tokens.Add(new Token("Employee.Name", employeeName, true));
                tokens.Add(new Token("Project.Name", projectName, true));
                tokens.Add(new Token("Designation.Name", designationName, true));

                var cCEmails = "";


                // event notification
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

            return new List<int>();
        }


        public virtual async Task<IList<int>> SendRatingsReminderMessageAsync(int languageId)
        {
            var store = await _storeContext.GetCurrentStoreAsync();
            languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);

            var messageTemplates = await GetActiveMessageTemplatesAsync(MessageTemplateSystemNames.SendRatingReminder, store.Id);
            if (!messageTemplates.Any())
                return new List<int>();

            string employeeManagerName = "";
            string senderEmail = "";
            var date = await _dateTimeHelper.GetUTCAsync();
            var currMonth = date.Month;
            var currentYear = date.Year;
            var startReminderDate = new DateTime(date.Year, date.Month, _teamPerformanceSettings.StartReminderDate);

            if (date.Date > startReminderDate)
            {
                // Calculate the previous month, with wrap-around for January
                var previousMonth = currMonth == 1 ? 12 : currMonth - 1;
                var previousYear = currMonth == 1 ? currentYear - 1 : currentYear;

                var employeeIds = await _employeeService.GetAllEmployeeIdsAsync();

                if (employeeIds != null)
                {
                    foreach (var empId in employeeIds)
                    {
                        var employee = await _employeeService.GetEmployeeByIdAsync(empId);

                        if (employee != null)
                        {
                            var employees = await _teamPerformanceMeasurementService.GetEmployeeForRatingReminder(empId, previousMonth, previousYear);

                            if (employees.Count > 0)
                            {
                                var employeeManager = await _employeeService.GetEmployeeByIdAsync(empId);
                                if (employeeManager != null)
                                {
                                    employeeManagerName = employeeManager.FirstName + " " + employeeManager.LastName;
                                }
                                string senderName = employeeManagerName;
                                senderEmail = employeeManager.OfficialEmail;
                                senderName = employeeManagerName;
                                //tokens
                                var commonTokens = new List<Token>
    {
        new Token("RatingReminder.SenderEmail", senderEmail),
        new Token("RatingReminder.SenderName", senderName)
    };
                                await messageTemplates.SelectAwait(async messageTemplate =>
                                {
                                    // email account
                                    var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId);

                                    var tokens = new List<Token>(commonTokens);
                                    await _messageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount);

                                    string fromEmail = emailAccount.Email;
                                    string fromName = emailAccount.DisplayName;


                                    string monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(previousMonth);

                                    // Build the HTML table with employee names
                                    string employeeTable = "<table style='width: 100%; border-collapse: collapse;'>";
                                    employeeTable += "<tr><th style='text-align: left; border: 1px solid #ddd; padding: 8px;'>Employee Name</th></tr>";

                                    foreach (var emp in employees)
                                    {
                                        employeeTable += $"<tr><td style='border: 1px solid #ddd; padding: 8px;'>{emp.FirstName} {emp.LastName}</td></tr>";
                                    }

                                    employeeTable += "</table>";


                                    string link = "<a href='https://portal.satyanamsoft.com/performance/addratings'>click here</a>";
                                    tokens.Add(new Token("EmployeeManager.Name", employeeManagerName, true));
                                    tokens.Add(new Token("Month.Name", monthName, true));
                                    tokens.Add(new Token("RemainingEmployees.Table", employeeTable, true));
                                    tokens.Add(new Token("AddRatingLink.ClickHere", link, true));


                                    var cCEmailsSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                                    var startCCDate = new DateTime(date.Year, date.Month, _teamPerformanceSettings.StartCCDate);
                                    if (date.Date > startCCDate)

                                    {
                                        cCEmailsSet.Add(_leaveSettings.HrEmail.Trim());

                                        var employeeManager = await _employeeService.GetEmployeeByIdAsync(employee.ManagerId);
                                        if (employeeManager != null)
                                        {
                                            cCEmailsSet.Add(employeeManager.OfficialEmail.Trim());
                                        }


                                        var projectManagers = await _projectEmployeeMappingService.GetProjectManagersByEmployeeIdAsync(empId);
                                        foreach (var projectManager in projectManagers)
                                        {
                                            var projectManagerEmployee = await _employeeService.GetEmployeeByIdAsync(projectManager);
                                            if (projectManagerEmployee != null)
                                            {
                                                cCEmailsSet.Add(projectManagerEmployee.OfficialEmail.Trim());
                                            }
                                        }


                                    }

                                    var cCEmails = string.Join(";", cCEmailsSet);


                                    // event notification
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
                }


            }
            return new List<int>();
        }




        #endregion

        #region Common

        /// <summary>
        /// Send notification
        /// </summary>
        /// <param name="messageTemplate">Message template</param>
        /// <param name="emailAccount">Email account</param>
        /// <param name="languageId">Language identifier</param>
        /// <param name="tokens">Tokens</param>
        /// <param name="toEmailAddress">Recipient email address</param>
        /// <param name="toName">Recipient name</param>
        /// <param name="attachmentFilePath">Attachment file path</param>
        /// <param name="attachmentFileName">Attachment file name</param>
        /// <param name="replyToEmailAddress">"Reply to" email</param>
        /// <param name="replyToName">"Reply to" name</param>
        /// <param name="fromEmail">Sender email. If specified, then it overrides passed "emailAccount" details</param>
        /// <param name="fromName">Sender name. If specified, then it overrides passed "emailAccount" details</param>
        /// <param name="subject">Subject. If specified, then it overrides subject of a message template</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the queued email identifier
        /// </returns>
        public virtual async Task<int> SendNotificationAsync(MessageTemplate messageTemplate,
            EmailAccount emailAccount, int languageId, IList<Token> tokens,
            string toEmailAddress, string toName,
            string attachmentFilePath = null, string attachmentFileName = null,
            string replyToEmailAddress = null, string replyToName = null,
            string fromEmail = null, string fromName = null, string subject = null)
        {
            if (messageTemplate == null)
                throw new ArgumentNullException(nameof(messageTemplate));

            if (emailAccount == null)
                throw new ArgumentNullException(nameof(emailAccount));

            //retrieve localized message template data
            var bcc = await _localizationService.GetLocalizedAsync(messageTemplate, mt => mt.BccEmailAddresses, languageId);
            if (string.IsNullOrEmpty(subject))
                subject = await _localizationService.GetLocalizedAsync(messageTemplate, mt => mt.Subject, languageId);
            var body = await _localizationService.GetLocalizedAsync(messageTemplate, mt => mt.Body, languageId);

            //Replace subject and body tokens 
            var subjectReplaced = _tokenizer.Replace(subject, tokens, false);
            var bodyReplaced = _tokenizer.Replace(body, tokens, true);

            //limit name length
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
                CC = string.Empty,
                Bcc = bcc,
                Subject = subjectReplaced,
                Body = bodyReplaced,
                AttachmentFilePath = attachmentFilePath,
                AttachmentFileName = attachmentFileName,
                AttachedDownloadId = messageTemplate.AttachedDownloadId,
                CreatedOnUtc = await _dateTimeHelper.GetUTCAsync()
                ,
                EmailAccountId = emailAccount.Id,
                DontSendBeforeDateUtc = !messageTemplate.DelayBeforeSend.HasValue ? null
                    : (DateTime?)(await _dateTimeHelper.GetUTCAsync() + TimeSpan.FromHours(messageTemplate.DelayPeriod.ToHours(messageTemplate.DelayBeforeSend.Value)))
            };

            await _queuedEmailService.InsertQueuedEmailAsync(email);
            return email.Id;
        }

        public virtual async Task<int> SendNotificationWithCCAsync(MessageTemplate messageTemplate,
          EmailAccount emailAccount, int languageId, IList<Token> tokens,
          string toEmailAddress, string toName,
          string attachmentFilePath = null, string attachmentFileName = null,
          string replyToEmailAddress = null, string replyToName = null, string subject = null,
          string fromEmail = null, string fromName = null, string cC = null)
        {
            if (messageTemplate == null)
                throw new ArgumentNullException(nameof(messageTemplate));

            if (emailAccount == null)
                throw new ArgumentNullException(nameof(emailAccount));

            //retrieve localized message template data
            var bcc = await _localizationService.GetLocalizedAsync(messageTemplate, mt => mt.BccEmailAddresses, languageId);
            //if (string.IsNullOrEmpty(subject))
            subject = await _localizationService.GetLocalizedAsync(messageTemplate, mt => mt.Subject, languageId);
            var body = await _localizationService.GetLocalizedAsync(messageTemplate, mt => mt.Body, languageId);

            //Replace subject and body tokens 
            var subjectReplaced = _tokenizer.Replace(subject, tokens, false);
            var bodyReplaced = _tokenizer.Replace(body, tokens, true);

            //limit name length
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
                DontSendBeforeDateUtc = !messageTemplate.DelayBeforeSend.HasValue ? null
                    : (DateTime?)(await _dateTimeHelper.GetUTCAsync() + TimeSpan.FromHours(messageTemplate.DelayPeriod.ToHours(messageTemplate.DelayBeforeSend.Value)))
            };

            await _queuedEmailService.InsertQueuedEmailAsync(email);
            return email.Id;
        }

        #endregion

        //send notificatin Interviewer to Hr

        public virtual async Task<IList<int>> SendInterviewertoHrMessageAsync(CandidatesResult result, int languageId)
        {
            if (result == null)
                throw new ArgumentNullException(nameof(result));

            var store = await _storeContext.GetCurrentStoreAsync();
            languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);

            var messageTemplates = await GetActiveMessageTemplatesAsync(MessageTemplateSystemNames.InterviewerToHrMessage, store.Id);
            if (!messageTemplates.Any())
                return new List<int>();

            //tokens
            var commonTokens = new List<Token>();
            await _messageTokenProvider.AddInterviewerToHrTokensAsync(commonTokens, result);
            //await _messageTokenProvider.AddTraineeTokensAsync(commonTokens);
            return await messageTemplates.SelectAwait(async messageTemplate =>
            {
                //email account
                var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId);

                var tokens = new List<Token>(commonTokens);
                await _messageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount);

                //event notification
                await _eventPublisher.MessageTokensAddedAsync(messageTemplate, tokens);

                var toEmail = emailAccount.Email;
                var toName = emailAccount.DisplayName;
                return await SendNotificationAsync(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
            }).ToListAsync();
        }

        public virtual async Task<IList<int>> SendInterviewerMessageAsync(CandidatesResumes trainee, int languageId)
        {
            if (trainee == null)
                throw new ArgumentNullException(nameof(trainee));

            var store = await _storeContext.GetCurrentStoreAsync();
            languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);

            var messageTemplates = await GetActiveMessageTemplatesAsync(MessageTemplateSystemNames.InterviewerMessage, store.Id);
            if (!messageTemplates.Any())
                return new List<int>();

            //tokens
            var commonTokens = new List<Token>();
            await _messageTokenProvider.AddInterviewerTokensAsync(commonTokens, trainee);
            //await _messageTokenProvider.AddTraineeTokensAsync(commonTokens);
            return await messageTemplates.SelectAwait(async messageTemplate =>
            {
                //email account
                var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId);

                var tokens = new List<Token>(commonTokens);
                await _messageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount);

                //event notification
                await _eventPublisher.MessageTokensAddedAsync(messageTemplate, tokens);
                var employee = await _employeeService.GetEmployeeByIdAsync(trainee.EmployeeId);
                var toEmail = employee.PersonalEmail;
                var toName = employee.FirstName + employee.LastName;

                return await SendNotificationAsync(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
            }).ToListAsync();
        }

        public virtual async Task<IList<int>> SendWeeklyUpdatetoHrMessageAsync(WeeklyReports result, int languageId)
        {
            if (result == null)
                throw new ArgumentNullException(nameof(result));

            var store = await _storeContext.GetCurrentStoreAsync();
            languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);

            var messageTemplates = await GetActiveMessageTemplatesAsync(MessageTemplateSystemNames.WeeklyupdateToHrMessage, store.Id);
            if (!messageTemplates.Any())
                return new List<int>();

            //tokens
            var commonTokens = new List<Token>();
            await _messageTokenProvider.AddWeeklyupdateToHrTokensAsync(commonTokens, result);
            //await _messageTokenProvider.AddTraineeTokensAsync(commonTokens);
            return await messageTemplates.SelectAwait(async messageTemplate =>
            {
                //email account
                var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId);

                var tokens = new List<Token>(commonTokens);
                await _messageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount);

                //event notification
                await _eventPublisher.MessageTokensAddedAsync(messageTemplate, tokens);

                var toEmail = emailAccount.Email;
                var toName = emailAccount.DisplayName;
                return await SendNotificationAsync(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
            }).ToListAsync();
        }

        #endregion

        #region Send Task Alert Email Methods

        public virtual async Task<IList<int>> SendTaskAlertEmailAsync(Employee employee, IList<string> ccEmails, string projectName, 
            string taskName, string estimationTime, string spentTime, string reason, string comment, decimal variation, bool commentRequired, int languageId)
        {
            var store = await _storeContext.GetCurrentStoreAsync();
            languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);

            var messageTemplates = await GetActiveMessageTemplatesAsync(MessageTemplateSystemNames.SendTaskAlertEmail, store.Id);
            if (!messageTemplates.Any())
                return new List<int>();

            var cCEmails = string.Join(";", ccEmails);

            var commonTokens = new List<Token>();
            return await messageTemplates.SelectAwait(async messageTemplate =>
            {
                var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId);

                var tokens = new List<Token>(commonTokens);
                tokens.Add(new Token("Task.AlertCount", variation));
                tokens.Add(new Token("Employee.Name", employee.FirstName + " " + employee.LastName));
                tokens.Add(new Token("Project.Name", projectName));
                tokens.Add(new Token("Task.Name", taskName));
                tokens.Add(new Token("Task.EstimationTime", estimationTime));
                tokens.Add(new Token("Task.SpentTime", spentTime));
                tokens.Add(new Token("Task.OverduePercentage", variation + "%"));
                tokens.Add(new Token("Task.CommentRequired", commentRequired));
                tokens.Add(new Token("Task.Reason", reason));
                tokens.Add(new Token("Task.Comment", comment));
                await _messageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount);

                await _eventPublisher.MessageTokensAddedAsync(messageTemplate, tokens);

                var toEmail = employee.OfficialEmail;
                var toName = employee.FirstName + " " + employee.LastName;

                return await SendNotificationWithCCAsync(messageTemplate, emailAccount, languageId, tokens, toEmail, toName,
                    fromEmail: emailAccount.Email, fromName: emailAccount.DisplayName, cC: cCEmails);
            }).ToListAsync();
        }

        #endregion

        #region Send Task Progress Email Methods
        
        public virtual async Task<IList<int>> SendTaskOverdueFollowUpEmailAsync(Employee employee, IList<string> ccEmails, int taskId, string managerName, 
            string projectName, string alertType, string taskName, string estimationTime, string spentTime, string reason, string comment, decimal variation, 
            bool reasonRequired, bool commentRequired, bool isNewETA, string etaHours, bool isOnTrack, int languageId)
        {
            var store = await _storeContext.GetCurrentStoreAsync();
            languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);

            var messageTemplates = await GetActiveMessageTemplatesAsync(MessageTemplateSystemNames.SendTaskProgressEmail, store.Id);
            if (!messageTemplates.Any())
                return new List<int>();

            var cCEmails = string.Join(";", ccEmails);

            var commonTokens = new List<Token>();
            return await messageTemplates.SelectAwait(async messageTemplate =>
            {
                var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId);

                string taskURL = store.Url + "ProjectTask/Edit?id=" + taskId;

                var tokens = new List<Token>(commonTokens);
                tokens.Add(new Token("Task.TaskURL", taskURL));
                tokens.Add(new Token("Manager.Name", managerName));
                tokens.Add(new Token("Task.TaskName", taskName));
                tokens.Add(new Token("Alert.AlertType", alertType));
                tokens.Add(new Token("Project.ProjectName", projectName));
                tokens.Add(new Token("Employee.EmployeeName", employee.FirstName + " " + employee.LastName));
                tokens.Add(new Token("Task.OriginalEstimation", estimationTime));
                tokens.Add(new Token("Task.TimeSpent", spentTime));
                tokens.Add(new Token("DeliverOnTime", isOnTrack));
                tokens.Add(new Token("Progress", Math.Round(variation, 2)));
                tokens.Add(new Token("Task.ReasonRequired", reasonRequired));
                tokens.Add(new Token("Task.CommentRequired", commentRequired));
                tokens.Add(new Token("Task.IsNewETA", isNewETA));
                tokens.Add(new Token("Reason.ReasonForDelay", reason));
                tokens.Add(new Token("Task.Comment", comment));
                tokens.Add(new Token("Task.ETAHours", etaHours));
                await _messageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount);

                await _eventPublisher.MessageTokensAddedAsync(messageTemplate, tokens);

                var toEmail = employee.OfficialEmail;
                var toName = employee.FirstName + " " + employee.LastName;

                return await SendNotificationWithCCAsync(messageTemplate, emailAccount, languageId, tokens, toEmail, toName,
                    fromEmail: emailAccount.Email, fromName: emailAccount.DisplayName, cC: cCEmails);
            }).ToListAsync();
        }

        #endregion
    }
}