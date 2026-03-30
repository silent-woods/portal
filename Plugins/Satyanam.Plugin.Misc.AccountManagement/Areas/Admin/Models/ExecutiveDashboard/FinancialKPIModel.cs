using System.Collections.Generic;

namespace Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Models.ExecutiveDashboard;

public partial record FinancialKPIModel
{
    public decimal TotalRevenue { get; set; }
    public decimal BillableHours { get; set; }
    public decimal AvgCostRatePerHour { get; set; }
    public decimal DirectCost { get; set; }
    public decimal GrossProfitPercent { get; set; }
    public decimal TotalExpense { get; set; }
    public decimal NetProfitPercent { get; set; }
    public decimal NetCashFlow { get; set; }

    public decimal PrevRevenue { get; set; }
    public decimal PrevGrossProfitPercent { get; set; }
    public decimal PrevNetProfitPercent { get; set; }
    public decimal PrevNetCashFlow { get; set; }

    public List<decimal> RevenuePoints { get; set; } = new();
    public List<decimal> GrossProfitPoints { get; set; } = new();
    public List<decimal> NetProfitPoints { get; set; } = new();
    public List<decimal> CashFlowPoints { get; set; } = new();
    public List<string> SparklineLabels { get; set; } = new();

    public string PeriodLabel { get; set; }
    public string PrevPeriodLabel { get; set; }
    public int GranularityId { get; set; }
}
