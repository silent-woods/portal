using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System;


namespace App.Web.Models.Extensions.TimeSheets
{
    public partial record TimeSheetSearchModel : BaseSearchModel
    {
        public TimeSheetSearchModel()
        {
           
            SelectedProjectIds = new List<int>();
            SelectedEmployeeIds = new List<int>();
            AvailableProjects = new List<SelectListItem>();
            AvailableEmployees = new List<SelectListItem>();
            AvailableBillableType = new List<SelectListItem>();
            PeriodList = new List<SelectListItem>();
            AvailableAssignedToEmployees = new List<SelectListItem>();
        }
        #region Properties

        [NopResourceDisplayName("Admin.TimeSheet.List.ProjectName")]
        public string ProjectName { get; set; }
        [NopResourceDisplayName("Admin.TimeSheet.List.EmployeeName")]
        public string EmployeeName { get; set; }

        [NopResourceDisplayName("Admin.TimeSheet.List.TaskName")]

        public string TaskName { get; set; }

        [NopResourceDisplayName("Admin.TimeSheet.List.From")]
        [UIHint("DateNullable")]

        public DateTime? From { get; set; }
        [NopResourceDisplayName("Admin.TimeSheet.List.To")]
        [UIHint("DateNullable")]


        public DateTime? To { get; set; }


         public int EmployeeId { get; set; }

        [NopResourceDisplayName("Admin.TimeSheet.Fields.AvailableProjects")]
        public IList<SelectListItem> AvailableProjects { get; set; }

        [NopResourceDisplayName("Admin.Common.Fields.SelectedProjectIds")]
        public IList<int> SelectedProjectIds { get; set; }



        [NopResourceDisplayName("Admin.TimeSheet.Fields.AvailableEmployees")]
        public IList<SelectListItem> AvailableEmployees { get; set; }


        [NopResourceDisplayName("Admin.TimeSheet.Fields.AvailableAssignedToEmployees")]
        public IList<SelectListItem> AvailableAssignedToEmployees { get; set; }

        [NopResourceDisplayName("Admin.Common.Fields.SelectedEmployeeIds")]
        public IList<int> SelectedEmployeeIds { get; set; }

        public IList<SelectListItem> AvailableBillableType { get; set; }



        [NopResourceDisplayName("Admin.Common.Fields.BillableType")]
        public int BillableType { get; set; }

        [NopResourceDisplayName("Admin.TimesheetReports.Fields.SearchPeriodId")]


        public int SearchPeriodId { get; set; }

        public List<SelectListItem> PeriodList { get; set; }

        #endregion
    }
}
