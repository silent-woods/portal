using App.Web.Areas.Admin.Models.PerformanceMeasurements;
using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace App.Web.Models.PerformanceMeasurements
{
    /// <summary>
    /// Represents a TeamPerformanceMeasurement model
    /// </summary>
    public partial record PerformanceMeasurementModel : BaseNopEntityModel
    {
        public PerformanceMeasurementModel()
        {
            KPIMaster = new List<KPIMasterModels>();
            Employees = new List<SelectListItem>();
            Manager = new List<SelectListItem>();
            Months = new List<SelectListItem>();
            Years = new List<SelectListItem>();
        }
        #region Properties

        [NopResourceDisplayName("Admin.TeamPerformanceMeasurementModel.Fields.KPIMasterId")]
        public int KPIMasterId { get; set; }

        [NopResourceDisplayName("Admin.TeamPerformanceMeasurementModel.Fields.KPIName")]
        public string KPIName { get; set; }
        //public IList<KPIMasterModels> KPIMaster { get; set; }

        [NopResourceDisplayName("Admin.TeamPerformanceMeasurementModel.Fields.EmployeeId")]
        [Required(ErrorMessage = "Employee is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a employee.")]
        public int EmployeeId { get; set; }

        [NopResourceDisplayName("Admin.TeamPerformanceMeasurementModel.Fields.EmployeeName")]
        public string EmployeeName { get; set; }
        public IList<SelectListItem> Employees { get; set; }
        [NopResourceDisplayName("Admin.TeamPerformanceMeasurementModel.Fields.EmployeeManagerId")]
        public int EmployeeManagerId { get; set; }

        [NopResourceDisplayName("Admin.TeamPerformanceMeasurementModel.Fields.EmployeeManagerName")]
        public string EmployeeManagerName { get; set; }
        public IList<SelectListItem> Manager { get; set; }

        [NopResourceDisplayName("Admin.TeamPerformanceMeasurementModel.Fields.Feedback")]
        public string Feedback { get; set; }

        [NopResourceDisplayName("Admin.TeamPerformanceMeasurementModel.Fields.Year")]

        [Required(ErrorMessage = "Year is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a year.")]
        public int Year { get; set; }

        [NopResourceDisplayName("Admin.TeamPerformanceMeasurementModel.Fields.MonthId")]
        //[Required(ErrorMessage = "Month is required.")]
        //[Range(1, int.MaxValue, ErrorMessage = "Please select a month.")]
        public int MonthId { get; set; }

        [NopResourceDisplayName("Admin.TeamPerformanceMeasurementModel.Fields.MonthName")]
        public string MonthName { get; set; }

        [NopResourceDisplayName("Admin.TeamPerformanceMeasurementModel.Fields.StartMonth")]
        //[Required(ErrorMessage = "Start Month is required.")]
        //[Range(1, int.MaxValue, ErrorMessage = "Please select a start month.")]
        public int StartMonth { get; set; }

        [NopResourceDisplayName("Admin.TeamPerformanceMeasurementModel.Fields.EndMonth")]
        //[Required(ErrorMessage = "End Month is required.")]
        //[Range(1, int.MaxValue, ErrorMessage = "Please select an end month.")]
        public int EndMonth { get; set; }

        [NopResourceDisplayName("Admin.TeamPerformanceMeasurementModel.Fields.StartYear")]
        //[Required(ErrorMessage = "Start Year is required.")]
        //[Range(1, int.MaxValue, ErrorMessage = "Please select a start year.")]
        public int StartYear { get; set; }

        [NopResourceDisplayName("Admin.TeamPerformanceMeasurementModel.Fields.EndYear")]
        //[Required(ErrorMessage = "End Year is required.")]
        //[Range(1, int.MaxValue, ErrorMessage = "Please select an end year.")]
        public int EndYear { get; set; }

        public int currCustomer { get; set; }
        public IList<SelectListItem> Years { get; set; }
        public IList<SelectListItem> Months { get; set; }

        [NopResourceDisplayName("Admin.TeamPerformanceMeasurementModel.Fields.CreateOn")]
        public DateTime CreateOn { get; set; }

        [NopResourceDisplayName("Admin.TeamPerformanceMeasurementModel.Fields.UpdateOn")]
        public DateTime UpdateOn { get; set; }
        public AvgMeasurementModel measurementModel { get; set; }

        public IList<KPIMasterModels> KPIMaster { get; set; }

        #endregion
    }
}