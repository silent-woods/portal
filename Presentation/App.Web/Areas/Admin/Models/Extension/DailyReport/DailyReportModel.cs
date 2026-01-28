using System;
using App.Web.Framework.Mvc.ModelBinding;
using App.Web.Framework.Models;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace App.Web.Areas.Admin.Models.dailyreport
{
    /// <summary>
    /// Represents a TimeSheet model
    /// </summary>
    public partial record DailyReportModel : BaseNopEntityModel
    {
        public DailyReportModel()
        {
            Employee = new List<SelectListItem>();
            Projects = new List<SelectListItem>();
        }
        #region Properties

        [NopResourceDisplayName("Admin.TimeSheet.Fields.EmployeeId")]
        public int EmployeeId { get; set; }

        [NopResourceDisplayName("Admin.TimeSheet.Fields.EmployeeName")]
        public string EmployeeName { get; set; }
        public IList<SelectListItem> Employee { get; set; }

        [NopResourceDisplayName("Admin.TimeSheet.Fields.ProjectId")]
        public int ProjectId { get; set; }

        [NopResourceDisplayName("Admin.TimeSheet.Fields.ProjectName")]
        public string ProjectName { get; set; }
        public IList<SelectListItem> Projects { get; set; }
        [NopResourceDisplayName("Admin.TimeSheet.Fields.EstimatedHours")]
        public decimal EstimatedHours { get; set; }
        [NopResourceDisplayName("Admin.TimeSheet.Fields.ProductiveHours")]
        public decimal ProductiveHours { get; set; }
        [NopResourceDisplayName("Admin.DailyReport.Fields.RNDHours")]
        public decimal RNDHours { get; set; }
        [NopResourceDisplayName("Admin.DailyReport.Fields.Total")]
        public decimal Total { get; set; }

        [NopResourceDisplayName("Admin.DailyReport.Fields.ProductivityRatio")]
        public decimal ProductivityRatio { get; set; }

        [NopResourceDisplayName("Admin.DailyReport.Fields.StartDate")]
        [UIHint("DateNullable")]
        public DateTime StartDate { get; set; }
        [NopResourceDisplayName("Admin.DailyReport.Fields.EndDate")]
        [UIHint("DateNullable")]
        public DateTime EndDate { get; set; }
        [NopResourceDisplayName("Admin.DailyReport.Fields.Date")]
        public DateTime Date { get; set; }

        [NopResourceDisplayName("Admin.DailyReport.Fields.Task")]
        public string Task { get; set; }

        #endregion
    }
}
