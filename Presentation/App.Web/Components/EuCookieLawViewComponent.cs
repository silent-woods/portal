using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using App.Core;
using App.Core.Domain;
using App.Core.Domain.Customers;
using App.Core.Http;
using App.Services.Common;
using App.Web.Framework.Components;

namespace App.Web.Components
{
    public partial class EuCookieLawViewComponent : NopViewComponent
    {
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;
        private readonly StoreInformationSettings _storeInformationSettings;

        public EuCookieLawViewComponent(IGenericAttributeService genericAttributeService,
            IStoreContext storeContext,
            IWorkContext workContext,
            StoreInformationSettings storeInformationSettings)
        {
            _genericAttributeService = genericAttributeService;
            _storeContext = storeContext;
            _workContext = workContext;
            _storeInformationSettings = storeInformationSettings;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            if (!_storeInformationSettings.DisplayEuCookieLawWarning)
                //disabled
                return Content("");

            //ignore search engines because some pages could be indexed with the EU cookie as description
            var customer = await _workContext.GetCurrentCustomerAsync();
            if (customer.IsSearchEngineAccount())
                return Content("");

            var store = await _storeContext.GetCurrentStoreAsync();

            if (await _genericAttributeService.GetAttributeAsync<bool>(customer, NopCustomerDefaults.EuCookieLawAcceptedAttribute, store.Id))
                //already accepted
                return Content("");

            //ignore notification?
            //right now it's used during logout so popup window is not displayed twice
            if (TempData[$"{NopCookieDefaults.Prefix}{NopCookieDefaults.IgnoreEuCookieLawWarning}"] != null && Convert.ToBoolean(TempData[$"{NopCookieDefaults.Prefix}{NopCookieDefaults.IgnoreEuCookieLawWarning}"]))
                return Content("");

            return View();
        }
    }
}