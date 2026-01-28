using App.Core;
using App.Core.Domain.Extension.Leaves;
using App.Core.Domain.Leaves;
using App.Services.Configuration;
using App.Services.EmployeeAttendances;
using App.Services.Employees;
using App.Services.Helpers;
using App.Services.Leaves;
using App.Services.Localization;
using App.Services.Messages;
using App.Services.Security;
using App.Web.Areas.Admin.Factories;
using App.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using App.Web.Areas.Admin.Models.LeaveManagement;
using App.Web.Framework.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App.Core.Domain.Security;

namespace App.Web.Areas.Admin.Controllers
{
    public partial class LeaveManagementController : BaseAdminController
    {
        #region Fields
        private readonly IPermissionService _permissionService;
        private readonly ILeaveManagementModelFactory _leaveManagementModelFactory;
        private readonly ILeaveManagementService _leaveManagementService;
        private readonly INotificationService _notificationService;
        private readonly ILocalizationService _localizationService;
        private readonly IWorkContext _workContext;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly IEmployeeService _employeeService;
        private readonly IEmployeeAttendanceService _employeeAttendanceService;
        private readonly ILeaveTransactionLogService _leaveTransactionLogService;
        private readonly ILeaveTypeService _leaveTypeService;
        private readonly LeaveSettings _leaveSettings;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ISettingService _settingService;
        
        #endregion

        #region Ctor

        public LeaveManagementController(IPermissionService permissionService,
            ILeaveManagementModelFactory leaveManagementModelFactory,
            ILeaveManagementService leaveManagementService,
            INotificationService notificationService,
            ILocalizationService localizationService,
            IWorkContext workContext,
            IWorkflowMessageService workflowMessageService,
            IEmployeeService employeeService,
            IEmployeeAttendanceService employeeAttendanceService,
            ILeaveTransactionLogService leaveTransactionLogService,
            ILeaveTypeService leaveTypeService,
            LeaveSettings leaveSettings,
            IDateTimeHelper dateTimeHelper,
            ISettingService settingService
            )
        {
            _permissionService = permissionService;
            _leaveManagementModelFactory = leaveManagementModelFactory;
            _leaveManagementService = leaveManagementService;
            _notificationService = notificationService;
            _localizationService = localizationService;
            _workContext= workContext;
            _employeeService = employeeService;
            _workflowMessageService = workflowMessageService;
            _employeeAttendanceService = employeeAttendanceService;
            _leaveTransactionLogService = leaveTransactionLogService;
            _leaveTypeService = leaveTypeService;
            _leaveSettings = leaveSettings;
            _dateTimeHelper = dateTimeHelper;
            _settingService = settingService;
        }

        #endregion

