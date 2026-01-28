using System.Threading.Tasks;
using App.Core.Caching;
using App.Core.Domain.Blogs;
using App.Core.Domain.Catalog;
using App.Core.Domain.Configuration;
using App.Core.Domain.Localization;
using App.Core.Domain.Media;
using App.Core.Domain.News;
using App.Core.Domain.Polls;
using App.Core.Domain.Topics;
using App.Core.Events;
using App.Services.Cms;
using App.Services.Events;
using App.Services.Plugins;

namespace App.Web.Infrastructure.Cache
{
    /// <summary>
    /// Model cache event consumer (used for caching of presentation layer models)
    /// </summary>
    public partial class ModelCacheEventConsumer :
        //settings
        IConsumer<EntityUpdatedEvent<Setting>>,
        //Topics
        IConsumer<EntityInsertedEvent<Topic>>,
        IConsumer<EntityUpdatedEvent<Topic>>,
        IConsumer<EntityDeletedEvent<Topic>>,
        //polls
        IConsumer<EntityInsertedEvent<Poll>>,
        IConsumer<EntityUpdatedEvent<Poll>>,
        IConsumer<EntityDeletedEvent<Poll>>,
        //blog posts
        IConsumer<EntityInsertedEvent<BlogPost>>,
        IConsumer<EntityUpdatedEvent<BlogPost>>,
        IConsumer<EntityDeletedEvent<BlogPost>>,
        //news items
        IConsumer<EntityInsertedEvent<NewsItem>>,
        IConsumer<EntityUpdatedEvent<NewsItem>>,
        IConsumer<EntityDeletedEvent<NewsItem>>,
        //plugins
        IConsumer<PluginUpdatedEvent>
    {
        #region Fields

        private readonly IStaticCacheManager _staticCacheManager;

        #endregion

        #region Ctor

        public ModelCacheEventConsumer(IStaticCacheManager staticCacheManager)
        {
            _staticCacheManager = staticCacheManager;
        }

        #endregion

        #region Methods

        #region Setting

        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task HandleEventAsync(EntityUpdatedEvent<Setting> eventMessage)
        {
            await _staticCacheManager.RemoveByPrefixAsync(NopModelCacheDefaults.HomepageBestsellersIdsPrefixCacheKey); //depends on CatalogSettings.NumberOfBestsellersOnHomepage
            await _staticCacheManager.RemoveByPrefixAsync(NopModelCacheDefaults.ProductsAlsoPurchasedIdsPrefixCacheKey); //depends on CatalogSettings.ProductsAlsoPurchasedNumber
            await _staticCacheManager.RemoveByPrefixAsync(NopModelCacheDefaults.BlogPrefixCacheKey); //depends on BlogSettings.NumberOfTags
            await _staticCacheManager.RemoveByPrefixAsync(NopModelCacheDefaults.NewsPrefixCacheKey); //depends on NewsSettings.MainPageNewsCount
            await _staticCacheManager.RemoveByPrefixAsync(NopModelCacheDefaults.SitemapPrefixCacheKey); //depends on distinct sitemap settings
            await _staticCacheManager.RemoveByPrefixAsync(NopModelCacheDefaults.WidgetPrefixCacheKey); //depends on WidgetSettings and certain settings of widgets
            await _staticCacheManager.RemoveByPrefixAsync(NopModelCacheDefaults.StoreLogoPathPrefixCacheKey); //depends on StoreInformationSettings.LogoPictureId
        }

        #endregion

        #region Topics

        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task HandleEventAsync(EntityInsertedEvent<Topic> eventMessage)
        {
            await _staticCacheManager.RemoveByPrefixAsync(NopModelCacheDefaults.SitemapPrefixCacheKey);
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task HandleEventAsync(EntityUpdatedEvent<Topic> eventMessage)
        {
            await _staticCacheManager.RemoveByPrefixAsync(NopModelCacheDefaults.SitemapPrefixCacheKey);
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task HandleEventAsync(EntityDeletedEvent<Topic> eventMessage)
        {
            await _staticCacheManager.RemoveByPrefixAsync(NopModelCacheDefaults.SitemapPrefixCacheKey);
        }

        #endregion

        #region Polls

        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task HandleEventAsync(EntityInsertedEvent<Poll> eventMessage)
        {
            await _staticCacheManager.RemoveByPrefixAsync(NopModelCacheDefaults.PollsPrefixCacheKey);
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task HandleEventAsync(EntityUpdatedEvent<Poll> eventMessage)
        {
            await _staticCacheManager.RemoveByPrefixAsync(NopModelCacheDefaults.PollsPrefixCacheKey);
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task HandleEventAsync(EntityDeletedEvent<Poll> eventMessage)
        {
            await _staticCacheManager.RemoveByPrefixAsync(NopModelCacheDefaults.PollsPrefixCacheKey);
        }

        #endregion

        #region Blog posts

        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task HandleEventAsync(EntityInsertedEvent<BlogPost> eventMessage)
        {
            await _staticCacheManager.RemoveByPrefixAsync(NopModelCacheDefaults.BlogPrefixCacheKey);
            await _staticCacheManager.RemoveByPrefixAsync(NopModelCacheDefaults.SitemapPrefixCacheKey);
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task HandleEventAsync(EntityUpdatedEvent<BlogPost> eventMessage)
        {
            await _staticCacheManager.RemoveByPrefixAsync(NopModelCacheDefaults.BlogPrefixCacheKey);
            await _staticCacheManager.RemoveByPrefixAsync(NopModelCacheDefaults.SitemapPrefixCacheKey);
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task HandleEventAsync(EntityDeletedEvent<BlogPost> eventMessage)
        {
            await _staticCacheManager.RemoveByPrefixAsync(NopModelCacheDefaults.BlogPrefixCacheKey);
            await _staticCacheManager.RemoveByPrefixAsync(NopModelCacheDefaults.SitemapPrefixCacheKey);
        }

        #endregion

        #region News items

        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task HandleEventAsync(EntityInsertedEvent<NewsItem> eventMessage)
        {
            await _staticCacheManager.RemoveByPrefixAsync(NopModelCacheDefaults.NewsPrefixCacheKey);
            await _staticCacheManager.RemoveByPrefixAsync(NopModelCacheDefaults.SitemapPrefixCacheKey);
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task HandleEventAsync(EntityUpdatedEvent<NewsItem> eventMessage)
        {
            await _staticCacheManager.RemoveByPrefixAsync(NopModelCacheDefaults.NewsPrefixCacheKey);
            await _staticCacheManager.RemoveByPrefixAsync(NopModelCacheDefaults.SitemapPrefixCacheKey);
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task HandleEventAsync(EntityDeletedEvent<NewsItem> eventMessage)
        {
            await _staticCacheManager.RemoveByPrefixAsync(NopModelCacheDefaults.NewsPrefixCacheKey);
            await _staticCacheManager.RemoveByPrefixAsync(NopModelCacheDefaults.SitemapPrefixCacheKey);
        }

        #endregion

        #region Plugin

        /// <summary>
        /// Handle plugin updated event
        /// </summary>
        /// <param name="eventMessage">Event message</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task HandleEventAsync(PluginUpdatedEvent eventMessage)
        {
            if (eventMessage?.Plugin?.Instance<IWidgetPlugin>() != null)
                await _staticCacheManager.RemoveByPrefixAsync(NopModelCacheDefaults.WidgetPrefixCacheKey);
        }

        #endregion

        #endregion
    }
}