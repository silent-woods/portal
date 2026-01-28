using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;
using System;
using System.ComponentModel.DataAnnotations;
using App.Web.Controllers;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace App.Web.Models.Extensions.LeaveManagement
{
    public partial record LeaveManagementSearchModel : BaseSearchModel

    {
        #region Properties

            public LeaveManagementSearchModel()
            {
                AvailableLeaveTypes = new List<SelectListItem>();
            AvailableStatusId = new List<SelectListItem>();
            SelectedEmployeeId = new List<int>();
            AvailableEmployees = new List<SelectListItem>();
            PeriodList = new List<SelectListItem>();
            //LeaveSummery = new List<LeaveTypeModel>();
        }

        [NopResourceDisplayName("MyAccount.Extension.LeaveManagement.Fields.LeaveTypeId")]

        public int LeaveTypeId { get; set; }
            public int CurrentCustomer { get; set; }
            public string LeaveName { get; set; }
        [NopResourceDisplayName("MyAccount.Extension.LeaveManagement.Fields.From")]

        public DateTime? From { get; set; }
        [NopResourceDisplayName("MyAccount.Extension.LeaveManagement.Fields.To")]

        public DateTime? To { get; set; }

        [NopResourceDisplayName("MyAccount.Extension.LeaveManagement.Fields.StatusId")]

        public int StatusId { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalRecords { get; set; }

        [NopResourceDisplayName("Admin.TimesheetReports.Fields.SearchPeriodId")]

        public int SearchPeriodId { get; set; }

        public List<SelectListItem> PeriodList { get; set; }

        public IList<SelectListItem> AvailableEmployees { get; set; }


        [NopResourceDisplayName("Admin.Common.Fields.SelectedEmployeeId")]
        //[AtLeastOneSelectedAttribute(ErrorMessage = "Please select Employee.")]
        public IList<int> SelectedEmployeeId { get; set; }
        public IList<SelectListItem> AvailableLeaveTypes { get; set; }

        public IList<SelectListItem> AvailableStatusId { get; set; }


        public IList<LeaveManagementModel> LeaveManagements { get; set; }



        #endregion


    }
}
