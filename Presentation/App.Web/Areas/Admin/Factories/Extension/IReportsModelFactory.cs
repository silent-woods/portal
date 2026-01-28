using App.Core.Domain.Extension.TimeSheets;
using App.Core.Domain.TimeSheets;
using App.Web.Areas.Admin.Models.Extension.ActivityTracking;
using App.Web.Areas.Admin.Models.Extension.MonthlyPerformanceReports;
using App.Web.Areas.Admin.Models.Extension.TimesheetReports;
using App.Web.Areas.Admin.Models.TimeSheets;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace App.Web.Areas.Admin.Factories.Extension
{
    public partial interface IReportsModelFactory
    {
        Task<TimesheetReportSearchModel> PrepareTimeSheetReportSearchModelAsync(TimesheetReportSearchModel searchModel);
        Task<TimesheetReportListModel> PrepareTimesheetReportListModelAsync
     (TimesheetReportSearchModel searchModel);

        Task<TimeSheetModel> PrepareTimeSheetModelAsync(TimeSheetModel model, TimeSheet timeSheet, bool excludeProperties = false);
        Task<TimesheetReportSearchModel> PrepareTimeSheetReportSearchModelForProjectAsync(TimesheetReportSearchModel searchModel);

        Task<TimesheetReportSearchModel> PrepareTimeSheetReportSearchModelForTaskAsync(TimesheetReportSearchModel searchModel);

        Task<TimesheetReportListModel> PrepareActivityLogReportListModelAsync
          (TimesheetReportSearchModel searchModel);

        Task<TimesheetReportModel> PrepareViewScreenShotsModel(int timesheetId);

        Task<ActivityTrackingSearchModel> PrepareActivityTrackingSearchModelAsync(ActivityTrackingSearchModel searchModel);

        Task<ActivityTrackingListModel> PrepareActivityTrackingListModelAsync
        (ActivityTrackingSearchModel searchModel);

        Task<ActivityTrackingSearchModel> PrepareActivityTrackingReportSearchModelAsync(ActivityTrackingSearchModel searchModel);
        Task<ActivityTrackingListModel> PrepareActivityTrackingReportListModelAsync(ActivityTrackingSearchModel searchModel);
        Task<IList<ActivityTrackingModel>> PrepareDateWiseActivityTrackingModelAsync(
     int employeeId, DateTime? from, DateTime? to);

        Task<IList<ActivityTrackingModel>> PrepareSubgridDataAsync(
    int employeeId, DateTime from, DateTime to, ShowByEnum showBy);
    }
}
