using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;
using System;

namespace App.Web.Areas.Admin.Models.TaskAlerts.TaskAlertReport;

public partial record TaskAlertReportModel : BaseNopEntityModel
{
    #region Properties

    [NopResourceDisplayName("Admin.TaskAlert.TaskAlertReport.Fields.EmployeeName")]
    public string EmployeeName { get; set; }

    [NopResourceDisplayName("Admin.TaskAlert.TaskAlertReport.Fields.TaskName")]
    public string TaskName { get; set; }

    [NopResourceDisplayName("Admin.TaskAlert.TaskAlertReport.Fields.AlertPercentage")]
    public decimal AlertPercentage { get; set; }

    [NopResourceDisplayName("Admin.TaskAlert.TaskAlertReport.Fields.Reason")]
    public string Reason { get; set; }

    [NopResourceDisplayName("Admin.TaskAlert.TaskAlertReport.Fields.Comment")]
    public string Comment { get; set; }

    [NopResourceDisplayName("Admin.TaskAlert.TaskAlertReport.Fields.ETAHours")]
    public string ETAHours { get; set; }

    [NopResourceDisplayName("Admin.TaskAlert.TaskAlertReport.Fields.DeliverOnTime")]
    public bool DeliverOnTime { get; set; }

    [NopResourceDisplayName("Admin.TaskAlert.TaskAlertReport.Fields.CreatedOn")]
    public DateTime CreatedOn { get; set; }

    #endregion
}
