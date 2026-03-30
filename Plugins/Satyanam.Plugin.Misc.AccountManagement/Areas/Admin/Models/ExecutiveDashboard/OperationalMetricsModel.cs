using System.Collections.Generic;

namespace Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Models.ExecutiveDashboard;

public partial record OperationalMetricsModel
{
    public decimal BillableUtilPct { get; set; }
    public decimal DotPct { get; set; }
    // Work Quality Score (0–100 scale, nullable — null when no tasks have WorkQuality in period)
    public decimal? WorkQualityScore { get; set; }
    public decimal? PrevWorkQualityScore { get; set; }
    public int WorkQualityTaskCount { get; set; }
    public int WorkQualityBugCount { get; set; }
    public int OverdueTaskCount { get; set; }
    public decimal PrevBillableUtilPct { get; set; }
    public decimal PrevDotPct { get; set; }
    public int PrevOverdueTaskCount { get; set; }
    public List<decimal> BillableHoursPoints { get; set; } = new();
    public List<decimal> NonBillableHoursPoints { get; set; } = new();
    public List<decimal> BillableUtilPoints { get; set; } = new();
    public List<decimal> DotPoints { get; set; } = new();
    public List<decimal?> WorkQualityPoints { get; set; } = new();
    public List<string> SparklineLabels { get; set; } = new();
    public string PeriodLabel { get; set; }
    public string PrevPeriodLabel { get; set; }
}
