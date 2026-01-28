using App.Web.Framework.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using App.Web.Framework.Mvc.ModelBinding;
using App.Web.Framework.Models;
using App.Web.Areas.Admin.Models.dailyreport;
using App.Core;

namespace App.Web.Models.Extensions.TimeSheets
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
            AvailableAssignedToEmployees = new List<SelectListItem>();
            PeriodList = new List<SelectListItem>();
        }

        #region Properties
        [NopResourceDisplayName("Admin.TimeSheet.Fields.EmployeeId")]
        public int EmployeeId { get; set; }

        [NopResourceDisplayName("Admin.TimeSheet.Fields.EmployeeName")]
        public string EmployeeName { get; set; }

        [NopResourceDisplayName("Admin.TimeSheet.Fields.Employee")]
        public IList<SelectListItem> Employee { get; set; }
       
        [NopResourceDisplayName("Admin.TimeSheet.Fields.ProjectId")]
        public int ProjectId { get; set; }

        [NopResourceDisplayName("Admin.TimeSheet.Fields.ProjectName")]
        public string ProjectName { get; set; }

        [NopResourceDisplayName("Admin.TimeSheet.Fields.Projects")]
        public IList<SelectListItem> Projects { get; set; }
     
        [NopResourceDisplayName("Admin.TimeSheet.Fields.SpentDate")]
        [UIHint("DateNullable")]
        public DateTime? SpentDate { get; set; }

        [NopResourceDisplayName("Admin.TimeSheet.Fields.SpentDates")]
        public string SpentDates { get; set; }
  
        [NopResourceDisplayName("Admin.TimeSheet.Fields.EstimatedHours")]
        public decimal EstimatedHours { get; set; }
        public string EstimatedHoursHHMM { get; set; }
        public string SpentTime { get; set; }
        public string TotalSpent { get; set; }
        public string Time { get; set; }

        [NopResourceDisplayName("Admin.TimeSheet.Fields.Task")]
        public int TaskId { get; set; }
        public string TaskName { get; set; }

        [NopResourceDisplayName("Admin.TimeSheet.Fields.SpentHours")]
        public decimal SpentHours { get; set; }

        [NopResourceDisplayName("Admin.TimeSheet.Fields.Billable")]
        public bool Billable { get; set; }
        public bool DeliveryOnTime { get; set; }
        public bool IsManualDOT { get; set; }

        [NopResourceDisplayName("Admin.TimeSheet.Fields.CreateOn")]
        public DateTime CreateOn { get; set; }

        [NopResourceDisplayName("Admin.TimeSheet.Fields.UpdateOn")]
        public DateTime UpdateOn { get; set; }
        public IList<DailyReportModel> report { get; set; }
        public DailyReportModel reports { get; set; }

        [NopResourceDisplayName("Admin.TimeSheet.Fields.AvailableEmployees")]
        public IList<SelectListItem> AvailableEmployees { get; set; }

        [NopResourceDisplayName("Admin.TimeSheet.Fields.AvailableAssignedToEmployees")]
        public IList<SelectListItem> AvailableAssignedToEmployees { get; set; }

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
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool IsManualEntry { get; set; }
        public bool IsQAEntry { get; set; }
        public List<SelectListItem> PeriodList { get; set; }
        #endregion
    }

    public class TimeSheetRowModel : BaseEntity
    {
        [NopResourceDisplayName("Admin.TimeSheet.Fields.ProjectId")]
        public int ProjectId { get; set; }

        [NopResourceDisplayName("Admin.TimeSheet.Fields.Task")]
        public int TaskId { get; set; }
        public int EmployeeId { get; set; }
        public DateTime SpentDate { get; set; }

        [NopResourceDisplayName("Admin.TimeSheet.Fields.EstimatedHours")]
        public decimal EstimatedHours { get; set; }
        public string EstimatedHoursHHMM { get; set; }

        [NopResourceDisplayName("Admin.TimeSheet.Fields.SpentHours")]
        public int SpentHours { get; set; }
        public int SpentMinutes { get; set; }
        public string SpentTime { get; set; }

        [NopResourceDisplayName("Admin.TimeSheet.Fields.TotalSpent")]
        public string TotalSpent { get; set; }
        public decimal prevSpentHours { get; set; }
        public int prevTaskId { get; set; }

        [NopResourceDisplayName("Admin.TimeSheet.Fields.Billable")]
        public bool Billable { get; set; }
        public string Task { get; set; }
        public bool IsNewEntry { get; set; }
        public int ActivityId { get; set; }
        [NopResourceDisplayName("Admin.TimeSheet.Fields.ActivityName")]
        public string ActivityName { get; set; }
        public string prevActivityName { get; set; }

    }
}
