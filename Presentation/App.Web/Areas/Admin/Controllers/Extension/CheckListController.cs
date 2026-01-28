using App.Core.Domain.Security;
using App.Services.Helpers;
using App.Services.Localization;
using App.Services.Logging;
using App.Services.Messages;
using App.Services.Security;
using App.Web.Areas.Admin.Factories;
using App.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using App.Web.Areas.Admin.Models.Extension.CheckLists;
using App.Web.Framework.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Satyanam.Nop.Core.Domains;
using Satyanam.Nop.Core.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace App.Web.Areas.Admin.Controllers
{
    [AuthorizeAdmin]
    [Area("Admin")]
    public partial class CheckListController : BaseAdminController
    {
        #region Fields

        private readonly ICheckListMasterService _checkListMasterService;
        private readonly ICheckListMasterModelFactory _checkListMasterModelFactory;
        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IDateTimeHelper _dateTimeHelper;

        #endregion

        #region Ctor

        public CheckListController(
            ICheckListMasterService checkListMasterService,
            ICheckListMasterModelFactory checkListMasterModelFactory,
            ILocalizationService localizationService,
            INotificationService notificationService,
            IPermissionService permissionService,
            ICustomerActivityService customerActivityService,
            IDateTimeHelper dateTimeHelper)
        {
            _checkListMasterService = checkListMasterService;
            _checkListMasterModelFactory = checkListMasterModelFactory;
            _localizationService = localizationService;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _customerActivityService = customerActivityService;
            _dateTimeHelper = dateTimeHelper;
        }

        #endregion

        #region List

        public virtual IActionResult Index()
        {
            return RedirectToAction("List");
        }

        public virtual async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageChecklist, PermissionAction.View))
                return AccessDeniedView();

            // prepare search model
            var model = await _checkListMasterModelFactory.PrepareCheckListMasterSearchModelAsync(new CheckListMasterSearchModel());

            return View("/Areas/Admin/Views/Extension/CheckLists/List.cshtml", model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> List(CheckListMasterSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageChecklist, PermissionAction.View))
                return await AccessDeniedDataTablesJson();

            var model = await _checkListMasterModelFactory.PrepareCheckListMasterListModelAsync(searchModel);

            return Json(model);
        }

        #endregion

        #region Create

        public virtual async Task<IActionResult> Create()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageChecklist, PermissionAction.Add))
                return AccessDeniedView();

            var model = await _checkListMasterModelFactory.PrepareCheckListMasterModelAsync(new CheckListMasterModel(), null);

            return View("/Areas/Admin/Views/Extension/CheckLists/Create.cshtml", model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Create(CheckListMasterModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageChecklist, PermissionAction.Add))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var entity = model.ToEntity<CheckListMaster>();
                entity.CreatedOn = await _dateTimeHelper.GetIndianTimeAsync();

                await _checkListMasterService.InsertCheckListAsync(entity);

                // log activity
                await _customerActivityService.InsertActivityAsync("AddNewCheckListMaster",
                    string.Format(await _localizationService.GetResourceAsync("ActivityLog.AddNewCheckListMaster"), entity.Title), entity);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.CheckLists.Added"));

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = entity.Id });
            }

            model = await _checkListMasterModelFactory.PrepareCheckListMasterModelAsync(model, null, true);
            return View("/Areas/Admin/Views/Extension/CheckLists/Create.cshtml", model);
        }

        #endregion

        #region Edit

        public virtual async Task<IActionResult> Edit(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageChecklist, PermissionAction.Edit))
                return AccessDeniedView();

            var entity = await _checkListMasterService.GetCheckListByIdAsync(id);
            if (entity == null)
                return RedirectToAction("List");

            var model = await _checkListMasterModelFactory.PrepareCheckListMasterModelAsync(null, entity);

            return View("/Areas/Admin/Views/Extension/CheckLists/Edit.cshtml", model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Edit(CheckListMasterModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageChecklist, PermissionAction.Edit))
                return AccessDeniedView();

            var entity = await _checkListMasterService.GetCheckListByIdAsync(model.Id);
            if (entity == null)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                entity = model.ToEntity(entity);

                await _checkListMasterService.UpdateCheckListAsync(entity);

                // log activity
                await _customerActivityService.InsertActivityAsync("EditCheckListMaster",
                    string.Format(await _localizationService.GetResourceAsync("ActivityLog.EditCheckListMaster"), entity.Title), entity);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.CheckLists.Updated"));

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = entity.Id });
            }

            model = await _checkListMasterModelFactory.PrepareCheckListMasterModelAsync(model, entity, true);
            return View("/Areas/Admin/Views/Extension/CheckLists/Edit.cshtml", model);
        }

        #endregion

        #region Delete

        [HttpPost]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageChecklist, PermissionAction.Delete))
                return AccessDeniedView();

            var entity = await _checkListMasterService.GetCheckListByIdAsync(id);
            if (entity == null)
                return RedirectToAction("List");

            await _checkListMasterService.DeleteCheckListAsync(entity);

            // log activity
            await _customerActivityService.InsertActivityAsync("DeleteCheckListMaster",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.DeleteCheckListMaster"), entity.Title), entity);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.CheckLists.Deleted"));

            return RedirectToAction("List");
        }

        [HttpPost]
        public virtual async Task<IActionResult> DeleteSelected(ICollection<int> selectedIds)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageChecklist, PermissionAction.Delete))
                return AccessDeniedView();

            if (selectedIds == null || selectedIds.Count == 0)
                return NoContent();

            var entities = await _checkListMasterService.GetCheckListsByIdsAsync(selectedIds.ToArray());
            foreach (var entity in entities)
                await _checkListMasterService.DeleteCheckListAsync(entity);

            return Json(new { Result = true });
        }

        #endregion
    }
}
