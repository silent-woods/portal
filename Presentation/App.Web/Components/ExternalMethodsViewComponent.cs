using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using App.Web.Factories;
using App.Web.Framework.Components;

namespace App.Web.Components
{
    public partial class ExternalMethodsViewComponent : NopViewComponent
    {
        #region Fields

        private readonly IExternalAuthenticationModelFactory _externalAuthenticationModelFactory;

        #endregion

        #region Ctor

        public ExternalMethodsViewComponent(IExternalAuthenticationModelFactory externalAuthenticationModelFactory)
        {
            _externalAuthenticationModelFactory = externalAuthenticationModelFactory;
        }

        #endregion

        #region Methods

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var model = await _externalAuthenticationModelFactory.PrepareExternalMethodsModelAsync();

            return View(model);
        }

        #endregion
    }
}
