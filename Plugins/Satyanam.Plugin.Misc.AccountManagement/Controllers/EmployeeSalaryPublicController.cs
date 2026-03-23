using App.Core;
using App.Core.Domain.Security;
using App.Services.Configuration;
using App.Services.Customers;
using App.Services.Designations;
using App.Services.Employees;
using App.Services.Media;
using App.Services.Security;
using App.Web.Controllers;
using Microsoft.AspNetCore.Mvc;
using QuestPDF.Fluent;
using Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Models.SalarySlip;
using Satyanam.Plugin.Misc.AccountManagement.Domain.Enums;
using Satyanam.Plugin.Misc.AccountManagement.Models.SalarySlip;
using Satyanam.Plugin.Misc.AccountManagement.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Satyanam.Plugin.Misc.AccountManagement.Controllers;

[AutoValidateAntiforgeryToken]
public partial class EmployeeSalaryPublicController : BasePublicController
{
    #region Fields

    private readonly ICustomerService _customerService;
    private readonly IDesignationService _designationService;
    private readonly IEmployeeService _employeeService;
    private readonly IEmployeeSalaryService _employeeSalaryService;
    private readonly IPermissionService _permissionService;
    private readonly IPictureService _pictureService;
    private readonly ISalaryComponentConfigService _salaryComponentConfigService;
    private readonly ISettingService _settingService;
    private readonly IWorkContext _workContext;

    private const string ListView = "~/Plugins/Misc.AccountManagement/Views/SalarySlip/MySalarySlips.cshtml";

    #endregion

    #region Ctor

    public EmployeeSalaryPublicController(
        ICustomerService customerService,
        IDesignationService designationService,
        IEmployeeService employeeService,
        IEmployeeSalaryService employeeSalaryService,
        IPermissionService permissionService,
        IPictureService pictureService,
        ISalaryComponentConfigService salaryComponentConfigService,
        ISettingService settingService,
        IWorkContext workContext)
    {
        _customerService = customerService;
        _designationService = designationService;
        _employeeService = employeeService;
        _employeeSalaryService = employeeSalaryService;
        _permissionService = permissionService;
        _pictureService = pictureService;
        _salaryComponentConfigService = salaryComponentConfigService;
        _settingService = settingService;
        _workContext = workContext;
    }

    #endregion

    #region Methods

    [HttpGet]
    public virtual async Task<IActionResult> MySalarySlips()
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        if (!await _customerService.IsRegisteredAsync(customer))
            return Challenge();

        var employee = await _employeeService.GetEmployeeByCustomerIdAsync(customer.Id);
        if (employee == null)
            return RedirectToRoute("HomePage");

        if (!await _permissionService.AuthorizeAsync(App.Services.Security.StandardPermissionProvider.PublicStoreSalarySlips, PermissionAction.View))
            return AccessDeniedView();

        var records = await _employeeSalaryService.GetAllSalaryRecordsAsync(
            employeeId: employee.Id,
            statusId: (int)SalaryStatusEnum.Paid,
            pageSize: int.MaxValue);

        var model = records
            .OrderByDescending(r => r.YearId).ThenByDescending(r => r.MonthId)
            .Select(r => new MySalarySlipItemModel
            {
                Id = r.Id,
                MonthYear = $"{new DateTime(2000, r.MonthId, 1):MMMM} {r.YearId}",
                GrossSalary = r.GrossSalary,
                NetSalary = r.NetSalary,
                PaidOn = r.UpdatedOnUtc.ToString("dd MMM yyyy")
            })
            .ToList();

        return View(ListView, model);
    }

    [HttpGet]
    public virtual async Task<IActionResult> DownloadMySalarySlip(int id)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        if (!await _customerService.IsRegisteredAsync(customer))
            return Challenge();

        var employee = await _employeeService.GetEmployeeByCustomerIdAsync(customer.Id);
        if (employee == null)
            return RedirectToRoute("HomePage");

        var salaryRecord = await _employeeSalaryService.GetSalaryRecordByIdAsync(id);

        // Security: only allow access to own paid salary slips
        if (salaryRecord == null
            || salaryRecord.EmployeeId != employee.Id
            || salaryRecord.StatusId != (int)SalaryStatusEnum.Paid)
            return AccessDeniedView();

        var payrollInfo = await _salaryComponentConfigService.GetEmployeePayrollInfoAsync(employee.Id);
        var components = await _salaryComponentConfigService.GetAllActiveComponentsAsync();

        var designation = employee.DesignationId > 0
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
            EmployeeName = $"{employee.FirstName} {employee.LastName}",
            Designation = designation?.Name,
            DateOfJoining = employee.DateofJoining.ToString("d MMMM yyyy"),
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

        if (salaryRecord.LeaveEncashmentAmount > 0)
            slipModel.Earnings.Add(new SalarySlipLine { Name = "Incentive", Amount = salaryRecord.LeaveEncashmentAmount });
        if (salaryRecord.LeaveDeductionAmount > 0)
            slipModel.Deductions.Add(new SalarySlipLine { Name = "Loss of Pay", Amount = salaryRecord.LeaveDeductionAmount });
        if (salaryRecord.OtherDeductions > 0)
            slipModel.Deductions.Add(new SalarySlipLine { Name = "Other Deductions", Amount = salaryRecord.OtherDeductions });

        slipModel.TotalDeductions = slipModel.Deductions.Sum(d => d.Amount);

        var document = new SalarySlipDocument(slipModel);
        var pdfBytes = document.GeneratePdf();

        var fileName = $"SalarySlip-{monthName}{salaryRecord.YearId}.pdf";
        return File(pdfBytes, "application/pdf", fileName);
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
}
