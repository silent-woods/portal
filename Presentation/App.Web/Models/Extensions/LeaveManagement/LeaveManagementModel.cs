using System;
using App.Web.Framework.Mvc.ModelBinding;
using App.Web.Framework.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using App.Core.Domain.Leaves;



namespace App.Web.Models.Extensions.LeaveManagement
{
    public partial record LeaveManagementModel : BaseNopEntityModel
    {
        public LeaveManagementModel()
        {
            Leave = new List<SelectListItem>();
            SelectedEmployeeId = new List<int>();
            AvailableEmployees = new List<SelectListItem>();
            NoOfDays = 1;
        }

        [NopResourceDisplayName("Admin.Extension.LeaveManagement.Fields.EmployeeName")]
        public string EmployeeName { get; set; }

        [Required(ErrorMessage = "Please select leave type.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select leave type")]
        [NopResourceDisplayName("Admin.Extension.LeaveManagement.Fields.LeaveTypeId")]
        public int LeaveTypeId { get; set; }
        [NopResourceDisplayName("Admin.Extension.LeaveManagement.Fields.LeaveType")]
        public string LeaveType { get; set; }

        public IList<SelectListItem> Leave { get; set; }

        [NopResourceDisplayName("Admin.Extension.LeaveManagement.Fields.From")]
        [UIHint("DateNullable")]

        [Required(ErrorMessage = "Please enter 'From' date.")]
        public DateTime? From { get; set; }

        [NopResourceDisplayName("Admin.Extension.LeaveManagement.Fields.DateFrom")]
        public String DateFrom { get; set; }
        public String DateTo { get; set; }

        [NopResourceDisplayName("Admin.Extension.LeaveManagement.Fields.To")]
        [UIHint("DateNullable")]
        [Required(ErrorMessage = "Please enter 'To' date.")]
        public DateTime? To { get; set; }

        [NopResourceDisplayName("Admin.Extension.LeaveManagement.Fields.ReasonForLeave")]
        [Required(ErrorMessage = "Please enter a reason for leave")]
        public string ReasonForLeave { get; set; }

        [Required(ErrorMessage = "No of Days must Upto zero")]
        [Range(0.1, double.MaxValue, ErrorMessage = "No of Days must be greater than zero")]
        [NopResourceDisplayName("Admin.Extension.LeaveManagement.Fields.NoOfDays")]
        public decimal NoOfDays { get; set; }

        public DateTime CreatedOnUTC { get; set; }
        
        public IList<SelectListItem> AvailableEmployees { get; set; }


        [NopResourceDisplayName("Admin.Common.Fields.SelectedEmployeeIdCC")]
        //[AtLeastOneSelectedAttribute(ErrorMessage = "Please select Employee.")]
        public IList<int> SelectedEmployeeId { get; set; }

        public int StatusId { get; set; }

        [NopResourceDisplayName("Admin.Extension.LeaveManagement.Fields.Status")]
        public string Status { get; set; }

        [NopResourceDisplayName("Admin.Extension.LeaveManagement.Fields.EmployeeId")]
        public int EmployeeId { get; set; }

        public bool IsArchived { get; set; }

        public string SendMailIds { get; set; }


        public IList<SelectListItem> Employee { get; set; }


    }
}
