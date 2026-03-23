using App.Core;
using Satyanam.Plugin.Misc.AccountManagement.Domain.Enums;
using System;

namespace Satyanam.Plugin.Misc.AccountManagement.Domain;

public partial class EmployeeMonthlySalary : BaseEntity
{
    #region Properties
    public int EmployeeId { get; set; }
    public int MonthId { get; set; }
    public int YearId { get; set; }
    public decimal GrossSalary { get; set; }
    public decimal WorkingDaysInMonth { get; set; }
    public decimal DailySalary { get; set; }
    public decimal LeaveDeductionDays { get; set; }
    public decimal LeaveDeductionAmount { get; set; }
    public decimal OtherDeductions { get; set; }
    public decimal OtherAdditions { get; set; }
    public decimal LeaveEncashmentDays { get; set; }
    public decimal LeaveEncashmentAmount { get; set; }
    public decimal NetSalary { get; set; }
    public string Remarks { get; set; }
    public int StatusId { get; set; }
    public bool IsManuallyModified { get; set; }
    public int AccountTransactionId { get; set; }
    public DateTime ProcessedOnUtc { get; set; }
    public DateTime CreatedOnUtc { get; set; }
    public DateTime UpdatedOnUtc { get; set; }
    #endregion

    #region Enum Accessors

    public SalaryStatusEnum Status
    {
        get => (SalaryStatusEnum)StatusId;
        set => StatusId = (int)value;
    }

    #endregion
}
