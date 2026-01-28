using App.Core.Domain.PerformanceMeasurements;
using App.Services.Localization;
using App.Services.Messages;
using App.Services.PerformanceMeasurements;
using App.Services.Security;
using App.Web.Areas.Admin.Factories;
using App.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using App.Web.Areas.Admin.Models.PerformanceMeasurements;
using App.Web.Framework.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App.Core.Domain.Security;

namespace App.Web.Areas.Admin.Controllers
{
    public partial class KPIMasterController : BaseAdminController
    {
        #region Fields
        private readonly IPermissionService _permissionService;
        private readonly IKPIMasterModelFactory _kPIMasterModelFactory;
        private readonly IKPIMasterService _kPIMasterService;
        private readonly INotificationService _notificationService;
        private readonly ILocalizationService _localizationService;
        #endregion

        #region Ctor

        public KPIMasterController(IPermissionService permissionService,
            IKPIMasterModelFactory kPIMasterModelFactory,
            IKPIMasterService kPIMasterService,
            INotificationService notificationService,
            ILocalizationService localizationService
            )
        {
            _permissionService = permissionService;
            _kPIMasterModelFactory = kPIMasterModelFactory;
            _kPIMasterService = kPIMasterService;
            _notificationService = notificationService;
            _localizationService = localizationService;
        }

        #endregion
        public virtual async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageKPIMaster, PermissionAction.View))
                return AccessDeniedView();
            //prepare model
            var model = await _kPIMasterModelFactory.PrepareKPIMasterSearchModelAsync(new KPIMasterSearchModel());
            return View("/Areas/Admin/Views/Extension/PerformanceMeasurements/KPIMaster/List.cshtml", model);
        }
        [HttpPost]
        public virtual async Task<IActionResult> List(KPIMasterSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageKPIMaster, PermissionAction.View))
                return AccessDeniedView();

            //prepare model
            var model = await _kPIMasterModelFactory.PrepareKPIMasterListModelAsync(searchModel);
            return Json(model);
        }

        public virtual async Task<IActionResult> Create()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageKPIMaster, PermissionAction.Add))
                return AccessDeniedView();

            //prepare model
            var model = await _kPIMasterModelFactory.PrepareKPIMasterModelAsync(new KPIMasterModel(), null);

            return View("/Areas/Admin/Views/Extension/PerformanceMeasurements/KPIMaster/Create.cshtml", model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Create(KPIMasterModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageKPIMaster, PermissionAction.Add))
                return AccessDeniedView();

            var kPIMaster = model.ToEntity<KPIMaster>();


            if (ModelState.IsValid)
            {
                kPIMaster.CreateOnUtc = DateTime.UtcNow;
                kPIMaster.UpdateOnUtc = DateTime.UtcNow;

                await _kPIMasterService.InsertKPIMasterAsync(kPIMaster);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.KPIMaster.Added"));

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = kPIMaster.Id });
            }
            //prepare model
            model = await _kPIMasterModelFactory.PrepareKPIMasterModelAsync(model, kPIMaster, true);

            //if we got this far, something failed, redisplay form

            return View("/Areas/Admin/Views/Extension/PerformanceMeasurements/KPIMaster/Create.cshtml", model);
        }

        public virtual async Task<IActionResult> Edit(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageKPIMaster, PermissionAction.Edit))
                return AccessDeniedView();

            var kPIMaster = await _kPIMasterService.GetKPIMasterByIdAsync(id);
            if (kPIMaster == null)
                return RedirectToAction("List");

            //prepare model
            var model = await _kPIMasterModelFactory.PrepareKPIMasterModelAsync(null, kPIMaster);

            return View("/Areas/Admin/Views/Extension/PerformanceMeasurements/KPIMaster/Edit.cshtml", model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Edit(KPIMasterModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageKPIMaster, PermissionAction.Edit))
                return AccessDeniedView();

            //try to get a project with the specified id
            var kPIMaster = await _kPIMasterService.GetKPIMasterByIdAsync(model.Id);
            if (kPIMaster == null)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                kPIMaster = model.ToEntity(kPIMaster);
                kPIMaster.UpdateOnUtc = DateTime.UtcNow;

                await _kPIMasterService.UpdateKPIMasterAsync(kPIMaster);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.KPIMaster.Updated"));

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = kPIMaster.Id });
            }
            //if we got this far, something failed, redisplay form
            return View("/Areas/Admin/Views/Extension/PerformanceMeasurements/KPIMaster/Edit.cshtml", model);
        }
        [HttpPost]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageKPIMaster, PermissionAction.Delete))
                return AccessDeniedView();

            //try to get a leaveType with the specified id
            var kPIMaster = await _kPIMasterService.GetKPIMasterByIdAsync(id);
            if (kPIMaster == null)
                return RedirectToAction("List");

            await _kPIMasterService.DeleteKPIMasterAsync(kPIMaster);

            _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Catalog.KPIMaster.Deleted"));

            return RedirectToAction("List");
        }
        [HttpPost]
        public virtual async Task<IActionResult> DeleteSelected(ICollection<int> selectedIds)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageKPIMaster, PermissionAction.Delete))
                return AccessDeniedView();

            if (selectedIds == null || selectedIds.Count == 0)
                return NoContent();

            var data = await _kPIMasterService.GetKPIMasterByIdsAsync(selectedIds.ToArray());

            foreach (var item in data)
            {
                await _kPIMasterService.DeleteKPIMasterAsync(item);
            }
            return Json(new { Result = true });
        }
    }
}