using System.Collections.Generic;

namespace Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Models.ExecutiveDashboard;

public partial record CRMInsightsModel
{
    public decimal TotalPipelineValue { get; set; }
    public int DealsWonCount { get; set; }
    public decimal DealsWonValue { get; set; }
    public decimal PrevDealsWonValue { get; set; }
    public int NewLeadsCount { get; set; }
    public int NewInquiriesCount { get; set; }
    public int PrevNewLeadsCount { get; set; }
    public int PrevNewInquiriesCount { get; set; }
    public int OverdueFollowupsCount { get; set; }
    public List<decimal> DealsWonPoints { get; set; } = new();
    public List<int> NewLeadsPoints { get; set; } = new();
    public List<string> SparklineLabels { get; set; } = new();
    public List<DealStagePoint> DealsByStage { get; set; } = new();
    public List<LeadSourcePoint> LeadsBySource { get; set; } = new();
    public List<int> MonthlyLeadTrend { get; set; } = new();
    public List<int> MonthlyDealTrend { get; set; } = new();
    public List<string> MonthlyTrendLabels { get; set; } = new();
    public int DealsClosedWon { get; set; }
    public int DealsClosedLost { get; set; }
    public int DealsClosedLostToComp { get; set; }
    public List<LinkedInStatusPoint> LinkedInStatusCounts { get; set; } = new();
    public int LinkedInTotalCount { get; set; }
    public int LinkedInConvertedCount { get; set; }
    public int LinkedInRepliedCount { get; set; }
    public decimal LinkedInConversionRate { get; set; }
    public string PeriodLabel { get; set; }
    public string PrevPeriodLabel { get; set; }
}

public record DealStagePoint(string StageName, int Count, decimal Value);
public record LeadSourcePoint(string SourceName, int Count);
public record LinkedInStatusPoint(string StatusName, int Count, string Color);
