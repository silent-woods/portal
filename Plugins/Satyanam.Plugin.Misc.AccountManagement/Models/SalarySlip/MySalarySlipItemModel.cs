using System.Collections.Generic;

namespace Satyanam.Plugin.Misc.AccountManagement.Models.SalarySlip;

public class MySalarySlipItemModel
{
    public int Id { get; set; }
    public string MonthYear { get; set; }
    public decimal GrossSalary { get; set; }
    public decimal NetSalary { get; set; }
    public string PaidOn { get; set; }
}
