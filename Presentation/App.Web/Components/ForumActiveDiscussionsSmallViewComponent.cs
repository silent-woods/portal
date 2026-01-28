using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using App.Web.Factories;
using App.Web.Framework.Components;

namespace App.Web.Components
{
    public partial class ForumActiveDiscussionsSmallViewComponent : NopViewComponent
    {
        private readonly IForumModelFactory _forumModelFactory;

        public ForumActiveDiscussionsSmallViewComponent(IForumModelFactory forumModelFactory)
        {
            _forumModelFactory = forumModelFactory;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var model = await _forumModelFactory.PrepareActiveDiscussionsModelAsync();
            if (!model.ForumTopics.Any())
                return Content("");

            return View(model);
        }
    }
}
