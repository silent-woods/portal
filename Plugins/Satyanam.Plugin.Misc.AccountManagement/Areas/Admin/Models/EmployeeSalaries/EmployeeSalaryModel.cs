using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;
using System.Collections.Generic;

namespace Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Models.EmployeeSalaries;

public partial record EmployeeSalaryModel : BaseNopEntityModel
{
    #region Properties
    public int EmployeeId { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.EmployeeSalary.Fields.EmployeeName")]
    public string EmployeeName { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.EmployeeSalary.Fields.Month")]
    public int MonthId { get; set; }
    public string MonthName { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.EmployeeSalary.Fields.Year")]
    public int YearId { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.EmployeeSalary.Fields.GrossSalary")]
    public decimal GrossSalary { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.EmployeeSalary.Fields.WorkingDays")]
    public decimal WorkingDaysInMonth { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.EmployeeSalary.Fields.DailySalary")]
    public decimal DailySalary { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.EmployeeSalary.Fields.LeaveDeductionDays")]
    public decimal LeaveDeductionDays { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.EmployeeSalary.Fields.LeaveDeductionAmount")]
    public decimal LeaveDeductionAmount { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.EmployeeSalary.Fields.OtherDeductions")]
    public decimal OtherDeductions { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.EmployeeSalary.Fields.OtherAdditions")]
    public decimal OtherAdditions { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.EmployeeSalary.Fields.LeaveEncashmentDays")]
    public decimal LeaveEncashmentDays { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.EmployeeSalary.Fields.LeaveEncashmentAmount")]
    public decimal LeaveEncashmentAmount { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.EmployeeSalary.Fields.NetSalary")]
    public decimal NetSalary { get; set; }
    public decimal ConfigDeductionsTotal { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.EmployeeSalary.Fields.Remarks")]
    public string Remarks { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.EmployeeSalary.Fields.Status")]
    public int StatusId { get; set; }
    public string StatusName { get; set; }
    public bool IsManuallyModified { get; set; }
    public bool IsPaid => StatusId == 3;
    public bool IsFinalized => StatusId == 2;
    public bool IsDraft => StatusId == 1;
    public IList<EmployeeSalaryAuditLogModel> AuditLogs { get; set; } = new List<EmployeeSalaryAuditLogModel>();
    public IList<SalaryBreakdownLineModel> EarningLines { get; set; } = new List<SalaryBreakdownLineModel>();
    public IList<SalaryBreakdownLineModel> DeductionLines { get; set; } = new List<SalaryBreakdownLineModel>();
    public IList<EmployeeSalaryCustomComponentModel> CustomComponents { get; set; } = new List<EmployeeSalaryCustomComponentModel>();
    #endregion
}

public record SalaryBreakdownLineModel
{
    public string Name { get; init; }
    public decimal Amount { get; init; }
}

public record EmployeeSalaryCustomComponentModel
{
    public int Id { get; init; }
    public int SalaryRecordId { get; init; }
    public int TypeId { get; init; }
    public string TypeName { get; init; }
    public string Name { get; init; }
    public decimal Amount { get; init; }
}
