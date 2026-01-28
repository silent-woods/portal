using App.Core.Domain.TimeSheets;
using App.Web.Areas.Admin.Models.Extension.MonthlyPerformanceReports;
using App.Web.Areas.Admin.Models.TimeSheets;
using System.Threading.Tasks;

namespace App.Web.Areas.Admin.Factories.Extension
{
    public partial interface IMonthlyPerformanceReportModelFactory
    {
        Task<MonthlyPerformanceReportSearchModel> PrepareTimeSheetSearchModelAsync(MonthlyPerformanceReportSearchModel searchModel);

        Task<MonthlyPerformanceReportListModel> PrepareMonthlyPerformanceReportListModelAsync
  (MonthlyPerformanceReportSearchModel searchModel);

        Task<TimeSheetModel> PrepareTimeSheetModelAsync(TimeSheetModel model, TimeSheet timeSheet, bool excludeProperties = false);

        Task<MonthlyPerformanceReportListModel> PreparePerformanceSummaryReportListModelAsync
  (MonthlyPerformanceReportSearchModel searchModel);
    }
}
