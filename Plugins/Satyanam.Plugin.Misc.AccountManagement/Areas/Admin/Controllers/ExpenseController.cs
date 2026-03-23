using App.Core;
using App.Core.Domain.Media;
using App.Core.Domain.Security;
using App.Services.Customers;
using App.Services.Employees;
using App.Services.Localization;
using App.Services.Media;
using App.Services.Messages;
using App.Services.Security;
using App.Web.Areas.Admin.Controllers;
using App.Web.Framework.Mvc;
using App.Web.Framework.Mvc.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Factories;
using Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Models.Expenses;
using Satyanam.Plugin.Misc.AccountManagement.Domain;
using Satyanam.Plugin.Misc.AccountManagement.Services;
using System;
using System.Threading.Tasks;

namespace Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Controllers;

[AuthorizeAdmin]
[Area("Admin")]
[AutoValidateAntiforgeryToken]
public partial class ExpenseController : BaseAdminController
{
    #region Fields

    protected readonly ICustomerService _customerService;
    protected readonly IDownloadService _downloadService;
    protected readonly IEmployeeService _employeeService;
    protected readonly IExpenseService _expenseService;
    protected readonly IExpenseModelFactory _expenseModelFactory;
    protected readonly ILocalizationService _localizationService;
    protected readonly INotificationService _notificationService;
    protected readonly IPermissionService _permissionService;
    protected readonly IWorkContext _workContext;

    private const string ListViewPath = "~/Plugins/Misc.AccountManagement/Areas/Admin/Views/Extension/Expense/Expenses.cshtml";
    private const string CreateViewPath = "~/Plugins/Misc.AccountManagement/Areas/Admin/Views/Extension/Expense/ExpenseCreate.cshtml";
    private const string EditViewPath = "~/Plugins/Misc.AccountManagement/Areas/Admin/Views/Extension/Expense/ExpenseEdit.cshtml";

    #endregion

    #region Ctor

    public ExpenseController(ICustomerService customerService,
        IDownloadService downloadService,
        IEmployeeService employeeService,
        IExpenseService expenseService,
        IExpenseModelFactory expenseModelFactory,
        ILocalizationService localizationService,
        INotificationService notificationService,
        IPermissionService permissionService,
        IWorkContext workContext)
    {
        _customerService = customerService;
        _downloadService = downloadService;
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

    public virtual async Task<IActionResult> Expenses()
    {
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageExpenses, PermissionAction.View))
            return AccessDeniedView();

