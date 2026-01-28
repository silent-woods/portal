using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace App.Web.Areas.Admin.Models.LeaveManagement
{
    /// <summary>
    /// Represents a LeaveManagement search model
    /// </summary>
    public partial record LeaveManagementSearchModel : BaseSearchModel
    {
       public LeaveManagementSearchModel()
        {
            AvailableLeaveType = new List<SelectListItem>();
            AvailableStatus = new List<SelectListItem>();
            PeriodList = new List<SelectListItem>();
        }

        #region Properties
        [NopResourceDisplayName("Admin.Extension.LeaveManagement.List.SearchLeaveManagementName")]
        public string SearchLeaveManagementName { get; set; }
        [NopResourceDisplayName("Admin.Extension.LeaveManagement.Fields.EmployeeName")]
        public string EmployeeName { get; set; }
        [NopResourceDisplayName("Admin.Extension.LeaveManagement.Fields.From")]
        [UIHint("DateNullable")]
        public DateTime? From { get; set; }

        [NopResourceDisplayName("Admin.Extension.LeaveManagement.Fields.To")]
        [UIHint("DateNullable")]
        public DateTime? To { get; set; }

        [NopResourceDisplayName("Admin.Extension.LeaveManagement.Fields.LeaveTypeId")]
        public int SearchLeaveTypeId { get; set; }

        public string LeaveTypeName { get; set; }
        public IList<SelectListItem> AvailableLeaveType { get; set; }

        [NopResourceDisplayName("Admin.Extension.LeaveManagement.Fields.StatusId")]
        public int StatusId { get; set; }
        public IList<SelectListItem> AvailableStatus { get; set; }

        [NopResourceDisplayName("Admin.Extension.LeaveManagement.Fields.SearchPeriodId")]

        public int SearchPeriodId { get; set; }

        public IList<SelectListItem> PeriodList { get; set; }

        #endregion
    }
}