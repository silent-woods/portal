using App.Core;
using App.Core.Domain.Security;
using App.Services.Customers;
using App.Services.Messages;
using App.Services.Employees;
using App.Services.Localization;
using App.Services.Security;
using App.Web.Areas.Admin.Controllers;
using App.Web.Framework.Mvc;
using App.Web.Framework.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Factories;
using Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Models.RecurringExpenses;
using Satyanam.Plugin.Misc.AccountManagement.Domain;
using Satyanam.Plugin.Misc.AccountManagement.Domain.Enums;
using Satyanam.Plugin.Misc.AccountManagement.Services;
using System;
using System.Threading.Tasks;

namespace Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Controllers;

[AuthorizeAdmin]
[Area("Admin")]
[AutoValidateAntiforgeryToken]
public partial class RecurringExpenseController : BaseAdminController
{
    #region Fields

    protected readonly ICustomerService _customerService;
    protected readonly IEmployeeService _employeeService;
    protected readonly IExpenseService _expenseService;
    protected readonly IExpenseModelFactory _expenseModelFactory;
    protected readonly ILocalizationService _localizationService;
    protected readonly INotificationService _notificationService;
    protected readonly IPermissionService _permissionService;
    protected readonly IWorkContext _workContext;

    private const string ListViewPath = "~/Plugins/Misc.AccountManagement/Areas/Admin/Views/Extension/RecurringExpense/RecurringExpenses.cshtml";
    private const string CreateViewPath = "~/Plugins/Misc.AccountManagement/Areas/Admin/Views/Extension/RecurringExpense/RecurringExpenseCreate.cshtml";
    private const string EditViewPath = "~/Plugins/Misc.AccountManagement/Areas/Admin/Views/Extension/RecurringExpense/RecurringExpenseEdit.cshtml";

    #endregion

    #region Ctor

    public RecurringExpenseController(ICustomerService customerService,
        IEmployeeService employeeService,
        IExpenseService expenseService,
        IExpenseModelFactory expenseModelFactory,
        ILocalizationService localizationService,
        INotificationService notificationService,
        IPermissionService permissionService,
        IWorkContext workContext)
    {
        _customerService = customerService;
        _employeeService = employeeService;
        _expenseService = expenseService;
        _expenseModelFactory = expenseModelFactory;
        _localizationService = localizationService;
        _notificationService = notificationService;
        _permissionService = permissionService;
        _workContext = workContext;
    }

    #endregion

    #region Utilities

