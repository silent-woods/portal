using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
using Nop.Core.Domain.Customers;

namespace App.Services.Messages
{
    /// <summary>
    /// Workflow message service
    /// </summary>
    public partial interface IWorkflowMessageService
    {
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
        Task<IList<int>> SendCustomerRegisteredStoreOwnerNotificationMessageAsync(Customer customer, int languageId);

        /// <summary>
        /// Sends a welcome message to a customer
        /// </summary>
        /// <param name="customer">Customer instance</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the queued email identifier
        /// </returns>
        Task<IList<int>> SendCustomerWelcomeMessageAsync(Customer customer, int languageId);

        /// <summary>
        /// Sends an email validation message to a customer
        /// </summary>
        /// <param name="customer">Customer instance</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the queued email identifier
        /// </returns>
        Task<IList<int>> SendCustomerEmailValidationMessageAsync(Customer customer, int languageId);

        /// <summary>
        /// Sends an email re-validation message to a customer
        /// </summary>
        /// <param name="customer">Customer instance</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the queued email identifier
        /// </returns>
        Task<IList<int>> SendCustomerEmailRevalidationMessageAsync(Customer customer, int languageId);

        /// <summary>
        /// Sends password recovery message to a customer
        /// </summary>
        /// <param name="customer">Customer instance</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the queued email identifier
        /// </returns>
        Task<IList<int>> SendCustomerPasswordRecoveryMessageAsync(Customer customer, int languageId);

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
        Task<IList<int>> SendNewsLetterSubscriptionActivationMessageAsync(NewsLetterSubscription subscription, int languageId);

        /// <summary>
        /// Sends a newsletter subscription deactivation message
        /// </summary>
        /// <param name="subscription">Newsletter subscription</param>
        /// <param name="languageId">Language identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the queued email identifier
        /// </returns>
        Task<IList<int>> SendNewsLetterSubscriptionDeactivationMessageAsync(NewsLetterSubscription subscription, int languageId);

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
        Task<IList<int>> SendWishlistEmailAFriendMessageAsync(Customer customer, int languageId,
            string customerEmail, string friendsEmail, string personalMessage);

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
        Task<IList<int>> SendNewForumTopicMessageAsync(Customer customer, ForumTopic forumTopic, Forum forum, int languageId);

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
        Task<IList<int>> SendNewForumPostMessageAsync(Customer customer, ForumPost forumPost,
            ForumTopic forumTopic, Forum forum, int friendlyForumTopicPageIndex, int languageId);

        /// <summary>
        /// Sends a private message notification
        /// </summary>
        /// <param name="privateMessage">Private message</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the queued email identifier
        /// </returns>
        Task<IList<int>> SendPrivateMessageNotificationAsync(PrivateMessage privateMessage, int languageId);

        #endregion

        #region Misc

        /// <summary>
        /// Sends a blog comment notification message to a store owner
        /// </summary>
        /// <param name="blogComment">Blog comment</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the queued email identifier
        /// </returns>
        Task<IList<int>> SendBlogCommentStoreOwnerNotificationMessageAsync(BlogComment blogComment, int languageId);

        /// <summary>
        /// Sends a news comment notification message to a store owner
        /// </summary>
        /// <param name="newsComment">News comment</param>
        /// <param name="languageId">Message language identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the queued email identifier
        /// </returns>
        Task<IList<int>> SendNewsCommentStoreOwnerNotificationMessageAsync(NewsComment newsComment, int languageId);

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
        Task<IList<int>> SendContactUsMessageAsync(int languageId, string senderEmail, string senderName, string subject, string body);

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
        Task<int> SendTestEmailAsync(int messageTemplateId, string sendToEmail, List<Token> tokens, int languageId);


        Task<IList<int>> SendLeaveApprovedMessageAsync(int languageId, string senderEmail, string senderName, int employeeId, int leaveId, IList<int> selectedEmployee = null);

        Task<IList<int>> SendWelcomeMessageAsync(int languageId, string senderEmail, string senderName, int employeeId);

        Task<IList<int>> SendTimesheetReminderMessageAsync(int languageId, IList<Employee> employeeList, DateTime date);

        Task<IList<int>> SendTimesheetReminder2MessageAsync(int languageId, IList<Employee> employeeList, DateTime date);

        Task<IList<int>> SendTeamMemberAddedMessageAsync(int languageId, int employeeId, int designationId, int projectId);

        Task<IList<int>> SendWeeklyReportMessageAsync(int languageId);
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
        Task<int> SendNotificationAsync(MessageTemplate messageTemplate,
            EmailAccount emailAccount, int languageId, IList<Token> tokens,
            string toEmailAddress, string toName,
            string attachmentFilePath = null, string attachmentFileName = null,
            string replyToEmailAddress = null, string replyToName = null,
            string fromEmail = null, string fromName = null, string subject = null);

        Task<int> SendNotificationWithCCAsync(MessageTemplate messageTemplate,
          EmailAccount emailAccount, int languageId, IList<Token> tokens,
          string toEmailAddress, string toName,
          string attachmentFilePath = null, string attachmentFileName = null,
          string replyToEmailAddress = null, string replyToName = null, string subject = null,
          string fromEmail = null, string fromName = null, string cC = null);

        Task<IList<int>> SendInterviewerMessageAsync(CandidatesResumes trainee, int languageId);
        Task<IList<int>> SendInterviewertoHrMessageAsync(CandidatesResult result, int languageId);
        Task<IList<int>> SendWeeklyUpdatetoHrMessageAsync(WeeklyReports result, int languageId);
        Task<IList<int>> SendLeaveRejectedMessageAsync(int languageId, string senderEmail,
            string senderName, int employeeId, int leaveId, IList<int> selectedEmployee = null);

        Task<IList<int>> SendLeaveCancelledMessageAsync(int languageId, string senderEmail,
              string senderName, int employeeId, int leaveId, IList<int> selectedEmployee = null);

        Task<IList<int>> SendLeaveRequestMessageAsync(int languageId, string senderEmail,
            string senderName, int employeeId, int leaveId, IList<int> selectedEmployee = null);

        Task<IList<int>> SendRatingsReminderMessageAsync(int languageId);

        Task<IList<int>> SendEmployeeOnBordingMessageAsync(int languageId, int employeeId, string onBordingBody);

        Task<IList<int>> SendEmployeeMentionMessageAsync(int languageId, int employeeId, int taskId, string comments);
        #endregion

        #region Send Task Alert Email Methods

        Task<IList<int>> SendTaskOverdueFollowUpEmailAsync(Employee employee, IList<string> ccEmails, int taskId, string managerName, string projectName, string alertType,
            string taskName, string estimationTime, string spentTime, string reason, string comment, decimal variation, bool reasonRequired, 
            bool commentRequired, bool isNewETA, string etaHours, bool isOnTrack, int languageId);

        #endregion
    }
}