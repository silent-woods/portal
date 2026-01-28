using App.Core;
using App.Core.Domain.Security;
using App.Services.Helpers;
using App.Services.Localization;
using App.Services.Messages;
using App.Services.Security;
using App.Web.Areas.Admin.Factories;
using App.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using App.Web.Areas.Admin.Models.CheckListMappings;
using App.Web.Framework.Mvc;
using App.Web.Framework.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Satyanam.Nop.Core.Domains;
using Satyanam.Nop.Core.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace App.Web.Areas.Admin.Controllers
{
    public partial class CheckListMappingController : BaseAdminController
    {
        #region Fields

        private readonly IPermissionService _permissionService;
        private readonly ICheckListMappingModelFactory _checkListMappingModelFactory;
        private readonly ICheckListMappingService _checkListMappingService;
        private readonly INotificationService _notificationService;
        private readonly ILocalizationService _localizationService;
        private readonly IWorkContext _workContext;
        private readonly ICheckListMasterService _checkListMasterService;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ITaskCategoryService _taskCategoryService;
        #endregion

        #region Ctor

        public CheckListMappingController(IPermissionService permissionService,
            ICheckListMappingModelFactory checkListMappingModelFactory,
            ICheckListMappingService checkListMappingService,
            INotificationService notificationService,
            ILocalizationService localizationService,
            IWorkContext workContext,
            ICheckListMasterService checkListMasterService,
            IWorkflowMessageService workflowMessageService,
            IDateTimeHelper dateTimeHelper,
            ITaskCategoryService taskCategoryService)
        {
            _permissionService = permissionService;
            _checkListMappingModelFactory = checkListMappingModelFactory;
            _checkListMappingService = checkListMappingService;
            _notificationService = notificationService;
            _localizationService = localizationService;
            _workContext = workContext;
            _checkListMasterService = checkListMasterService;
            _workflowMessageService = workflowMessageService;
            _dateTimeHelper = dateTimeHelper;
            _taskCategoryService = taskCategoryService;
        }

        #endregion

        #region Methods

        public virtual async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageTaskCategoryChecklists, PermissionAction.View))
                return AccessDeniedView();

            var model = await _checkListMappingModelFactory
                .PrepareCheckListMappingSearchModelAsync(new CheckListMappingSearchModel());

            return View("/Areas/Admin/Views/Extension/TaskCategories/_CreateOrUpdateCheckListMapping.cshtml", model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> List(CheckListMappingSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageTaskCategoryChecklists, PermissionAction.View))
                return AccessDeniedView();

            var model = await _checkListMappingModelFactory
                .PrepareCheckListMappingListModelAsync(searchModel);

            return Json(model);
        }

        public virtual async Task<IActionResult> Create(int taskCategoryId)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageTaskCategoryChecklists, PermissionAction.Add))
                return AccessDeniedView();

            var model = await _checkListMappingModelFactory
                .PrepareCheckListMappingModelAsync(new CheckListMappingModel(), null);

            model.TaskCategoryId = taskCategoryId;

            var taskCategory = await _taskCategoryService.GetTaskCategoryByIdAsync(taskCategoryId);
            if (taskCategory != null)
                model.TaskCategoryName = taskCategory.CategoryName;

            ViewBag.RefreshPage = false;

            return View("/Areas/Admin/Views/Extension/TaskCategories/CheckListMappingCreate.cshtml", model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Create(CheckListMappingModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageTaskCategoryChecklists, PermissionAction.Add))
                return AccessDeniedView();

            var entity = model.ToEntity<CheckListMapping>();

            if (!string.IsNullOrWhiteSpace(model.CheckListName))
            {
                var existingCheckList = await _checkListMasterService.GetCheckListByNameAsync(model.CheckListName.Trim());

                if (existingCheckList != null)
                {
                    entity.CheckListId = existingCheckList.Id;
                }
                else
                {
                    var newCheckList = new CheckListMaster
                    {
                        Title = model.CheckListName.Trim(),
                        CreatedOn = await _dateTimeHelper.GetIndianTimeAsync(),
                        IsActive =true
                    };

                    await _checkListMasterService.InsertCheckListAsync(newCheckList);
                    entity.CheckListId = newCheckList.Id;
                }
            }
            else
            {
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Common.ChecklistRequired"));
                model = await _checkListMappingModelFactory.PrepareCheckListMappingModelAsync(model, entity, true);
                ViewBag.RefreshPage = false;
                return View("/Areas/Admin/Views/Extension/TaskCategories/CheckListMappingCreate.cshtml", model);
            }

            if (await _checkListMappingService.IsMappingExistAsync(model.TaskCategoryId, model.StatusId, model.CheckListId))
            {
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Common.CheckListMappingExist"));
                model = await _checkListMappingModelFactory.PrepareCheckListMappingModelAsync(model, entity, true);
                ViewBag.RefreshPage = false;
                return View("/Areas/Admin/Views/Extension/TaskCategories/CheckListMappingCreate.cshtml", model);
            }

            if (ModelState.IsValid)
            {
                await _checkListMappingService.InsertCheckListMappingAsync(entity);

                string successMessage = await _localizationService.GetResourceAsync("Admin.Catalog.CheckListMapping.Added");
                TempData["SuccessMessage"] = successMessage;
                ViewBag.RefreshPage = true;
            }

            return View("/Areas/Admin/Views/Extension/TaskCategories/CheckListMappingCreate.cshtml", model);
        }

        public virtual async Task<IActionResult> Edit(int checklistId, int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageTaskCategoryChecklists, PermissionAction.Edit))
                return AccessDeniedView();

            var entity = await _checkListMappingService.GetCheckListMappingByIdAsync(id);
            ViewBag.RefreshPage = false;
            if (entity == null)
                return RedirectToAction("List");

            var model = await _checkListMappingModelFactory.PrepareCheckListMappingModelAsync(null, entity);
            model.CheckListId = checklistId;
            var checklist = await _checkListMasterService.GetCheckListByIdAsync(checklistId);
            if (checklist != null)
                model.CheckListName = checklist.Title;

            return View("/Areas/Admin/Views/Extension/TaskCategories/CheckListMappingEdit.cshtml", model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Edit(CheckListMappingModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageTaskCategoryChecklists, PermissionAction.Edit))
                return AccessDeniedView();

            var entity = await _checkListMappingService.GetCheckListMappingByIdAsync(model.Id);
            if (entity == null)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                var existingCheckList = await _checkListMasterService.GetCheckListByNameAsync(model.CheckListName?.Trim());

                if (existingCheckList != null)
                {
                    model.CheckListId = existingCheckList.Id;
                }
                else
                {
                    var newCheckList = new CheckListMaster
                    {
                        Title = model.CheckListName?.Trim(),
                        CreatedOn = await _dateTimeHelper.GetIndianTimeAsync(),
                        IsActive= true
                    };
                    await _checkListMasterService.InsertCheckListAsync(newCheckList);
                    model.CheckListId = newCheckList.Id;
                }

                entity = model.ToEntity(entity);
                await _checkListMappingService.UpdateCheckListMappingAsync(entity);
                string successMessage = await _localizationService.GetResourceAsync("Admin.Catalog.CheckListMapping.Updated");
                TempData["SuccessMessage"] = successMessage;
                ViewBag.RefreshPage = true;
            }

            return View("/Areas/Admin/Views/Extension/TaskCategories/CheckListMappingEdit.cshtml", model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageTaskCategoryChecklists, PermissionAction.Delete))
                return AccessDeniedView();

            var entity = await _checkListMappingService.GetCheckListMappingByIdAsync(id);
            if (entity == null)
                return RedirectToAction("List");

            await _checkListMappingService.DeleteCheckListMappingAsync(entity);

            return new NullJsonResult();
        }

        [HttpPost]
        public virtual async Task<IActionResult> DeleteSelected(ICollection<int> selectedIds)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageTaskCategoryChecklists, PermissionAction.Delete))
                return AccessDeniedView();

            if (selectedIds == null || selectedIds.Count == 0)
                return NoContent();

            var data = await _checkListMappingService.GetCheckListMappingsByIdsAsync(selectedIds.ToArray());

            foreach (var item in data)
            {
                await _checkListMappingService.DeleteCheckListMappingAsync(item);
            }

            return Json(new { Result = true });
        }

        [HttpGet]
        public virtual async Task<IActionResult> SearchCheckList(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
                return Json(new List<object>());

            var checklists = await _checkListMasterService.GetAllCheckListsAsync(searchText);

            var results = checklists
                .Select(c => new { Value = c.Id, Text = c.Title })
                .ToList();

            return Json(results);
        }
        #endregion
    }
}
