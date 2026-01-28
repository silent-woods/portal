using System.Collections.Generic;
using App.Web.Framework.Models;
using App.Web.Models.Common;

namespace App.Web.Models.Boards
{
    public partial record CustomerForumSubscriptionsModel : BaseNopModel
    {
        public CustomerForumSubscriptionsModel()
        {
            ForumSubscriptions = new List<ForumSubscriptionModel>();
        }

        public IList<ForumSubscriptionModel> ForumSubscriptions { get; set; }
        public PagerModel PagerModel { get; set; }

        #region Nested classes

        public partial record ForumSubscriptionModel : BaseNopEntityModel
        {
            public int ForumId { get; set; }
            public int ForumTopicId { get; set; }
            public bool TopicSubscription { get; set; }
            public string Title { get; set; }
            public string Slug { get; set; }
        }

        #endregion
    }
}