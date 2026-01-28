using App.Core;
using App.Core.Domain.Common;
using App.Core.Domain.Extension.Leaves;
using App.Core.Domain.Extension.PerformanceMeasurements;
using App.Core.Domain.Extension.TimeSheets;
using App.Core.Domain.Messages;
using App.Core.Domain.TimeSheets;
using App.Core.Events;
using App.Data;
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
using Satyanam.Nop.Core.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Satyanam.Nop.Plugin.Misc.SatyanamCRM.Services
{
    /// <summary>
    /// Workflow message service
    /// </summary>
    public partial class WorkflowMessageCRMService : IWorkflowMessageCRMService
    {
        #region Fields
        private readonly EmailAccountSettings _emailAccountSettings;
        private readonly IEmailAccountService _emailAccountService;
        private readonly IEventPublisher _eventPublisher;
        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly IMessageTemplateService _messageTemplateService;
        private readonly IMessageTokenProvider _messageTokenProvider;
        private readonly IStoreContext _storeContext;
        private readonly SatyanamAPISettings _satyanamAPISettings;
        private readonly IWorkflowMessageService _workflowMessageService;

        #endregion

        #region Ctor

        public WorkflowMessageCRMService(
            EmailAccountSettings emailAccountSettings,
            IEmailAccountService emailAccountService,
            IEventPublisher eventPublisher,
            ILanguageService languageService,
            ILocalizationService localizationService,
            IMessageTemplateService messageTemplateService,
            IMessageTokenProvider messageTokenProvider,
            IStoreContext storeContext,
            SatyanamAPISettings satyanamAPISettings,
            IWorkflowMessageService workflowMessageService)
        {
            _emailAccountSettings = emailAccountSettings;
            _emailAccountService = emailAccountService;
            _eventPublisher = eventPublisher;
            _languageService = languageService;
            _localizationService = localizationService;
            _messageTemplateService = messageTemplateService;
            _messageTokenProvider = messageTokenProvider;
            _storeContext = storeContext;
            _satyanamAPISettings = satyanamAPISettings;
            _workflowMessageService = workflowMessageService;
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
        public virtual async Task<EmailAccount> GetEmailAccountOfMessageTemplateAsync(MessageTemplate messageTemplate, int languageId)
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

        #endregion

        #region Methods
        public virtual async Task<IList<int>> SendNewInquiryNotificationAsync(Inquiry inquiry)
        {
            var store = await _storeContext.GetCurrentStoreAsync();
            int languageId = await EnsureLanguageIsActiveAsync(0, store.Id);

            var messageTemplates = await GetActiveMessageTemplatesAsync(MessageTemplateSystemNames.InquiryEmail, store.Id);
            if (!messageTemplates.Any())
                return new List<int>();

            var emailSetting = _satyanamAPISettings.InquiryEmailSendTo;
            if (string.IsNullOrWhiteSpace(emailSetting))
                return new List<int>();

            var recipientEmails = emailSetting
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(e => e.Trim())
                .Where(e => !string.IsNullOrWhiteSpace(e))
                .ToList();

            if (!recipientEmails.Any())
                return new List<int>();

            var emailIds = new List<int>();

            foreach (var toEmail in recipientEmails)
            {
                foreach (var messageTemplate in messageTemplates)
                {
                    var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId);
                    var tokens = new List<Token>();
                    await _messageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount);

                    tokens.Add(new Token("Inquiry.FirstName", inquiry.FirstName ?? "N/A"));
                    tokens.Add(new Token("Inquiry.LastName", inquiry.LastName ?? "N/A"));
                    tokens.Add(new Token("Inquiry.Email", inquiry.Email ?? "N/A"));
                    tokens.Add(new Token("Inquiry.ContactNo", inquiry.ContactNo ?? "N/A"));
                    tokens.Add(new Token("Inquiry.Company", inquiry.Company ?? "N/A"));
                    tokens.Add(new Token("Inquiry.Describe", inquiry.Describe ?? "N/A", true));
                    tokens.Add(new Token("Inquiry.Budget", inquiry.Budget ?? "N/A"));
                    tokens.Add(new Token("Inquiry.WantToHire", inquiry.WantToHire ?? "N/A"));
                    tokens.Add(new Token("Inquiry.ProjectType", inquiry.ProjectType ?? "N/A"));
                    tokens.Add(new Token("Inquiry.EngagementDuration", inquiry.EngagementDuration ?? "N/A"));
                    tokens.Add(new Token("Inquiry.TimeCommitment", inquiry.TimeCommitment ?? "N/A"));
                    tokens.Add(new Token("Inquiry.Source", inquiry.SourceId > 0
                        ? ((InquirySourceType)inquiry.SourceId).ToString()
                        : "N/A"));
                    tokens.Add(new Token("Inquiry.CreatedOnUtc", inquiry.CreatedOnUtc.ToString("g")));


                    await _eventPublisher.MessageTokensAddedAsync(messageTemplate, tokens);

                    var toName = toEmail;

                    var emailId = await _workflowMessageService.SendNotificationAsync(
                        messageTemplate,
                        emailAccount,
                        languageId,
                        tokens,
                        toEmail,
                        toName
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