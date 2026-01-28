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
using Satyanam.Nop.Plugin.SatyanamCRM.Models.Industrys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Satyanam.Nop.Plugin.Misc.SatyanamCRM.Controllers
{
    [AutoValidateAntiforgeryToken]
    [AuthorizeAdmin]
    [Area(AreaNames.Admin)]
    public class IndustryController : BasePluginController
    {
        #region Fields

        private readonly IPermissionService _permissionService;
        private readonly IIndustryService _industryService;
        private readonly INotificationService _notificationService;
        private readonly ILocalizationService _localizationService;

        #endregion

        #region Ctor 

        public IndustryController(IPermissionService permissionService,
                               IIndustryService industryService,
                               INotificationService notificationService,
                               ILocalizationService localizationService)
        {
            _permissionService = permissionService;
            _industryService = industryService;
            _notificationService = notificationService;
            _localizationService = localizationService;
        }

        #endregion

        #region Utilities

        public virtual async Task<IndustrySearchModel> PrepareIndustrySearchModelAsync(IndustrySearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //prepare page parameters
            searchModel.SetGridPageSize();

            return searchModel;
        }

        public virtual async Task<IndustryListModel> PrepareIndustryListModelAsync(IndustrySearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get industry
            var industrys = await _industryService.GetAllIndustryAsync(showHidden: true,
                name: searchModel.SearchIndustryName,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            //prepare grid model
            var model = await new IndustryListModel().PrepareToGridAsync(searchModel, industrys, () =>
            {
                //fill in model values from the entity
                return industrys.SelectAwait(async industrys =>
                {
                    var industryModel = new IndustryModel();
                    industryModel.Id = industrys.Id;
                    industryModel.Name = industrys.Name;

                    return industryModel;
                });
            });
            return model;
        }

        public virtual async Task<IndustryModel> PrepareIndustryModelAsync(IndustryModel model, Industry industry, bool excludeProperties = false)
        {
            if (industry != null)
            {
                //fill in model values from the entity
                if (model == null)
                {
                    model = new IndustryModel();
                    model.Id = industry.Id;
                    model.Name = industry.Name;
                }
            }
            return model;
        }
        #endregion

        #region Methods

        public virtual async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageIndustries, PermissionAction.View))
                return AccessDeniedView();

            //prepare model
            var model = await PrepareIndustrySearchModelAsync(new IndustrySearchModel());

            return View("~/Plugins/Misc.SatyanamCRM/Views/Industrys/List.cshtml", model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> List(IndustrySearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageIndustries, PermissionAction.View))
                return await AccessDeniedDataTablesJson();

            //prepare model
            var model = await PrepareIndustryListModelAsync(searchModel);

            return Json(model);
        }

        public virtual async Task<IActionResult> Create()
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageIndustries, PermissionAction.Add))
                return AccessDeniedView();

            //prepare model
            var model = await PrepareIndustryModelAsync(new IndustryModel(), null);

            return View("~/Plugins/Misc.SatyanamCRM/Views/Industrys/Create.cshtml", model);
        }
        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Create(IndustryModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageIndustries, PermissionAction.Add))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var industry = new Industry();
                industry.Id = model.Id;
                industry.Name = model.Name;

                await _industryService.InsertIndustryAsync(industry);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Plugin.SatyanamCRM.Industry.Added"));

                //ViewBag.RefreshPage = true;

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = industry.Id });
            }

            //prepare model
            model = await PrepareIndustryModelAsync(model, null, true);

            //if we got this far, something failed, redisplay form
            return View("~/Plugins/Misc.SatyanamCRM/Views/Industrys/Create.cshtml", model);
        }

        public virtual async Task<IActionResult> Edit(int id)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageIndustries, PermissionAction.Edit))
                return AccessDeniedView();

            //try to get a industry with the specified id
            var industry = await _industryService.GetIndustryByIdAsync(id);
            if (industry == null)
                return RedirectToAction("List");

            //prepare model
            var model = await PrepareIndustryModelAsync(null, industry);

            return View("~/Plugins/Misc.SatyanamCRM/Views/Industrys/Edit.cshtml", model);
        }
        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Edit(IndustryModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageIndustries, PermissionAction.Edit))
                return AccessDeniedView();

            //try to get a industry with the specified id
            var industry = await _industryService.GetIndustryByIdAsync(model.Id);
            if (industry == null)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                industry = new Industry();
                industry.Id = model.Id;
                industry.Name = model.Name;

                await _industryService.UpdateIndustryAsync(industry);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Plugin.SatyanamCRM.Industry.Updated"));

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = industry.Id });
                //ViewBag.RefreshPage = true;

            }

            //prepare model
            model = await PrepareIndustryModelAsync(model, industry, true);

            //if we got this far, something failed, redisplay form
            return View("~/Plugins/Misc.SatyanamCRM/Views/Industrys/Edit.cshtml", model);
        }

        public virtual async Task<IActionResult> DeleteSelected(ICollection<int> selectedIds)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageIndustries, PermissionAction.Delete))
                return AccessDeniedView();

            if (selectedIds == null || selectedIds.Count == 0)
                return NoContent();

            var data = await _industryService.GetIndustryByIdsAsync(selectedIds.ToArray());

            foreach (var item in data)
            {
                await _industryService.DeleteIndustryAsync(item);
            }

            return Json(new { Result = true });
        }
        #endregion
    }
}
