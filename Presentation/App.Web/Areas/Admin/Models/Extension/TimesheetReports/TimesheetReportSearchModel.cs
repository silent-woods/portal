using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography.Pkcs;

namespace App.Web.Areas.Admin.Models.Extension.TimesheetReports
{
    public partial record TimesheetReportSearchModel : BaseSearchModel
    {


        public TimesheetReportSearchModel()
        {
            AvailableProjects = new List<SelectListItem>();


            AvailableEmployees = new List<SelectListItem>();
            Years = new List<SelectListItem>();
            SelectedEmployeeIds = new List<int>();
            Months = new List<SelectListItem>();
            ShowByList= new List<SelectListItem>();
            HoursList = new List<SelectListItem>();
            SelectedProjectIds = new List<int>();
            AvailableBillableType = new List<SelectListItem>();
            PeriodList= new List<SelectListItem>();

        }
        [NopResourceDisplayName("Admin.MonthlyPerformanceReport.Fields.SearchEmployeeId")]
        [Required(ErrorMessage = "Please select status")]
 
        public int SearchEmployeeId { get; set; }
        [NopResourceDisplayName("Admin.MonthlyPerformanceReport.Fields.MonthId")]

        public int MonthId { get; set; }


        [NopResourceDisplayName("Admin.TimesheetReports.Fields.EmployeeId")]

        public int EmployeeId { get; set; }

        public string SearchMonth { get; set; }

        [NopResourceDisplayName("Admin.MonthlyPerformanceReport.Fields.Year")]

        public int Year { get; set; }
        public string SearchEmployeeName { get; set; }

        [NopResourceDisplayName("Admin.TimesheetReports.Fields.From")]
        [UIHint("DateNullable")]
        [Required(ErrorMessage = "Please select From Date")]
        public DateTime? From { get; set; }

        [NopResourceDisplayName("Admin.TimesheetReports.Fields.To")]
        [UIHint("DateNullable")]

        public DateTime? To { get; set; }
        [NopResourceDisplayName("Admin.TimesheetReports.Fields.ShowById")]

        public int ShowById { get; set; }
        [NopResourceDisplayName("Admin.TimesheetReports.Fields.HoursId")]

        public int HoursId { get; set; }

        public string Hours { get; set; }

        [NopResourceDisplayName("Admin.Common.Fields.BillableType")]
        public int BillableType { get; set; }


        [NopResourceDisplayName("Admin.TimesheetReports.Fields.ProjectId")]

        public int ProjectId { get; set; }

        [NopResourceDisplayName("Admin.TimesheetReports.Fields.SearchPeriodId")]

        public int SearchPeriodId { get; set; }


        public string ShowByName {  get; set; }
        [NopResourceDisplayName("Admin.TimesheetReports.Fields.IsHideEmpty")]

        public bool IsHideEmpty { get; set; }

        public List<SelectListItem> ShowByList { get; set; }

        public List<SelectListItem> PeriodList { get; set; }


        public List<SelectListItem> HoursList { get; set; }



        public IList<SelectListItem> Years { get; set; }

        [NopResourceDisplayName("Admin.TimesheetReports.Fields.SelectedEmployeeIds")]

        public IList<int> SelectedEmployeeIds { get; set; }
        [NopResourceDisplayName("Admin.TimesheetReports.Fields.SelectedProjectIds")]

        public IList<int> SelectedProjectIds { get; set; }
        [NopResourceDisplayName("Admin.TimesheetReports.Fields.SelectedTaskIds")]

        public IList<int>SelectedTaskIds { get; set; }

        [NopResourceDisplayName("Admin.TimesheetReports.Fields.TaskName")]

        public string TaskName { get; set; }

        public string LastLeaveBalanceUpdate { get; set; }

        [NopResourceDisplayName("Admin.TimesheetReports.Fields.ActivityName")]

        public string ActivityName { get; set; }

        public IList<SelectListItem> AvailableProjects { get; set; }

        public IList<SelectListItem> AvailableTasks { get; set; }

        public IList<SelectListItem> AvailableBillableType { get; set; }
        public IList<SelectListItem> AvailableEmployees { get; set; }

        public IList<SelectListItem> Months { get; set; }
    }
}
