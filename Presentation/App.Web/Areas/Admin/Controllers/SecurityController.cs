using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using App.Core;
using App.Core.Domain.Security;
using App.Services.Customers;
using App.Services.Localization;
using App.Services.Logging;
using App.Services.Messages;
using App.Services.Security;
using App.Web.Areas.Admin.Factories;
using App.Web.Areas.Admin.Models.Security;

namespace App.Web.Areas.Admin.Controllers
{
    public partial class SecurityController : BaseAdminController
    {
        #region Fields

        private readonly ICustomerService _customerService;
        private readonly ILocalizationService _localizationService;
        private readonly ILogger _logger;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly ISecurityModelFactory _securityModelFactory;
        private readonly IWorkContext _workContext;

        #endregion

        #region Ctor

        public SecurityController(ICustomerService customerService,
            ILocalizationService localizationService,
            ILogger logger,
            INotificationService notificationService,
            IPermissionService permissionService,
            ISecurityModelFactory securityModelFactory,
            IWorkContext workContext)
        {
            _customerService = customerService;
            _localizationService = localizationService;
            _logger = logger;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _securityModelFactory = securityModelFactory;
            _workContext = workContext;
        }

        #endregion

        #region Methods

        public virtual async Task<IActionResult> AccessDenied(string pageUrl)
        {
            var currentCustomer = await _workContext.GetCurrentCustomerAsync();
            if (currentCustomer == null || await _customerService.IsGuestAsync(currentCustomer))
            {
                await _logger.InformationAsync($"Access denied to anonymous request on {pageUrl}");
                return View();
            }

            await _logger.InformationAsync($"Access denied to user #{currentCustomer.Email} '{currentCustomer.Email}' on {pageUrl}");

            return View();
        }

