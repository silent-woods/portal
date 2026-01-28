using App.Web.Framework.Mvc.ModelBinding;
using App.Web.Framework.Models;
using System;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using FluentMigrator.Infrastructure;

namespace App.Web.Areas.Admin.Models.LeaveManagement
{
    /// <summary>
    /// Represents a LeaveManagement model
    /// </summary>
    public partial record LeaveManagementModel : BaseNopEntityModel
    {
        public LeaveManagementModel()
        {
            Leave = new List<SelectListItem>();
            Employee = new List<SelectListItem>();
            ApprovedStatus = new List<SelectListItem>();
            SelectedEmployeeId = new List<int>();
            AvailableEmployees = new List<SelectListItem>();

            SelectedEmployeeIdForEmail = new List<int>();
        }
        #region Properties

       
        [NopResourceDisplayName("Admin.Extension.LeaveManagement.Fields.EmployeeId")]
       
        public int EmployeeId { get; set; }
       
        [NopResourceDisplayName("Admin.Extension.LeaveManagement.Fields.EmployeeName")]
      
        public string EmployeeName { get; set; }
       
        public IList<SelectListItem>Employee { get; set; }


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

        //[Required(ErrorMessage = "No of Days must be greater than zero")]
        //[Range(0.1, double.MaxValue, ErrorMessage = "No of Days must be greater than zero")]
        [NopResourceDisplayName("Admin.Extension.LeaveManagement.Fields.NoOfDays")]
        public decimal NoOfDays { get; set; }
              
        [NopResourceDisplayName("Admin.Extension.LeaveManagement.Fields.Status")]
        public string Status { get; set; }
        public IList<SelectListItem> ApprovedStatus { get; set; }

        [Required(ErrorMessage = "Please select status")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select status")]
        public int StatusId { get; set; }

        [NopResourceDisplayName("Admin.Extension.LeaveManagement.Fields.ApprovedId")]
        public int ApprovedId { get; set; }

        public bool IsArchived { get; set; }
        public decimal LeaveBalance { get; set; }



        [NopResourceDisplayName("Admin.Extension.LeaveManagement.Fields.ApprovedOnUTC")]
        public DateTime? ApprovedOnUTC { get; set; }

        [NopResourceDisplayName("Admin.Extension.LeaveManagement.Fields.CreatedOnUTC")]
        public DateTime CreatedOnUTC { get; set; }


        public string SendMailIds { get; set; }

        [NopResourceDisplayName("Admin.Extension.LeaveManagement.Fields.SelectedEmployeeIdForEmail")]

        public IList<int> SelectedEmployeeIdForEmail { get; set; }

        public IList<SelectListItem> AvailableEmployees { get; set; }


        [NopResourceDisplayName("Admin.Common.Fields.SelectedEmployeeId")]
        //[AtLeastOneSelectedAttribute(ErrorMessage = "Please select Employee.")]
        public IList<int> SelectedEmployeeId { get; set; }
        #endregion
    }
}