        public virtual async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageLeaveManagement, PermissionAction.View))
                return AccessDeniedView();
            //prepare model
            var model = await _leaveManagementModelFactory.PrepareLeaveManagementSearchModelAsync(new LeaveManagementSearchModel());
            model.SearchPeriodId = 9;
           
            return View("/Areas/Admin/Views/Extension/LeaveManagements/List.cshtml", model);
        }
        [HttpPost]
        public virtual async Task<IActionResult> List(LeaveManagementSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageLeaveManagement, PermissionAction.View))
                return AccessDeniedView();
        
            //prepare model
            var model = await _leaveManagementModelFactory.PrepareLeaveManagementListModelAsync(searchModel);

            
            return Json(model);
        }

        public virtual async Task<IActionResult> Create()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageLeaveManagement, PermissionAction.Add))
                return AccessDeniedView();

            //prepare model
            var model = await _leaveManagementModelFactory.PrepareLeaveManagementModelAsync(new LeaveManagementModel(), null);

            return View("/Areas/Admin/Views/Extension/LeaveManagements/Create.cshtml", model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Create(LeaveManagementModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageLeaveManagement, PermissionAction.Add))
                return AccessDeniedView();

            int selectedEmployeeId = model.SelectedEmployeeId.FirstOrDefault();
            model.EmployeeId = selectedEmployeeId;

            

            if (selectedEmployeeId == 0)
            {
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Common.SelectEmployeeValidation"));
                model = await _leaveManagementModelFactory.PrepareLeaveManagementModelAsync(model, null, true);

                //if we got this far, something failed, redisplay form

                return View("/Areas/Admin/Views/Extension/LeaveManagements/Create.cshtml", model);
            }
       
                if (ModelState.IsValid)
                {
                var leaveManagement = model.ToEntity<LeaveManagement>();
                if(model.SelectedEmployeeIdForEmail !=null)
                {
                    leaveManagement.SendMailIds = string.Join(",", model.SelectedEmployeeIdForEmail);
                }

                if(leaveManagement.From > leaveManagement.To)
                {
                    _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Catalog.leaveManagement.Error.ToGreaterThenFrom"));
                    //prepare model
                    model = await _leaveManagementModelFactory.PrepareLeaveManagementModelAsync(model, null, true);

                    //if we got this far, something failed, redisplay form

                    return View("/Areas/Admin/Views/Extension/LeaveManagements/Create.cshtml", model);

                }
              
                var diffInDays =await _leaveManagementService.GetDifferenceByFromToAsync(model.From.Value,model.To.Value);
               

                if (model.NoOfDays != diffInDays - 0.5m && model.NoOfDays != diffInDays )
                {
                    _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Catalog.leaveManagement.Error.NoOfDaysisMore"));
                    model = await _leaveManagementModelFactory.PrepareLeaveManagementModelAsync(model, null, true);
                    //if we got this far, something failed, redisplay form
                    return View("/Areas/Admin/Views/Extension/LeaveManagements/Create.cshtml", model);
                }
                if(await _leaveManagementService.IsLeaveAlreadyTaken(model.EmployeeId, model.From.Value, model.To.Value))
                {
                    _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Catalog.leaveManagement.Error.LeaveAlreadyExist"));
                    model = await _leaveManagementModelFactory.PrepareLeaveManagementModelAsync(model, null, true);
                    //if we got this far, something failed, redisplay form
                    return View("/Areas/Admin/Views/Extension/LeaveManagements/Create.cshtml", model);
                }
                if (model.From.Value.Month != model.To.Value.Month || model.From.Value.Year != model.To.Value.Year)
                {
                    _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Catalog.leaveManagement.Error.MonthMustBeSame"));
                    model = await _leaveManagementModelFactory.PrepareLeaveManagementModelAsync(model, null, true);
                    //if we got this far, something failed, redisplay form
                    return View("/Areas/Admin/Views/Extension/LeaveManagements/Create.cshtml", model);
                }
                leaveManagement.CreatedOnUTC = await _dateTimeHelper.GetUTCAsync();

                if (model.StatusId== 2)
                {                    
                        leaveManagement.ApprovedOnUTC= await _dateTimeHelper.GetUTCAsync();                 
                }
                leaveManagement.ApprovedId= (await _workContext.GetCurrentCustomerAsync()).Id;

               
                await _leaveManagementService.InsertLeaveManagementAsync(leaveManagement);


                if(leaveManagement != null)
                {
                    await _employeeAttendanceService.UpdateEmployeeAttendanceBasedOnLeave(leaveManagement.EmployeeId, leaveManagement.From, leaveManagement.To, leaveManagement.NoOfDays, leaveManagement.StatusId);

                    //decimal balanceChange = 0;
                    //if (leaveManagement.StatusId == 2)
                    //    balanceChange = 0 -leaveManagement.NoOfDays;

                    //await _leaveTransactionLogService.AddLeaveTransactionLogAsync(leaveManagement.EmployeeId, leaveManagement.Id, leaveManagement.StatusId, leaveManagement.LeaveTypeId, leaveManagement.NoOfDays, balanceChange, await _localizationService.GetResourceAsync("Admin.LeaveManagement.LeaveTransactionLogComment.LeaveCreated"));
                }

                if (leaveManagement.StatusId == 1)
                {
                    var employee = await _employeeService.GetEmployeeByIdAsync(leaveManagement.EmployeeId);
                    await _workflowMessageService.SendLeaveRequestMessageAsync((await _workContext.GetWorkingLanguageAsync()).Id,
         employee.OfficialEmail.Trim(), employee.FirstName + " " + employee.LastName, employee.Id, leaveManagement.Id, model.SelectedEmployeeIdForEmail);
                }

                if (leaveManagement.StatusId == 2)
                {

                    var employee = await _employeeService.GetEmployeeByIdAsync(leaveManagement.EmployeeId);
                    await _workflowMessageService.SendLeaveApprovedMessageAsync((await _workContext.GetWorkingLanguageAsync()).Id,
         employee.OfficialEmail.Trim(), employee.FirstName + " " + employee.LastName, employee.Id, leaveManagement.Id,model.SelectedEmployeeIdForEmail);
                }
                if (leaveManagement.StatusId == 3)
                {
                    var employee = await _employeeService.GetEmployeeByIdAsync(leaveManagement.EmployeeId);
                    await _workflowMessageService.SendLeaveRejectedMessageAsync((await _workContext.GetWorkingLanguageAsync()).Id,
         employee.OfficialEmail.Trim(), employee.FirstName + " " + employee.LastName, employee.Id, leaveManagement.Id,model.SelectedEmployeeIdForEmail);
                }
                if (leaveManagement.StatusId == 4)
                {
                    var employee = await _employeeService.GetEmployeeByIdAsync(leaveManagement.EmployeeId);
                    await _workflowMessageService.SendLeaveCancelledMessageAsync((await _workContext.GetWorkingLanguageAsync()).Id,
         employee.OfficialEmail.Trim(), employee.FirstName + " " + employee.LastName, employee.Id, leaveManagement.Id,model.SelectedEmployeeIdForEmail);
                }
                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.leaveManagement.Added"));

               
                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = leaveManagement.Id });
            }
            //prepare model
            model = await _leaveManagementModelFactory.PrepareLeaveManagementModelAsync(model, null, true);

            //if we got this far, something failed, redisplay form

            return View("/Areas/Admin/Views/Extension/LeaveManagements/Create.cshtml", model);
        }

        public virtual async Task<IActionResult> Edit(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageLeaveManagement, PermissionAction.Edit))
                return AccessDeniedView();

            var leaveManagement = await _leaveManagementService.GetLeaveManagementByIdAsync(id);
            if (leaveManagement == null)
                return RedirectToAction("List");

            //prepare model
            var model = await _leaveManagementModelFactory.PrepareLeaveManagementModelAsync(null, leaveManagement);

            return View("/Areas/Admin/Views/Extension/LeaveManagements/Edit.cshtml", model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Edit(LeaveManagementModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageLeaveManagement, PermissionAction.Edit))
                return AccessDeniedView();

            //try to get a leaveManagement with the specified id
            var leaveManagement = await _leaveManagementService.GetLeaveManagementByIdAsync(model.Id);
            int prevStatusId = 0;
            decimal prevNoOfDays=0;
            decimal balanceChange = 0;
            int prevEmployeeId = 0;
            int prevLeaveType = 0;
            DateTime prevFrom = new DateTime();
            DateTime prevTo = new DateTime();
          
            if (leaveManagement != null)
            {
                prevStatusId = leaveManagement.StatusId;
                prevNoOfDays = leaveManagement.NoOfDays;
                prevLeaveType = leaveManagement.LeaveTypeId;
                prevEmployeeId = leaveManagement.EmployeeId;
                prevFrom = leaveManagement.From;
                prevTo = leaveManagement.To;

            }
            if (leaveManagement == null)
                return RedirectToAction("List");

           model.EmployeeId =leaveManagement.EmployeeId;

        

            if (leaveManagement.EmployeeId == 0)
            {

                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Common.SelectEmployeeValidation"));
                model = await _leaveManagementModelFactory.PrepareLeaveManagementModelAsync(model, null, true);

                //if we got this far, something failed, redisplay form

                return View("/Areas/Admin/Views/Extension/LeaveManagements/Edit.cshtml", model);
            }

            if (ModelState.IsValid)
            {
               
                leaveManagement = model.ToEntity(leaveManagement);
                if (model.SelectedEmployeeIdForEmail != null)
                {
                    leaveManagement.SendMailIds = string.Join(",", model.SelectedEmployeeIdForEmail);
                }
                if (leaveManagement.From > leaveManagement.To)
                {
                    _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Catalog.leaveManagement.Error.ToGreaterThenFrom"));
                    //prepare model
                    model = await _leaveManagementModelFactory.PrepareLeaveManagementModelAsync(model, null, true);

                    //if we got this far, something failed, redisplay form

                    return View("/Areas/Admin/Views/Extension/LeaveManagements/Edit.cshtml", model);

                }
                // Calculate the difference in days
                var diffInDays = await _leaveManagementService.GetDifferenceByFromToAsync(model.From.Value, model.To.Value);
                // Check if NoOfDays is valid
                if (model.NoOfDays != diffInDays - 0.5m && model.NoOfDays != diffInDays)
                {
                    _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Catalog.leaveManagement.Error.NoOfDaysisMore"));
                    model = await _leaveManagementModelFactory.PrepareLeaveManagementModelAsync(model, null, true);
                    //if we got this far, something failed, redisplay form
                    return View("/Areas/Admin/Views/Extension/LeaveManagements/Edit.cshtml", model);
                }
                if (await _leaveManagementService.IsLeaveAlreadyTaken(model.EmployeeId, model.From.Value, model.To.Value, model.Id) &&( prevFrom != model.From || prevTo != model.To || prevEmployeeId != model.EmployeeId))
                {
                    _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Catalog.leaveManagement.Error.LeaveAlreadyExist"));
                    model = await _leaveManagementModelFactory.PrepareLeaveManagementModelAsync(model, null, true);
                    //if we got this far, something failed, redisplay form
                    return View("/Areas/Admin/Views/Extension/LeaveManagements/Edit.cshtml", model);
                }
                if (model.From.Value.Month != model.To.Value.Month || model.From.Value.Year != model.To.Value.Year)
                {
                    _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Catalog.leaveManagement.Error.MonthMustBeSame"));
                    model = await _leaveManagementModelFactory.PrepareLeaveManagementModelAsync(model, null, true);
                    //if we got this far, something failed, redisplay form
                    return View("/Areas/Admin/Views/Extension/LeaveManagements/Edit.cshtml", model);
                }
                if (model.StatusId == 2)
                {
                    if (model.ApprovedOnUTC == null)
                    {
                        leaveManagement.ApprovedOnUTC = await _dateTimeHelper.GetUTCAsync();
                    }
                   
                }
                if(prevStatusId != 2 && leaveManagement.StatusId == 2)
                {
                        var employee = await _employeeService.GetEmployeeByIdAsync(leaveManagement.EmployeeId);
                        await _workflowMessageService.SendLeaveApprovedMessageAsync((await _workContext.GetWorkingLanguageAsync()).Id,
             employee.OfficialEmail.Trim(), employee.FirstName+" "+employee.LastName, employee.Id,leaveManagement.Id, model.SelectedEmployeeIdForEmail);

                   balanceChange = 0-leaveManagement.NoOfDays;
                }
                if(prevStatusId ==2 && leaveManagement.StatusId == 2  && prevNoOfDays != leaveManagement.NoOfDays)
                {
                    if (prevNoOfDays > leaveManagement.NoOfDays)
                    {
                        balanceChange = (prevNoOfDays -  leaveManagement.NoOfDays);
                    }
                    else
                    {
                        balanceChange = 0 - (leaveManagement.NoOfDays - prevNoOfDays);
                    }

                }
                if (prevStatusId == 2 && leaveManagement.StatusId != 2)
                {                 
                    balanceChange = leaveManagement.NoOfDays;
                }
                if (prevStatusId != 3 && leaveManagement.StatusId == 3)
                    {
                        var employee = await _employeeService.GetEmployeeByIdAsync(leaveManagement.EmployeeId);
                        await _workflowMessageService.SendLeaveRejectedMessageAsync((await _workContext.GetWorkingLanguageAsync()).Id,
             employee.OfficialEmail.Trim(), employee.FirstName + " " + employee.LastName, employee.Id, leaveManagement.Id, model.SelectedEmployeeIdForEmail);
                }
                    if (prevStatusId != 4 && leaveManagement.StatusId == 4)
                    {
                        var employee = await _employeeService.GetEmployeeByIdAsync(leaveManagement.EmployeeId);
                        await _workflowMessageService.SendLeaveCancelledMessageAsync((await _workContext.GetWorkingLanguageAsync()).Id,
             employee.OfficialEmail.Trim(), employee.FirstName + " " + employee.LastName, employee.Id, leaveManagement.Id,model.SelectedEmployeeIdForEmail);
                     }
                
                
                  
                   await _leaveManagementService.UpdateLeaveManagementAsync(leaveManagement);

                //update attendance when status id chnage
                await _employeeAttendanceService.UpdateEmployeeAttendanceBasedOnLeave(leaveManagement.EmployeeId, leaveManagement.From, leaveManagement.To, leaveManagement.NoOfDays, leaveManagement.StatusId, prevFrom,prevTo);

                //if(prevLeaveType != leaveManagement.LeaveTypeId || prevEmployeeId != leaveManagement.EmployeeId)
                //{
                //    decimal leaveBalanceBefore = 0;
                //    decimal leaveBalanceAfter = 0;

                //    if (prevStatusId ==2 && leaveManagement.StatusId == 2)
                //    {
                //        leaveBalanceBefore = prevNoOfDays;
                //        leaveBalanceAfter = (0 - leaveManagement.NoOfDays);
                //    }
                //    else if(prevStatusId ==2 && prevStatusId != 2)
                //    {
                //        leaveBalanceBefore = prevNoOfDays;
                //        leaveBalanceAfter = leaveManagement.NoOfDays;
                //    }
                //    else if(prevStatusId !=2 && prevStatusId == 2)
                //    {
                //        leaveBalanceBefore = 0;
                //        leaveBalanceAfter = (0 - leaveManagement.NoOfDays);
                //    }

                //    await _leaveTransactionLogService.AddLeaveTransactionLogAsync(prevEmployeeId, leaveManagement.Id, leaveManagement.StatusId, prevLeaveType, leaveManagement.NoOfDays, leaveBalanceBefore, await _localizationService.GetResourceAsync("Admin.LeaveManagement.LeaveTransactionLogComment.LeaveEdited"));

                //    await _leaveTransactionLogService.AddLeaveTransactionLogAsync(leaveManagement.EmployeeId, leaveManagement.Id, leaveManagement.StatusId, leaveManagement.LeaveTypeId, leaveManagement.NoOfDays, leaveBalanceAfter, await _localizationService.GetResourceAsync("Admin.LeaveManagement.LeaveTransactionLogComment.LeaveEdited"));
                //}
                //else
                //await _leaveTransactionLogService.AddLeaveTransactionLogAsync(leaveManagement.EmployeeId, leaveManagement.Id, leaveManagement.StatusId, leaveManagement.LeaveTypeId, leaveManagement.NoOfDays, balanceChange, await _localizationService.GetResourceAsync("Admin.LeaveManagement.LeaveTransactionLogComment.LeaveEdited"));

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.leaveManagement.Updated"));

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = leaveManagement.Id });
            }
            //prepare model
            model = await _leaveManagementModelFactory.PrepareLeaveManagementModelAsync(model, null, true);
            //if we got this far, something failed, redisplay form
            return View("/Areas/Admin/Views/Extension/LeaveManagements/Edit.cshtml", model);
        }
        [HttpPost]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageLeaveManagement, PermissionAction.Delete))
                return AccessDeniedView();

            //try to get a leaveType with the specified id
            var leaveManagement = await _leaveManagementService.GetLeaveManagementByIdAsync(id);
            if (leaveManagement == null)
                return RedirectToAction("List");

            await _leaveManagementService.DeleteLeaveManagementAsync(leaveManagement);

            //decimal balanceChange = 0;
            //    if (leaveManagement.StatusId == 2)
            //    balanceChange = leaveManagement.NoOfDays;

            //await _leaveTransactionLogService.AddLeaveTransactionLogAsync(leaveManagement.EmployeeId, leaveManagement.Id, leaveManagement.StatusId, leaveManagement.LeaveTypeId, leaveManagement.NoOfDays, balanceChange, await _localizationService.GetResourceAsync("Admin.LeaveManagement.LeaveTransactionLogComment.LeaveEdited"));

            _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Catalog.leaveManagement.Deleted"));

            return RedirectToAction("List");
        }
        [HttpPost]
        public virtual async Task<IActionResult> DeleteSelected(ICollection<int> selectedIds)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageLeaveManagement, PermissionAction.Delete))
                return AccessDeniedView();

            if (selectedIds == null || selectedIds.Count == 0)
                return NoContent();

            var data = await _leaveManagementService.GetLeaveManagementsByIdsAsync(selectedIds.ToArray());

            foreach (var item in data)
            {
                await _leaveManagementService.DeleteLeaveManagementAsync(item);
                //decimal balanceChange = 0;
                //if (item.StatusId == 2)
                //    balanceChange = item.NoOfDays;

                //await _leaveTransactionLogService.AddLeaveTransactionLogAsync(item.EmployeeId, item.Id, item.StatusId, item.LeaveTypeId, item.NoOfDays, balanceChange, await _localizationService.GetResourceAsync("Admin.LeaveManagement.LeaveTransactionLogComment.LeaveDeleted"));

            }
            return Json(new { Result = true });
        }


        [HttpPost]
        public virtual async Task<IActionResult> ApproveSelected(ICollection<int> selectedIds)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageBlog))
                return AccessDeniedView();

            if (selectedIds == null || selectedIds.Count == 0)
                return NoContent();

            //filter not approved leaves
            var notapprovedLeaves = (await _leaveManagementService.GetLeaveManagementsByIdsAsync(selectedIds.ToArray())).Where(leavereq => leavereq.StatusId !=2);

            foreach (var notapprovedLeave in notapprovedLeaves)
            {
                IList<int> selectedEmployeeIdForEmail = new List<int>();
                if (notapprovedLeave.SendMailIds != null && notapprovedLeave.SendMailIds != "")
                    selectedEmployeeIdForEmail = notapprovedLeave.SendMailIds
                            .Split(',')                         // Split by comma
                            .Select(int.Parse)                  // Convert each item to int
                            .ToList();


                notapprovedLeave.StatusId = 2;
                var employee = await _employeeService.GetEmployeeByIdAsync(notapprovedLeave.EmployeeId);
                await _workflowMessageService.SendLeaveApprovedMessageAsync((await _workContext.GetWorkingLanguageAsync()).Id,
                   employee.OfficialEmail.Trim(), employee.FirstName + " " + employee.LastName, employee.Id, notapprovedLeave.Id, selectedEmployeeIdForEmail);
                if (notapprovedLeave.ApprovedOnUTC == null)
                {
                    notapprovedLeave.ApprovedOnUTC = await _dateTimeHelper.GetUTCAsync();
                }
                await _leaveManagementService.UpdateLeaveManagementAsync(notapprovedLeave);

                await _employeeAttendanceService.UpdateEmployeeAttendanceBasedOnLeave(notapprovedLeave.EmployeeId, notapprovedLeave.From, notapprovedLeave.To, notapprovedLeave.NoOfDays, notapprovedLeave.StatusId);

                //decimal balanceChange = 0 - notapprovedLeave.NoOfDays;

                //await _leaveTransactionLogService.AddLeaveTransactionLogAsync(notapprovedLeave.EmployeeId, notapprovedLeave.Id, notapprovedLeave.StatusId, notapprovedLeave.LeaveTypeId, notapprovedLeave.NoOfDays, balanceChange, await _localizationService.GetResourceAsync("Admin.LeaveManagement.LeaveTransactionLogComment.LeaveEdited"));
            }
            return Json(new { Result = true });
        }

        [HttpGet]
        public async Task<JsonResult> CalculateNoOfDays(DateTime fromDate, DateTime toDate)
        {
            var diffInDays = await _leaveManagementService.GetDifferenceByFromToAsync(fromDate, toDate);
           
            return Json(diffInDays);
        }

        
       [HttpPost]
        public virtual async Task<IActionResult> ResetLeaves()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageLeaveManagement))
                return AccessDeniedView();

            var leaves = await _leaveManagementService.GetAllLeaveManagementAsync(null,null,null);
            if(leaves !=null)
            foreach(var leave in leaves)
            {
                    if (leave != null)
                    {
                        leave.IsArchived = true;
                        await _leaveManagementService.UpdateLeaveManagementAsync(leave);
                    }
            }

            var allEmployees = await _employeeService.GetAllEmployeeIdsAsync();
            var allLeaveType = await _leaveTypeService.GetAllLeaveTypeAsync("");
            var currTime = await _dateTimeHelper.GetIndianTimeAsync();
            var monthYear = new DateTime(currTime.Year,1, 1);
            foreach (var employee in allEmployees)
            {
                foreach (var leaveType in allLeaveType)
                {
                    if (leaveType.Id == _leaveSettings.SeletedLeaveTypeId) {
                        await _leaveTransactionLogService.AddLeaveTransactionLogAsync(employee, 0, 0, leaveType.Id, 0, 0, await _localizationService.GetResourceAsync("Admin.LeaveManagement.LeaveTransactionLogComment.LeaveReset"), true, monthYear);
                    }
                    else {
                        
                        await _leaveTransactionLogService.AddLeaveTransactionLogAsync(employee, 0, 0, leaveType.Id, 0,leaveType.Total_Allowed  , await _localizationService.GetResourceAsync("Admin.LeaveManagement.LeaveTransactionLogComment.LeaveReset"), true, monthYear);
                    }
                }
            }

            return Json(new { Result = true });
        }

        public virtual async Task<IActionResult> UpdateBalance()
        {
            var currTime = await _dateTimeHelper.GetIndianTimeAsync();

            if (_leaveSettings.LeaveTestDate != DateTime.MinValue)
                currTime = _leaveSettings.LeaveTestDate;

            DateTime? lastUpdateOn = null;
            if (!string.IsNullOrEmpty(_leaveSettings.LastUpdateBalance))
            {
                if (DateTime.TryParseExact(_leaveSettings.LastUpdateBalance, "d-MMM-yyyy HH:mm",
                                           System.Globalization.CultureInfo.InvariantCulture,
                                           System.Globalization.DateTimeStyles.None, out DateTime parsedDate))
                {
                    lastUpdateOn = parsedDate;
                }
            }

            await _leaveManagementService.ExecuteLeaveBalanceCalculation();
            _leaveSettings.LastUpdateBalance = currTime.ToString("d-MMM-yyyy HH:mm");

            await _settingService.SaveSettingAsync(_leaveSettings);
            return Json(new { Result = true });

        }


    }

}