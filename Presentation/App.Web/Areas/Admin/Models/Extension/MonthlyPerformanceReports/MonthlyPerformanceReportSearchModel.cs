using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;
using DocumentFormat.OpenXml.Bibliography;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace App.Web.Areas.Admin.Models.Extension.MonthlyPerformanceReports
{
    public partial record MonthlyPerformanceReportSearchModel : BaseSearchModel
    {
        

        public MonthlyPerformanceReportSearchModel()
        {
            AvailableProjects = new List<SelectListItem>();
            AvailableEmployees = new List<SelectListItem>();
            Years = new List<SelectListItem>();
            SelectedEmployeeIds = new List<int>();
            SelectedProjectIds = new List<int>();
            Months = new List<SelectListItem>();
            PeriodList = new List<SelectListItem>();
            AvailableOverEstimations = new List<SelectListItem>();
            AvailableProcessWorkflow = new List<SelectListItem>();
            AvailableDeliveryOnTime = new List<SelectListItem>();
            AvailableStatus = new List<SelectListItem>();

        }
        [NopResourceDisplayName("Admin.MonthlyPerformanceReport.Fields.SearchEmployeeId")]
        public int SearchEmployeeId { get; set; }
        [NopResourceDisplayName("Admin.MonthlyPerformanceReport.Fields.MonthId")]

        public int MonthId { get; set; }

        [NopResourceDisplayName("Admin.TimesheetReports.Fields.From")]
        [UIHint("DateNullable")]
     
        public DateTime? From { get; set; }

        [NopResourceDisplayName("Admin.TimesheetReports.Fields.To")]
        [UIHint("DateNullable")]
   
        public DateTime? To { get; set; }

        [NopResourceDisplayName("Admin.TimesheetReports.Fields.SearchTaskName")]

        public string SearchTaskName { get; set; }


        [NopResourceDisplayName("Admin.ProjectTasks.Fields.SearchDueDate")]
        [UIHint("DateNullable")]

        public DateTime? SearchDueDate { get; set; }



        [NopResourceDisplayName("Admin.TimeSheet.Fields.SearchDeliveryOnTime")]

        public int SearchDeliveryOnTime { get; set; }
        public IList<SelectListItem> AvailableDeliveryOnTime { get; set; }

        public string SearchMonth { get; set; }

        [NopResourceDisplayName("Admin.MonthlyPerformanceRtask eport.Fields.Year")]

        public int Year { get; set; }

        [NopResourceDisplayName("Admin.MonthlyPerformanceReport.Fields.OverEstimation")]

        public int OverEstimation { get; set; }

        [NopResourceDisplayName("Admin.TimesheetReports.Fields.SearchPeriodId")]

        public int SearchPeriodId { get; set; }
        public string SearchEmployeeName { get; set; }

        [NopResourceDisplayName("Admin.ProjectTasks.Fields.SearchProcessWorkflowId")]

        public int SearchProcessWorkflowId { get; set; }
        public IList<SelectListItem> AvailableProcessWorkflow { get; set; }


        [NopResourceDisplayName("Admin.ProjectTasks.Fields.SearchStatusId")]

        public int SearchStatusId { get; set; }
        public IList<SelectListItem> AvailableStatus { get; set; }

        public IList<SelectListItem> Years { get; set; }

        public List<SelectListItem> PeriodList { get; set; }
        public IList<int> SelectedEmployeeIds { get; set; }

        [NopResourceDisplayName("Admin.MonthlyPerformanceReport.Fields.SelectedProjectIds")]
        public IList<int> SelectedProjectIds { get; set; }

        public IList<SelectListItem> AvailableProjects { get; set; }

        public IList<SelectListItem> AvailableOverEstimations { get; set; }


        public IList<SelectListItem> AvailableEmployees { get; set; }

       public IList<SelectListItem> Months { get; set; }


    }
}
