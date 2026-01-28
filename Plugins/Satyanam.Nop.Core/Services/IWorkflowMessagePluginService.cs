using App.Core;
using App.Core.Domain.Blogs;
using App.Core.Domain.Customers;
using App.Core.Domain.Employees;
using App.Core.Domain.Extension.WeeklyQuestions;
using App.Core.Domain.Forums;
using App.Core.Domain.ManageResumes;
using App.Core.Domain.Messages;
using App.Core.Domain.News;
using App.Core.Domain.result;
using App.Services.Messages;
using Nop.Core.Domain.Customers;
using Satyanam.Nop.Core.Domains;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Satyanam.Nop.Core.Services
{
    /// <summary>
    /// Workflow message service
    /// </summary>
    public partial interface IWorkflowMessagePluginService
    {
        Task<IList<int>> SendWeeklyReportMessageAsync(int languageId);

        Task<IList<int>> SendOverDueReminderMessageAsync(int languageId);
        Task<int> SendNotificationWithCCAsync(MessageTemplate messageTemplate,
                  EmailAccount emailAccount, int languageId, IList<Token> tokens,
                  string toEmailAddress, string toName,
                  string attachmentFilePath = null, string attachmentFileName = null,
                  string replyToEmailAddress = null, string replyToName = null, string subject = null,
                  string fromEmail = null, string fromName = null, string cC = null, DateTime? dontSendBeforeDateUtc = null);

        Task<IList<int>> SendAnnouncementMessageAsync(
    Announcement announcement, int languageId, IList<int> employeeIds);
        Task<EmailAccount> GetEmailAccountOfMessageTemplateAsync(MessageTemplate messageTemplate, int languageId);
    }
}