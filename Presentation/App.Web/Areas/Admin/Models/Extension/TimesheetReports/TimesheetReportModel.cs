using App.Web.Framework.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace App.Web.Areas.Admin.Models.Extension.TimesheetReports
{
    public partial record TimesheetReportModel : BaseNopEntityModel
    {
        public TimesheetReportModel()
        {
            AvailableProjects = new List<SelectListItem>();

            AvailableEmployees = new List<SelectListItem>();
            Years = new List<SelectListItem>();
            TimeSheets = new List<SelectListItem>();
            Screenshots =new List<ScreenshotModel>();

        }
        #region Properties
        public int EmployeeId { get; set; }

        public string Month { get; set; }

        public string EmployeeName { get; set; }


        public int ProjectId { get; set; }

        public int ActivityId { get; set; }

        public string ActivityName { get; set; }


        public decimal ExtraTime { get; set; }
        public string ProjectName { get; set; }

        public int TaskId { get; set; }

        public string TaskName { get; set; }

        public decimal EstimatedTime { get; set; }

        public decimal SpentTime { get; set; }

      
        public DateTime? SpentDate { get; set; }

        public string SpentDateFormat { get; set; }
        public string SpentTimeFormat { get; set; }

        public string Time { get; set; }



        public decimal AllowedVariations { get; set; }
        public bool DeliveredOnTime { get; set; }


        public int BugCount { get; set; }

        public string QualityComments { get; set; }

        public string TotalSpentTime { get; set; }

        public int TotalTask { get; set; }

        public bool IsHoliday { get; set; }

        public bool IsWeekend { get; set; }




        public int KeyboardHits { get; set; }

        public int MouseHits { get; set; }

        public int TotalDeleverdOnTime { get; set; }

        public decimal ResultPercentage { get; set; }

        public string WeekDay { get; set; }

        public List<ScreenshotModel> Screenshots { get; set; }

        public IList<SelectListItem> TimeSheets { get; set; }

        public IList<SelectListItem> Years { get; set; }

        public IList<SelectListItem> AvailableProjects { get; set; }


        public IList<SelectListItem> AvailableEmployees { get; set; }


        #endregion
    }

    public partial record ScreenshotModel
    {
        public int KeyboardHits { get; set; }
        public int MouseHits { get; set; }
        public string ScreenshotUrl { get; set; }
        public DateTime CreateOnUtc { get; set; }
    }

    public partial record ActivityEventJsonModel
    {
        public int KeyboardHits { get; set; }
        public int MouseHits { get; set; }
        public string ScreenshotUrl { get; set; }
        public int StatusId { get; set; }

        public string StatusName { get; set; }

        public int Duration { get; set; }
        public DateTime CreateOnUtc { get; set; }

        public DateTime CreatedOnUtc { get; set; }

        public string FormattedTime { get; set; }

        public string DurationHHMM { get; set; }
    }

}
