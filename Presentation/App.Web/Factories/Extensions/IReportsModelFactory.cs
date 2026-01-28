using App.Core.Domain.TimeSheets;
using App.Web.Areas.Admin.Models.Extension.MonthlyPerformanceReports;
using App.Web.Models.Extension.TimesheetReports;
using App.Web.Models.Extensions.TimeSheets;

using System.Threading.Tasks;

namespace App.Web.Factories.Extensions
{
    public partial interface IReportsModelFactory
    {
        Task<TimesheetReportSearchModel> PrepareTimeSheetReportSearchModelAsync(TimesheetReportSearchModel searchModel);
        Task<TimesheetReportListModel> PrepareTimesheetReportListModelAsync
     (TimesheetReportSearchModel searchModel);

        //   Task<TimeSheetModel> PrepareTimeSheetModelAsync(TimeSheetModel model, TimeSheet timeSheet, bool excludeProperties = false);
        Task<TimesheetReportSearchModel> PrepareTimeSheetReportSearchModelForProjectAsync(TimesheetReportSearchModel searchModel);

        Task<TimesheetReportSearchModel> PrepareTimeSheetReportSearchModelForTaskAsync(TimesheetReportSearchModel searchModel);

        Task<TimesheetReportSearchModel> PrepareTimeSheetSearchModelAsync(TimesheetReportSearchModel searchModel);

        Task<TimesheetReportListModel> PrepareMonthlyPerformanceReportListModelAsync
      (TimesheetReportSearchModel searchModel);
    }
}
