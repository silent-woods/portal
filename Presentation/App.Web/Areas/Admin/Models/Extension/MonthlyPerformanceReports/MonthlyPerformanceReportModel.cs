using App.Web.Areas.Admin.Models.ProjectEmployeeMappings;
using App.Web.Areas.Admin.Models.TimeSheets;
using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Org.BouncyCastle.Bcpg.OpenPgp;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace App.Web.Areas.Admin.Models.Extension.MonthlyPerformanceReports
{
    public partial record MonthlyPerformanceReportModel : BaseNopEntityModel
    {
        public MonthlyPerformanceReportModel()
        {
            AvailableProjects = new List<SelectListItem>();

            AvailableEmployees = new List<SelectListItem>();
            Years = new List<SelectListItem>();
            TimeSheets = new List<SelectListItem>();

        }
        #region Properties
        public int EmployeeId {  get; set; }

        public string Month {  get; set; }

        public string EmployeeName { get; set; }


        public int ProjectId {  get; set; }
        public string ExtraTime {  get; set; }
        public string ProjectName { get; set; }

        public int TaskId {  get; set; }

        public string TaskName { get; set; }

        public decimal EstimatedTime {  get; set; }

        public string EstimatedTimeFormat { get; set; }


        public decimal SpentTime {  get; set; }

        public string SpentTimeFormat {  get; set; }

        public string QaTimeFormat { get; set; }

        public decimal AllowedVariations { get; set; }
        public bool DeliveredOnTime { get; set; }


        public int BugCount { get; set; }

        public string QualityComments { get; set; }


        public int TotalTask { get; set; }

        public int TotalDeleverdOnTime { get; set; }

        public decimal ResultPercentage { get; set; }

        public string DueDateFormat { get; set; }

        public string StatusName { get; set; }

        public decimal OverduePercentage { get; set; }

        public string TaskTypeName { get; set; }

        public string BugTime { get; set; }


        public string ParentTaskName { get; set; }

        public decimal? WorkQuality { get; set; }


        public decimal? DOTPercentage { get; set; }

        public bool HasBugTasks { get; set; }

        public IList<SelectListItem> TimeSheets { get; set; }

        public IList<SelectListItem> Years { get; set; }

        public IList<SelectListItem> AvailableProjects { get; set; }

  
        public IList<SelectListItem> AvailableEmployees { get; set; }


    

      
        public int NoOfOverdueTask { get; set; }

        public string DOTPercentageString { get; set; }
        public string WorkQualityString { get; set; }

        public decimal DOTConut { get; set; }

        public string OverEstimationSummary { get; set; }



        #endregion
    }


}
