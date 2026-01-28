using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace App.Web.Areas.Admin.Models.TaskAlerts.TaskAlertReport;

public partial record TaskAlertReportSearchModel : BaseSearchModel
{
    #region Ctor

    public TaskAlertReportSearchModel()
    {
        SelectedEmployeeIds = new List<int>();
        AvailableEmployees = new List<SelectListItem>();
        AvailableTaskAlertTypes = new List<SelectListItem>();
        AvailableTaskAlertConfigurations = new List<SelectListItem>();
    }

    #endregion

    #region Properties

    [UIHint("DateNullable")]
    [NopResourceDisplayName("Admin.TaskAlert.TaskAlertReport.Fields.SearchFromDate")]
    public DateTime? SearchFromDate { get; set; }

    [UIHint("DateNullable")]
    [NopResourceDisplayName("Admin.TaskAlert.TaskAlertReport.Fields.SearchToDate")]
    public DateTime? SearchToDate { get; set; }

    [NopResourceDisplayName("Admin.TaskAlert.TaskAlertReport.Fields.SelectedEmployeeIds")]
    public IList<int> SelectedEmployeeIds { get; set; }

    [NopResourceDisplayName("Admin.TaskAlert.TaskAlertReport.Fields.SearchTaskAlertTypeId")]
    public int SearchTaskAlertTypeId { get; set; }

    [NopResourceDisplayName("Admin.TaskAlert.TaskAlertReport.Fields.SearchTaskAlertConfigurationId")]
    public int SearchTaskAlertConfigurationId { get; set; }

    public IList<SelectListItem> AvailableEmployees { get; set; }

    public IList<SelectListItem> AvailableTaskAlertTypes { get; set; }

    public IList<SelectListItem> AvailableTaskAlertConfigurations { get; set; }

    #endregion
}
