using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using App.Services.Security;
using App.Web.Framework;
using App.Web.Framework.Controllers;
using App.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Misc.WebApi.Frontend.Controllers
{
    [AutoValidateAntiforgeryToken]
    [AuthorizeAdmin]
    [Area(AreaNames.Admin)]
    public class WebApiFrontendController : BasePluginController
    {
        #region Fields

        private readonly IPermissionService _permissionService;

        #endregion

        #region Ctor 

        public WebApiFrontendController(IPermissionService permissionService)
        {
            _permissionService = permissionService;
        }

        #endregion

        #region Methods

        public virtual async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            return View("~/Plugins/Misc.WebApi.Frontend/Views/Configure.cshtml");
        }

        #endregion
    }
}
