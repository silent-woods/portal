using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using App.Web.Factories;
using App.Web.Framework.Components;

namespace App.Web.Components
{
    public partial class PollBlockViewComponent : NopViewComponent
    {
        private readonly IPollModelFactory _pollModelFactory;

        public PollBlockViewComponent(IPollModelFactory pollModelFactory)
        {
            _pollModelFactory = pollModelFactory;
        }

        public async Task<IViewComponentResult> InvokeAsync(string systemKeyword)
        {

            if (string.IsNullOrWhiteSpace(systemKeyword))
                return Content("");

            var model = await _pollModelFactory.PreparePollModelBySystemNameAsync(systemKeyword);
            if (model == null)
                return Content("");

            return View(model);
        }
    }
}
