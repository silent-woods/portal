using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using App.Core;
using App.Core.Domain.Customers;
using App.Services.Catalog;
using App.Services.Customers;
using App.Services.Localization;
using App.Services.Logging;
using App.Services.Messages;
using App.Services.Security;
using App.Web.Areas.Admin.Factories;
using App.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using App.Web.Areas.Admin.Models.Customers;
using App.Web.Framework.Controllers;
using App.Web.Framework.Mvc.Filters;

namespace App.Web.Areas.Admin.Controllers
{
    public partial class CustomerRoleController : BaseAdminController
    {
        #region Fields

        private readonly ICustomerActivityService _customerActivityService;
        private readonly ICustomerRoleModelFactory _customerRoleModelFactory;
        private readonly ICustomerService _customerService;
        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly IWorkContext _workContext;

        #endregion

        #region Ctor

        public CustomerRoleController(ICustomerActivityService customerActivityService,
            ICustomerRoleModelFactory customerRoleModelFactory,
            ICustomerService customerService,
            ILocalizationService localizationService,
            INotificationService notificationService,
            IPermissionService permissionService,
            IWorkContext workContext)
        {
            _customerActivityService = customerActivityService;
            _customerRoleModelFactory = customerRoleModelFactory;
            _customerService = customerService;
            _localizationService = localizationService;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _workContext = workContext;
        }

        #endregion

        #region Methods

        public virtual IActionResult Index()
        {
            return RedirectToAction("List");
        }

        public virtual async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            //prepare model
            var model = await _customerRoleModelFactory.PrepareCustomerRoleSearchModelAsync(new CustomerRoleSearchModel());

            return View(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> List(CustomerRoleSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCustomers))
                return await AccessDeniedDataTablesJson();

            //prepare model
            var model = await _customerRoleModelFactory.PrepareCustomerRoleListModelAsync(searchModel);

            return Json(model);
        }

        public virtual async Task<IActionResult> Create()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCustomers) || !await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAcl))
                return AccessDeniedView();

            //prepare model
            var model = await _customerRoleModelFactory.PrepareCustomerRoleModelAsync(new CustomerRoleModel(), null);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Create(CustomerRoleModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCustomers) || !await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAcl))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var customerRole = model.ToEntity<CustomerRole>();
                await _customerService.InsertCustomerRoleAsync(customerRole);

                //activity log
                await _customerActivityService.InsertActivityAsync("AddNewCustomerRole",
                    string.Format(await _localizationService.GetResourceAsync("ActivityLog.AddNewCustomerRole"), customerRole.Name), customerRole);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Customers.CustomerRoles.Added"));

                return continueEditing ? RedirectToAction("Edit", new { id = customerRole.Id }) : RedirectToAction("List");
            }

            //prepare model
            model = await _customerRoleModelFactory.PrepareCustomerRoleModelAsync(model, null, true);

            //if we got this far, something failed, redisplay form
            return View(model);
        }

        public virtual async Task<IActionResult> Edit(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCustomers) || !await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAcl))
                return AccessDeniedView();

            //try to get a customer role with the specified id
            var customerRole = await _customerService.GetCustomerRoleByIdAsync(id);
            if (customerRole == null)
                return RedirectToAction("List");

            //prepare model
            var model = await _customerRoleModelFactory.PrepareCustomerRoleModelAsync(null, customerRole);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Edit(CustomerRoleModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCustomers) || !await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAcl))
                return AccessDeniedView();

            //try to get a customer role with the specified id
            var customerRole = await _customerService.GetCustomerRoleByIdAsync(model.Id);
            if (customerRole == null)
                return RedirectToAction("List");

            try
            {
                if (ModelState.IsValid)
                {
                    if (customerRole.IsSystemRole && !model.Active)
                        throw new NopException(await _localizationService.GetResourceAsync("Admin.Customers.CustomerRoles.Fields.Active.CantEditSystem"));

                    if (customerRole.IsSystemRole && !customerRole.SystemName.Equals(model.SystemName, StringComparison.InvariantCultureIgnoreCase))
                        throw new NopException(await _localizationService.GetResourceAsync("Admin.Customers.CustomerRoles.Fields.SystemName.CantEditSystem"));

                    //if (NopCustomerDefaults.RegisteredRoleName.Equals(customerRole.SystemName, StringComparison.InvariantCultureIgnoreCase) &&
                    //    model.PurchasedWithProductId > 0)
                    //    throw new NopException(await _localizationService.GetResourceAsync("Admin.Customers.CustomerRoles.Fields.PurchasedWithProduct.Registered"));

                    customerRole = model.ToEntity(customerRole);
                    await _customerService.UpdateCustomerRoleAsync(customerRole);

                    //activity log
                    await _customerActivityService.InsertActivityAsync("EditCustomerRole",
                        string.Format(await _localizationService.GetResourceAsync("ActivityLog.EditCustomerRole"), customerRole.Name), customerRole);

                    _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Customers.CustomerRoles.Updated"));

                    return continueEditing ? RedirectToAction("Edit", new { id = customerRole.Id }) : RedirectToAction("List");
                }

                //prepare model
                model = await _customerRoleModelFactory.PrepareCustomerRoleModelAsync(model, customerRole, true);

                //if we got this far, something failed, redisplay form
                return View(model);
            }
            catch (Exception exc)
            {
                await _notificationService.ErrorNotificationAsync(exc);
                return RedirectToAction("Edit", new { id = customerRole.Id });
            }
        }

        [HttpPost]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCustomers) || !await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAcl))
                return AccessDeniedView();

            //try to get a customer role with the specified id
            var customerRole = await _customerService.GetCustomerRoleByIdAsync(id);
            if (customerRole == null)
                return RedirectToAction("List");

            try
            {
                await _customerService.DeleteCustomerRoleAsync(customerRole);

                //activity log
                await _customerActivityService.InsertActivityAsync("DeleteCustomerRole",
                    string.Format(await _localizationService.GetResourceAsync("ActivityLog.DeleteCustomerRole"), customerRole.Name), customerRole);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Customers.CustomerRoles.Deleted"));

                return RedirectToAction("List");
            }
            catch (Exception exc)
            {
                _notificationService.ErrorNotification(exc.Message);
                return RedirectToAction("Edit", new { id = customerRole.Id });
            }
        }

        #endregion
    }
}