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
using Satyanam.Nop.Plugin.SatyanamCRM.Models.LeadSources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Satyanam.Nop.Plugin.Misc.SatyanamCRM.Controllers
{
    [AutoValidateAntiforgeryToken]
    [AuthorizeAdmin]
    [Area(AreaNames.Admin)]
    public class LeadSourceController : BasePluginController
    {
        #region Fields

        private readonly IPermissionService _permissionService;
        private readonly ILeadSourceService _leadSourceService;
        private readonly INotificationService _notificationService;
        private readonly ILocalizationService _localizationService;

        #endregion

        #region Ctor 

        public LeadSourceController(IPermissionService permissionService,
                               ILeadSourceService leadSourceService,
                               INotificationService notificationService,
                               ILocalizationService localizationService)
        {
            _permissionService = permissionService;
            _leadSourceService = leadSourceService;
            _notificationService = notificationService;
            _localizationService = localizationService;
        }

        #endregion

        #region Utilities

        public virtual async Task<LeadSourceSearchModel> PrepareLeadSourceSearchModelAsync(LeadSourceSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //prepare page parameters
            searchModel.SetGridPageSize();

            return searchModel;
        }

        public virtual async Task<LeadSourceListModel> PrepareLeadSourceListModelAsync(LeadSourceSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get leadSource
            var leadSources = await _leadSourceService.GetAllLeadSourceAsync(showHidden: true,
                name: searchModel.SearchLeadSourceName,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            //prepare grid model
            var model = await new LeadSourceListModel().PrepareToGridAsync(searchModel, leadSources, () =>
            {
                //fill in model values from the entity
                return leadSources.SelectAwait(async leadSources =>
                {
                    var leadSourcesModel = new LeadSourceModel();
                    leadSourcesModel.Id = leadSources.Id;
                    leadSourcesModel.Name = leadSources.Name;

                    return leadSourcesModel;
                });
            });
            return model;
        }

        public virtual async Task<LeadSourceModel> PrepareLeadSourceModelAsync(LeadSourceModel model, LeadSource leadSource, bool excludeProperties = false)
        {
            if (leadSource != null)
            {
                //fill in model values from the entity
                if (model == null)
                {
                    model = new LeadSourceModel();
                    model.Id = leadSource.Id;
                    model.Name = leadSource.Name;
                }
            }
            return model;
        }
        #endregion

        #region Methods

        public virtual async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageLeadSources, PermissionAction.View))
                return AccessDeniedView();

            //prepare model
            var model = await PrepareLeadSourceSearchModelAsync(new LeadSourceSearchModel());

            return View("~/Plugins/Misc.SatyanamCRM/Views/LeadSources/List.cshtml", model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> List(LeadSourceSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageLeadSources, PermissionAction.View))
                return await AccessDeniedDataTablesJson();

            //prepare model
            var model = await PrepareLeadSourceListModelAsync(searchModel);

            return Json(model);
        }

        public virtual async Task<IActionResult> Create()
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageLeadSources, PermissionAction.Add))
                return AccessDeniedView();

            //prepare model
            var model = await PrepareLeadSourceModelAsync(new LeadSourceModel(), null);

            return View("~/Plugins/Misc.SatyanamCRM/Views/LeadSources/Create.cshtml", model);
        }
        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Create(LeadSourceModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageLeadSources, PermissionAction.Add))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var leadSource = new LeadSource();
                leadSource.Id = model.Id;
                leadSource.Name = model.Name;

                await _leadSourceService.InsertLeadSourceAsync(leadSource);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Lead Source has been added successfully "));

                //ViewBag.RefreshPage = true;

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = leadSource.Id });
            }

            //prepare model
            model = await PrepareLeadSourceModelAsync(model, null, true);

            //if we got this far, something failed, redisplay form
            return View("~/Plugins/Misc.SatyanamCRM/Views/LeadSources/Create.cshtml", model);
        }

        public virtual async Task<IActionResult> Edit(int id)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageLeadSources, PermissionAction.Edit))
                return AccessDeniedView();

            //try to get a leadSource with the specified id
            var leadSource = await _leadSourceService.GetLeadSourceByIdAsync(id);
            if (leadSource == null)
                return RedirectToAction("List");

            //prepare model
            var model = await PrepareLeadSourceModelAsync(null, leadSource);

            return View("~/Plugins/Misc.SatyanamCRM/Views/LeadSources/Edit.cshtml", model);
        }
        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Edit(LeadSourceModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageLeadSources, PermissionAction.Edit))
                return AccessDeniedView();

            //try to get a leadSource with the specified id
            var leadSource = await _leadSourceService.GetLeadSourceByIdAsync(model.Id);
            if (leadSource == null)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                leadSource = new LeadSource();
                leadSource.Id = model.Id;
                leadSource.Name = model.Name;

                await _leadSourceService.UpdateLeadSourceAsync(leadSource);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Plugin.SatyanamCRM.LeadSource.Updated"));

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = leadSource.Id });
                //ViewBag.RefreshPage = true;

            }

            //prepare model
            model = await PrepareLeadSourceModelAsync(model, leadSource, true);

            //if we got this far, something failed, redisplay form
            return View("~/Plugins/Misc.SatyanamCRM/Views/LeadSources/Edit.cshtml", model);
        }

        public virtual async Task<IActionResult> DeleteSelected(ICollection<int> selectedIds)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageLeadSources, PermissionAction.Delete))
                return AccessDeniedView();

            if (selectedIds == null || selectedIds.Count == 0)
                return NoContent();

            var data = await _leadSourceService.GetLeadSourceByIdsAsync(selectedIds.ToArray());

            foreach (var item in data)
            {
                await _leadSourceService.DeleteLeadSourceAsync(item);
            }

            return Json(new { Result = true });
        }
        #endregion
    }
}
