using App.Core.Domain.TaskAlerts;
using App.Web.Areas.Admin.Models.TaskAlerts.TaskAlertConfiguration;
using App.Web.Areas.Admin.Models.TaskAlerts.TaskAlertReason;
using App.Web.Areas.Admin.Models.TaskAlerts.TaskAlertReport;
using System.Threading.Tasks;

namespace App.Web.Areas.Admin.Factories;

public partial interface ITaskAlertModelFactory
{
    #region Task Alert Configuration Methods

    Task<TaskAlertConfigurationSearchModel> PrepareTaskAlertConfigurationSearchModelAsync(TaskAlertConfigurationSearchModel searchModel);

    Task<TaskAlertConfigurationListModel> PrepareTaskAlertConfigurationListModelAsync(TaskAlertConfigurationSearchModel searchModel);

    Task<TaskAlertConfigurationModel> PrepareTaskAlertConfigurationModelAsync(TaskAlertConfigurationModel model, 
        TaskAlertConfiguration taskAlertConfiguration);

    #endregion

    #region Task Alert Reasons Methods

    Task<TaskAlertReasonSearchModel> PrepareTaskAlertReasonSearchModelAsync(TaskAlertReasonSearchModel searchModel);

    Task<TaskAlertReasonListModel> PrepareTaskAlertReasonListModelAsync(TaskAlertReasonSearchModel searchModel);

    Task<TaskAlertReasonModel> PrepareTaskAlertReasonModelAsync(TaskAlertReasonModel model, TaskAlertReason taskAlertReason);

    #endregion

    #region Task Alert Report Methods

    Task<TaskAlertReportSearchModel> PrepareTaskAlertReportSearchModelAsync(TaskAlertReportSearchModel searchModel);

    Task<TaskAlertReportListModel> PrepareTaskAlertReportListModelAsync(TaskAlertReportSearchModel searchModel);

    Task<TaskAlertReportModel> PrepareTaskAlertReportModelAsync(TaskAlertReportModel model, TaskAlertLog taskAlertLog);

    #endregion
}
