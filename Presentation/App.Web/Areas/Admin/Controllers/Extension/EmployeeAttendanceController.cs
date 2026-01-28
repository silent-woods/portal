using App.Core.Domain.EmployeeAttendances;
using App.Services.EmployeeAttendances;
using App.Services.Helpers;
using App.Services.Localization;
using App.Services.Messages;
using App.Services.Security;
using App.Web.Areas.Admin.Factories;
using App.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using App.Web.Areas.Admin.Models.EmployeeAttendances;
using App.Web.Framework.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App.Core.Domain.Security;

namespace App.Web.Areas.Admin.Controllers
{
    public partial class EmployeeAttendanceController : BaseAdminController
    {
        #region Fields
        private readonly IPermissionService _permissionService;
        private readonly IEmployeeAttendanceModelFactory _employeeAttendanceModelFactory;
        private readonly IEmployeeAttendanceService _employeeAttendanceService;
        private readonly INotificationService _notificationService;
        private readonly ILocalizationService _localizationService;
        private readonly IDateTimeHelper _dateTimeHelper;
        #endregion

        #region Ctor

        public EmployeeAttendanceController(IPermissionService permissionService,
            IEmployeeAttendanceModelFactory employeeAttendanceModelFactory,
            IEmployeeAttendanceService employeeAttendanceService,
            INotificationService notificationService,
            ILocalizationService localizationService,
            IDateTimeHelper dateTimeHelper
            )
        {
            _permissionService = permissionService;
            _employeeAttendanceModelFactory = employeeAttendanceModelFactory;
            _employeeAttendanceService = employeeAttendanceService;
            _notificationService = notificationService;
            _localizationService = localizationService;
            _dateTimeHelper = dateTimeHelper;
        }