        public virtual async Task<IActionResult> Permissions()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAcl))
                return AccessDeniedView();

            //prepare model
            var model = await _securityModelFactory.PreparePermissionMappingModelAsync(new PermissionMappingModel());

            return View(model);
        }

        [HttpPost, ActionName("Permissions")]
        public virtual async Task<IActionResult> PermissionsSave(PermissionMappingModel model, IFormCollection form)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAcl))
                return AccessDeniedView();

            var permissionRecords = await _permissionService.GetAllPermissionRecordsAsync();
            var customerRoles = await _customerService.GetAllCustomerRolesAsync(true);

            foreach (var cr in customerRoles)
            {
                var formKey = "allow_" + cr.Id;
                var permissionRecordSystemNamesToRestrict = !StringValues.IsNullOrEmpty(form[formKey])
                    ? form[formKey].ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList()
                    : new List<string>();

                foreach (var pr in permissionRecords)
                {
                    var allow = permissionRecordSystemNamesToRestrict.Contains(pr.SystemName);

                    var permissionKey = pr.SystemName.Replace(".", "_");

                    bool add = form.ContainsKey($"perm_{permissionKey}_Add_{cr.Id}");
                    bool edit = form.ContainsKey($"perm_{permissionKey}_Edit_{cr.Id}");
                    bool delete = form.ContainsKey($"perm_{permissionKey}_Delete_{cr.Id}");
                    bool view = form.ContainsKey($"perm_{permissionKey}_View_{cr.Id}");
                    bool full = form.ContainsKey($"perm_{permissionKey}_Full_{cr.Id}");

                    if (full) add = edit = delete = view = true;

                    var existingPermissionRecord = await _permissionService.GetExistingActionsByPermissionAndRoleAsync(pr.Id, cr.Id);
                    if (existingPermissionRecord != null)
                    {
                        existingPermissionRecord.Add = add;
                        existingPermissionRecord.Edit = edit;
                        existingPermissionRecord.Delete = delete;
                        existingPermissionRecord.View = view;
                        existingPermissionRecord.Full = full;
                        existingPermissionRecord.UpdatedOnUtc = DateTime.UtcNow;
                        await _permissionService.UpdatePermissionRecordCustomerRoleMappingAsync(existingPermissionRecord);
                    }

                    if (allow == await _permissionService.AuthorizeAsync(pr.SystemName, cr.Id))
                        continue;

                    if (allow)
                    {
                        await _permissionService.InsertPermissionRecordCustomerRoleMappingAsync(new PermissionRecordCustomerRoleMapping
                        {
                            PermissionRecordId = pr.Id,
                            CustomerRoleId = cr.Id,
                            Add = add,
                            Edit = edit,
                            Delete = delete,
                            View = view,
                            Full = full,
                            CreatedOnUtc = DateTime.UtcNow,
                            UpdatedOnUtc = DateTime.UtcNow
                        });
                    }
                    else
                    {
                        await _permissionService.DeletePermissionRecordCustomerRoleMappingAsync(pr.Id, cr.Id);
                    }

                    await _permissionService.UpdatePermissionRecordAsync(pr);
                }
            }

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.ACL.Updated"));

            return RedirectToAction("Permissions");
        }

        #endregion

        #region Permission Record User Mapping Methods

        public virtual async Task<IActionResult> UserPermissions()
        {
            var model = await _securityModelFactory.PreparePermissionUserMappingModelAsync(new PermissionUserMappingModel());

            return View(model);
        }

        public virtual async Task<IActionResult> AddUserPermission()
        {
            var model = await _securityModelFactory.PreparePermissionUserRecordModelAsync(new PermissionUserRecordModel());

            return View(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> AddUserPermission(PermissionUserRecordModel model, bool continueEditing)
        {
            if (ModelState.IsValid)
            {
                var existingPermissionUserRecord = await _permissionService.GetExistingPermissionRecordByPermissionAndRoleAsync(model.PermissionId, model.UserId);
                if (existingPermissionUserRecord != null)
                {
                    _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Configuration.UserPermissions.PermissionAlreadyExists"));
                    return RedirectToAction(nameof(UserPermissions));
                }

                var permissionRecordUserMapping = new PermissionRecordUserMapping()
                {
                    PermissionId = model.PermissionId,
                    UserId = model.UserId,
                    Add = model.Add,
                    Edit = model.Edit,
                    Delete = model.Delete,
                    View = model.View,
                    Full = model.Full,
                    CreatedOnUtc = DateTime.UtcNow,
                    UpdatedOnUtc = DateTime.UtcNow
                };
                await _permissionService.InsertPermissionRecordUserMappingAsync(permissionRecordUserMapping);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.UserPermissions.Saved"));

                return RedirectToAction(nameof(UserPermissions));
            }

            model = await _securityModelFactory.PreparePermissionUserRecordModelAsync(model);

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateUserPermission(int userId, int permissionRecordId, bool add, bool edit, bool delete, bool view, bool full)
        {
            var existingPermissionUserRecord = await _permissionService.GetExistingPermissionRecordByPermissionAndRoleAsync(permissionRecordId, userId);
            if (existingPermissionUserRecord == null)
            {
                existingPermissionUserRecord = new PermissionRecordUserMapping
                {
                    UserId = userId,
                    PermissionId = permissionRecordId,
                    CreatedOnUtc = DateTime.UtcNow
                };

                await _permissionService.InsertPermissionRecordUserMappingAsync(existingPermissionUserRecord);
            }

            existingPermissionUserRecord.Add = add;
            existingPermissionUserRecord.Edit = edit;
            existingPermissionUserRecord.Delete = delete;
            existingPermissionUserRecord.View = view;

            if ((add || edit || delete) && !view)
                existingPermissionUserRecord.View = true;

            existingPermissionUserRecord.Full = existingPermissionUserRecord.Add && existingPermissionUserRecord.Edit && existingPermissionUserRecord.Delete && 
                existingPermissionUserRecord.View;
            existingPermissionUserRecord.UpdatedOnUtc = DateTime.UtcNow;

            await _permissionService.UpdatePermissionRecordUserMappingAsync(existingPermissionUserRecord);

            return Json(new
            {
                add = existingPermissionUserRecord.Add,
                edit = existingPermissionUserRecord.Edit,
                delete = existingPermissionUserRecord.Delete,
                view = existingPermissionUserRecord.View,
                full = existingPermissionUserRecord.Full
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUserPermission(int userId, int permissionRecordId)
        {
            var existingPermissionUserRecord = await _permissionService.GetExistingPermissionRecordByPermissionAndRoleAsync(permissionRecordId, userId);

            await _permissionService.DeletePermissionRecordUserMappingAsync(existingPermissionUserRecord);

            return Ok();
        }


        #endregion
    }
}