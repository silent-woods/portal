using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using App.Core.Domain.Blogs;
using App.Core.Domain.Customers;
using App.Core.Domain.Forums;
using App.Core.Domain.Messages;
using App.Core.Domain.News;
using App.Data;
using App.Services.Blogs;
using App.Services.Customers;
using App.Services.Forums;
using App.Services.Messages;
using App.Services.News;
using NUnit.Framework;

namespace App.Tests.App.Services.Tests.Messages
{
    [TestFixture]
    public class WorkflowMessageServiceTests : ServiceTest
    {
        private readonly IWorkflowMessageService _workflowMessageService;

        private readonly List<int> _notActiveTempletes = new();
        private readonly IMessageTemplateService _messageTemplateService;
        private Customer _customer;
        private readonly IRepository<QueuedEmail> _queuedEmailRepository;
        private IList<MessageTemplate> _allMessageTemplates;
        private NewsLetterSubscription _subscription;
        private Forum _forum;
        private ForumTopic _forumTopic;
        private ForumPost _forumPost;
        private PrivateMessage _privateMessage;
        private BlogComment _blogComment;
        private NewsComment _newsComment;
        private readonly IForumService _forumService;

        public WorkflowMessageServiceTests()
        {
            _workflowMessageService = GetService<IWorkflowMessageService>();
            _messageTemplateService = GetService<IMessageTemplateService>();
            _queuedEmailRepository = GetService<IRepository<QueuedEmail>>();
            _forumService = GetService<IForumService>();
        }

        [OneTimeSetUp]
        public async Task OneTimeSetUp()
        {
            var customerService = GetService<ICustomerService>();
            var blogService = GetService<IBlogService>();
            var newsService = GetService<INewsService>();

            _customer = await customerService.GetCustomerByEmailAsync(NopTestsDefaults.AdminEmail);
            _subscription = new NewsLetterSubscription {Active = true, Email = NopTestsDefaults.AdminEmail};
            _forum = await _forumService.GetForumByIdAsync(1);
            _forumTopic = new ForumTopic {CustomerId = _customer.Id, ForumId = _forum.Id, Subject = "Subject"};
            await _forumService.InsertTopicAsync(_forumTopic, false);
            _forumPost = new ForumPost { CustomerId = _customer.Id, TopicId = _forumTopic.Id, Text = "Text"};
            await _forumService.InsertPostAsync(_forumPost, false);

            _privateMessage = new PrivateMessage
            {
                FromCustomerId = 1, ToCustomerId = 2, Subject = string.Empty, Text = string.Empty
            };
            _blogComment = await blogService.GetBlogCommentByIdAsync(1);
            _newsComment = await newsService.GetNewsCommentByIdAsync(1);
    
            _allMessageTemplates = await _messageTemplateService.GetAllMessageTemplatesAsync(0);

            foreach (var template in _allMessageTemplates.Where(t=>!t.IsActive))
            {
                template.IsActive = true;
                _notActiveTempletes.Add(template.Id);
                await _messageTemplateService.UpdateMessageTemplateAsync(template);
            }
        }

        [OneTimeTearDown]
        public async Task OneTimeTearDown()
        {
            foreach (var template in _allMessageTemplates.Where(t => _notActiveTempletes.Contains(t.Id)))
            {
                template.IsActive = false;
                await _messageTemplateService.UpdateMessageTemplateAsync(template);
            }

            await _forumService.DeletePostAsync(_forumPost);
            await _forumService.DeleteTopicAsync(_forumTopic);
        }

        [SetUp]
        public async Task SetUp()
        {
            await _queuedEmailRepository.TruncateAsync();
        }

        protected async Task CheckData(Func<Task<IList<int>>> func)
        {
            var queuedEmails = await _queuedEmailRepository.GetAllAsync(query => query);
            queuedEmails.Count.Should().Be(0);

            var emailIds = await func();

            emailIds.Count.Should().BeGreaterThan(0);

            queuedEmails = await _queuedEmailRepository.GetAllAsync(query => query);
            queuedEmails.Count.Should().Be(emailIds.Count);
        }