        #endregion
        public virtual async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageEmployeeAttendance, PermissionAction.View))
                return AccessDeniedView();
            //prepare model
            var model = await _employeeAttendanceModelFactory.PrepareEmployeeAttendanceSearchModelAsync(new EmployeeAttendanceSearchModel());
            return View("/Areas/Admin/Views/Extension/EmployeeAttendances/List.cshtml", model);
        }
        [HttpPost]
        public virtual async Task<IActionResult> List(EmployeeAttendanceSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageEmployeeAttendance, PermissionAction.View))
                return AccessDeniedView();

            //prepare model
            var model = await _employeeAttendanceModelFactory.PrepareEmployeeAttendanceListModelAsync(searchModel);
            return Json(model);
        }

        public virtual async Task<IActionResult> Create()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageEmployeeAttendance, PermissionAction.Add))
                return AccessDeniedView();

            //prepare model
            var model = await _employeeAttendanceModelFactory.PrepareEmployeeAttendanceModelAsync(new EmployeeAttendanceModel(), null);

            return View("/Areas/Admin/Views/Extension/EmployeeAttendances/Create.cshtml", model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Create(EmployeeAttendanceModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageEmployeeAttendance, PermissionAction.Add))
                return AccessDeniedView();

            int selectedEmployeeId = model.SelectedEmployeeId.FirstOrDefault();
            model.EmployeeId = selectedEmployeeId;

            var employeeAttendance = model.ToEntity<EmployeeAttendance>();

            if (selectedEmployeeId == 0)
            {
                //prepare model
                model = await _employeeAttendanceModelFactory.PrepareEmployeeAttendanceModelAsync(model, employeeAttendance, true);
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Common.SelectEmployeeValidation"));

                //if we got this far, something failed, redisplay form

                return View("/Areas/Admin/Views/Extension/EmployeeAttendances/Create.cshtml", model);
            }

            if (employeeAttendance.CheckIn.Date != employeeAttendance.CheckOut.Date)
            {
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Catalog.EmployeeAttendance.Error.SameDate"));
                model = await _employeeAttendanceModelFactory.PrepareEmployeeAttendanceModelAsync(model, employeeAttendance, true);
                return View("/Areas/Admin/Views/Extension/EmployeeAttendances/Create.cshtml", model);
            }
            if (employeeAttendance.CheckIn >= employeeAttendance.CheckOut)
            {
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Catalog.EmployeeAttendance.Error.ToGraterThenFrom"));

                model = await _employeeAttendanceModelFactory.PrepareEmployeeAttendanceModelAsync(model, employeeAttendance, true);

                //if we got this far, something failed, redisplay form

                return View("/Areas/Admin/Views/Extension/EmployeeAttendances/Create.cshtml", model);
            }

            if (ModelState.IsValid)
            {
                employeeAttendance.CreateOnUtc = await _dateTimeHelper.GetUTCAsync();
                employeeAttendance.UpdateOnUtc = await _dateTimeHelper.GetUTCAsync();

             
                await _employeeAttendanceService.InsertEmployeeAttendanceAsync(employeeAttendance);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.EmployeeAttendance.Added"));

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = employeeAttendance.Id });
            }
            //prepare model
            model = await _employeeAttendanceModelFactory.PrepareEmployeeAttendanceModelAsync(model, employeeAttendance, true);

            //if we got this far, something failed, redisplay form

            return View("/Areas/Admin/Views/Extension/EmployeeAttendances/Create.cshtml", model);
        }

        public virtual async Task<IActionResult> Edit(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageEmployeeAttendance, PermissionAction.Edit))
                return AccessDeniedView();

            var employeeAttendance = await _employeeAttendanceService.GetEmployeeAttendanceByIdAsync(id);
            if (employeeAttendance == null)
                return RedirectToAction("List");

            //prepare model
            var model = await _employeeAttendanceModelFactory.PrepareEmployeeAttendanceModelAsync(null, employeeAttendance);

            return View("/Areas/Admin/Views/Extension/EmployeeAttendances/Edit.cshtml", model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Edit(EmployeeAttendanceModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageEmployeeAttendance, PermissionAction.Edit))
                return AccessDeniedView();


          
            //try to get a project with the specified id
            var employeeAttendance = await _employeeAttendanceService.GetEmployeeAttendanceByIdAsync(model.Id);
            if (employeeAttendance == null)
                return RedirectToAction("List");


            int selectedEmployeeId = model.SelectedEmployeeId.FirstOrDefault();
            model.EmployeeId = selectedEmployeeId;

            if (selectedEmployeeId == 0)
            {
               
                model = await _employeeAttendanceModelFactory.PrepareEmployeeAttendanceModelAsync(model, employeeAttendance, true);
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Common.SelectEmployeeValidation"));


                return View("/Areas/Admin/Views/Extension/EmployeeAttendances/Edit.cshtml", model);
            }

            employeeAttendance = model.ToEntity(employeeAttendance);

            if (employeeAttendance.CheckIn.Date != employeeAttendance.CheckOut.Date)
            {
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Catalog.EmployeeAttendance.Error.SameDate"));
                model = await _employeeAttendanceModelFactory.PrepareEmployeeAttendanceModelAsync(model, employeeAttendance, true);
                return View("/Areas/Admin/Views/Extension/EmployeeAttendances/Edit.cshtml", model);
            }
            if (employeeAttendance.CheckIn >= employeeAttendance.CheckOut)
            {
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Catalog.EmployeeAttendance.Error.ToGraterThenFrom"));

                model = await _employeeAttendanceModelFactory.PrepareEmployeeAttendanceModelAsync(model, employeeAttendance, true);

                return View("/Areas/Admin/Views/Extension/EmployeeAttendances/Create.cshtml", model);
            }
            if (ModelState.IsValid)
            {
               
                employeeAttendance.UpdateOnUtc = await _dateTimeHelper.GetUTCAsync();

                await _employeeAttendanceService.UpdateEmployeeAttendanceAsync(employeeAttendance);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.EmployeeAttendance.Updated"));

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = employeeAttendance.Id });
            }

            model = await _employeeAttendanceModelFactory.PrepareEmployeeAttendanceModelAsync(model, employeeAttendance, true);

            //if we got this far, something failed, redisplay form
            return View("/Areas/Admin/Views/Extension/EmployeeAttendances/Edit.cshtml", model);
        }
        [HttpPost]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageEmployeeAttendance, PermissionAction.Delete))
                return AccessDeniedView();

            //try to get a leaveType with the specified id
            var employeeAttendance = await _employeeAttendanceService.GetEmployeeAttendanceByIdAsync(id);
            if (employeeAttendance == null)
                return RedirectToAction("List");

            await _employeeAttendanceService.DeleteEmployeeAttendanceAsync(employeeAttendance);

            _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Catalog.EmployeeAttendance.Deleted"));

            return RedirectToAction("List");
        }
        [HttpPost]
        public virtual async Task<IActionResult> DeleteSelected(ICollection<int> selectedIds)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageEmployeeAttendance, PermissionAction.Delete))
                return AccessDeniedView();

            if (selectedIds == null || selectedIds.Count == 0)
                return NoContent();

            var data = await _employeeAttendanceService.GetEmployeeAttendanceByIdsAsync(selectedIds.ToArray());

            foreach (var item in data)
            {
                await _employeeAttendanceService.DeleteEmployeeAttendanceAsync(item);
            }
            return Json(new { Result = true });
        }
    }
}