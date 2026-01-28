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

namespace Satyanam.Nop.Plugin.Misc.SatyanamCRM.Services
{
    /// <summary>
    /// Workflow message service
    /// </summary>
    public partial interface IWorkflowMessageCRMService
    {
        Task<EmailAccount> GetEmailAccountOfMessageTemplateAsync(MessageTemplate messageTemplate, int languageId);

        Task<IList<int>> SendNewInquiryNotificationAsync(Inquiry inquiry);
    }
}