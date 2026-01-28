using App.Core.Domain.Extension.Leaves;
using App.Core.Domain.Leaves;
using App.Core.Domain.Security;
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
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace App.Web.Areas.Admin.Controllers
{
    public partial class LeaveTransactionLogController : BaseAdminController
    {
        #region Fields
        private readonly IPermissionService _permissionService;
        private readonly ILeaveTypeModelFactory _leaveTypeModelFactory;
        private readonly ILeaveTypeService _leaveTypeService;
        private readonly INotificationService _notificationService;
        private readonly ILocalizationService _localizationService;
        private readonly ILeaveTransactionLogModelFactory _leaveTransactionLogModelFactory;
        private readonly ILeaveTransactionLogService _leaveTransactionLogService;
        private readonly IEmployeeService _employeeService;
        private readonly LeaveSettings _leaveSettings;
        private readonly IDateTimeHelper _dateTimeHelper;
        #endregion

        #region Ctor

        public LeaveTransactionLogController(IPermissionService permissionService,
            ILeaveTypeModelFactory leaveTypeModelFactory,
            ILeaveTypeService leaveTypeService,
            INotificationService notificationService,
            ILocalizationService localizationService,ILeaveTransactionLogModelFactory leaveTransactionLogModelFactory,
            ILeaveTransactionLogService leaveTransactionLogService,
            IEmployeeService employeeService,
            LeaveSettings leaveSettings,
            IDateTimeHelper dateTimeHelper
            )
        {
            _permissionService = permissionService;
            _leaveTypeModelFactory = leaveTypeModelFactory;
            _leaveTypeService = leaveTypeService;
            _notificationService = notificationService;
            _localizationService = localizationService;
            _leaveTransactionLogModelFactory = leaveTransactionLogModelFactory;
            _leaveTransactionLogService = leaveTransactionLogService;
            _employeeService = employeeService;
            _leaveSettings = leaveSettings;
            _dateTimeHelper = dateTimeHelper;
        }

        #endregion
        public virtual async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageLeaveTransaction, PermissionAction.View))
                return AccessDeniedView();
            //prepare model
            var model = await _leaveTransactionLogModelFactory.PrepareLeaveTransactionLogSearchModelAsync(new LeaveTransactionLogSearchModel());
            return View("/Areas/Admin/Views/Extension/LeaveTransactionLogs/List.cshtml", model);
        }
        [HttpPost]
        public virtual async Task<IActionResult> List(LeaveTransactionLogSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageLeaveTransaction, PermissionAction.View))
                return AccessDeniedView();

            //prepare model
            var model = await _leaveTransactionLogModelFactory.PrepareLeaveTransactionListModelAsync(searchModel);
            return Json(model);
        }

        public virtual async Task<IActionResult> Create()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageLeaveTransaction, PermissionAction.Add))
                return AccessDeniedView();

            //prepare model
            var model = await _leaveTransactionLogModelFactory.PrepareLeaveTransactionLogModelAsync(new LeaveTransactionLogModel(), null);

            ViewBag.RefreshPage = false;

            return View("/Areas/Admin/Views/Extension/LeaveTransactionLogs/Create.cshtml", model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Create(LeaveTransactionLogModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageLeaveTransaction, PermissionAction.Add))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                LeaveTransactionLog leave = new LeaveTransactionLog();
                leave.Id = model.Id;
                leave.LeaveId = 0;
                //leave.LeaveBalance = model.LeaveBalance;
                leave.BalanceChange = model.BalanceChange;
                leave.CreatedOnUTC = await _dateTimeHelper.GetUTCAsync();
                leave.Comment = model.Comment;
                leave.EmployeeId = model.EmployeeId;
                leave.ApprovedId = model.ApprovedId;
                leave.StatusId = model.StatusId;
                leave.BalanceMonthYear = new DateTime(model.Year, model.MonthId, 1);

                await _leaveTransactionLogService.AddLeaveTransactionLogAsync(leave.EmployeeId, leave.LeaveId, leave.StatusId, leave.ApprovedId, leave.LeaveBalance,leave.BalanceChange,leave.Comment,false,leave.BalanceMonthYear) ;

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.leavetransactionlog.Added"));

                ViewBag.RefreshPage = true;

                return View("/Areas/Admin/Views/Extension/LeaveTransactionLogs/Create.cshtml", model);

            }

            ViewBag.RefreshPage = false;
            var prevmodel = await _leaveTransactionLogModelFactory.PrepareLeaveTransactionLogModelAsync(new LeaveTransactionLogModel(), null);
            return View("/Areas/Admin/Views/Extension/LeaveTransactionLogs/Create.cshtml", prevmodel);

        }

        public virtual async Task<IActionResult> Edit(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageLeaveTransaction, PermissionAction.Edit))
                return AccessDeniedView();

            var leaveLog = await _leaveTransactionLogService.GetLeaveTransactionLogByIdAsync(id);
            if (leaveLog == null)
                return RedirectToAction("List");

            ViewBag.RefreshPage = false;
            //prepare model
            var model = await _leaveTransactionLogModelFactory.PrepareLeaveTransactionLogModelAsync(null, leaveLog);
            model.CreatedOnUTC = leaveLog.CreatedOnUTC;
            model.MonthId = leaveLog.BalanceMonthYear.Month;
            model.Year = leaveLog.BalanceMonthYear.Year;
            model.YearMonth = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(model.MonthId) + " " + model.Year;

            return View("/Areas/Admin/Views/Extension/LeaveTransactionLogs/Edit.cshtml", model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Edit(LeaveTransactionLogModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageLeaveTransaction, PermissionAction.Edit))
                return AccessDeniedView();


            var leaveLog = await _leaveTransactionLogService.GetLeaveTransactionLogByIdAsync(model.Id);
            if (leaveLog == null)
                return RedirectToAction("List");

            string prevComment = leaveLog.Comment;
            decimal prevChange = leaveLog.BalanceChange;



            if (ModelState.IsValid)
            {
                leaveLog = model.ToEntity(leaveLog);
                leaveLog.BalanceMonthYear = new DateTime(model.Year, model.MonthId, 1);
                leaveLog.CreatedOnUTC = model.CreatedOnUTC;
 
                var diffBalancechange = leaveLog.BalanceChange - prevChange;
                leaveLog.ManualBalanceChange = diffBalancechange;
                await _leaveTransactionLogService.UpdateLeaveTransactionLogAsync(leaveLog);


                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.leavetype.Updated"));

                string successMessage = await _localizationService.GetResourceAsync("Admin.Catalog.designation.Updated");
                TempData["SuccessMessage"] = successMessage;

                ViewBag.RefreshPage = true;

                return View("/Areas/Admin/Views/Extension/LeaveTransactionLogs/Edit.cshtml", model);

            }
            ViewBag.RefreshPage = false;
            model = await _leaveTransactionLogModelFactory.PrepareLeaveTransactionLogModelAsync(null, leaveLog);
            //if we got this far, something failed, redisplay form
            return View("/Areas/Admin/Views/Extension/LeaveTransactionLogs/Edit.cshtml", model);
        }
        [HttpPost]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageLeaveTransaction, PermissionAction.Delete))
                return AccessDeniedView();

            //try to get a leaveType with the specified id
            var leaveTransactionLog = await _leaveTransactionLogService.GetLeaveTransactionLogByIdAsync(id);
            if (leaveTransactionLog == null)
                return RedirectToAction("List");

            await _leaveTransactionLogService.DeleteLeaveTransationLogAsync(leaveTransactionLog);

            _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Catalog.LeaveTransactionLog.Deleted"));

            return RedirectToAction("List");
        }
        [HttpPost]
        public virtual async Task<IActionResult> DeleteSelected(ICollection<int> selectedIds)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageLeaveTransaction, PermissionAction.Delete))
                return AccessDeniedView();

            if (selectedIds == null || selectedIds.Count == 0)
                return NoContent();

            var data = await _leaveTransactionLogService.GetLeaveTrnasactionLogsByIdsAsync(selectedIds.ToArray());

            foreach (var item in data)
            {
                await _leaveTransactionLogService.DeleteLeaveTransationLogAsync(item);
            }
            return Json(new { Result = true });
        }

        [HttpPost]
        public virtual async Task<IActionResult> DeleteAll()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageLeaveTransaction, PermissionAction.Delete))
                return AccessDeniedView();

            var allLogs = await _leaveTransactionLogService.GetAllLeaveTransactionLogAsync("",0,null,null,0,0,"");

            foreach(var log in allLogs)
            {
                await _leaveTransactionLogService.DeleteLeaveTransationLogAsync(log);
            }

            return Json(new {Result  = true});

        }


        public virtual async Task<IActionResult> AddMonthlyLeave()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageLeaveTransaction, PermissionAction.Add))
                return AccessDeniedView();
            ViewBag.RefreshPage = false;
            var model = await _leaveTransactionLogModelFactory.PrepareAddMonthlyLeave(new LeaveTransactionLogModel());

            return View("/Areas/Admin/Views/Extension/LeaveTransactionLogs/AddMonthlyLeave.cshtml", model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> AddMonthlyLeave(LeaveTransactionLogModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageLeaveTransaction, PermissionAction.Add))
                return AccessDeniedView();

            if (model.MonthId == 0)
            {
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Catalog.LeaveTransactionLog.SelectMonthId"));
                ViewBag.RefreshPage = false;
                var leavelogmodel = await _leaveTransactionLogModelFactory.PrepareAddMonthlyLeave(new LeaveTransactionLogModel());
                return View("/Areas/Admin/Views/Extension/LeaveTransactionLogs/AddMonthlyLeave.cshtml", leavelogmodel);
            }


            await _leaveTransactionLogService.AddMonthlyLeave(model.MonthId, model.Year);
            //var employeeIds = await _employeeService.GetAllEmployeeIdsAsync();
            //var selectedMonthOption = model.MonthId;
            //var monthName = ((MonthEnum)selectedMonthOption).ToString();

            //// Selected leave type
            //var leaveType = 0;
            //if (_leaveSettings.SeletedLeaveTypeId != 0)
            //    leaveType = _leaveSettings.SeletedLeaveTypeId;

            //// Get all leave types
            //var leaveTypes = await _leaveTypeService.GetAllLeaveTypeAsync("");

            //if (employeeIds != null)
            //{
            //    foreach (var employeeId in employeeIds)
            //    {
            //        foreach (var type in leaveTypes)
            //        {
            //            //var leaveLog = await _leaveTransactionLogService.GetLeaveBalanceByLog(employeeId, type.Id);
            //            var leaveLog = await _leaveTransactionLogService.GetLeaveBalanceByLog(employeeId, type.Id);

            //            if (leaveLog != null)
            //            {
            //                // Deduct negative balance for all leave types
            //                if (leaveLog.LeaveBalance < 0)
            //                {
            //                    var monthYear = new DateTime(model.Year, model.MonthId, 1);
            //                    await _leaveTransactionLogService.AddLeaveTransactionLogAsync(
            //                        employeeId,
            //                        0, 0, type.Id, 0,
            //                        -leaveLog.LeaveBalance,
            //                        await _localizationService.GetResourceAsync("Admin.LeaveManagement.LeaveTransactionLogComment.DeductedReset"),false, monthYear);
            //                }

            //            }


            //            // Add monthly leave only for the selected leave type
            //            if (type.Id == leaveType)
            //            {
            //                var currTime = await _dateTimeHelper.GetIndianTimeAsync(); 
            //                var monthYear = new DateTime(model.Year, model.MonthId, 1);
            //                if (monthYear.Month == 1)
            //                {
            //                    await _leaveTransactionLogService.AddLeaveTransactionLogAsync(
            //                   employeeId,
            //                   0, 0, leaveType, 0,
            //                   1,
            //                   $"{await _localizationService.GetResourceAsync("admin.leavemanagement.leavetransactionlogcomment.monthlyadded")} - {monthName}", true, monthYear);
            //                }
            //                else
            //                {
            //                    await _leaveTransactionLogService.AddLeaveTransactionLogAsync(
            //                        employeeId,
            //                        0, 0, leaveType, 0,
            //                        1,
            //                        $"{await _localizationService.GetResourceAsync("admin.leavemanagement.leavetransactionlogcomment.monthlyadded")} - {monthName}", false, monthYear);
            //                }
            //            }
            //            else
            //            {
            //                //add sick leave record
            //                var lastsickleave = await _leaveTransactionLogService.GetLeaveBalanceByLogForPreviousMonth(employeeId, type.Id, model.MonthId, model.Year);
            //                if (lastsickleave != null)
            //                {
            //                    var currTime = await _dateTimeHelper.GetIndianTimeAsync(); 
            //                    var monthYear = new DateTime(model.Year, model.MonthId, 1);
            //                    await _leaveTransactionLogService.AddLeaveTransactionLogAsync(
            //                           employeeId,
            //                           0, 0, type.Id, 0,
            //                           0,
            //                           $"{await _localizationService.GetResourceAsync("admin.leavemanagement.leavetransactionlogcomment.monthlyadded")} - {monthName} sick leave", false, monthYear);
            //                }
            //            }
            //        }
            //    }
            //}

            ViewBag.RefreshPage = true;

            return View("/Areas/Admin/Views/Extension/LeaveTransactionLogs/AddMonthlyLeave.cshtml", model);
        }


        public virtual async Task<IActionResult> UpdateLeaveBalance(int employeeId, int leaveTypeId, decimal balanceChange, int monthId, int year)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageLeaveTransaction, PermissionAction.Edit))
                return AccessDeniedView();

            var leaveLog = await _leaveTransactionLogService.GetLeaveBalanceByLogForCurrentMonth(employeeId, leaveTypeId,monthId,year);
            decimal leaveBalance = 0;
            if (leaveLog != null)
            {
                leaveBalance = leaveLog.LeaveBalance + balanceChange;
            }

            return Json(new { newLeaveBalance = leaveBalance });
        }

        public virtual async Task<IActionResult> UpdateLeaveBalanceEdit(int Id,int employeeId, int leaveTypeId, decimal balanceChange, int monthId, int year)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageLeaveTransaction, PermissionAction.Edit))
                return AccessDeniedView();

            // Get the leave log for the current month
            var leaveLog = await _leaveTransactionLogService.GetLeaveTransactionLogByIdAsync(Id);
            decimal leaveBalance = 0;

            if (leaveLog != null)
            {
                decimal previousBalanceChange = leaveLog.BalanceChange; // Get previous balance change
                decimal changeDifference = balanceChange - previousBalanceChange; // Calculate the difference

                leaveBalance = leaveLog.LeaveBalance + changeDifference; // Apply only the difference

                
            }

            return Json(new { newLeaveBalance = leaveBalance });
        }




    }
}