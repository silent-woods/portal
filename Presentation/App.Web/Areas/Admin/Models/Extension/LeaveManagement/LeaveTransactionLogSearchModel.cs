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
    public partial record LeaveTransactionLogSearchModel : BaseSearchModel
    {
       public LeaveTransactionLogSearchModel()
        {
            AvailableLeaveType = new List<SelectListItem>();
            AvailableStatus = new List<SelectListItem>();

        }

        #region Properties
        [NopResourceDisplayName("Admin.Extension.LeaveTransactionLog.List.SearchLeaveManagementName")]
        public string SearchLeaveManagementName { get; set; }
       

        [NopResourceDisplayName("Admin.Extension.LeaveTransactionLog.Fields.SearchEmployeeId")]

        public string EmployeeName { get; set; }


        public int EmployeeId { get; set; }
        [NopResourceDisplayName("Admin.Extension.LeaveTransactionLog.Fields.From")]
        [UIHint("DateNullable")]
        public DateTime? From { get; set; }

        [NopResourceDisplayName("Admin.Extension.LeaveTransactionLog.Fields.To")]
        [UIHint("DateNullable")]
        public DateTime? To { get; set; }

        [NopResourceDisplayName("Admin.Extension.LeaveTransactionLog.Fields.LeaveTypeId")]
        public int SearchLeaveTypeId { get; set; }

        [NopResourceDisplayName("Admin.Extension.LeaveTransactionLog.Fields.SearchLeaveId")]

        public int SearchLeaveId { get; set; }

        [NopResourceDisplayName("Admin.Extension.LeaveTransactionLog.Fields.SearchComment")]

        public string SearchComment { get; set; }

        public string LeaveTypeName { get; set; }
        public IList<SelectListItem> AvailableLeaveType { get; set; }

        [NopResourceDisplayName("Admin.Extension.LeaveTransactionLog.Fields.StatusId")]
        public int StatusId { get; set; }
        public IList<SelectListItem> AvailableStatus { get; set; }

        #endregion
    }
}