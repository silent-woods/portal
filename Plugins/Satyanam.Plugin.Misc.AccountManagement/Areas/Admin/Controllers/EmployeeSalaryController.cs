using App.Core;
using App.Core.Domain.Security;
using App.Services.Customers;
using App.Services.Messages;
using App.Services.Configuration;
using App.Services.Designations;
using App.Services.Employees;
using App.Services.Localization;
using App.Services.Media;
using App.Services.Security;
using App.Web.Areas.Admin.Controllers;
using App.Web.Framework.Mvc;
using App.Web.Framework.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using QuestPDF.Fluent;
using Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Factories;
using Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Models.EmployeeSalaries;
using Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Models.SalarySlip;
using Satyanam.Plugin.Misc.AccountManagement.Domain.Enums;
using Satyanam.Plugin.Misc.AccountManagement.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Controllers;

[AuthorizeAdmin]
[Area("Admin")]
[AutoValidateAntiforgeryToken]
public partial class EmployeeSalaryController : BaseAdminController
{
    #region Fields

    protected readonly ICustomerService _customerService;
    protected readonly IEmployeeService _employeeService;
    protected readonly IEmployeeSalaryService _employeeSalaryService;
    protected readonly IExpenseModelFactory _expenseModelFactory;
    protected readonly ILocalizationService _localizationService;
    protected readonly INotificationService _notificationService;
    protected readonly IPermissionService _permissionService;
    protected readonly ISalaryComponentConfigService _salaryComponentConfigService;
    protected readonly IDesignationService _designationService;
    protected readonly IWorkContext _workContext;
    protected readonly IPictureService _pictureService;
    protected readonly ISettingService _settingService;
    protected readonly IWorkflowMessageService _workflowMessageService;
    protected readonly IEmployeeSalaryCustomComponentService _salaryCustomComponentService;

    private const string ListViewPath = "~/Plugins/Misc.AccountManagement/Areas/Admin/Views/Extension/EmployeeSalary/EmployeeSalaries.cshtml";
    private const string EditViewPath = "~/Plugins/Misc.AccountManagement/Areas/Admin/Views/Extension/EmployeeSalary/EmployeeSalaryEdit.cshtml";

    #endregion

    #region Ctor

    public EmployeeSalaryController(ICustomerService customerService,
        IEmployeeService employeeService,
        IEmployeeSalaryService employeeSalaryService,
        IExpenseModelFactory expenseModelFactory,
        ILocalizationService localizationService,
        INotificationService notificationService,
        IPermissionService permissionService,
        ISalaryComponentConfigService salaryComponentConfigService,
        IDesignationService designationService,
        IWorkContext workContext,
        IPictureService pictureService,
        ISettingService settingService,
        IWorkflowMessageService workflowMessageService,
        IEmployeeSalaryCustomComponentService salaryCustomComponentService)
    {
        _customerService = customerService;
        _employeeService = employeeService;
        _employeeSalaryService = employeeSalaryService;
        _expenseModelFactory = expenseModelFactory;
        _localizationService = localizationService;
        _notificationService = notificationService;
        _permissionService = permissionService;
        _salaryComponentConfigService = salaryComponentConfigService;
        _designationService = designationService;
        _workContext = workContext;
        _pictureService = pictureService;
        _settingService = settingService;
        _workflowMessageService = workflowMessageService;
        _salaryCustomComponentService = salaryCustomComponentService;
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

    public virtual async Task<IActionResult> EmployeeSalaries()
    {
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageSalaryProcessing, PermissionAction.View))
            return AccessDeniedView();

