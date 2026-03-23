using App.Core;
using App.Core.Domain.Security;
using App.Services.Localization;
using App.Services.Messages;
using App.Services.Security;
using App.Web.Areas.Admin.Controllers;
using App.Web.Framework.Mvc;
using App.Web.Framework.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Factories;
using Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Models.ExpenseCategories;
using Satyanam.Plugin.Misc.AccountManagement.Domain;
using Satyanam.Plugin.Misc.AccountManagement.Services;
using System;
using System.Threading.Tasks;

namespace Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Controllers;

[AuthorizeAdmin]
[Area("Admin")]
[AutoValidateAntiforgeryToken]
public partial class ExpenseCategoryController : BaseAdminController
{
    #region Fields

    protected readonly IExpenseCategoryService _expenseCategoryService;
    protected readonly IExpenseModelFactory _expenseModelFactory;
    protected readonly ILocalizationService _localizationService;
    protected readonly INotificationService _notificationService;
    protected readonly IPermissionService _permissionService;

    private const string ListViewPath = "~/Plugins/Misc.AccountManagement/Areas/Admin/Views/Extension/ExpenseCategory/ExpenseCategories.cshtml";
    private const string CreateViewPath = "~/Plugins/Misc.AccountManagement/Areas/Admin/Views/Extension/ExpenseCategory/ExpenseCategoryCreate.cshtml";
    private const string EditViewPath = "~/Plugins/Misc.AccountManagement/Areas/Admin/Views/Extension/ExpenseCategory/ExpenseCategoryEdit.cshtml";

    #endregion

    #region Ctor

    public ExpenseCategoryController(IExpenseCategoryService expenseCategoryService,
        IExpenseModelFactory expenseModelFactory,
        ILocalizationService localizationService,
        INotificationService notificationService,
        IPermissionService permissionService)
    {
        _expenseCategoryService = expenseCategoryService;
        _expenseModelFactory = expenseModelFactory;
        _localizationService = localizationService;
        _notificationService = notificationService;
        _permissionService = permissionService;
    }

    #endregion

    #region Methods

    public virtual async Task<IActionResult> ExpenseCategories()
    {
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageExpenseCategories, PermissionAction.View))
            return AccessDeniedView();

        var searchModel = await _expenseModelFactory.PrepareExpenseCategorySearchModelAsync(new ExpenseCategorySearchModel());
        return View(ListViewPath, searchModel);
    }

    [HttpPost]
    public virtual async Task<IActionResult> ExpenseCategories(ExpenseCategorySearchModel searchModel)
    {
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageExpenseCategories, PermissionAction.View))
            return await AccessDeniedDataTablesJson();

        var model = await _expenseModelFactory.PrepareExpenseCategoryListModelAsync(searchModel);
        return Json(model);
    }

    public virtual async Task<IActionResult> ExpenseCategoryCreate()
    {
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageExpenseCategories, PermissionAction.Add))
            return AccessDeniedView();

        var model = await _expenseModelFactory.PrepareExpenseCategoryModelAsync(null, null);
        return View(CreateViewPath, model);
    }

    [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
    public virtual async Task<IActionResult> ExpenseCategoryCreate(ExpenseCategoryModel model, bool continueEditing)
    {
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageExpenseCategories, PermissionAction.Add))
            return AccessDeniedView();

        if (ModelState.IsValid)
        {
            var expenseCategory = new ExpenseCategory
            {
                Name = model.Name,
                Description = model.Description,
                CategoryTypeId = model.CategoryTypeId,
                IsSystem = false,
                IsActive = model.IsActive,
                DisplayOrder = model.DisplayOrder,
                Deleted = false,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow
            };

            await _expenseCategoryService.InsertExpenseCategoryAsync(expenseCategory);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.ExpenseCategory.Added"));

            if (!continueEditing)
                return RedirectToAction("ExpenseCategories");

            return RedirectToAction("ExpenseCategoryEdit", new { id = expenseCategory.Id });
        }

        model = await _expenseModelFactory.PrepareExpenseCategoryModelAsync(model, null);
        return View(CreateViewPath, model);
    }

    public virtual async Task<IActionResult> ExpenseCategoryEdit(int id)
    {
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageExpenseCategories, PermissionAction.Edit))
            return AccessDeniedView();

        var expenseCategory = await _expenseCategoryService.GetExpenseCategoryByIdAsync(id);
        if (expenseCategory == null || expenseCategory.Deleted)
            return RedirectToAction("ExpenseCategories");

        var model = await _expenseModelFactory.PrepareExpenseCategoryModelAsync(null, expenseCategory);
        return View(EditViewPath, model);
    }

    [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
    public virtual async Task<IActionResult> ExpenseCategoryEdit(ExpenseCategoryModel model, bool continueEditing)
    {
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageExpenseCategories, PermissionAction.Edit))
            return AccessDeniedView();

        var expenseCategory = await _expenseCategoryService.GetExpenseCategoryByIdAsync(model.Id);
        if (expenseCategory == null || expenseCategory.Deleted)
            return RedirectToAction("ExpenseCategories");

        if (ModelState.IsValid)
        {
            expenseCategory.Name = model.Name;
            expenseCategory.Description = model.Description;
            expenseCategory.CategoryTypeId = model.CategoryTypeId;
            expenseCategory.IsActive = model.IsActive;
            expenseCategory.DisplayOrder = model.DisplayOrder;

            await _expenseCategoryService.UpdateExpenseCategoryAsync(expenseCategory);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.ExpenseCategory.Updated"));

            if (!continueEditing)
                return RedirectToAction("ExpenseCategories");

            return RedirectToAction("ExpenseCategoryEdit", new { id = expenseCategory.Id });
        }

        model = await _expenseModelFactory.PrepareExpenseCategoryModelAsync(model, expenseCategory);
        return View(EditViewPath, model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> ExpenseCategoryDelete(int id)
    {
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageExpenseCategories, PermissionAction.Delete))
            return await AccessDeniedDataTablesJson();

        var expenseCategory = await _expenseCategoryService.GetExpenseCategoryByIdAsync(id);
        if (expenseCategory == null || expenseCategory.Deleted)
            return ErrorJson(await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.ExpenseCategory.NotFound"));

        await _expenseCategoryService.DeleteExpenseCategoryAsync(expenseCategory);

        return new NullJsonResult();
    }

    #endregion
}
