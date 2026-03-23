using App.Core.Domain.Security;
using App.Services.Localization;
using App.Services.Security;
using App.Web.Areas.Admin.Controllers;
using App.Web.Framework.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Models.EmployeePayrollInfo;
using Satyanam.Plugin.Misc.AccountManagement.Domain;
using Satyanam.Plugin.Misc.AccountManagement.Services;
using System;
using System.Threading.Tasks;

namespace Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Controllers;

[AuthorizeAdmin]
[Area("Admin")]
[AutoValidateAntiforgeryToken]
public partial class EmployeePayrollInfoController : BaseAdminController
{
    #region Fields

    private readonly ISalaryComponentConfigService _salaryComponentConfigService;
    private readonly IPermissionService _permissionService;
    private readonly ILocalizationService _localizationService;

    #endregion

    #region Ctor

    public EmployeePayrollInfoController(
        ISalaryComponentConfigService salaryComponentConfigService,
        IPermissionService permissionService,
        ILocalizationService localizationService)
    {
        _salaryComponentConfigService = salaryComponentConfigService;
        _permissionService = permissionService;
        _localizationService = localizationService;
    }

    #endregion

    #region Methods

    [HttpGet]
    public virtual async Task<IActionResult> GetPayrollInfo(int employeeId)
    {
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageSalaryProcessing, PermissionAction.View))
            return Json(new { success = false, message = await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.Common.AccessDenied") });

        var info = await _salaryComponentConfigService.GetEmployeePayrollInfoAsync(employeeId);

        return Json(new
        {
            success = true,
            ctc = info?.CTC ?? string.Empty,
            panCardNumber = info?.PanCardNumber ?? string.Empty,
            bankName = info?.BankName ?? string.Empty,
            bankAccountNumber = info?.BankAccountNumber ?? string.Empty,
            ifscCode = info?.IFSCCode ?? string.Empty,
            beneficiaryName = info?.BeneficiaryName ?? string.Empty
        });
    }

    [HttpPost]
    public virtual async Task<IActionResult> SavePayrollInfo(EmployeePayrollInfoModel model)
    {
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageSalaryProcessing, PermissionAction.Edit))
            return Json(new { success = false, message = await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.Common.AccessDenied") });

        if (model.EmployeeId <= 0)
            return Json(new { success = false, message = await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.EmployeePayrollInfo.InvalidEmployee") });

        var existing = await _salaryComponentConfigService.GetEmployeePayrollInfoAsync(model.EmployeeId);

        if (existing == null)
        {
            existing = new EmployeePayrollInfo
            {
                EmployeeId = model.EmployeeId,
                CTC = model.CTC,
                PanCardNumber = model.PanCardNumber,
                BankName = model.BankName,
                BankAccountNumber = model.BankAccountNumber,
                IFSCCode = model.IFSCCode,
                BeneficiaryName = model.BeneficiaryName,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow
            };
        }
        else
        {
            existing.CTC = model.CTC;
            existing.PanCardNumber = model.PanCardNumber;
            existing.BankName = model.BankName;
            existing.BankAccountNumber = model.BankAccountNumber;
            existing.IFSCCode = model.IFSCCode;
            existing.BeneficiaryName = model.BeneficiaryName;
            existing.UpdatedOnUtc = DateTime.UtcNow;
        }

        await _salaryComponentConfigService.SaveEmployeePayrollInfoAsync(existing);

        return Json(new { success = true });
    }

    #endregion
}
