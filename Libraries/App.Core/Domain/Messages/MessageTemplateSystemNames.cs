using System.Threading.Tasks;

namespace App.Core.Domain.Messages
{
    /// <summary>
    /// Represents message template system names
    /// </summary>
    public static partial class MessageTemplateSystemNames
    {
        #region Customer

        /// <summary>
        /// Represents system name of notification about new registration
        /// </summary>
        public const string CustomerRegisteredStoreOwnerNotification = "NewCustomer.Notification";

        /// <summary>
        /// Represents system name of customer welcome message
        /// </summary>
        public const string CustomerWelcomeMessage = "Customer.WelcomeMessage";

        /// <summary>
        /// Represents system name of email validation message
        /// </summary>
        public const string CustomerEmailValidationMessage = "Customer.EmailValidationMessage";

        /// <summary>
        /// Represents system name of email revalidation message
        /// </summary>
        public const string CustomerEmailRevalidationMessage = "Customer.EmailRevalidationMessage";

        /// <summary>
        /// Represents system name of password recovery message
        /// </summary>
        public const string CustomerPasswordRecoveryMessage = "Customer.PasswordRecovery";

        #endregion

        #region Newsletter

        /// <summary>
        /// Represents system name of subscription activation message
        /// </summary>
        public const string NewsletterSubscriptionActivationMessage = "NewsLetterSubscription.ActivationMessage";

        /// <summary>
        /// Represents system name of subscription deactivation message
        /// </summary>
        public const string NewsletterSubscriptionDeactivationMessage = "NewsLetterSubscription.DeactivationMessage";

        #endregion

        #region To friend

        /// <summary>
        /// Represents system name of 'Email a friend' message
        /// </summary>
        public const string EmailAFriendMessage = "Service.EmailAFriend";

        /// <summary>
        /// Represents system name of 'Email a friend' message with wishlist
        /// </summary>
        public const string WishlistToFriendMessage = "Wishlist.EmailAFriend";

        #endregion

        #region Forum

        /// <summary>
        /// Represents system name of notification about new forum topic
        /// </summary>
        public const string NewForumTopicMessage = "Forums.NewForumTopic";

        /// <summary>
        /// Represents system name of notification about new forum post
        /// </summary>
        public const string NewForumPostMessage = "Forums.NewForumPost";

        /// <summary>
        /// Represents system name of notification about new private message
        /// </summary>
        public const string PrivateMessageNotification = "Customer.NewPM";

        #endregion

        #region Misc

        /// <summary>
        /// Represents system name of notification store owner about new blog comment
        /// </summary>
        public const string BlogCommentStoreOwnerNotification = "Blog.BlogComment";

        /// <summary>
        /// Represents system name of notification store owner about new news comment
        /// </summary>
        public const string NewsCommentStoreOwnerNotification = "News.NewsComment";

        /// <summary>
        /// Represents system name of 'Contact us' message
        /// </summary>
        public const string ContactUsMessage = "Service.ContactUs";

        /// <summary>
        /// Represents system name of 'Contact us' message
        /// </summary>
        public const string InterviewerMessage = "Service.Interviewer";
        public const string InterviewerToHrMessage = "Service.InterviewerToHr";
        public const string WeeklyupdateToHrMessage = "Service.WeeklyupdateToHrMessage";
        public const string SendLeaveapprovalMail=  "LeaveManagement.ApproveLeave";
        public const string SendLeaveRejectMail = "LeaveManagement.RejectLeave";
        public const string SendLeaveCancelMail = "LeaveManagement.CancelledLeave";
        public const string SendLeaveRequestMail = "LeaveManagement.RequestLeave";
        public const string SendWelcomeEmployeeMail = "Employee.WelcomeMessage";
        public const string SendTimeSheetReminderMail = "TimeSheet.Reminder";
        public const string SendTimeSheetReminder2Mail = "TimeSheet.Reminder2";
        public const string SendTeamMemberAddedMail = "Project.TeamMemberAdded";
        public const string SendRatingReminder = "Rating.Reminder";
        public const string SendEmployeeOnBordingEmail = "Employee.OnBordingEmail";
        public const string SendWeeklyReportEmail = "Employee.SendWeeklyReportEmail";
        public const string SendOverdueReminderEmail = "Employee.SendOverdueReminderEmail";
        public const string SendMentionEmail = "Employee.SendMentionEmail";
        public const string AnnouncementEmail = "Announcement.Email";
        public const string InquiryEmail = "Inquiry.Email";
        #endregion

        #region Send Task Alert Email

        public const string SendTaskAlertEmail = "Employee.SendTaskAlertEmail";

        #endregion

        #region Send Task Progress Email

        public const string SendTaskProgressEmail = "Employee.SendTaskProgressEmail";

        #endregion
    }
}