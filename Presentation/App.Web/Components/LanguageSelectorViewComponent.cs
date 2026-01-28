using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using App.Web.Factories;
using App.Web.Framework.Components;

namespace App.Web.Components
{
    public partial class LanguageSelectorViewComponent : NopViewComponent
    {
        private readonly ICommonModelFactory _commonModelFactory;

        public LanguageSelectorViewComponent(ICommonModelFactory commonModelFactory)
        {
            _commonModelFactory = commonModelFactory;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var model = await _commonModelFactory.PrepareLanguageSelectorModelAsync();

            if (model.AvailableLanguages.Count == 1)
                return Content("");

            return View(model);
        }
    }
}