    protected virtual async Task<int> GetCurrentEmployeeIdAsync()
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        if (!await _customerService.IsRegisteredAsync(customer))
            return 0;
        var employee = await _employeeService.GetEmployeeByCustomerIdAsync(customer.Id);
        return employee?.Id ?? 0;
    }

    #endregion

    #region Methods

    public virtual async Task<IActionResult> RecurringExpenses()
    {
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageRecurringExpenses, PermissionAction.View))
            return AccessDeniedView();

        var searchModel = await _expenseModelFactory.PrepareRecurringExpenseSearchModelAsync(new RecurringExpenseSearchModel());
        return View(ListViewPath, searchModel);
    }

    [HttpPost]
    public virtual async Task<IActionResult> RecurringExpenses(RecurringExpenseSearchModel searchModel)
    {
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageRecurringExpenses, PermissionAction.View))
            return await AccessDeniedDataTablesJson();

        var model = await _expenseModelFactory.PrepareRecurringExpenseListModelAsync(searchModel);
        return Json(model);
    }

    public virtual async Task<IActionResult> RecurringExpenseCreate()
    {
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageRecurringExpenses, PermissionAction.Add))
            return AccessDeniedView();

        var model = await _expenseModelFactory.PrepareRecurringExpenseModelAsync(null, null);
        return View(CreateViewPath, model);
    }

    [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
    public virtual async Task<IActionResult> RecurringExpenseCreate(RecurringExpenseModel model, bool continueEditing)
    {
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageRecurringExpenses, PermissionAction.Add))
            return AccessDeniedView();

        if (ModelState.IsValid)
        {
            var currentEmployeeId = await GetCurrentEmployeeIdAsync();

            var nextGenDate = CalculateNextDateFromNow(model);

            var recurringExpense = new RecurringExpense
            {
                ExpenseCategoryId = model.ExpenseCategoryId,
                Title = model.Title,
                Description = model.Description,
                Amount = model.Amount,
                CurrencyCode = string.IsNullOrWhiteSpace(model.CurrencyCode) ? "INR" : model.CurrencyCode,
                FrequencyId = model.FrequencyId,
                RecurrenceDay = model.RecurrenceDay,
                RecurrenceMonth = model.FrequencyId == (int)RecurringFrequencyEnum.Yearly ? model.RecurrenceMonth : null,
                StartDate = model.StartDate,
                EndDate = (model.EndDate == null || model.EndDate == DateTime.MinValue) ? (DateTime?)null : model.EndDate,
                NextGenerateOnUtc = nextGenDate,
                IsActive = model.IsActive,
                AccountGroupId = model.AccountGroupId,
                PaymentMethodId = model.PaymentMethodId,
                CreatedByEmployeeId = currentEmployeeId,
                Deleted = false
            };

            await _expenseService.InsertRecurringExpenseAsync(recurringExpense);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.RecurringExpense.Added"));

            if (!continueEditing)
                return RedirectToAction("RecurringExpenses");

            return RedirectToAction("RecurringExpenseEdit", new { id = recurringExpense.Id });
        }

        model = await _expenseModelFactory.PrepareRecurringExpenseModelAsync(model, null);
        return View(CreateViewPath, model);
    }

    public virtual async Task<IActionResult> RecurringExpenseEdit(int id)
    {
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageRecurringExpenses, PermissionAction.Edit))
            return AccessDeniedView();

        var recurringExpense = await _expenseService.GetRecurringExpenseByIdAsync(id);
        if (recurringExpense == null || recurringExpense.Deleted)
            return RedirectToAction("RecurringExpenses");

        var model = await _expenseModelFactory.PrepareRecurringExpenseModelAsync(null, recurringExpense);
        return View(EditViewPath, model);
    }

    [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
    public virtual async Task<IActionResult> RecurringExpenseEdit(RecurringExpenseModel model, bool continueEditing)
    {
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageRecurringExpenses, PermissionAction.Edit))
            return AccessDeniedView();

        var recurringExpense = await _expenseService.GetRecurringExpenseByIdAsync(model.Id);
        if (recurringExpense == null || recurringExpense.Deleted)
            return RedirectToAction("RecurringExpenses");

        if (ModelState.IsValid)
        {
            var scheduleChanged = recurringExpense.FrequencyId != model.FrequencyId
                || recurringExpense.RecurrenceDay != model.RecurrenceDay
                || recurringExpense.RecurrenceMonth != (model.FrequencyId == (int)RecurringFrequencyEnum.Yearly ? model.RecurrenceMonth : null);

            recurringExpense.ExpenseCategoryId = model.ExpenseCategoryId;
            recurringExpense.Title = model.Title;
            recurringExpense.Description = model.Description;
            recurringExpense.Amount = model.Amount;
            recurringExpense.CurrencyCode = string.IsNullOrWhiteSpace(model.CurrencyCode) ? "INR" : model.CurrencyCode;
            recurringExpense.FrequencyId = model.FrequencyId;
            recurringExpense.RecurrenceDay = model.RecurrenceDay;
            recurringExpense.RecurrenceMonth = model.FrequencyId == (int)RecurringFrequencyEnum.Yearly ? model.RecurrenceMonth : null;
            recurringExpense.AccountGroupId = model.AccountGroupId;
            recurringExpense.PaymentMethodId = model.PaymentMethodId;
            recurringExpense.StartDate = model.StartDate;
            recurringExpense.EndDate = (model.EndDate == null || model.EndDate == DateTime.MinValue) ? (DateTime?)null : model.EndDate;
            recurringExpense.NextGenerateOnUtc = scheduleChanged
                ? CalculateNextDateFromNow(model)
                : (model.NextGenerateOnUtc == null || model.NextGenerateOnUtc == DateTime.MinValue) ? (DateTime?)null : model.NextGenerateOnUtc;
            recurringExpense.IsActive = model.IsActive;

            await _expenseService.UpdateRecurringExpenseAsync(recurringExpense);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.RecurringExpense.Updated"));

            if (!continueEditing)
                return RedirectToAction("RecurringExpenses");

            return RedirectToAction("RecurringExpenseEdit", new { id = recurringExpense.Id });
        }

        model = await _expenseModelFactory.PrepareRecurringExpenseModelAsync(model, recurringExpense);
        return View(EditViewPath, model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> RecurringExpenseDelete(int id)
    {
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageRecurringExpenses, PermissionAction.Delete))
            return await AccessDeniedDataTablesJson();

        var recurringExpense = await _expenseService.GetRecurringExpenseByIdAsync(id);
        if (recurringExpense == null || recurringExpense.Deleted)
            return ErrorJson(await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.RecurringExpense.NotFound"));

        await _expenseService.DeleteRecurringExpenseAsync(recurringExpense);

        return new NullJsonResult();
    }

    #endregion

    #region Utilities

    protected virtual DateTime CalculateNextDateFromNow(RecurringExpenseModel model)
    {
        var today = DateTime.Today;
        switch ((RecurringFrequencyEnum)model.FrequencyId)
        {
            case RecurringFrequencyEnum.Weekly:
                var targetDow = model.RecurrenceDay == 7 ? DayOfWeek.Sunday : (DayOfWeek)model.RecurrenceDay;
                var next = today;
                while (next.DayOfWeek != targetDow)
                    next = next.AddDays(1);
                return next;

            case RecurringFrequencyEnum.Monthly:
                var dim = DateTime.DaysInMonth(today.Year, today.Month);
                var thisMonth = new DateTime(today.Year, today.Month, Math.Min(model.RecurrenceDay, dim));
                if (thisMonth >= today) return thisMonth;
                var nm = today.AddMonths(1);
                return new DateTime(nm.Year, nm.Month, Math.Min(model.RecurrenceDay, DateTime.DaysInMonth(nm.Year, nm.Month)));

            case RecurringFrequencyEnum.Quarterly:
                var check = today;
                for (var i = 0; i <= 12; i++)
                {
                    if (check.Month == 1 || check.Month == 4 || check.Month == 7 || check.Month == 10)
                    {
                        var qDays = DateTime.DaysInMonth(check.Year, check.Month);
                        var qCandidate = new DateTime(check.Year, check.Month, Math.Min(model.RecurrenceDay, qDays));
                        if (qCandidate >= today) return qCandidate;
                    }
                    check = new DateTime(check.Year, check.Month, 1).AddMonths(1);
                }
                return today;

            case RecurringFrequencyEnum.Yearly:
                var yearlyMonth = model.RecurrenceMonth ?? today.Month;
                var yDays = DateTime.DaysInMonth(today.Year, yearlyMonth);
                var thisYear = new DateTime(today.Year, yearlyMonth, Math.Min(model.RecurrenceDay, yDays));
                if (thisYear >= today) return thisYear;
                var nyDays = DateTime.DaysInMonth(today.Year + 1, yearlyMonth);
                return new DateTime(today.Year + 1, yearlyMonth, Math.Min(model.RecurrenceDay, nyDays));

            default:
                return today;
        }
    }

    #endregion
}
