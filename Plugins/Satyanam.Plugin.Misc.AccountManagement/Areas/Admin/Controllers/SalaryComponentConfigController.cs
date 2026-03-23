using App.Core.Domain.Security;
using App.Services.Localization;
using App.Services.Messages;
using App.Services.Security;
using App.Web.Areas.Admin.Controllers;
using App.Web.Framework.Mvc;
using App.Web.Framework.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Factories;
using Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Models.SalaryComponentConfigs;
using Satyanam.Plugin.Misc.AccountManagement.Domain;
using Satyanam.Plugin.Misc.AccountManagement.Domain.Enums;
using Satyanam.Plugin.Misc.AccountManagement.Services;
using System;
using System.Threading.Tasks;

namespace Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Controllers;

[AuthorizeAdmin]
[Area("Admin")]
[AutoValidateAntiforgeryToken]
public partial class SalaryComponentConfigController : BaseAdminController
{
    #region Fields

    protected readonly ISalaryComponentConfigService _salaryComponentConfigService;
    protected readonly IExpenseModelFactory _expenseModelFactory;
    protected readonly ILocalizationService _localizationService;
    protected readonly INotificationService _notificationService;
    protected readonly IPermissionService _permissionService;

    private const string ListViewPath = "~/Plugins/Misc.AccountManagement/Areas/Admin/Views/Extension/SalaryComponentConfig/SalaryComponentConfigs.cshtml";
    private const string CreateViewPath = "~/Plugins/Misc.AccountManagement/Areas/Admin/Views/Extension/SalaryComponentConfig/SalaryComponentConfigCreate.cshtml";
    private const string EditViewPath = "~/Plugins/Misc.AccountManagement/Areas/Admin/Views/Extension/SalaryComponentConfig/SalaryComponentConfigEdit.cshtml";

    #endregion

    #region Ctor

    public SalaryComponentConfigController(ISalaryComponentConfigService salaryComponentConfigService,
        IExpenseModelFactory expenseModelFactory,
        ILocalizationService localizationService,
        INotificationService notificationService,
        IPermissionService permissionService)
    {
        _salaryComponentConfigService = salaryComponentConfigService;
        _expenseModelFactory = expenseModelFactory;
        _localizationService = localizationService;
        _notificationService = notificationService;
        _permissionService = permissionService;
    }

    #endregion

    #region Methods

    public virtual async Task<IActionResult> SalaryComponentConfigs()
    {
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageSalaryProcessing, PermissionAction.View))
            return AccessDeniedView();

        var searchModel = await _expenseModelFactory.PrepareSalaryComponentConfigSearchModelAsync(new SalaryComponentConfigSearchModel());
        return View(ListViewPath, searchModel);
    }

    [HttpPost]
    public virtual async Task<IActionResult> SalaryComponentConfigs(SalaryComponentConfigSearchModel searchModel)
    {
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageSalaryProcessing, PermissionAction.View))
            return await AccessDeniedDataTablesJson();

        var model = await _expenseModelFactory.PrepareSalaryComponentConfigListModelAsync(searchModel);
        return Json(model);
    }

    public virtual async Task<IActionResult> SalaryComponentConfigCreate()
    {
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageSalaryProcessing, PermissionAction.Add))
            return AccessDeniedView();

        var model = await _expenseModelFactory.PrepareSalaryComponentConfigModelAsync(null, null);
        return View(CreateViewPath, model);
    }

    [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
    public virtual async Task<IActionResult> SalaryComponentConfigCreate(SalaryComponentConfigModel model, bool continueEditing)
    {
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageSalaryProcessing, PermissionAction.Add))
            return AccessDeniedView();

        if (ModelState.IsValid)
        {
            var component = new SalaryComponentConfig
            {
                Name = model.Name,
                ComponentTypeId = model.ComponentTypeId,
                IsPercentage = model.IsPercentage,
                Value = model.Value,
                IsRemainder = model.IsRemainder,
                IsActive = model.IsActive,
                DisplayOrder = model.DisplayOrder,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow
            };

            await _salaryComponentConfigService.InsertComponentAsync(component);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.SalaryComponentConfig.Added"));

            if (!continueEditing)
                return RedirectToAction("SalaryComponentConfigs");

            return RedirectToAction("SalaryComponentConfigEdit", new { id = component.Id });
        }

        model = await _expenseModelFactory.PrepareSalaryComponentConfigModelAsync(model, null);
        return View(CreateViewPath, model);
    }

    public virtual async Task<IActionResult> SalaryComponentConfigEdit(int id)
    {
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageSalaryProcessing, PermissionAction.Edit))
            return AccessDeniedView();

        var component = await _salaryComponentConfigService.GetComponentByIdAsync(id);
        if (component == null)
            return RedirectToAction("SalaryComponentConfigs");

        var model = await _expenseModelFactory.PrepareSalaryComponentConfigModelAsync(null, component);
        return View(EditViewPath, model);
    }

    [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
    public virtual async Task<IActionResult> SalaryComponentConfigEdit(SalaryComponentConfigModel model, bool continueEditing)
    {
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageSalaryProcessing, PermissionAction.Edit))
            return AccessDeniedView();

        var component = await _salaryComponentConfigService.GetComponentByIdAsync(model.Id);
        if (component == null)
            return RedirectToAction("SalaryComponentConfigs");

        if (ModelState.IsValid)
        {
            component.Name = model.Name;
            component.ComponentTypeId = model.ComponentTypeId;
            component.IsPercentage = model.IsPercentage;
            component.Value = model.Value;
            component.IsRemainder = model.IsRemainder;
            component.IsActive = model.IsActive;
            component.DisplayOrder = model.DisplayOrder;

            await _salaryComponentConfigService.UpdateComponentAsync(component);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.SalaryComponentConfig.Updated"));

            if (!continueEditing)
                return RedirectToAction("SalaryComponentConfigs");

            return RedirectToAction("SalaryComponentConfigEdit", new { id = component.Id });
        }

        model = await _expenseModelFactory.PrepareSalaryComponentConfigModelAsync(model, component);
        return View(EditViewPath, model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> SalaryComponentConfigDelete(int id)
    {
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageSalaryProcessing, PermissionAction.Delete))
            return await AccessDeniedDataTablesJson();

        var component = await _salaryComponentConfigService.GetComponentByIdAsync(id);
        if (component == null)
            return ErrorJson(await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.SalaryComponentConfig.NotFound"));

        await _salaryComponentConfigService.DeleteComponentAsync(component);

        return new NullJsonResult();
    }

    #endregion
}
