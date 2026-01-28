using App.Core.Domain.Extension.Leaves;
using App.Core.Domain.Leaves;
using App.Core.Domain.Security;
using App.Services.Employees;
using App.Services.Leaves;
using App.Services.Localization;
using App.Services.Messages;
using App.Services.Security;
using App.Web.Areas.Admin.Factories;
using App.Web.Areas.Admin.Models.Leavetypes;
using App.Web.Framework.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace App.Web.Areas.Admin.Controllers
{
    public partial class LeaveTypeController : BaseAdminController
    {
        #region Fields
        private readonly IPermissionService _permissionService;
        private readonly ILeaveTypeModelFactory _leaveTypeModelFactory;
        private readonly ILeaveTypeService _leaveTypeService;
        private readonly INotificationService _notificationService;
        private readonly ILocalizationService _localizationService;
        private readonly ILeaveTransactionLogService _leaveTransactionLogService;
        private readonly IEmployeeService _employeeService;
        private readonly LeaveSettings _leaveSettings;
        #endregion

        #region Ctor

        public LeaveTypeController(IPermissionService permissionService,
            ILeaveTypeModelFactory leaveTypeModelFactory,
            ILeaveTypeService leaveTypeService,
            INotificationService notificationService,
            ILocalizationService localizationService,
            ILeaveTransactionLogService leaveTransactionLogService,
            IEmployeeService employeeService,
            LeaveSettings leaveSettings
            )
        {
            _permissionService = permissionService;
            _leaveTypeModelFactory = leaveTypeModelFactory;
            _leaveTypeService = leaveTypeService;
            _notificationService = notificationService;
            _localizationService = localizationService;
            _leaveTransactionLogService = leaveTransactionLogService;
            _employeeService = employeeService;
            _leaveSettings = leaveSettings;
        }

        #endregion
        public virtual async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageLeaveType, PermissionAction.View))
                return AccessDeniedView();
            //prepare model
            var model = await _leaveTypeModelFactory.PrepareLeaveTypeSearchModelAsync(new LeaveTypeSearchModel());
            return View("/Areas/Admin/Views/Extension/LeaveType/List.cshtml", model);
        }
        [HttpPost]
        public virtual async Task<IActionResult> List(LeaveTypeSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageLeaveType, PermissionAction.View))
                return AccessDeniedView();

            //prepare model
            var model = await _leaveTypeModelFactory.PrepareLeaveTypeListModelAsync(searchModel);
            return Json(model);
        }

        public virtual async Task<IActionResult> Create()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageLeaveType, PermissionAction.Add))
                return AccessDeniedView();

            //prepare model
            var model = await _leaveTypeModelFactory.PrepareLeaveTypeModelAsync(new LeaveTypeModel(), null);

            return View("/Areas/Admin/Views/Extension/LeaveType/Create.cshtml", model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Create(LeaveTypeModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageLeaveType, PermissionAction.Add))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                Leave leave = new Leave();
                leave.Id = model.Id;
                leave.Type = model.Type;
                leave.Total_Allowed = model.Total_Allowed;
                leave.Description = model.Description;
                leave.CreateOnUtc = DateTime.UtcNow;
                leave.UpdateOnUtc = DateTime.UtcNow;
               

                await _leaveTypeService.InsertLeaveTypeAsync(leave);

                var allEmployee  = await _employeeService.GetAllEmployeeIdsAsync();
                if (allEmployee != null) {
                    foreach (var employeeId in allEmployee)
                        await _leaveTransactionLogService.AddLeaveTransactionLogAsync(employeeId, 0, 0, leave.Id, 0, leave.Total_Allowed,  await _localizationService.GetResourceAsync("Admin.LeaveManagement.LeaveTransactionLogComment.LeaveTypeAdded"));
                        }
                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.leavetype.Added"));

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = leave.Id });
            }
            //prepare model
            model = await _leaveTypeModelFactory.PrepareLeaveTypeModelAsync(model, null, true);

            //if we got this far, something failed, redisplay form

            return View("/Areas/Admin/Views/Extension/LeaveType/Create.cshtml", model);
        }

        public virtual async Task<IActionResult> Edit(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageLeaveType, PermissionAction.Edit))
                return AccessDeniedView();

            var leaveType = await _leaveTypeService.GetLeaveTypeByIdAsync(id);
            if (leaveType == null)
                return RedirectToAction("List");

            //prepare model
            var model = await _leaveTypeModelFactory.PrepareLeaveTypeModelAsync(null, leaveType);

            return View("/Areas/Admin/Views/Extension/LeaveType/Edit.cshtml", model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Edit(LeaveTypeModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageLeaveType, PermissionAction.Edit))
                return AccessDeniedView();

            //try to get a leaveType with the specified id
            var leaveType = await _leaveTypeService.GetLeaveTypeByIdAsync(model.Id);
            if (leaveType == null)
                return RedirectToAction("List");


            var prevTotal_Allowed = leaveType.Total_Allowed;

            if (ModelState.IsValid)
            {
                leaveType.Type = model.Type;
                leaveType.Total_Allowed = model.Total_Allowed;
                leaveType.Description = model.Description;
                leaveType.UpdateOnUtc = DateTime.UtcNow;

                await _leaveTypeService.UpdateLeaveTypeAsync(leaveType);

                if (prevTotal_Allowed != leaveType.Total_Allowed && leaveType.Id != _leaveSettings.SeletedLeaveTypeId)
                {
                    decimal changeInTotal =  leaveType.Total_Allowed- prevTotal_Allowed;

                    var allEmployee = await _employeeService.GetAllEmployeeIdsAsync();
                    if(allEmployee!=null)
                        foreach(var employeeId in allEmployee)
                        {
                         
                            await _leaveTransactionLogService.AddLeaveTransactionLogAsync(employeeId, 0, 0, leaveType.Id, 0, changeInTotal, await _localizationService.GetResourceAsync("Admin.LeaveManagement.LeaveTransactionLogComment.AllowedLeaveChanged"));

                        }
                }



                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.leavetype.Updated"));

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = leaveType.Id });
            }
            //if we got this far, something failed, redisplay form
            return View("/Areas/Admin/Views/Extension/LeaveType/Edit.cshtml", model);
        }
        [HttpPost]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageLeaveType, PermissionAction.Delete))
                return AccessDeniedView();

            //try to get a leaveType with the specified id
            var leaveType = await _leaveTypeService.GetLeaveTypeByIdAsync(id);
            if (leaveType == null)
                return RedirectToAction("List");

            await _leaveTypeService.DeleteLeaveTypeAsync(leaveType);

            _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Catalog.LeaveType.Deleted"));

            return RedirectToAction("List");
        }
        [HttpPost]
        public virtual async Task<IActionResult> DeleteSelected(ICollection<int> selectedIds)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageLeaveType, PermissionAction.Delete))
                return AccessDeniedView();

            if (selectedIds == null || selectedIds.Count == 0)
                return NoContent();

            var data = await _leaveTypeService.GetLeaveTypesByIdsAsync(selectedIds.ToArray());

            foreach (var item in data)
            {
                await _leaveTypeService.DeleteLeaveTypeAsync(item);
            }
            return Json(new { Result = true });
        }
    }
}