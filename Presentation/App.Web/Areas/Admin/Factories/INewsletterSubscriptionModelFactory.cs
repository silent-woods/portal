using System.Threading.Tasks;
using App.Web.Areas.Admin.Models.Messages;

namespace App.Web.Areas.Admin.Factories
{
    /// <summary>
    /// Represents the newsletter subscription model factory
    /// </summary>
    public partial interface INewsletterSubscriptionModelFactory
    {
        /// <summary>
        /// Prepare newsletter subscription search model
        /// </summary>
        /// <param name="searchModel">Newsletter subscription search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the newsletter subscription search model
        /// </returns>
        Task<NewsletterSubscriptionSearchModel> PrepareNewsletterSubscriptionSearchModelAsync(NewsletterSubscriptionSearchModel searchModel);

        /// <summary>
        /// Prepare paged newsletter subscription list model
        /// </summary>
        /// <param name="searchModel">Newsletter subscription search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the newsletter subscription list model
        /// </returns>
        Task<NewsletterSubscriptionListModel> PrepareNewsletterSubscriptionListModelAsync(NewsletterSubscriptionSearchModel searchModel);
    }
}