        var searchModel = await _expenseModelFactory.PrepareExpenseSearchModelAsync(new ExpenseSearchModel());
        return View(ListViewPath, searchModel);
    }

    [HttpPost]
    public virtual async Task<IActionResult> Expenses(ExpenseSearchModel searchModel)
    {
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageExpenses, PermissionAction.View))
            return await AccessDeniedDataTablesJson();

        var model = await _expenseModelFactory.PrepareExpenseListModelAsync(searchModel);
        return Json(model);
    }

    public virtual async Task<IActionResult> ExpenseCreate()
    {
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageExpenses, PermissionAction.Add))
            return AccessDeniedView();

        var model = await _expenseModelFactory.PrepareExpenseModelAsync(null, null);
        return View(CreateViewPath, model);
    }

    [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
    public virtual async Task<IActionResult> ExpenseCreate(ExpenseModel model, IFormFile receiptFile, bool continueEditing)
    {
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageExpenses, PermissionAction.Add))
            return AccessDeniedView();

        if (ModelState.IsValid)
        {
            var currentEmployeeId = await GetCurrentEmployeeIdAsync();

            int receiptDownloadId = 0;
            if (receiptFile != null && receiptFile.Length > 0)
            {
                var fileBinary = await _downloadService.GetDownloadBitsAsync(receiptFile);
                var download = new Download
                {
                    DownloadGuid = Guid.NewGuid(),
                    UseDownloadUrl = false,
                    DownloadUrl = string.Empty,
                    DownloadBinary = fileBinary,
                    ContentType = receiptFile.ContentType,
                    Filename = System.IO.Path.GetFileNameWithoutExtension(receiptFile.FileName),
                    Extension = System.IO.Path.GetExtension(receiptFile.FileName)?.ToLowerInvariant(),
                    IsNew = true
                };
                await _downloadService.InsertDownloadAsync(download);
                receiptDownloadId = download.Id;
            }

            var expense = new Expense
            {
                ExpenseCategoryId = model.ExpenseCategoryId,
                Title = model.Title,
                Description = model.Description,
                Amount = model.Amount,
                CurrencyCode = string.IsNullOrWhiteSpace(model.CurrencyCode) ? "INR" : model.CurrencyCode,
                ExpenseDate = model.ExpenseDate,
                SubmittedByEmployeeId = currentEmployeeId,
                ReceiptDownloadId = receiptDownloadId,
                PaymentMethodId = model.PaymentMethodId,
                EmployeeSalaryRecordId = 0,
                RecurringExpenseId = 0,
                Deleted = false
            };

            await _expenseService.InsertExpenseAsync(expense);

            if (model.AccountGroupId > 0)
                await _expenseService.CreateLinkedTransactionAsync(expense, model.AccountGroupId, model.PaymentMethodId, currentEmployeeId);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.Expense.Added"));

            if (!continueEditing)
                return RedirectToAction("Expenses");

            return RedirectToAction("ExpenseEdit", new { id = expense.Id });
        }

        model = await _expenseModelFactory.PrepareExpenseModelAsync(model, null);
        return View(CreateViewPath, model);
    }

    public virtual async Task<IActionResult> ExpenseEdit(int id)
    {
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageExpenses, PermissionAction.Edit))
            return AccessDeniedView();

        var expense = await _expenseService.GetExpenseByIdAsync(id);
        if (expense == null || expense.Deleted)
            return RedirectToAction("Expenses");

        var model = await _expenseModelFactory.PrepareExpenseModelAsync(null, expense);
        return View(EditViewPath, model);
    }

    [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
    public virtual async Task<IActionResult> ExpenseEdit(ExpenseModel model, IFormFile receiptFile, bool continueEditing)
    {
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageExpenses, PermissionAction.Edit))
            return AccessDeniedView();

        var expense = await _expenseService.GetExpenseByIdAsync(model.Id);
        if (expense == null || expense.Deleted)
            return RedirectToAction("Expenses");

        if (ModelState.IsValid)
        {
            expense.ExpenseCategoryId = model.ExpenseCategoryId;
            expense.Title = model.Title;
            expense.Description = model.Description;
            expense.Amount = model.Amount;
            expense.CurrencyCode = string.IsNullOrWhiteSpace(model.CurrencyCode) ? "INR" : model.CurrencyCode;
            expense.ExpenseDate = model.ExpenseDate;
            expense.PaymentMethodId = model.PaymentMethodId;

            if (receiptFile != null && receiptFile.Length > 0)
            {
                var fileBinary = await _downloadService.GetDownloadBitsAsync(receiptFile);
                var download = new Download
                {
                    DownloadGuid = Guid.NewGuid(),
                    UseDownloadUrl = false,
                    DownloadUrl = string.Empty,
                    DownloadBinary = fileBinary,
                    ContentType = receiptFile.ContentType,
                    Filename = System.IO.Path.GetFileNameWithoutExtension(receiptFile.FileName),
                    Extension = System.IO.Path.GetExtension(receiptFile.FileName)?.ToLowerInvariant(),
                    IsNew = true
                };
                await _downloadService.InsertDownloadAsync(download);
                expense.ReceiptDownloadId = download.Id;
            }

            await _expenseService.UpdateExpenseAsync(expense);

            if (model.AccountGroupId == 0 && expense.AccountTransactionId > 0)
            {
                await _expenseService.RemoveLinkedTransactionAsync(expense);
            }
            else if (model.AccountGroupId > 0 && expense.AccountTransactionId > 0)
            {
                await _expenseService.UpdateLinkedTransactionAccountGroupAsync(expense, model.AccountGroupId);
            }
            else if (expense.AccountTransactionId <= 0 && model.AccountGroupId > 0)
            {
                var currentEmployeeId = await GetCurrentEmployeeIdAsync();
                await _expenseService.CreateLinkedTransactionAsync(expense, model.AccountGroupId, model.PaymentMethodId, currentEmployeeId);
            }

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.Expense.Updated"));

            if (!continueEditing)
                return RedirectToAction("Expenses");

            return RedirectToAction("ExpenseEdit", new { id = expense.Id });
        }

        model = await _expenseModelFactory.PrepareExpenseModelAsync(model, expense);
        return View(EditViewPath, model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> ExpenseDelete(int id)
    {
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageExpenses, PermissionAction.Delete))
            return await AccessDeniedDataTablesJson();

        var expense = await _expenseService.GetExpenseByIdAsync(id);
        if (expense == null || expense.Deleted)
            return ErrorJson(await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.Expense.NotFound"));

        if (expense.EmployeeSalaryRecordId > 0)
            return ErrorJson(await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.Expense.CannotDeleteSalaryLinked"));

        await _expenseService.DeleteExpenseAsync(expense);

        return new NullJsonResult();
    }

    #endregion
}
