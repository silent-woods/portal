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
using Satyanam.Nop.Plugin.SatyanamCRM.Models.LeadStatus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Satyanam.Nop.Plugin.Misc.SatyanamCRM.Controllers
{
    [AutoValidateAntiforgeryToken]
    [AuthorizeAdmin]
    [Area(AreaNames.Admin)]
    public class LeadStatusController : BasePluginController
    {
        #region Fields

        private readonly IPermissionService _permissionService;
        private readonly ILeadStatusService _leadStatusService;
        private readonly INotificationService _notificationService;
        private readonly ILocalizationService _localizationService;

        #endregion

        #region Ctor 

        public LeadStatusController(IPermissionService permissionService,
                               ILeadStatusService leadStatusService,
                               INotificationService notificationService,
                               ILocalizationService localizationService)
        {
            _permissionService = permissionService;
            _leadStatusService = leadStatusService;
            _notificationService = notificationService;
            _localizationService = localizationService;
        }

        #endregion

        #region Utilities

        public virtual async Task<LeadStatusSearchModel> PrepareLeadStatusSearchModelAsync(LeadStatusSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //prepare page parameters
            searchModel.SetGridPageSize();

            return searchModel;
        }

        public virtual async Task<LeadStatusListModel> PrepareLeadStatusListModelAsync(LeadStatusSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get LeadStatus
            var leadStatus = await _leadStatusService.GetAllLeadStatusAsync(showHidden: true,
                name: searchModel.SearchLeadStatusName,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            //prepare grid model
            var model = await new LeadStatusListModel().PrepareToGridAsync(searchModel, leadStatus, () =>
            {
                //fill in model values from the entity
                return leadStatus.SelectAwait(async leadStatus =>
                {
                    var leadStatusModel = new LeadStatusModel();
                    leadStatusModel.Id = leadStatus.Id;
                    leadStatusModel.Name = leadStatus.Name;

                    return leadStatusModel;
                });
            });
            return model;
        }

        public virtual async Task<LeadStatusModel> PrepareLeadStatusModelAsync(LeadStatusModel model, LeadStatus leadStatus, bool excludeProperties = false)
        {
            if (leadStatus != null)
            {
                //fill in model values from the entity
                if (model == null)
                {
                    model = new LeadStatusModel();
                    model.Id = leadStatus.Id;
                    model.Name = leadStatus.Name;
                }
            }
            return model;
        }
        #endregion

        #region Methods

        public virtual async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageLeadStatuses, PermissionAction.View))
                return AccessDeniedView();

            //prepare model
            var model = await PrepareLeadStatusSearchModelAsync(new LeadStatusSearchModel());

            return View("~/Plugins/Misc.SatyanamCRM/Views/LeadStatus/List.cshtml", model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> List(LeadStatusSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageLeadStatuses, PermissionAction.View))
                return await AccessDeniedDataTablesJson();

            //prepare model
            var model = await PrepareLeadStatusListModelAsync(searchModel);

            return Json(model);
        }

        public virtual async Task<IActionResult> Create()
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageLeads, PermissionAction.Add))
                return AccessDeniedView();

            //prepare model
            var model = await PrepareLeadStatusModelAsync(new LeadStatusModel(), null);

            return View("~/Plugins/Misc.SatyanamCRM/Views/LeadStatus/Create.cshtml", model);
        }
        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Create(LeadStatusModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageLeads, PermissionAction.Add))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var leadStatus = new LeadStatus();
                leadStatus.Id = model.Id;
                leadStatus.Name = model.Name;

                await _leadStatusService.InsertLeadStatusAsync(leadStatus);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Plugin.SatyanamCRM.LeadStatus.Added"));

                //ViewBag.RefreshPage = true;

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = leadStatus.Id });
            }

            //prepare model
            model = await PrepareLeadStatusModelAsync(model, null, true);

            //if we got this far, something failed, redisplay form
            return View("~/Plugins/Misc.SatyanamCRM/Views/LeadStatus/Create.cshtml", model);
        }

        public virtual async Task<IActionResult> Edit(int id)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageLeads, PermissionAction.Edit))
                return AccessDeniedView();

            //try to get a LeadStatus with the specified id
            var leadStatus = await _leadStatusService.GetLeadStatusByIdAsync(id);
            if (leadStatus == null)
                return RedirectToAction("List");

            //prepare model
            var model = await PrepareLeadStatusModelAsync(null, leadStatus);

            return View("~/Plugins/Misc.SatyanamCRM/Views/LeadStatus/Edit.cshtml", model);
        }
        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Edit(LeadStatusModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageLeads, PermissionAction.Edit))
                return AccessDeniedView();

            //try to get a LeadStatus with the specified id
            var leadStatus = await _leadStatusService.GetLeadStatusByIdAsync(model.Id);
            if (leadStatus == null)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                leadStatus = new LeadStatus();
                leadStatus.Id = model.Id;
                leadStatus.Name = model.Name;

                await _leadStatusService.UpdateLeadStatusAsync(leadStatus);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Plugin.SatyanamCRM.LeadStatus.Updated"));

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = leadStatus.Id });
                //ViewBag.RefreshPage = true;

            }

            //prepare model
            model = await PrepareLeadStatusModelAsync(model, leadStatus, true);

            //if we got this far, something failed, redisplay form
            return View("~/Plugins/Misc.SatyanamCRM/Views/LeadStatus/Edit.cshtml", model);
        }

        public virtual async Task<IActionResult> DeleteSelected(ICollection<int> selectedIds)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageLeads, PermissionAction.Delete))
                return AccessDeniedView();

            if (selectedIds == null || selectedIds.Count == 0)
                return NoContent();

            var data = await _leadStatusService.GetLeadStatusByIdsAsync(selectedIds.ToArray());

            foreach (var item in data)
            {
                await _leadStatusService.DeleteLeadStatusAsync(item);
            }

            return Json(new { Result = true });
        }
        #endregion
    }
}
