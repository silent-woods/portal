using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using App.Core;
using App.Core.Domain.Blogs;
using App.Core.Domain.Catalog;
using App.Core.Domain.News;
using App.Core.Domain.Seo;
using App.Core.Domain.Topics;
using App.Services.Seo;
using App.Services.Topics;

namespace App.Web.Framework.Mvc.Routing
{
    /// <summary>
    /// Represents the helper implementation to build specific URLs within an application 
    /// </summary>
    public partial class NopUrlHelper : INopUrlHelper
    {
        #region Fields

        private readonly CatalogSettings _catalogSettings;
        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly IStoreContext _storeContext;
        private readonly ITopicService _topicService;
        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly IUrlRecordService _urlRecordService;

        #endregion

        #region Ctor

        public NopUrlHelper(CatalogSettings catalogSettings,
            IActionContextAccessor actionContextAccessor,
            IStoreContext storeContext,
            ITopicService topicService,
            IUrlHelperFactory urlHelperFactory,
            IUrlRecordService urlRecordService)
        {
            _catalogSettings = catalogSettings;
            _actionContextAccessor = actionContextAccessor;
            _storeContext = storeContext;
            _topicService = topicService;
            _urlHelperFactory = urlHelperFactory;
            _urlRecordService = urlRecordService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Generate a generic URL for the specified entity type and route values
        /// </summary>
        /// <typeparam name="TEntity">Entity type that supports slug</typeparam>
        /// <param name="values">An object that contains route values</param>
        /// <param name="protocol">The protocol for the URL, such as "http" or "https"</param>
        /// <param name="host">The host name for the URL</param>
        /// <param name="fragment">The fragment for the URL</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the generated URL
        /// </returns>
        public virtual async Task<string> RouteGenericUrlAsync<TEntity>(object values = null, string protocol = null, string host = null, string fragment = null)
            where TEntity : BaseEntity, ISlugSupported
        {
            var urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);
            return typeof(TEntity) switch
            {
                var entityType when entityType == typeof(NewsItem)
                    => urlHelper.RouteUrl(NopRoutingDefaults.RouteName.Generic.NewsItem, values, protocol, host, fragment),
                var entityType when entityType == typeof(BlogPost)
                    => urlHelper.RouteUrl(NopRoutingDefaults.RouteName.Generic.BlogPost, values, protocol, host, fragment),
                var entityType when entityType == typeof(Topic)
                    => urlHelper.RouteUrl(NopRoutingDefaults.RouteName.Generic.Topic, values, protocol, host, fragment),
                var entityType => urlHelper.RouteUrl(entityType.Name, values, protocol, host, fragment)
            };
        }

        /// <summary>
        /// Generate a URL for topic by the specified system name
        /// </summary>
        /// <param name="systemName">Topic system name</param>
        /// <param name="protocol">The protocol for the URL, such as "http" or "https"</param>
        /// <param name="host">The host name for the URL</param>
        /// <param name="fragment">The fragment for the URL</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the generated URL
        /// </returns>
        public virtual async Task<string> RouteTopicUrlAsync(string systemName, string protocol = null, string host = null, string fragment = null)
        {
            var store = await _storeContext.GetCurrentStoreAsync();
            var topic = await _topicService.GetTopicBySystemNameAsync(systemName, store.Id);
            if (topic is null)
                return string.Empty;

            var seName = await _urlRecordService.GetSeNameAsync(topic);
            return await RouteGenericUrlAsync<Topic>(new { SeName = seName }, protocol, host, fragment);
        }

        #endregion
    }
}