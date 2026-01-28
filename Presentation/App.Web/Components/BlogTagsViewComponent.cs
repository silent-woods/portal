using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using App.Core.Domain.Blogs;
using App.Web.Factories;
using App.Web.Framework.Components;

namespace App.Web.Components
{
    public partial class BlogTagsViewComponent : NopViewComponent
    {
        private readonly BlogSettings _blogSettings;
        private readonly IBlogModelFactory _blogModelFactory;

        public BlogTagsViewComponent(BlogSettings blogSettings, IBlogModelFactory blogModelFactory)
        {
            _blogSettings = blogSettings;
            _blogModelFactory = blogModelFactory;
        }

        public async Task<IViewComponentResult> InvokeAsync(int currentCategoryId, int currentProductId)
        {
            if (!_blogSettings.Enabled)
                return Content("");

            var model = await _blogModelFactory.PrepareBlogPostTagListModelAsync();
            return View(model);
        }
    }
}
