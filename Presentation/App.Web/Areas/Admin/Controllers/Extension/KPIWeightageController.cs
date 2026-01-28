using App.Core.Domain.PerformanceMeasurements;
using App.Core.Domain.Security;
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

namespace App.Web.Areas.Admin.Controllers
{
    public partial class KPIWeightageController : BaseAdminController
    {
        #region Fields
        private readonly IPermissionService _permissionService;
        private readonly IKPIWeightageModelFactory _kPIWeightageModelFactory;
        private readonly IKPIWeightageService _kPIWeightageService;
        private readonly INotificationService _notificationService;
        private readonly ILocalizationService _localizationService;
        #endregion

        #region Ctor

        public KPIWeightageController(IPermissionService permissionService,
            IKPIWeightageModelFactory kPIWeightageModelFactory,
            IKPIWeightageService kPIWeightageService,
            INotificationService notificationService,
            ILocalizationService localizationService
            )
        {
            _permissionService = permissionService;
            _kPIWeightageModelFactory = kPIWeightageModelFactory;
            _kPIWeightageService = kPIWeightageService;
            _notificationService = notificationService;
            _localizationService = localizationService;
        }

        #endregion
        public virtual async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageKPIWeightage, PermissionAction.View))
                return AccessDeniedView();
            //prepare model
            var model = await _kPIWeightageModelFactory.PrepareKPIWeightageSearchModelAsync(new KPIWeightageSearchModel());
            return View("/Areas/Admin/Views/Extension/PerformanceMeasurements/KPIWeightage/List.cshtml", model);
        }
        [HttpPost]
        public virtual async Task<IActionResult> List(KPIWeightageSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageKPIWeightage, PermissionAction.View))
                return AccessDeniedView();

            //prepare model
            var model = await _kPIWeightageModelFactory.PrepareKPIWeightageListModelAsync(searchModel);
            return Json(model);
        }

        public virtual async Task<IActionResult> Create()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageKPIWeightage, PermissionAction.Add))
                return AccessDeniedView();

            //prepare model
            var model = await _kPIWeightageModelFactory.PrepareKPIWeightageModelAsync(new KPIWeightageModel(), null);

            return View("/Areas/Admin/Views/Extension/PerformanceMeasurements/KPIWeightage/Create.cshtml", model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Create(KPIWeightageModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageKPIWeightage, PermissionAction.Add))
                return AccessDeniedView();

            var kPIWeightage = model.ToEntity<KPIWeightage>();


            if (ModelState.IsValid)
            {
                kPIWeightage.CreateOnUtc = DateTime.UtcNow;
                kPIWeightage.UpdateOnUtc = DateTime.UtcNow;

                await _kPIWeightageService.InsertKPIWeightageAsync(kPIWeightage);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.KPIWeightage.Added"));

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = kPIWeightage.Id });
            }
            //prepare model
            model = await _kPIWeightageModelFactory.PrepareKPIWeightageModelAsync(model, kPIWeightage, true);

            //if we got this far, something failed, redisplay form

            return View("/Areas/Admin/Views/Extension/PerformanceMeasurements/KPIWeightage/Create.cshtml", model);
        }

        public virtual async Task<IActionResult> Edit(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageKPIWeightage, PermissionAction.Edit))
                return AccessDeniedView();

            var kPIWeightage = await _kPIWeightageService.GetKPIWeightageByIdAsync(id);
            if (kPIWeightage == null)
                return RedirectToAction("List");

            //prepare model
            var model = await _kPIWeightageModelFactory.PrepareKPIWeightageModelAsync(null, kPIWeightage);

            return View("/Areas/Admin/Views/Extension/PerformanceMeasurements/KPIWeightage/Edit.cshtml", model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Edit(KPIWeightageModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageKPIWeightage, PermissionAction.Edit))
                return AccessDeniedView();

            //try to get a project with the specified id
            var kPIWeightage = await _kPIWeightageService.GetKPIWeightageByIdAsync(model.Id);
            if (kPIWeightage == null)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                kPIWeightage = model.ToEntity(kPIWeightage);
                kPIWeightage.UpdateOnUtc = DateTime.UtcNow;

                await _kPIWeightageService.UpdateKPIWeightageAsync(kPIWeightage);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.KPIWeightage.Updated"));

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = kPIWeightage.Id });
            }
            //if we got this far, something failed, redisplay form
            return View("/Areas/Admin/Views/Extension/PerformanceMeasurements/KPIWeightage/Edit.cshtml", model);
        }
        [HttpPost]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageKPIWeightage, PermissionAction.Delete))
                return AccessDeniedView();

            //try to get a leaveType with the specified id
            var kPIWeightage = await _kPIWeightageService.GetKPIWeightageByIdAsync(id);
            if (kPIWeightage == null)
                return RedirectToAction("List");

            await _kPIWeightageService.DeleteKPIWeightageAsync(kPIWeightage);

            _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Catalog.KPIWeightage.Deleted"));

            return RedirectToAction("List");
        }
        [HttpPost]
        public virtual async Task<IActionResult> DeleteSelected(ICollection<int> selectedIds)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageKPIWeightage, PermissionAction.Delete))
                return AccessDeniedView();

            if (selectedIds == null || selectedIds.Count == 0)
                return NoContent();

            var data = await _kPIWeightageService.GetKPIWeightageByIdsAsync(selectedIds.ToArray());

            foreach (var item in data)
            {
                await _kPIWeightageService.DeleteKPIWeightageAsync(item);
            }
            return Json(new { Result = true });
        }
    }
}