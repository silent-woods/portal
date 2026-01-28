using System;
using App.Web.Framework.Mvc.ModelBinding;
using App.Web.Framework.Models;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using FluentMigrator.Infrastructure;
using System.ComponentModel.DataAnnotations;

namespace App.Web.Areas.Admin.Models.PerformanceMeasurements
{
    /// <summary>
    /// Represents a KPIWeightage model
    /// </summary>
    public partial record KPIWeightageModel : BaseNopEntityModel
    {
        public KPIWeightageModel()
        {
            KPIMaster = new List<SelectListItem>();
            Designations = new List<SelectListItem>();
        }
        #region Properties


        [Required(ErrorMessage = "Please Select KPI Weightage.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please Select KPI Weightage.")]
        [NopResourceDisplayName("Admin.KPIWeightage.Model.Fields.KPIMasterId")]
        public int KPIMasterId { get; set; }
        [NopResourceDisplayName("Admin.KPIWeightage.Model.Fields.KPIName")]
        public string KPIName { get; set; }
        public IList<SelectListItem> KPIMaster { get; set; }

        [Required(ErrorMessage = "Please Select Designation.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please Select Designation.")]
        [NopResourceDisplayName("Admin.KPIWeightage.Model.Fields.DesignationId")]
        public int DesignationId { get; set; }

        [NopResourceDisplayName("Admin.KPIWeightage.Model.Fields.DesignationName")] 
        public string DesignationName { get; set; }
        public IList<SelectListItem> Designations { get; set; }
        [NopResourceDisplayName("Admin.KPIWeightage.Model.Fields.Percentage")]
        [Required(ErrorMessage = "Percentage must be greater than zero")]
        [Range(0.1, double.MaxValue, ErrorMessage = "Percentage must be greater than zero")]
        public decimal Percentage { get; set; }
        public string Percentages { get; set; }

        [NopResourceDisplayName("Admin.KPIWeightage.Model.Fields.CreateOn")]
        public DateTime CreateOn { get; set; }

        [NopResourceDisplayName("Admin.KPIWeightage.Model.Fields.UpdateOn")]
        public DateTime UpdateOn { get; set; }

        #endregion
    }
}