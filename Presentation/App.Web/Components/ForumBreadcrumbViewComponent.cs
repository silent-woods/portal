using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using App.Web.Factories;
using App.Web.Framework.Components;

namespace App.Web.Components
{
    public partial class ForumBreadcrumbViewComponent : NopViewComponent
    {
        private readonly IForumModelFactory _forumModelFactory;

        public ForumBreadcrumbViewComponent(IForumModelFactory forumModelFactory)
        {
            _forumModelFactory = forumModelFactory;
        }

        public async Task<IViewComponentResult> InvokeAsync(int? forumGroupId, int? forumId, int? forumTopicId)
        {
            var model = await _forumModelFactory.PrepareForumBreadcrumbModelAsync(forumGroupId, forumId, forumTopicId);
            return View(model);
        }
    }
}
