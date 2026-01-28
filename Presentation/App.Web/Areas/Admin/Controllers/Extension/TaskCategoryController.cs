using App.Core;
using App.Core.Domain.Security;
using App.Services.Helpers;
using App.Services.Localization;
using App.Services.Logging;
using App.Services.Messages;
using App.Services.Security;
using App.Web.Areas.Admin.Factories;
using App.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using App.Web.Areas.Admin.Models.Extension.ProcessWorkflows;
using App.Web.Areas.Admin.Models.Extension.TaskCategories;
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
    [AuthorizeAdmin]
    [Area("Admin")]
    public partial class TaskCategoryController : BaseAdminController
    {
        #region Fields

        private readonly ITaskCategoryService _taskCategoryService;
        private readonly ITaskCategoryModelFactory _taskCategoryModelFactory;
        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ICheckListMappingService _checkListMappingService;

        #endregion

        #region Ctor
        public TaskCategoryController(
            ITaskCategoryService taskCategoryService,
            ITaskCategoryModelFactory taskCategoryModelFactory,
            ILocalizationService localizationService,
            INotificationService notificationService,
            IPermissionService permissionService,
            ICustomerActivityService customerActivityService,
            IDateTimeHelper dateTimeHelper,
            ICheckListMappingService checkListMappingService)
        {
            _taskCategoryService = taskCategoryService;
            _taskCategoryModelFactory = taskCategoryModelFactory;
            _localizationService = localizationService;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _customerActivityService = customerActivityService;
            _dateTimeHelper = dateTimeHelper;
            _checkListMappingService = checkListMappingService;
        }

        #endregion

        #region List
        public virtual IActionResult Index()
        {
            return RedirectToAction("List");
        }

        public virtual async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageTaskCategory, PermissionAction.View))
                return AccessDeniedView();

            var model = await _taskCategoryModelFactory.PrepareTaskCategorySearchModelAsync(new TaskCategorySearchModel());

            return View("/Areas/Admin/Views/Extension/TaskCategories/List.cshtml", model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> List(TaskCategorySearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageTaskCategory, PermissionAction.View))
                return await AccessDeniedDataTablesJson();

            var model = await _taskCategoryModelFactory.PrepareTaskCategoryListModelAsync(searchModel);

            return Json(model);
        }

        #endregion

        #region Create
        public virtual async Task<IActionResult> Create()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageTaskCategory, PermissionAction.Add))
                return AccessDeniedView();

            var model = await _taskCategoryModelFactory.PrepareTaskCategoryModelAsync(new TaskCategoryModel(), null);

            return View("/Areas/Admin/Views/Extension/TaskCategories/Create.cshtml", model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Create(TaskCategoryModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageTaskCategory, PermissionAction.Add))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var entity = model.ToEntity<TaskCategory>();
                entity.CreatedOn = await _dateTimeHelper.GetIndianTimeAsync();

                await _taskCategoryService.InsertTaskCategoryAsync(entity);

                await _customerActivityService.InsertActivityAsync("AddNewTaskCategory",
                    string.Format(await _localizationService.GetResourceAsync("ActivityLog.AddNewTaskCategory"), entity.CategoryName), entity);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.TaskCategories.Added"));

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = entity.Id });
            }

            model = await _taskCategoryModelFactory.PrepareTaskCategoryModelAsync(model, null, true);
            return View("/Areas/Admin/Views/Extension/TaskCategories/Create.cshtml", model);
        }

        #endregion

        #region Edit

        public virtual async Task<IActionResult> Edit(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageTaskCategory, PermissionAction.Edit))
                return AccessDeniedView();

            var entity = await _taskCategoryService.GetTaskCategoryByIdAsync(id);
            if (entity == null)
                return RedirectToAction("List");

            var model = await _taskCategoryModelFactory.PrepareTaskCategoryModelAsync(null, entity);

            return View("/Areas/Admin/Views/Extension/TaskCategories/Edit.cshtml", model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Edit(TaskCategoryModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageTaskCategory, PermissionAction.Edit))
                return AccessDeniedView();

            var entity = await _taskCategoryService.GetTaskCategoryByIdAsync(model.Id);
            if (entity == null)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                entity = model.ToEntity(entity);

                await _taskCategoryService.UpdateTaskCategoryAsync(entity);

                await _customerActivityService.InsertActivityAsync("EditTaskCategory",
                    string.Format(await _localizationService.GetResourceAsync("ActivityLog.EditTaskCategory"), entity.CategoryName), entity);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.TaskCategories.Updated"));

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = entity.Id });
            }

            model = await _taskCategoryModelFactory.PrepareTaskCategoryModelAsync(model, entity, true);
            return View("/Areas/Admin/Views/Extension/TaskCategories/Edit.cshtml", model);
        }

        #endregion

        #region Delete

        [HttpPost]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageTaskCategory, PermissionAction.Delete))
                return AccessDeniedView();

            var entity = await _taskCategoryService.GetTaskCategoryByIdAsync(id);
            if (entity == null)
                return RedirectToAction("List");

            await _taskCategoryService.DeleteTaskCategoryAsync(entity);

            await _customerActivityService.InsertActivityAsync("DeleteTaskCategory",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.DeleteTaskCategory"), entity.CategoryName), entity);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.TaskCategories.Deleted"));

            return RedirectToAction("List");
        }

        [HttpPost]
        public virtual async Task<IActionResult> DeleteSelected(ICollection<int> selectedIds)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageTaskCategory, PermissionAction.Delete))
                return AccessDeniedView();

            if (selectedIds == null || selectedIds.Count == 0)
                return NoContent();

            var entities = await _taskCategoryService.GetTaskCategoriesByIdsAsync(selectedIds.ToArray());
            foreach (var entity in entities)
                await _taskCategoryService.DeleteTaskCategoryAsync(entity);

            return Json(new { Result = true });
        }

        #endregion

        [HttpPost]
        public virtual async Task<IActionResult> CopyTaskCategory(int sourceId)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageTaskCategory, PermissionAction.Edit))
                return AccessDeniedView();

            var sourceCategory = await _taskCategoryService.GetTaskCategoryByIdAsync(sourceId);
            if (sourceCategory == null)
                return NotFound();

            var newCategory = new TaskCategory
            {
                CategoryName = string.Format("{0} - Copy", sourceCategory.CategoryName),
                Description = sourceCategory.Description,
                IsActive = sourceCategory.IsActive,
                CreatedOn = DateTime.UtcNow,
                DisplayName = sourceCategory.DisplayName
            };

            await _taskCategoryService.InsertTaskCategoryAsync(newCategory);
            var mappings = await _checkListMappingService.GetAllCheckListMappingsAsync(sourceId);
            foreach (var map in mappings)
            {
                var newMap = new CheckListMapping
                {
                    TaskCategoryId = newCategory.Id,
                    StatusId = map.StatusId,
                    CheckListId = map.CheckListId,
                    IsMandatory = map.IsMandatory,
                    OrderBy = map.OrderBy
                };
                await _checkListMappingService.InsertCheckListMappingAsync(newMap);
            }

            _notificationService.SuccessNotification("Task category copied successfully.");

            return RedirectToAction("Edit", new { id = newCategory.Id });
        }

    }
}
