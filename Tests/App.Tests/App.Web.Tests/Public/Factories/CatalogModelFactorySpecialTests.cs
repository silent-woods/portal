using System.Threading.Tasks;
using App.Core.Domain.Catalog;
using App.Services.Configuration;
using NUnit.Framework;

namespace App.Tests.App.Web.Tests.Public.Factories
{
    [TestFixture]
    public class CatalogModelFactorySpecialTests: WebTest
    {
        private ISettingService _settingsService;
        private CatalogSettings _catalogSettings;
     
        [OneTimeSetUp]
        public async Task SetUp()
        {
            _settingsService = GetService<ISettingService>();
          
            _catalogSettings = GetService<CatalogSettings>();

            _catalogSettings.AllowProductViewModeChanging = false;
            _catalogSettings.CategoryBreadcrumbEnabled = false;
            _catalogSettings.ShowProductsFromSubcategories = true;
            _catalogSettings.ShowCategoryProductNumber = true;
            _catalogSettings.ShowCategoryProductNumberIncludingSubcategories = true;
            _catalogSettings.NumberOfProductTags = 20;

            await _settingsService.SaveSettingAsync(_catalogSettings);

        }

        [OneTimeTearDown]
        public async Task TearDown()
        {
            _catalogSettings.AllowProductViewModeChanging = true;
            _catalogSettings.CategoryBreadcrumbEnabled = true;
            _catalogSettings.ShowProductsFromSubcategories = false;
            _catalogSettings.ShowCategoryProductNumber = false;
            _catalogSettings.ShowCategoryProductNumberIncludingSubcategories = false;
            _catalogSettings.NumberOfProductTags = 15;
            await _settingsService.SaveSettingAsync(_catalogSettings);
        }
    }
}