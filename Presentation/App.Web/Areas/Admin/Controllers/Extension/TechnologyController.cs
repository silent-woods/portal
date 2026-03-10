using App.Services.Localization;
using App.Services.Messages;
using App.Services.Security;
using App.Web.Areas.Admin.Factories;
using App.Web.Areas.Admin.Models.Extension.Technologys;
using App.Web.Framework.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Satyanam.Nop.Core.Domains;
using Satyanam.Nop.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace App.Web.Areas.Admin.Controllers
{
    public partial class TechnologyController : BaseAdminController
    {
        #region Fields
        private readonly IPermissionService _permissionService;
        private readonly INotificationService _notificationService;
        private readonly ILocalizationService _localizationService;
        private readonly ITechnologyService _technologyService;
        private readonly ITechnologyModelFactory _technologyModelFactory;
        #endregion

        #region Ctor

        public TechnologyController(IPermissionService permissionService,
            INotificationService notificationService,
            ILocalizationService localizationService
,
            ITechnologyService technologyService,
            ITechnologyModelFactory technologyModelFactory)
        {
            _permissionService = permissionService;
            _notificationService = notificationService;
            _localizationService = localizationService;
            _technologyService = technologyService;
            _technologyModelFactory = technologyModelFactory;
        }

        #endregion

        #region Utilities



        #endregion

        #region Methods 
        public virtual async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageTechnology))
                return AccessDeniedView();

            var model = await _technologyModelFactory.PrepareTechnologySearchModelAsync(new TechnologySearchModel());

            return View("~/Areas/Admin/Views/Extension/Technology/List.cshtml", model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> List(TechnologySearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageTechnology))
                return AccessDeniedView();

            var model = await _technologyModelFactory.PrepareTechnologyListModelAsync(searchModel);

            return Json(model);
        }

        public virtual async Task<IActionResult> Create()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCandidates))
                return AccessDeniedView();

            //prepare model
            var model = await _technologyModelFactory.PrepareTechnologyModelAsync(new TechnologyModel(), null);

            return View("~/Areas/Admin/Views/Extension/Technology/Create.cshtml", model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Create(TechnologyModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageTechnology))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var entity = new Technology
                {
                    Name = model.Name,
                    DisplayOrder = model.DisplayOrder,
                    Published = model.Published,
                    CreatedOnUtc = DateTime.UtcNow,
                    UpdatedOnUtc = DateTime.UtcNow
                };

                await _technologyService.InsertTechnologyAsync(entity);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Technology.Added"));

                if (!continueEditing)
                    return RedirectToAction("List");
                return RedirectToAction("Edit", new { id = entity.Id });
            }
            model = await _technologyModelFactory.PrepareTechnologyModelAsync(model, null, true);

            return View("~/Areas/Admin/Views/Extension/Technology/Create.cshtml", model);
        }

        public virtual async Task<IActionResult> Edit(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageTechnology))
                return AccessDeniedView();

            var entity = await _technologyService.GetTechnologyByIdAsync(id);
            if (entity == null)
                return RedirectToAction("List");

            var model = await _technologyModelFactory.PrepareTechnologyModelAsync(null, entity);

            return View("~/Areas/Admin/Views/Extension/Technology/Edit.cshtml", model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Edit(TechnologyModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageTechnology))
                return AccessDeniedView();

            var entity = await _technologyService.GetTechnologyByIdAsync(model.Id);
            if (entity == null)
                return RedirectToAction("List");
            if (ModelState.IsValid)
            {
                entity.Name = model.Name;
                entity.DisplayOrder = model.DisplayOrder;
                entity.Published = model.Published;
                entity.UpdatedOnUtc = DateTime.UtcNow;

                await _technologyService.UpdateTechnologyAsync(entity);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Technology.Updated"));

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = entity.Id });
            }
            model = await _technologyModelFactory.PrepareTechnologyModelAsync(model, null, true);

            return View("~/Areas/Admin/Views/Extension/Technology/Create.cshtml", model);
        }
        public virtual async Task<IActionResult> DeleteSelected(ICollection<int> selectedIds)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCandidates))
                return AccessDeniedView();

            if (selectedIds == null || selectedIds.Count == 0)
                return NoContent();

            var data = await _technologyService.GetTechnologyByIdsAsync(selectedIds.ToArray());

            foreach (var item in data)
            {
                await _technologyService.DeleteTechnologyAsync(item);
            }

            return Json(new { Result = true });
        }

        #endregion
    }
}