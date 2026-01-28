using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace App.Web.Areas.Admin.Models.PerformanceMeasurements
{
    /// <summary>
    /// Represents a TeamPerformanceMeasurement model
    /// </summary>
    public partial record TeamPerformanceMeasurementModel : BaseNopEntityModel
    {
        public TeamPerformanceMeasurementModel()
        {
            KPIMaster = new List<KPIMasterModels>();
            Employees = new List<SelectListItem>();
            Manager = new List<SelectListItem>();
            Months = new List<SelectListItem>();
            Years = new List<SelectListItem>();


            SelectedEmployeeId = new List<int>();
            AvailableEmployees = new List<SelectListItem>();

            SelectedManagerId = new List<int>();
            AvailableManagers = new List<SelectListItem>();
        }
        #region Properties

        [NopResourceDisplayName("Admin.TeamPerformanceMeasurementModel.Fields.KPIMasterId")]
        public int KPIMasterId { get; set; }

        [NopResourceDisplayName("Admin.TeamPerformanceMeasurementModel.Fields.KPIName")]
        public string KPIName { get; set; }
        public IList<KPIMasterModels> KPIMaster { get; set; }

        [NopResourceDisplayName("Admin.TeamPerformanceMeasurementModel.Fields.EmployeeId")]
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

        [Required(ErrorMessage = "Please select Year.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select Year.")]
        [NopResourceDisplayName("Admin.TeamPerformanceMeasurementModel.Fields.Year")]
        public int Year { get; set; }

        [Required(ErrorMessage = "Please select Month.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select Month.")]
        [NopResourceDisplayName("Admin.TeamPerformanceMeasurementModel.Fields.MonthId")]
        public int MonthId { get; set; }

        [NopResourceDisplayName("Admin.TeamPerformanceMeasurementModel.Fields.MonthName")]
        public string MonthName { get; set; }

        [NopResourceDisplayName("Admin.TeamPerformanceMeasurementModel.Fields.StartMonth")]
        public int StartMonth { get; set; }

        [NopResourceDisplayName("Admin.TeamPerformanceMeasurementModel.Fields.EndMonth")]
        public int EndMonth { get; set; }

        [NopResourceDisplayName("Admin.TeamPerformanceMeasurementModel.Fields.StartYear")]
        public int StartYear { get; set; }

        [NopResourceDisplayName("Admin.TeamPerformanceMeasurementModel.Fields.EndYear")]
        public int EndYear { get; set; }
        public IList<SelectListItem> Years { get; set; }

   
        public IList<SelectListItem> Months { get; set; }

        [NopResourceDisplayName("Admin.TeamPerformanceMeasurementModel.Fields.CreateOn")]
        public DateTime CreateOn { get; set; }

        [NopResourceDisplayName("Admin.TeamPerformanceMeasurementModel.Fields.UpdateOn")]
        public DateTime UpdateOn { get; set; }
        public AvgMeasurementModel measurementModel { get; set; }
        public IList<SelectListItem> AvailableEmployees { get; set; }


        [NopResourceDisplayName("Admin.Common.Fields.SelectedEmployeeId")]
 
        public IList<int> SelectedEmployeeId { get; set; }

        public IList<SelectListItem> AvailableManagers { get; set; }


        [NopResourceDisplayName("Admin.Common.Fields.SelectedManagerId")]
      
        public IList<int> SelectedManagerId { get; set; }
        #endregion
    }
}