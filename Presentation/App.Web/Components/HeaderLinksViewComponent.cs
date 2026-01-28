using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using App.Web.Factories;
using App.Web.Framework.Components;

namespace App.Web.Components
{
    public partial class HeaderLinksViewComponent : NopViewComponent
    {
        private readonly ICommonModelFactory _commonModelFactory;

        public HeaderLinksViewComponent(ICommonModelFactory commonModelFactory)
        {
            _commonModelFactory = commonModelFactory;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var model = await _commonModelFactory.PrepareHeaderLinksModelAsync();
            return View(model);
        }
    }
}
