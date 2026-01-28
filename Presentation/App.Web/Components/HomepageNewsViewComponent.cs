using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using App.Core.Domain.News;
using App.Web.Factories;
using App.Web.Framework.Components;

namespace App.Web.Components
{
    public partial class HomepageNewsViewComponent : NopViewComponent
    {
        private readonly INewsModelFactory _newsModelFactory;
        private readonly NewsSettings _newsSettings;

        public HomepageNewsViewComponent(INewsModelFactory newsModelFactory, NewsSettings newsSettings)
        {
            _newsModelFactory = newsModelFactory;
            _newsSettings = newsSettings;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            if (!_newsSettings.Enabled || !_newsSettings.ShowNewsOnMainPage)
                return Content("");

            var model = await _newsModelFactory.PrepareHomepageNewsItemsModelAsync();
            return View(model);
        }
    }
}
