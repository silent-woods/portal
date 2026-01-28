using App.Core.Domain.Security;
using App.Data.Extensions;
using App.Services.Localization;
using App.Services.Messages;
using App.Services.Security;
using App.Web.Framework;
using App.Web.Framework.Controllers;
using App.Web.Framework.Models.Extensions;
using App.Web.Framework.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Satyanam.Nop.Core.Domains;
using Satyanam.Nop.Core.Services;
using Satyanam.Nop.Plugin.SatyanamCRM.Models.Categorys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Satyanam.Nop.Plugin.Misc.SatyanamCRM.Controllers
{
    [AutoValidateAntiforgeryToken]
    [AuthorizeAdmin]
    [Area(AreaNames.Admin)]
    public class CategorysController : BasePluginController
    {
        #region Fields

        private readonly IPermissionService _permissionService;
        private readonly ICategorysService _categorysService;
        private readonly INotificationService _notificationService;
        private readonly ILocalizationService _localizationService;

        #endregion

        #region Ctor 

        public CategorysController(IPermissionService permissionService,
                               ICategorysService categorysService,
                               INotificationService notificationService,
                               ILocalizationService localizationService)
        {
            _permissionService = permissionService;
            _categorysService = categorysService;
            _notificationService = notificationService;
            _localizationService = localizationService;
        }

        #endregion

        #region Utilities

        public virtual async Task<CategorysSearchModel> PrepareCategorysSearchModelAsync(CategorysSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //prepare page parameters
            searchModel.SetGridPageSize();

            return searchModel;
        }

        public virtual async Task<CategorysListModel> PrepareCategorysListModelAsync(CategorysSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get categorys
            var categorys = await _categorysService.GetAllCategorysAsync(showHidden: true,
                name: searchModel.SearchCategorysName,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            //prepare grid model
            var model = await new CategorysListModel().PrepareToGridAsync(searchModel, categorys, () =>
            {
                //fill in model values from the entity
                return categorys.SelectAwait(async categorys =>
                {
                    var categorysModel = new CategorysModel();
                    categorysModel.Id = categorys.Id;
                    categorysModel.Name = categorys.Name;

                    return categorysModel;
                });
            });
            return model;
        }

        public virtual async Task<CategorysModel> PrepareCategorysModelAsync(CategorysModel model, Categorys categorys, bool excludeProperties = false)
        {
            if (categorys != null)
            {
                //fill in model values from the entity
                if (model == null)
                {
                    model = new CategorysModel();
                    model.Id = categorys.Id;
                    model.Name = categorys.Name;
                }
            }
            return model;
        }
        #endregion

        #region Methods

        public virtual async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageCategories, PermissionAction.View))
                return AccessDeniedView();

            //prepare model
            var model = await PrepareCategorysSearchModelAsync(new CategorysSearchModel());

            return View("~/Plugins/Misc.SatyanamCRM/Views/Categorys/List.cshtml", model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> List(CategorysSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageCategories, PermissionAction.View))
                return await AccessDeniedDataTablesJson();

            //prepare model
            var model = await PrepareCategorysListModelAsync(searchModel);

            return Json(model);
        }

        public virtual async Task<IActionResult> Create()
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageCategories, PermissionAction.Add))
                return AccessDeniedView();

            //prepare model
            var model = await PrepareCategorysModelAsync(new CategorysModel(), null);

            return View("~/Plugins/Misc.SatyanamCRM/Views/Categorys/Create.cshtml", model);
        }
        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Create(CategorysModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageCategories, PermissionAction.Add))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var categorys = new Categorys();
                categorys.Id = model.Id;
                categorys.Name = model.Name;

                await _categorysService.InsertCategorysAsync(categorys);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Plugin.SatyanamCRM.Categorys.Added"));

                //ViewBag.RefreshPage = true;

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = categorys.Id });
            }

            //prepare model
            model = await PrepareCategorysModelAsync(model, null, true);

            //if we got this far, something failed, redisplay form
            return View("~/Plugins/Misc.SatyanamCRM/Views/Categorys/Create.cshtml", model);
        }

        public virtual async Task<IActionResult> Edit(int id)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageCategories, PermissionAction.Edit))
                return AccessDeniedView();

            //try to get a categorys with the specified id
            var categorys = await _categorysService.GetCategorysByIdAsync(id);
            if (categorys == null)
                return RedirectToAction("List");

            //prepare model
            var model = await PrepareCategorysModelAsync(null, categorys);

            return View("~/Plugins/Misc.SatyanamCRM/Views/Categorys/Edit.cshtml", model);
        }
        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Edit(CategorysModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageCategories, PermissionAction.Edit))
                return AccessDeniedView();

            //try to get a categorys with the specified id
            var categorys = await _categorysService.GetCategorysByIdAsync(model.Id);
            if (categorys == null)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                categorys = new Categorys();
                categorys.Id = model.Id;
                categorys.Name = model.Name;

                await _categorysService.UpdateCategorysAsync(categorys);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Plugin.SatyanamCRM.Categorys.Updated"));

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = categorys.Id });
                //ViewBag.RefreshPage = true;

            }

            //prepare model
            model = await PrepareCategorysModelAsync(model, categorys, true);

            //if we got this far, something failed, redisplay form
            return View("~/Plugins/Misc.SatyanamCRM/Views/Categorys/Edit.cshtml", model);
        }

        public virtual async Task<IActionResult> DeleteSelected(ICollection<int> selectedIds)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageCategories, PermissionAction.Delete))
                return AccessDeniedView();

            if (selectedIds == null || selectedIds.Count == 0)
                return NoContent();

            var data = await _categorysService.GetCategorysByIdsAsync(selectedIds.ToArray());

            foreach (var item in data)
            {
                await _categorysService.DeleteCategorysAsync(item);
            }

            return Json(new { Result = true });
        }
        #endregion
    }
}
