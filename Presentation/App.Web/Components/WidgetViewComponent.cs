using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using App.Web.Factories;
using App.Web.Framework.Components;

namespace App.Web.Components
{
    public partial class WidgetViewComponent : NopViewComponent
    {
        private readonly IWidgetModelFactory _widgetModelFactory;

        public WidgetViewComponent(IWidgetModelFactory widgetModelFactory)
        {
            _widgetModelFactory = widgetModelFactory;
        }

        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData = null)
        {
            var model = await _widgetModelFactory.PrepareRenderWidgetModelAsync(widgetZone, additionalData);

            //no data?
            if (!model.Any())
                return Content("");

            return View(model);
        }
    }
}