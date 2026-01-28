using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Org.BouncyCastle.Asn1.Crmf;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace App.Web.Areas.Admin.Models.TimeSheets
{
    /// <summary>
    /// Represents a timesheet search model
    /// </summary>
    public partial record TimeSheetSearchModel : BaseSearchModel
    {
        public TimeSheetSearchModel()
        {
            SelectedEmployeeIds = new List<int>();
            AvailableEmployees = new List<SelectListItem>();
            SelectedProjectIds = new List<int>();
            AvailableProjects = new List<SelectListItem>();

            PeriodList = new List<SelectListItem>();
            AvailableBillableType = new List<SelectListItem>();

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


        [NopResourceDisplayName("Admin.TimeSheet.Fields.AvailableEmployees")]
        public IList<SelectListItem> AvailableEmployees { get; set; }

        [NopResourceDisplayName("Admin.Common.Fields.SelectedEmployeeId")]
        public IList<int> SelectedEmployeeIds { get; set; }

        [NopResourceDisplayName("Admin.TimeSheet.Fields.AvailableProjects")]
        public IList<SelectListItem> AvailableProjects { get; set; }

        [NopResourceDisplayName("Admin.Common.Fields.SelectedProjectIds")]
        public IList<int> SelectedProjectIds { get; set; }

        public IList<SelectListItem> AvailableBillableType { get; set; }

      

        [NopResourceDisplayName("Admin.Common.Fields.BillableType")]
        public int BillableType { get; set; }

        [NopResourceDisplayName("Admin.TimeSheet.Fields.SearchPeriodId")]

        public int  SearchPeriodId { get; set; }

        public IList<SelectListItem> PeriodList { get; set; }

        #endregion
    }
}