        #region Customer workflow

        [Test]
        public async Task CanSendCustomerRegisteredNotificationMessage()
        {
            await CheckData(async () =>
                await _workflowMessageService.SendCustomerRegisteredStoreOwnerNotificationMessageAsync(_customer, 1));
        }

        [Test]
        public async Task CanSendCustomerWelcomeMessage()
        {
            await CheckData(async () => 
                await _workflowMessageService.SendCustomerWelcomeMessageAsync(_customer, 1));
        }

        [Test]
        public async Task CanSendCustomerEmailValidationMessage()
        {
            await CheckData(async () =>
                await _workflowMessageService.SendCustomerEmailValidationMessageAsync(_customer, 1));
        }

        [Test]
        public async Task CanSendCustomerEmailRevalidationMessage()
        {
            _customer.EmailToRevalidate = NopTestsDefaults.AdminEmail;
            await CheckData(async () =>
                await _workflowMessageService.SendCustomerEmailRevalidationMessageAsync(_customer, 1));
        }

        [Test]
        public async Task CanSendCustomerPasswordRecoveryMessage()
        {
            await CheckData(async () =>
                await _workflowMessageService.SendCustomerPasswordRecoveryMessageAsync(_customer, 1));
        }

        #endregion

        #region Newsletter workflow

        [Test]
        public async Task CanSendNewsLetterSubscriptionActivationMessage()
        {
            await CheckData(async () =>
                await _workflowMessageService.SendNewsLetterSubscriptionActivationMessageAsync(_subscription, 1));
        }

        [Test]
        public async Task CanSendNewsLetterSubscriptionDeactivationMessage()
        {
            await CheckData(async () =>
                await _workflowMessageService.SendNewsLetterSubscriptionDeactivationMessageAsync(_subscription, 1));
        }

        #endregion

        #region Send a message to a friend

        [Test]
        public async Task CanSendWishlistEmailAFriendMessage()
        {
            await CheckData(async () =>
                await _workflowMessageService.SendWishlistEmailAFriendMessageAsync(_customer, 1, NopTestsDefaults.AdminEmail, NopTestsDefaults.AdminEmail, string.Empty));
        }

        #endregion

        #region Forum Notifications

        [Test]
        public async Task CanSendNewForumTopicMessage()
        {
            await CheckData(async () =>
                await _workflowMessageService.SendNewForumTopicMessageAsync(_customer, _forumTopic, _forum, 1));
        }

        [Test]
        public async Task CanSendNewForumPostMessage()
        {
            await CheckData(async () =>
                await _workflowMessageService.SendNewForumPostMessageAsync(_customer, _forumPost, _forumTopic, _forum, 1, 1));
        }

        [Test]
        public async Task CanSendPrivateMessageNotification()
        {
            await CheckData(async () =>
                await _workflowMessageService.SendPrivateMessageNotificationAsync(_privateMessage, 1));
        }

        #endregion

        #region Misc

        [Test]
        public async Task CanSendBlogCommentNotificationMessage()
        {
            await CheckData(async () =>
                await _workflowMessageService.SendBlogCommentStoreOwnerNotificationMessageAsync(_blogComment, 1));
        }

        [Test]
        public async Task CanSendNewsCommentNotificationMessage()
        {
            await CheckData(async () =>
                await _workflowMessageService.SendNewsCommentStoreOwnerNotificationMessageAsync(_newsComment, 1));
        }

        [Test]
        public async Task CanSendContactUsMessage()
        {
            await CheckData(async () =>
                await _workflowMessageService.SendContactUsMessageAsync(1, NopTestsDefaults.AdminEmail, "sender name", "subject", "body"));
        }

        #endregion
    }
}
