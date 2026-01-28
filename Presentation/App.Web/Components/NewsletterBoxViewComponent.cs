using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using App.Core.Domain.Customers;
using App.Web.Factories;
using App.Web.Framework.Components;

namespace App.Web.Components
{
    public partial class NewsletterBoxViewComponent : NopViewComponent
    {
        private readonly CustomerSettings _customerSettings;
        private readonly INewsletterModelFactory _newsletterModelFactory;

        public NewsletterBoxViewComponent(CustomerSettings customerSettings, INewsletterModelFactory newsletterModelFactory)
        {
            _customerSettings = customerSettings;
            _newsletterModelFactory = newsletterModelFactory;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            if (_customerSettings.HideNewsletterBlock)
                return Content("");

            var model = await _newsletterModelFactory.PrepareNewsletterBoxModelAsync();
            return View(model);
        }
    }
}
