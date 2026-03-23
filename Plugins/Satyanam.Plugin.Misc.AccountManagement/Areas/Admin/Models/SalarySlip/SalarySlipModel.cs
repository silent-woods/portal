using System;
using System.Collections.Generic;

namespace Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Models.SalarySlip;

public class SalarySlipModel
{
    public string CompanyName { get; set; }
    public string CompanyAddress { get; set; }
    public string CompanyCIN { get; set; }
    public byte[] LogoBytes { get; set; }

    public string MonthYear { get; set; }
    public DateTime SlipDate { get; set; }

    public string EmployeeName { get; set; }
    public string Designation { get; set; }
    public string DateOfJoining { get; set; }
    public string PanCardNumber { get; set; }
    public string BankName { get; set; }
    public string BankAccountNumber { get; set; }
    public decimal WorkingDays { get; set; }

    public IList<SalarySlipLine> Earnings { get; set; } = new List<SalarySlipLine>();
    public IList<SalarySlipLine> Deductions { get; set; } = new List<SalarySlipLine>();

    public IList<SalarySlipLine> AdjustmentAdditions { get; set; } = new List<SalarySlipLine>();
    public IList<SalarySlipLine> AdjustmentDeductions { get; set; } = new List<SalarySlipLine>();

    public decimal GrossSalary { get; set; }
    public decimal LeaveEncashmentAmount { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal NetSalary { get; set; }
    public string NetSalaryInWords { get; set; }

    public string HrPersonName { get; set; }
    public byte[] HrSignatureBytes { get; set; }
}

public class SalarySlipLine
{
    public string Name { get; set; }
    public decimal Amount { get; set; }
}
