using App.Core.Domain.Designations;
using App.Services.Designations;
using App.Services.Localization;
using App.Services.Messages;
using App.Services.Security;
using App.Web.Areas.Admin.Factories;
using App.Web.Areas.Admin.Models.Designation;
using App.Web.Framework.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App.Core.Domain.Security;

namespace App.Web.Areas.Admin.Controllers
{
    public partial class DesignationController : BaseAdminController
    {
        #region Fields
        private readonly IPermissionService _permissionService;
        private readonly IDesignationModelFactory _designationModelFactory;
        private readonly IDesignationService _designationService;
        private readonly INotificationService _notificationService;
        private readonly ILocalizationService _localizationService;
        #endregion

        #region Ctor

        public DesignationController(IPermissionService permissionService,
            IDesignationModelFactory designationModelFactory,
            IDesignationService designationService,
            INotificationService notificationService,
            ILocalizationService localizationService
            )
        {
            _permissionService = permissionService;
            _designationModelFactory = designationModelFactory;
            _designationService = designationService;
            _notificationService = notificationService;
            _localizationService = localizationService;
        }

        #endregion

        #region Methods

        public virtual async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageDesignation, PermissionAction.View))
                return AccessDeniedView();

            if (TempData["SuccessMessage"] != null)
            {
                ViewBag.SuccessMessage = TempData["SuccessMessage"].ToString();
                _notificationService.SuccessNotification(ViewBag.SuccessMessage);
            }
            //prepare model
            var model = await _designationModelFactory.PrepareDesignationSearchModelAsync(new DesignationSearchModel());
            return View("/Areas/Admin/Views/Extension/Designation/List.cshtml", model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> List(DesignationSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageDesignation, PermissionAction.View))
                return AccessDeniedView();

            //prepare model
            var model = await _designationModelFactory.PrepareDesignationListModelAsync(searchModel);
            return Json(model);
        }

        public virtual async Task<IActionResult> Create()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageDesignation, PermissionAction.Add))
                return AccessDeniedView();

            //prepare model
            var model = await _designationModelFactory.PrepareDesignationModelAsync(new DesignationModel(), null);
            ViewBag.RefreshPage = false;

            return View("/Areas/Admin/Views/Extension/Designation/Create.cshtml", model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Create(DesignationModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageDesignation, PermissionAction.Add))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                Designation designation = new Designation();
                designation.Name = model.Name;
                designation.CanGiveRatings = model.CanGiveRatings;
                designation.CreateOnUtc = DateTime.UtcNow;
                designation.UpdateOnUtc = DateTime.UtcNow;

                await _designationService.InsertDesignationAsync(designation);


                //_notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.designation.Added"));

                string successMessage = await _localizationService.GetResourceAsync("Admin.Catalog.designation.Added");
                TempData["SuccessMessage"] = successMessage;

                ViewBag.RefreshPage = true;
            }
            //if we got this far, something failed, redisplay form

            return View("/Areas/Admin/Views/Extension/Designation/Create.cshtml", model);
        }

        public virtual async Task<IActionResult> Edit(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageDesignation, PermissionAction.Edit))
                return AccessDeniedView();

            //try to get a _designation with the specified id
            var designation = await _designationService.GetDesignationByIdAsync(id);
            if (designation == null)
                return RedirectToAction("List");

            //prepare model
            var model = await _designationModelFactory.PrepareDesignationModelAsync(null, designation);

            return View("/Areas/Admin/Views/Extension/Designation/Edit.cshtml", model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Edit(DesignationModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageDesignation, PermissionAction.Edit))
                return AccessDeniedView();

            //try to get a designation with the specified id
            var designation = await _designationService.GetDesignationByIdAsync(model.Id);
            if (designation == null)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                designation.Name = model.Name;
                designation.UpdateOnUtc = DateTime.UtcNow;
                designation.CanGiveRatings = model.CanGiveRatings;

                await _designationService.UpdateDesignationAsync(designation);

                //_notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.designation.Updated"));

                string successMessage = await _localizationService.GetResourceAsync("Admin.Catalog.designation.Updated");
                TempData["SuccessMessage"] = successMessage;

                ViewBag.RefreshPage = true;
            }

            //if we got this far, something failed, redisplay form
            return View("/Areas/Admin/Views/Extension/Designation/Edit.cshtml", model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageDesignation, PermissionAction.Delete))
                return AccessDeniedView();

            //try to get a designation with the specified id
            var designation = await _designationService.GetDesignationByIdAsync(id);
            if (designation == null)
                return RedirectToAction("List");

            await _designationService.DeleteDesignationAsync(designation);
            _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Designation.Deleted"));

            return RedirectToAction("List");
        }

        [HttpPost]
        public virtual async Task<IActionResult> DeleteSelected(ICollection<int> selectedIds)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageDesignation, PermissionAction.Delete))
                return AccessDeniedView();

            if (selectedIds == null || selectedIds.Count == 0)
                return NoContent();

            var data = await _designationService.GetDesignationByIdsAsync(selectedIds.ToArray());

            foreach (var item in data)
            {
                await _designationService.DeleteDesignationAsync(item);
            }

            return Json(new { Result = true });
        }

        #endregion
    }
}