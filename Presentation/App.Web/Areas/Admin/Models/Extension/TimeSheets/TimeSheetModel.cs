using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using App.Web.Framework.Mvc.ModelBinding;
using App.Web.Framework.Models;
using App.Web.Areas.Admin.Models.dailyreport;
using App.Core;

namespace App.Web.Areas.Admin.Models.TimeSheets
{
    public partial record TimeSheetModel : BaseNopEntityModel
    {
        public TimeSheetModel()
        {
            Employee = new List<SelectListItem>();
            Projects = new List<SelectListItem>();
            report = new List<DailyReportModel>();
            reports = new DailyReportModel();
            SelectedEmployeeId = new List<int>();
            AvailableEmployees = new List<SelectListItem>();
            AvailableProjects = new List<SelectListItem>();
            TimeSheetRows = new List<TimeSheetRowModel>();
            Tasks = new List<SelectListItem>();
        }

        #region Properties
        //[Required(ErrorMessage = "Please select EM.")]
        //[Range(1, int.MaxValue, ErrorMessage = "Please select a valid Project.")]
        [NopResourceDisplayName("Admin.TimeSheet.Fields.EmployeeId")]
        public int EmployeeId { get; set; }

        [NopResourceDisplayName("Admin.TimeSheet.Fields.EmployeeName")]
        public string EmployeeName { get; set; }

        [NopResourceDisplayName("Admin.TimeSheet.Fields.Employee")]
        public IList<SelectListItem> Employee { get; set; }

        //[Required(ErrorMessage = "Please select Project.")]
        //[Range(1, int.MaxValue, ErrorMessage = "Please select a valid Project.")]
        [NopResourceDisplayName("Admin.TimeSheet.Fields.ProjectId")]
        public int ProjectId { get; set; }

        [NopResourceDisplayName("Admin.TimeSheet.Fields.ProjectName")]
        public string ProjectName { get; set; }

        [NopResourceDisplayName("Admin.TimeSheet.Fields.Projects")]
        public IList<SelectListItem> Projects { get; set; }

        //[Required(ErrorMessage = "Please select Spent Date.")]

        [NopResourceDisplayName("Admin.TimeSheet.Fields.SpentDate")]
        [UIHint("DateNullable")]
        public DateTime? SpentDate { get; set; }

        [NopResourceDisplayName("Admin.TimeSheet.Fields.SpentDates")]
        public string SpentDates { get; set; }

        //[Required(ErrorMessage = "Estimated Hours must be greater than zero")]
        //[Range(0.1, double.MaxValue, ErrorMessage = "Estimated Hours must be greater than zero")]
        [NopResourceDisplayName("Admin.TimeSheet.Fields.EstimatedHours")]
        public decimal EstimatedHours { get; set; }

        public string EstimatedTimeHHMM { get; set; }


        //[Required(ErrorMessage = "Please enter Task")]
        [NopResourceDisplayName("Admin.TimeSheet.Fields.Task")]
        public int TaskId { get; set; }

        public string TaskName { get; set; }
        //[Required(ErrorMessage = "Spent Hours must be greater than zero")]
        //[Range(0.1, double.MaxValue, ErrorMessage = "Spent Hours must be greater than zero")]
        [NopResourceDisplayName("Admin.TimeSheet.Fields.SpentHours")]
        public int SpentHours { get; set; }


        [NopResourceDisplayName("Admin.TimeSheet.Fields.SpentTime")]

        public string SpentTime { get; set; }


        public int SpentMinutes { get; set; }


        [NopResourceDisplayName("Admin.TimeSheet.Fields.Billable")]
        public bool Billable { get; set; }


        public bool DeliveryOnTime { get; set; }

        public DateTime? StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        public bool IsManualDOT { get; set; }


        [NopResourceDisplayName("Admin.TimeSheet.Fields.CreateOn")]
        public DateTime CreateOn { get; set; }

        [NopResourceDisplayName("Admin.TimeSheet.Fields.UpdateOn")]
        public DateTime UpdateOn { get; set; }

        public IList<DailyReportModel> report { get; set; }
        public DailyReportModel reports { get; set; }

        [NopResourceDisplayName("Admin.TimeSheet.Fields.AvailableEmployees")]
        public IList<SelectListItem> AvailableEmployees { get; set; }

        [NopResourceDisplayName("Admin.TimeSheet.Fields.AvailableProjects")]
        public IList<SelectListItem> AvailableProjects { get; set; }

        [NopResourceDisplayName("Admin.Common.Fields.SelectedEmployeeId")]
        public IList<int> SelectedEmployeeId { get; set; }

        [NopResourceDisplayName("Admin.TimeSheet.Fields.TimeSheetRows")]
        public IList<TimeSheetRowModel> TimeSheetRows { get; set; }

        [NopResourceDisplayName("Admin.TimeSheet.Fields.Tasks")]
        public IList<SelectListItem> Tasks { get; set; }

        public int CurrentEmployeeId { get; set; }


        public int ActivityId { get; set; }

        [NopResourceDisplayName("Admin.TimeSheet.Fields.ActivityName")]

        public string ActivityName { get; set; }


     

        public bool IsManualEntry { get; set; }


        #endregion
    }

    public class TimeSheetRowModel: BaseEntity
    {
        //[Required(ErrorMessage = "Project ID is required")]
     
        [NopResourceDisplayName("Admin.TimeSheet.Fields.ProjectId")]

        public int ProjectId { get; set; }

        //[Required(ErrorMessage = "Task is required")]
        [NopResourceDisplayName("Admin.TimeSheet.Fields.Task")]

        public int TaskId { get; set; }

        public int EmployeeId { get; set; }


        //[Required(ErrorMessage = "Estimated Hours must be greater than zero")]
        //[Range(0.1, double.MaxValue, ErrorMessage = "Estimated Hours must be greater than zero")]

        [NopResourceDisplayName("Admin.TimeSheet.Fields.EstimatedHours")]

        public decimal EstimatedHours { get; set; }

        public string EstimatedTimeHHMM { get; set; }

        //[Required(ErrorMessage = "Spent Hours must be greater than zero")]
        //[Range(0.1, double.MaxValue, ErrorMessage = "Spent Hours must be greater than zero")]
        [NopResourceDisplayName("Admin.TimeSheet.Fields.SpentHours")]

        public int SpentHours { get; set; }
        public int SpentMinutes { get; set; }
        public string SpentTime { get; set; }



        [NopResourceDisplayName("Admin.TimeSheet.Fields.TotalSpent")]

        public string TotalSpent  { get; set; }

        public DateTime SpentDate { get; set; }

        public decimal prevSpentHours { get; set; }

        public string prevActivityName { get; set; }

        public int prevTaskId { get; set; }

        public int ActivityId { get; set; }


        [NopResourceDisplayName("Admin.TimeSheet.Fields.ActivityName")]


        public string ActivityName { get; set; }


        [NopResourceDisplayName("Admin.TimeSheet.Fields.Billable")]

        //[Required(ErrorMessage = "Billable status is required")]
        public bool Billable { get; set; }

        // Add the Task property
        //[Required(ErrorMessage = "Task is required")]
        public string Task { get; set; }

        public bool IsNewEntry { get; set; }
    }
}
