using System.Collections.Generic;
using System.Threading.Tasks;
using App.Core.Domain.Blogs;
using App.Core.Domain.Customers;
using App.Core.Domain.Extension.WeeklyQuestions;
using App.Core.Domain.Forums;
using App.Core.Domain.ManageResumes;
using App.Core.Domain.Messages;
using App.Core.Domain.News;
using App.Core.Domain.result;
using App.Core.Domain.Stores;
using Nop.Core.Domain.Customers;

namespace App.Services.Messages
{
    /// <summary>
    /// Message token provider
    /// </summary>
    public partial interface IMessageTokenProvider
    {
        /// <summary>
        /// Add store tokens
        /// </summary>
        /// <param name="tokens">List of already added tokens</param>
        /// <param name="store">Store</param>
        /// <param name="emailAccount">Email account</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task AddStoreTokensAsync(IList<Token> tokens, Store store, EmailAccount emailAccount);

        /// <summary>
        /// Add customer tokens
        /// </summary>
        /// <param name="tokens">List of already added tokens</param>
        /// <param name="customerId">Customer identifier</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task AddCustomerTokensAsync(IList<Token> tokens, int customerId);

        /// <summary>
        /// Add customer tokens
        /// </summary>
        /// <param name="tokens">List of already added tokens</param>
        /// <param name="customer">Customer</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task AddCustomerTokensAsync(IList<Token> tokens, Customer customer);
        /// <summary>
        /// Add Interviewer tokens
        /// </summary>
        /// <param name="tokens">List of already added tokens</param>
        /// <param name="customer">Customer</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task AddInterviewerTokensAsync(IList<Token> tokens, CandidatesResumes trainee);
        //Task AddTraineeTokensAsync(IList<Token> tokens, Candidate trainee);
        /// <summary>
        /// Add newsletter subscription tokens
        /// </summary>
        /// <param name="tokens">List of already added tokens</param>
        /// <param name="subscription">Newsletter subscription</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task AddNewsLetterSubscriptionTokensAsync(IList<Token> tokens, NewsLetterSubscription subscription);

        /// <summary>
        /// Add blog comment tokens
        /// </summary>
        /// <param name="tokens">List of already added tokens</param>
        /// <param name="blogComment">Blog post comment</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task AddBlogCommentTokensAsync(IList<Token> tokens, BlogComment blogComment);

        /// <summary>
        /// Add news comment tokens
        /// </summary>
        /// <param name="tokens">List of already added tokens</param>
        /// <param name="newsComment">News comment</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task AddNewsCommentTokensAsync(IList<Token> tokens, NewsComment newsComment);

        /// <summary>
        /// Add forum tokens
        /// </summary>
        /// <param name="tokens">List of already added tokens</param>
        /// <param name="forum">Forum</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task AddForumTokensAsync(IList<Token> tokens, Forum forum);

        /// <summary>
        /// Add forum topic tokens
        /// </summary>
        /// <param name="tokens">List of already added tokens</param>
        /// <param name="forumTopic">Forum topic</param>
        /// <param name="friendlyForumTopicPageIndex">Friendly (starts with 1) forum topic page to use for URL generation</param>
        /// <param name="appendedPostIdentifierAnchor">Forum post identifier</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task AddForumTopicTokensAsync(IList<Token> tokens, ForumTopic forumTopic,
            int? friendlyForumTopicPageIndex = null, int? appendedPostIdentifierAnchor = null);

        /// <summary>
        /// Add forum post tokens
        /// </summary>
        /// <param name="tokens">List of already added tokens</param>
        /// <param name="forumPost">Forum post</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task AddForumPostTokensAsync(IList<Token> tokens, ForumPost forumPost);

        /// <summary>
        /// Add private message tokens
        /// </summary>
        /// <param name="tokens">List of already added tokens</param>
        /// <param name="privateMessage">Private message</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task AddPrivateMessageTokensAsync(IList<Token> tokens, PrivateMessage privateMessage);

        /// <summary>
        /// Get collection of allowed (supported) message tokens for campaigns
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the collection of allowed (supported) message tokens for campaigns
        /// </returns>
        Task<IEnumerable<string>> GetListOfCampaignAllowedTokensAsync();

        /// <summary>
        /// Get collection of allowed (supported) message tokens
        /// </summary>
        /// <param name="tokenGroups">Collection of token groups; pass null to get all available tokens</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the collection of allowed message tokens
        /// </returns>
        Task<IEnumerable<string>> GetListOfAllowedTokensAsync(IEnumerable<string> tokenGroups = null);

        /// <summary>
        /// Get token groups of message template
        /// </summary>
        /// <param name="messageTemplate">Message template</param>
        /// <returns>Collection of token group names</returns>
        IEnumerable<string> GetTokenGroups(MessageTemplate messageTemplate);
        Task AddInterviewerToHrTokensAsync(IList<Token> tokens, CandidatesResult trainee);
        Task AddWeeklyupdateToHrTokensAsync(IList<Token> tokens, WeeklyReports trainee);
    }
}