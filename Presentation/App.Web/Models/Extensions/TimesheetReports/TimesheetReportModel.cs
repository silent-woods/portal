using App.Web.Areas.Admin.Controllers.Extension;
using App.Web.Framework.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace App.Web.Models.Extension.TimesheetReports
{                                                     
    public partial record TimesheetReportModel : BaseNopEntityModel
    {
        public TimesheetReportModel()
        {
            AvailableProjects = new List<SelectListItem>();

            AvailableEmployees = new List<SelectListItem>();
            Years = new List<SelectListItem>();
            TimeSheets = new List<SelectListItem>();

        }
        #region Properties
        public int EmployeeId { get; set; }

        public string Month { get; set; }

        public string EmployeeName { get; set; }


        public int ProjectId { get; set; }
        public string ExtraTime { get; set; }
        public string ProjectName { get; set; }

        public int TaskId { get; set; }

        public string TaskName { get; set; }

        public decimal EstimatedTime { get; set; }
        public string EstimatedTimeFormat { get; set; }

        
        public decimal SpentTime { get; set; }

        public string SpentTimeFormat { get; set; }



        public DateTime? SpentDate { get; set; }

        public string SpentDateFormat { get; set; }



        public decimal AllowedVariations { get; set; }
        public bool DeliveredOnTime { get; set; }


        public int BugCount { get; set; }

        public string QualityComments { get; set; }

        public string TotalSpentTime { get; set; }

        public int TotalTask { get; set; }

        public bool IsHoliday { get; set; }

        public bool IsWeekend { get; set; }



        public int TotalDeleverdOnTime { get; set; }

        public decimal ResultPercentage { get; set; }

        public string WeekDay { get; set; }

        public string DueDateFormat {get; set;}

        public string StatusName { get; set; }

        public string QaTime { get; set; }

        public string BugTime { get; set; }

        public decimal? WorkQuality { get; set; }

        public decimal? DOTPercentage { get; set; }

        public decimal OverduePercentage { get; set; }

        public bool HasBugTasks { get; set; }

        public IList<SelectListItem> TimeSheets { get; set; }

        public IList<SelectListItem> Years { get; set; }

        public IList<SelectListItem> AvailableProjects { get; set; }


        public IList<SelectListItem> AvailableEmployees { get; set; }


        #endregion
    }
}
