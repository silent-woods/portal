using App.Core.Domain.Departments;
using App.Core.Domain.Security;
using App.Services.Departments;
using App.Services.Localization;
using App.Services.Logging;
using App.Services.Messages;
using App.Services.Security;
using App.Web.Areas.Admin.Factories;
using App.Web.Areas.Admin.Models.Departments;
using App.Web.Framework.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace App.Web.Areas.Admin.Controllers.Extension
{
    public partial class DepartmentController : BaseAdminController
    {
        #region Fields

        private readonly ICustomerActivityService _customerActivityService;
        private readonly ILocalizationService _localizationService;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly IDepartmentModelFactory _departmentModelFactory;
        private readonly IDepartmentService _departmentService;

        #endregion Fields

        #region Ctor

        public DepartmentController(
            ICustomerActivityService customerActivityService,
            ILocalizationService localizationService,
            INotificationService notificationService,
            IPermissionService permissionService,
            IDepartmentModelFactory departmentModelFactory,
            IDepartmentService departmentService)
        {
            _customerActivityService = customerActivityService;
            _localizationService = localizationService;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _departmentModelFactory = departmentModelFactory;
            _departmentService = departmentService;
        }

        #endregion

        #region List

        public virtual IActionResult Index()
        {
            return RedirectToAction("List");
        }

        public virtual async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageDepartment, PermissionAction.View))
                return AccessDeniedView();

            //prepare model
            var model = await _departmentModelFactory.PrepareDepartmentSearchModelAsync(new DepartmentSearchModel());

            return View("/Areas/Admin/Views/Extension/Department/List.cshtml", model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> List(DepartmentSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageDepartment, PermissionAction.View))
                return await AccessDeniedDataTablesJson();

            //prepare model
            var model = await _departmentModelFactory.PrepareDepartmentListModelAsync(searchModel);

            return Json(model);
        }

        #endregion

        #region Create / Edit / Delete

        public virtual async Task<IActionResult> Create()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageDepartment, PermissionAction.Add))
                return AccessDeniedView();

            //prepare model
            var model = await _departmentModelFactory.PrepareDepartmentModelAsync(new DepartmentModel(), null);
            ViewBag.RefreshPage = false;

            return View("/Areas/Admin/Views/Extension/Department/Create.cshtml", model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Create(DepartmentModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageDepartment, PermissionAction.Add))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                //var department = model.ToEntity<Department>();
                Department department = new Department();
                department.Name = model.Name;
                department.CreatedOnUtc = DateTime.UtcNow;
                department.UpdatedOnUtc = DateTime.UtcNow;

                await _departmentService.InsertDepartmentAsync(department);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Department.Added"));

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = department.Id });
            }

            //prepare model
            model = await _departmentModelFactory.PrepareDepartmentModelAsync(model, null, true);

            //if we got this far, something failed, redisplay form
            return View("/Areas/Admin/Views/Extension/Department/Create.cshtml", model);
        }

        public virtual async Task<IActionResult> Edit(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageDepartment, PermissionAction.Edit))
                return AccessDeniedView();
            DepartmentModel model = new DepartmentModel();
            //try to get a department with the specified id
            var department = await _departmentService.GetDepartmentByIdAsync(id);
            if (department == null)
                return RedirectToAction("List");

            //prepare model
            model = await _departmentModelFactory.PrepareDepartmentModelAsync(model, department);

            return View("/Areas/Admin/Views/Extension/Department/Edit.cshtml", model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Edit(DepartmentModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageDepartment, PermissionAction.Edit))
                return AccessDeniedView();

            //try to get a department with the specified id
            var department = await _departmentService.GetDepartmentByIdAsync(model.Id);
            if (department == null)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                //department = model.ToEntity(department);
                department.Name = model.Name;
                department.UpdatedOnUtc = DateTime.UtcNow;
                await _departmentService.UpdateDepartmentAsync(department);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Department.Updated"));

                if (!continueEditing)
                    return RedirectToAction("list");

                return RedirectToAction("Edit", new { id = department.Id });

            }

            //prepare model
            model = await _departmentModelFactory.PrepareDepartmentModelAsync(model, department, true);

            //if we got this far, something failed, redisplay form
            return View("/Areas/Admin/Views/Extension/Department/Edit.cshtml", model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageDepartment, PermissionAction.Delete))
                return AccessDeniedView();

            //try to get a topic with the specified id
            var department = await _departmentService.GetDepartmentByIdAsync(id);
            if (department == null)
                return RedirectToAction("List");

            await _departmentService.DeleteDepartmentAsync(department);

            _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Department.Deleted"));

            return RedirectToAction("List");
        }

        public virtual async Task<IActionResult> DeleteSelected(ICollection<int> selectedIds)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageDepartment, PermissionAction.Delete))
                return AccessDeniedView();

            if (selectedIds == null || selectedIds.Count == 0)
                return NoContent();

            var data = await _departmentService.GetDepartmentByIdsAsync(selectedIds.ToArray());

            foreach (var item in data)
            {
                await _departmentService.DeleteDepartmentAsync(item);
            }

            return Json(new { Result = true });
        }

        #endregion
    }
}