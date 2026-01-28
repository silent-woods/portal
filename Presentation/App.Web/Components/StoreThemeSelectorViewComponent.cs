using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using App.Core.Domain;
using App.Web.Factories;
using App.Web.Framework.Components;

namespace App.Web.Components
{
    public partial class StoreThemeSelectorViewComponent : NopViewComponent
    {
        private readonly ICommonModelFactory _commonModelFactory;
        private readonly StoreInformationSettings _storeInformationSettings;

        public StoreThemeSelectorViewComponent(ICommonModelFactory commonModelFactory,
            StoreInformationSettings storeInformationSettings)
        {
            _commonModelFactory = commonModelFactory;
            _storeInformationSettings = storeInformationSettings;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            if (!_storeInformationSettings.AllowCustomerToSelectTheme)
                return Content("");

            var model = await _commonModelFactory.PrepareStoreThemeSelectorModelAsync();
            return View(model);
        }
    }
}