        var searchModel = await _expenseModelFactory.PrepareEmployeeSalarySearchModelAsync(new EmployeeSalarySearchModel());
        return View(ListViewPath, searchModel);
    }

    [HttpPost]
    public virtual async Task<IActionResult> EmployeeSalaries(EmployeeSalarySearchModel searchModel)
    {
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageSalaryProcessing, PermissionAction.View))
            return await AccessDeniedDataTablesJson();

        var model = await _expenseModelFactory.PrepareEmployeeSalaryListModelAsync(searchModel);
        return Json(model);
    }

    public virtual async Task<IActionResult> EmployeeSalaryEdit(int id)
    {
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageSalaryProcessing, PermissionAction.Edit))
            return AccessDeniedView();

        var salaryRecord = await _employeeSalaryService.GetSalaryRecordByIdAsync(id);
        if (salaryRecord == null)
            return RedirectToAction("EmployeeSalaries");

        var model = await _expenseModelFactory.PrepareEmployeeSalaryModelAsync(null, salaryRecord);
        return View(EditViewPath, model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> EmployeeSalaryEdit(EmployeeSalaryModel model)
    {
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageSalaryProcessing, PermissionAction.Edit))
            return AccessDeniedView();

        var salaryRecord = await _employeeSalaryService.GetSalaryRecordByIdAsync(model.Id);
        if (salaryRecord == null)
            return RedirectToAction("EmployeeSalaries");

        if (salaryRecord.StatusId == (int)SalaryStatusEnum.Paid)
        {
            _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.EmployeeSalary.CannotEditPaid"));
            var readOnlyModel = await _expenseModelFactory.PrepareEmployeeSalaryModelAsync(null, salaryRecord);
            return View(EditViewPath, readOnlyModel);
        }

        if (ModelState.IsValid)
        {
            var currentEmployeeId = await GetCurrentEmployeeIdAsync();

            var changes = new System.Collections.Generic.List<(string field, string oldVal, string newVal)>();

            if (salaryRecord.GrossSalary != model.GrossSalary)
                changes.Add((await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.EmployeeSalary.Audit.GrossSalary"), salaryRecord.GrossSalary.ToString("F2"), model.GrossSalary.ToString("F2")));
            if (salaryRecord.WorkingDaysInMonth != model.WorkingDaysInMonth)
                changes.Add((await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.EmployeeSalary.Audit.WorkingDays"), salaryRecord.WorkingDaysInMonth.ToString("F2"), model.WorkingDaysInMonth.ToString("F2")));
            if (salaryRecord.LeaveDeductionDays != model.LeaveDeductionDays)
                changes.Add((await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.EmployeeSalary.Audit.LeaveDeductionDays"), salaryRecord.LeaveDeductionDays.ToString("F2"), model.LeaveDeductionDays.ToString("F2")));
            if (salaryRecord.OtherDeductions != model.OtherDeductions)
                changes.Add((await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.EmployeeSalary.Audit.OtherDeductions"), salaryRecord.OtherDeductions.ToString("F2"), model.OtherDeductions.ToString("F2")));
            if (salaryRecord.OtherAdditions != model.OtherAdditions)
                changes.Add((await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.EmployeeSalary.Audit.OtherAdditions"), salaryRecord.OtherAdditions.ToString("F2"), model.OtherAdditions.ToString("F2")));
            if (salaryRecord.LeaveEncashmentDays != model.LeaveEncashmentDays)
                changes.Add((await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.EmployeeSalary.Audit.LeaveEncashmentDays"), salaryRecord.LeaveEncashmentDays.ToString("F2"), model.LeaveEncashmentDays.ToString("F2")));
            if (salaryRecord.LeaveEncashmentAmount != model.LeaveEncashmentAmount)
                changes.Add((await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.EmployeeSalary.Audit.LeaveEncashmentAmount"), salaryRecord.LeaveEncashmentAmount.ToString("F2"), model.LeaveEncashmentAmount.ToString("F2")));
            if ((salaryRecord.Remarks ?? string.Empty) != (model.Remarks ?? string.Empty))
                changes.Add((await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.EmployeeSalary.Audit.Remarks"), salaryRecord.Remarks ?? string.Empty, model.Remarks ?? string.Empty));

            salaryRecord.GrossSalary = model.GrossSalary;
            salaryRecord.WorkingDaysInMonth = model.WorkingDaysInMonth > 0 ? model.WorkingDaysInMonth : salaryRecord.WorkingDaysInMonth;
            salaryRecord.LeaveDeductionDays = model.LeaveDeductionDays;
            salaryRecord.OtherDeductions = model.OtherDeductions;
            salaryRecord.OtherAdditions = model.OtherAdditions;
            salaryRecord.Remarks = model.Remarks;
            salaryRecord.LeaveEncashmentDays = model.LeaveEncashmentDays;

            salaryRecord.DailySalary = salaryRecord.WorkingDaysInMonth > 0
                ? System.Math.Round(salaryRecord.GrossSalary / salaryRecord.WorkingDaysInMonth, 4)
                : 0;
            salaryRecord.LeaveDeductionAmount = System.Math.Round(salaryRecord.LeaveDeductionDays * salaryRecord.DailySalary, 4);

            if (salaryRecord.LeaveEncashmentDays > 0)
                salaryRecord.LeaveEncashmentAmount = System.Math.Round(salaryRecord.LeaveEncashmentDays * salaryRecord.DailySalary, 4);
            else
                salaryRecord.LeaveEncashmentAmount = model.LeaveEncashmentAmount;

            var editComponents = await _salaryComponentConfigService.GetAllActiveComponentsAsync();
            var configDeductionsTotal = editComponents?
                .Where(c => !c.IsRemainder && c.ComponentTypeId == (int)Domain.Enums.SalaryComponentTypeEnum.Deduction)
                .Sum(c => c.IsPercentage ? System.Math.Round(salaryRecord.GrossSalary * c.Value / 100m, 2) : c.Value) ?? 0m;

            var customComponents = await _salaryCustomComponentService.GetComponentsBySalaryRecordIdAsync(salaryRecord.Id);
            var customAdditions = customComponents?.Where(c => c.TypeId == (int)Domain.SalaryCustomComponentType.Addition).Sum(c => c.Amount) ?? 0m;
            var customDeductions = customComponents?.Where(c => c.TypeId == (int)Domain.SalaryCustomComponentType.Deduction).Sum(c => c.Amount) ?? 0m;

            salaryRecord.NetSalary = Math.Round(salaryRecord.GrossSalary - configDeductionsTotal - salaryRecord.LeaveDeductionAmount + salaryRecord.LeaveEncashmentAmount + customAdditions - customDeductions, 0, MidpointRounding.AwayFromZero);
            salaryRecord.IsManuallyModified = true;

            await _employeeSalaryService.UpdateSalaryRecordAsync(salaryRecord);

            foreach (var (field, oldVal, newVal) in changes)
            {
                await _employeeSalaryService.InsertSalaryAuditLogAsync(new Domain.EmployeeSalaryAuditLog
                {
                    EmployeeSalaryRecordId = salaryRecord.Id,
                    FieldName = field,
                    OldValue = oldVal,
                    NewValue = newVal,
                    ChangedByEmployeeId = currentEmployeeId,
                    ChangedOnUtc = System.DateTime.UtcNow
                });
            }

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.EmployeeSalary.Updated"));

            return RedirectToAction("EmployeeSalaryEdit", new { id = salaryRecord.Id });
        }

        model = await _expenseModelFactory.PrepareEmployeeSalaryModelAsync(model, salaryRecord);
        return View(EditViewPath, model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> FinalizeSalary(int id)
    {
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageSalaryProcessing, PermissionAction.Edit))
            return Json(new { success = false, message = await _localizationService.GetResourceAsync(
                "Satyanam.Plugin.Misc.AccountManagement.Admin.Common.AccessDenied")
            });

        var salaryRecord = await _employeeSalaryService.GetSalaryRecordByIdAsync(id);
        if (salaryRecord == null)
            return Json(new { success = false, message = await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.EmployeeSalary.NotFound") });

        if (salaryRecord.StatusId != (int)SalaryStatusEnum.Draft)
            return Json(new { success = false, message = await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.EmployeeSalary.AlreadyFinalized") });

        var currentEmployeeId = await GetCurrentEmployeeIdAsync();
        await _employeeSalaryService.FinalizeSalaryRecordAsync(salaryRecord, currentEmployeeId);

        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.EmployeeSalary.Finalized"));

        return Json(new { success = true });
    }

    [HttpPost]
    public virtual async Task<IActionResult> MarkAsPaid(int id)
    {
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageSalaryProcessing, PermissionAction.Edit))
            return Json(new { success = false,
                message = await _localizationService.GetResourceAsync(
                "Satyanam.Plugin.Misc.AccountManagement.Admin.Common.AccessDenied")
            });

        var salaryRecord = await _employeeSalaryService.GetSalaryRecordByIdAsync(id);
        if (salaryRecord == null)
            return Json(new { success = false,
                message = await _localizationService.GetResourceAsync(
                "Satyanam.Plugin.Misc.AccountManagement.Admin.EmployeeSalary.NotFound")
            });

        if (salaryRecord.StatusId != (int)SalaryStatusEnum.Finalized)
            return Json(new { success = false, message = await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.EmployeeSalary.MustBeFinalizedFirst") });

        var currentEmployeeId = await GetCurrentEmployeeIdAsync();
        await _employeeSalaryService.MarkSalaryAsPaidAsync(salaryRecord, currentEmployeeId);

        var employee = await _employeeService.GetEmployeeByIdAsync(salaryRecord.EmployeeId);
        if (employee != null && !string.IsNullOrEmpty(employee.OfficialEmail) && !employee.IsVendor)
        {
            var monthName = new DateTime(2000, salaryRecord.MonthId, 1).ToString("MMMM");
            var monthYear = $"{monthName} {salaryRecord.YearId}";
            var employeeFullName = $"{employee.FirstName} {employee.LastName}".Trim();
            await _workflowMessageService.SendSalaryPaidEmailAsync(0, employeeFullName,
                employee.OfficialEmail, monthYear, salaryRecord.GrossSalary, salaryRecord.NetSalary, salaryRecord.Id);
        }

        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.EmployeeSalary.MarkedAsPaid"));

        return Json(new { success = true });
    }

    [HttpPost]
    public virtual async Task<IActionResult> EmployeeSalaryDelete(int id)
    {
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageSalaryProcessing, PermissionAction.Delete))
            return await AccessDeniedDataTablesJson();

        var salaryRecord = await _employeeSalaryService.GetSalaryRecordByIdAsync(id);
        if (salaryRecord == null)
            return ErrorJson(await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.EmployeeSalary.NotFound"));

        await _employeeSalaryService.DeleteSalaryRecordAsync(salaryRecord);

        return new NullJsonResult();
    }

    [HttpPost]
    public virtual async Task<IActionResult> ProcessSalariesNow(int month, int year)
    {
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageSalaryProcessing, PermissionAction.Add))
            return Json(new { success = false, message = await _localizationService.GetResourceAsync(
                "Satyanam.Plugin.Misc.AccountManagement.Admin.Common.AccessDenied")
            });

        if (month < 1 || month > 12 || year < 2020)
            return Json(new { success = false, message = await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.EmployeeSalary.InvalidMonthYear") });

        var currentEmployeeId = await GetCurrentEmployeeIdAsync();
        await _employeeSalaryService.ProcessMonthlySalariesAsync(month, year, currentEmployeeId);
        return Json(new { success = true });
    }

    [HttpPost]
    public virtual async Task<IActionResult> BulkUpdateStatus(int[] ids)
    {
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageSalaryProcessing, PermissionAction.Edit))
            return Json(new { success = false, message = await _localizationService.GetResourceAsync(
                "Satyanam.Plugin.Misc.AccountManagement.Admin.Common.AccessDenied")
            });

        if (ids == null || ids.Length == 0)
            return Json(new { success = false, message = await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.EmployeeSalary.NoRecordsSelected") });

        var currentEmployeeId = await GetCurrentEmployeeIdAsync();
        var updated = 0;
        var skipped = 0;

        foreach (var id in ids)
        {
            var record = await _employeeSalaryService.GetSalaryRecordByIdAsync(id);
            if (record == null)
                continue;

            if (record.StatusId == (int)SalaryStatusEnum.Draft)
            {
                await _employeeSalaryService.FinalizeSalaryRecordAsync(record, currentEmployeeId);
                updated++;
            }
            else if (record.StatusId == (int)SalaryStatusEnum.Finalized)
            {
                await _employeeSalaryService.MarkSalaryAsPaidAsync(record, currentEmployeeId);
                updated++;

                var emp = await _employeeService.GetEmployeeByIdAsync(record.EmployeeId);
                if (emp != null && !string.IsNullOrEmpty(emp.OfficialEmail))
                {
                    var monthName = new DateTime(2000, record.MonthId, 1).ToString("MMMM");
                    await _workflowMessageService.SendSalaryPaidEmailAsync(0,
                        $"{emp.FirstName} {emp.LastName}".Trim(), emp.OfficialEmail,
                        $"{monthName} {record.YearId}", record.GrossSalary, record.NetSalary, record.Id);
                }
            }
            else
            {
                skipped++;
            }
        }

        return Json(new { success = true, updated, skipped });
    }

    [HttpGet]
    public virtual async Task<IActionResult> DownloadSalarySlip(int id)
    {
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageSalaryProcessing, PermissionAction.View))
            return AccessDeniedView();

        var salaryRecord = await _employeeSalaryService.GetSalaryRecordByIdAsync(id);
        if (salaryRecord == null)
            return RedirectToAction("EmployeeSalaries");

        var employee = await _employeeService.GetEmployeeByIdAsync(salaryRecord.EmployeeId);
        var payrollInfo = await _salaryComponentConfigService.GetEmployeePayrollInfoAsync(salaryRecord.EmployeeId);
        var components = await _salaryComponentConfigService.GetAllActiveComponentsAsync();

        var designation = employee?.DesignationId > 0
            ? await _designationService.GetDesignationByIdAsync(employee.DesignationId)
            : null;

        var monthName = new DateTime(2000, salaryRecord.MonthId, 1).ToString("MMMM");

        var accountSettings = await _settingService.LoadSettingAsync<AccountManagementSettings>();
        var expenseSettings = await _settingService.LoadSettingAsync<ExpenseManagementSettings>();

        byte[] logoBytes = null;
        if (accountSettings.InvoiceLogoId > 0)
        {
            var picture = await _pictureService.GetPictureByIdAsync(accountSettings.InvoiceLogoId);
            if (picture != null)
                logoBytes = await _pictureService.LoadPictureBinaryAsync(picture);
        }

        byte[] hrSignatureBytes = null;
        if (expenseSettings.HrSignaturePictureId > 0)
        {
            var sigPicture = await _pictureService.GetPictureByIdAsync(expenseSettings.HrSignaturePictureId);
            if (sigPicture != null)
                hrSignatureBytes = await _pictureService.LoadPictureBinaryAsync(sigPicture);
        }

        var slipModel = new SalarySlipModel
        {
            CompanyName = expenseSettings.CompanyName,
            CompanyAddress = expenseSettings.CompanyAddress,
            CompanyCIN = expenseSettings.CompanyCIN,
            LogoBytes = logoBytes,
            MonthYear = $"{monthName} {salaryRecord.YearId}",
            SlipDate = DateTime.Now,
            EmployeeName = employee != null ? $"{employee.FirstName} {employee.LastName}" : $"Employee #{salaryRecord.EmployeeId}",
            Designation = designation?.Name,
            DateOfJoining = employee?.DateofJoining.ToString("d MMMM yyyy"),
            PanCardNumber = payrollInfo?.PanCardNumber,
            BankName = payrollInfo?.BankName,
            BankAccountNumber = payrollInfo?.BankAccountNumber,
            WorkingDays = salaryRecord.WorkingDaysInMonth,
            GrossSalary = salaryRecord.GrossSalary,
            NetSalary = salaryRecord.NetSalary,
            NetSalaryInWords = NumberToWords((long)salaryRecord.NetSalary),
            HrPersonName = expenseSettings.HrPersonName,
            HrSignatureBytes = hrSignatureBytes,
            LeaveEncashmentAmount = salaryRecord.LeaveEncashmentAmount
        };

        var totalConfigDeductions = components
            .Where(c => !c.IsRemainder && c.ComponentTypeId == (int)SalaryComponentTypeEnum.Deduction)
            .Sum(c => c.IsPercentage ? Math.Round(salaryRecord.GrossSalary * c.Value / 100m, 0) : c.Value);
        var totalNonRemainderEarnings = components
            .Where(c => !c.IsRemainder && c.ComponentTypeId == (int)SalaryComponentTypeEnum.Earning)
            .Sum(c => c.IsPercentage ? Math.Round(salaryRecord.GrossSalary * c.Value / 100m, 0) : c.Value);

        foreach (var c in components)
        {
            decimal amount;
            if (c.IsRemainder)
                amount = Math.Round(salaryRecord.GrossSalary - totalNonRemainderEarnings, 0);
            else
                amount = c.IsPercentage
                    ? Math.Round(salaryRecord.GrossSalary * c.Value / 100m, 0)
                    : c.Value;

            var line = new SalarySlipLine { Name = c.Name, Amount = amount };
            if (c.ComponentTypeId == (int)SalaryComponentTypeEnum.Earning)
                slipModel.Earnings.Add(line);
            else
                slipModel.Deductions.Add(line);
        }

        if (salaryRecord.LeaveDeductionAmount > 0)
            slipModel.Deductions.Add(new SalarySlipLine { Name = $"Loss of Pay ({salaryRecord.LeaveDeductionDays:0.##} days)", Amount = salaryRecord.LeaveDeductionAmount });

        if (salaryRecord.LeaveEncashmentAmount > 0)
            slipModel.AdjustmentAdditions.Add(new SalarySlipLine { Name = "Leave Encashment (Incentive)", Amount = salaryRecord.LeaveEncashmentAmount });

        var slipCustomComponents = await _salaryCustomComponentService.GetComponentsBySalaryRecordIdAsync(salaryRecord.Id);
        foreach (var cc in slipCustomComponents)
        {
            var line = new SalarySlipLine { Name = cc.Name, Amount = cc.Amount };
            if (cc.ComponentType == Domain.SalaryCustomComponentType.Addition)
                slipModel.AdjustmentAdditions.Add(line);
            else
                slipModel.AdjustmentDeductions.Add(line);
        }

        slipModel.TotalDeductions = slipModel.Deductions.Sum(d => d.Amount);

        var document = new SalarySlipDocument(slipModel);
        var pdfBytes = document.GeneratePdf();

        var fileName = $"SalarySlip-{slipModel.EmployeeName.Replace(" ", "_")}-{monthName}{salaryRecord.YearId}.pdf";
        return File(pdfBytes, "application/pdf", fileName);
    }

    [HttpGet]
    public virtual async Task<IActionResult> PreviewHdfcCsv(int monthId, int yearId)
    {
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageSalaryProcessing, PermissionAction.View))
            return AccessDeniedView();

        // Default to previous month if not specified
        var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow,
            TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));
        if (monthId == 0 || yearId == 0)
        {
            var prev = now.AddMonths(-1);
            monthId = prev.Month;
            yearId = prev.Year;
        }

        var allRecords = await _employeeSalaryService.GetAllSalaryRecordsAsync(
            monthId: monthId, yearId: yearId, pageSize: int.MaxValue);

        var records = allRecords.Where(r => r.StatusId >= (int)SalaryStatusEnum.Finalized).ToList();

        var rows = new List<HdfcCsvRowModel>();
        var seq = 1;

        foreach (var record in records)
        {
            var employee = await _employeeService.GetEmployeeByIdAsync(record.EmployeeId);
            if (employee == null) continue;

            var payrollInfo = await _salaryComponentConfigService.GetEmployeePayrollInfoAsync(record.EmployeeId);

            var fallbackName = $"{employee.FirstName ?? string.Empty} {employee.LastName ?? string.Empty}".Trim();
            var beneficiaryName = (payrollInfo?.BeneficiaryName?.Trim() is { Length: > 0 } b ? b : fallbackName).ToUpper();

            rows.Add(new HdfcCsvRowModel
            {
                TransactionType = "N",
                SeqNo = seq++,
                AccountNumber = payrollInfo?.BankAccountNumber?.Trim() ?? string.Empty,
                Amount = Math.Round(record.NetSalary, 0, MidpointRounding.AwayFromZero).ToString("F0"),
                BeneficiaryName = beneficiaryName,
                Narration = "VENDOR",
                IFSCCode = payrollInfo?.IFSCCode?.Trim() ?? string.Empty,
                Email = employee.PersonalEmail?.Trim() is { Length: > 0 } pe
                    ? pe
                    : employee.OfficialEmail?.Trim() ?? string.Empty
            });
        }

        var safeMonthId = monthId < 1 || monthId > 12 ? 1 : monthId;
        var monthName = new DateTime(2000, safeMonthId, 1).ToString("MMMM");
        ViewBag.FileName = $"HDFC_Salary_{monthName}_{yearId}.csv";
        ViewBag.MonthId = monthId;
        ViewBag.YearId = yearId;
        ViewBag.MonthName = monthName;
        ViewBag.Year = yearId;
        return View("~/Plugins/Misc.AccountManagement/Areas/Admin/Views/Extension/EmployeeSalary/PreviewHdfcCsv.cshtml", rows);
    }

    #endregion

    #region Helpers

    private static string NumberToWords(long number)
    {
        if (number == 0) return "zero";
        if (number < 0) return "minus " + NumberToWords(-number);

        string words = "";
        if (number / 10000000 > 0) { words += NumberToWords(number / 10000000) + " crore "; number %= 10000000; }
        if (number / 100000 > 0) { words += NumberToWords(number / 100000) + " lakh "; number %= 100000; }
        if (number / 1000 > 0) { words += NumberToWords(number / 1000) + " thousand "; number %= 1000; }
        if (number / 100 > 0) { words += NumberToWords(number / 100) + " hundred "; number %= 100; }

        if (number > 0)
        {
            string[] ones = { "", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine",
                              "ten", "eleven", "twelve", "thirteen", "fourteen", "fifteen", "sixteen",
                              "seventeen", "eighteen", "nineteen" };
            string[] tens = { "", "", "twenty", "thirty", "forty", "fifty", "sixty", "seventy", "eighty", "ninety" };
            if (number < 20) words += ones[number];
            else words += tens[number / 10] + (number % 10 > 0 ? " " + ones[number % 10] : "");
        }

        return words.Trim();
    }

    #endregion

    #region Custom Component AJAX

    [HttpPost]
    public virtual async Task<IActionResult> AddCustomComponent(int salaryRecordId, int typeId, string name, decimal amount)
    {
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageSalaryProcessing, PermissionAction.Edit))
            return Json(new { success = false, message = await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.EmployeeSalary.AccessDenied") });

        var salaryRecord = await _employeeSalaryService.GetSalaryRecordByIdAsync(salaryRecordId);
        if (salaryRecord == null)
            return Json(new { success = false, message = await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.EmployeeSalary.RecordNotFound") });

        if (salaryRecord.StatusId == (int)SalaryStatusEnum.Paid)
            return Json(new { success = false, message = await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.EmployeeSalary.CannotModifyPaid") });

        if (string.IsNullOrWhiteSpace(name) || amount <= 0)
            return Json(new { success = false, message = await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.EmployeeSalary.ComponentRequired") });

        var component = new Domain.EmployeeSalaryCustomComponent
        {
            SalaryRecordId = salaryRecordId,
            TypeId = typeId,
            Name = name.Trim(),
            Amount = System.Math.Round(amount, 2)
        };
        await _salaryCustomComponentService.InsertComponentAsync(component);

        var currentEmployeeId = await GetCurrentEmployeeIdAsync();
        var typeLabelAdd = typeId == (int)Domain.SalaryCustomComponentType.Addition
            ? await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.EmployeeSalary.Audit.OtherAddition")
            : await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.EmployeeSalary.Audit.OtherDeduction");
        await _employeeSalaryService.InsertSalaryAuditLogAsync(new Domain.EmployeeSalaryAuditLog
        {
            EmployeeSalaryRecordId = salaryRecordId,
            FieldName = typeLabelAdd,
            OldValue = "—",
            NewValue = $"{component.Name}: {component.Amount:N0}",
            ChangedByEmployeeId = currentEmployeeId,
            ChangedOnUtc = System.DateTime.UtcNow
        });

        await RecalculateNetSalaryAsync(salaryRecord);

        var components = await _salaryCustomComponentService.GetComponentsBySalaryRecordIdAsync(salaryRecordId);
        var result = components?.Select(c => new
        {
            id = c.Id,
            typeId = c.TypeId,
            typeName = c.ComponentType == Domain.SalaryCustomComponentType.Addition ? "Addition" : "Deduction",
            name = c.Name,
            amount = c.Amount
        });

        return Json(new { success = true, components = result, netSalary = salaryRecord.NetSalary });
    }

    [HttpPost]
    public virtual async Task<IActionResult> DeleteCustomComponent(int id)
    {
        if (!await _permissionService.AuthorizeAsync(AccountManagementPermissionProvider.ManageSalaryProcessing, PermissionAction.Edit))
            return Json(new { success = false, message = await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.EmployeeSalary.AccessDenied") });

        var component = await _salaryCustomComponentService.GetComponentByIdAsync(id);
        if (component == null)
            return Json(new { success = false, message = await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.EmployeeSalary.ComponentNotFound") });

        var salaryRecord = await _employeeSalaryService.GetSalaryRecordByIdAsync(component.SalaryRecordId);
        if (salaryRecord == null || salaryRecord.StatusId == (int)SalaryStatusEnum.Paid)
            return Json(new { success = false, message = await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.EmployeeSalary.CannotModifyPaid") });

        var currentEmpId = await GetCurrentEmployeeIdAsync();
        var typeLabelDel = component.ComponentType == Domain.SalaryCustomComponentType.Addition
            ? await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.EmployeeSalary.Audit.OtherAddition")
            : await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.EmployeeSalary.Audit.OtherDeduction");
        await _employeeSalaryService.InsertSalaryAuditLogAsync(new Domain.EmployeeSalaryAuditLog
        {
            EmployeeSalaryRecordId = salaryRecord.Id,
            FieldName = typeLabelDel,
            OldValue = $"{component.Name}: {component.Amount:N0}",
            NewValue = await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.EmployeeSalary.Audit.Removed"),
            ChangedByEmployeeId = currentEmpId,
            ChangedOnUtc = System.DateTime.UtcNow
        });

        await _salaryCustomComponentService.DeleteComponentAsync(component);

        await RecalculateNetSalaryAsync(salaryRecord);

        var components = await _salaryCustomComponentService.GetComponentsBySalaryRecordIdAsync(salaryRecord.Id);
        var result = components?.Select(c => new
        {
            id = c.Id,
            typeId = c.TypeId,
            typeName = c.ComponentType == Domain.SalaryCustomComponentType.Addition ? "Addition" : "Deduction",
            name = c.Name,
            amount = c.Amount
        });

        return Json(new { success = true, components = result, netSalary = salaryRecord.NetSalary });
    }

    private async Task RecalculateNetSalaryAsync(Domain.EmployeeMonthlySalary salaryRecord)
    {
        var editComponents = await _salaryComponentConfigService.GetAllActiveComponentsAsync();
        var configDeductionsTotal = editComponents?
            .Where(c => !c.IsRemainder && c.ComponentTypeId == (int)Domain.Enums.SalaryComponentTypeEnum.Deduction)
            .Sum(c => c.IsPercentage ? System.Math.Round(salaryRecord.GrossSalary * c.Value / 100m, 2) : c.Value) ?? 0m;

        var customComponents = await _salaryCustomComponentService.GetComponentsBySalaryRecordIdAsync(salaryRecord.Id);
        var customAdditions = customComponents?.Where(c => c.TypeId == (int)Domain.SalaryCustomComponentType.Addition).Sum(c => c.Amount) ?? 0m;
        var customDeductions = customComponents?.Where(c => c.TypeId == (int)Domain.SalaryCustomComponentType.Deduction).Sum(c => c.Amount) ?? 0m;

        salaryRecord.NetSalary = Math.Round(salaryRecord.GrossSalary - configDeductionsTotal - salaryRecord.LeaveDeductionAmount + salaryRecord.LeaveEncashmentAmount + customAdditions - customDeductions, 0, MidpointRounding.AwayFromZero);
        await _employeeSalaryService.UpdateSalaryRecordAsync(salaryRecord);
    }

    #endregion
}
