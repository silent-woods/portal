using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using App.Services.Security;
using App.Web.Framework;
using App.Web.Framework.Controllers;
using App.Web.Framework.Mvc.Filters;

namespace Satyanam.Nop.Core.Controllers
{
    [AutoValidateAntiforgeryToken]
    [AuthorizeAdmin]
    [Area(AreaNames.Admin)]
    public class WebApiController : BasePluginController
    {
        #region Fields

        private readonly IPermissionService _permissionService;

        #endregion

        #region Ctor 

        public WebApiController(IPermissionService permissionService)
        {
            _permissionService = permissionService;
        }

        #endregion

        #region Methods

        public virtual async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            return View("~/Plugins/Misc.SatyanamNopCore/Views/Configure.cshtml");
        }

        #endregion
    }
}
