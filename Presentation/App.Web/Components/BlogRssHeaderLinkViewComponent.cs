using Microsoft.AspNetCore.Mvc;
using App.Core.Domain.Blogs;
using App.Web.Framework.Components;

namespace App.Web.Components
{
    public partial class BlogRssHeaderLinkViewComponent : NopViewComponent
    {
        private readonly BlogSettings _blogSettings;

        public BlogRssHeaderLinkViewComponent(BlogSettings blogSettings)
        {
            _blogSettings = blogSettings;
        }

        public IViewComponentResult Invoke(int currentCategoryId, int currentProductId)
        {
            if (!_blogSettings.Enabled || !_blogSettings.ShowHeaderRssUrl)
                return Content("");

            return View();
        }
    }
}
