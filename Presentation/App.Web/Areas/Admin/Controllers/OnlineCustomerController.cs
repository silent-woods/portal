using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using App.Services.Security;
using App.Web.Areas.Admin.Factories;
using App.Web.Areas.Admin.Models.Customers;

namespace App.Web.Areas.Admin.Controllers
{
    public partial class OnlineCustomerController : BaseAdminController
    {
        #region Fields

        private readonly ICustomerModelFactory _customerModelFactory;
        private readonly IPermissionService _permissionService;

        #endregion

        #region Ctor

        public OnlineCustomerController(ICustomerModelFactory customerModelFactory,
            IPermissionService permissionService)
        {
            _customerModelFactory = customerModelFactory;
            _permissionService = permissionService;
        }

        #endregion
        
        #region Methods

        public virtual async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            //prepare model
            var model = await _customerModelFactory.PrepareOnlineCustomerSearchModelAsync(new OnlineCustomerSearchModel());

            return View(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> List(OnlineCustomerSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCustomers))
                return await AccessDeniedDataTablesJson();

            //prepare model
            var model = await _customerModelFactory.PrepareOnlineCustomerListModelAsync(searchModel);

            return Json(model);
        }

        #endregion
    }
}