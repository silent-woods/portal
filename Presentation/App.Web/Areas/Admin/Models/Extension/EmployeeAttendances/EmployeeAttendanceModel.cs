using System;
using App.Web.Framework.Mvc.ModelBinding;
using App.Web.Framework.Models;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace App.Web.Areas.Admin.Models.EmployeeAttendances
{
    /// <summary>
    /// Represents a EmployeeAttendance model
    /// </summary>
    public partial record EmployeeAttendanceModel : BaseNopEntityModel
    {
        public EmployeeAttendanceModel()
        {
            Employee = new List<SelectListItem>();
            Status = new List<SelectListItem>();

            SelectedEmployeeId = new List<int>();
            AvailableEmployees = new List<SelectListItem>();
        }
        #region Properties

        [NopResourceDisplayName("Admin.EmployeeAttendanceModel.Fields.EmployeeId")]
  
        public int EmployeeId { get; set; }

        [NopResourceDisplayName("Admin.EmployeeAttendanceModel.Fields.EmployeeName")]
        public string EmployeeName { get; set; }
        public IList<SelectListItem> Employee { get; set; }

        [UIHint("DateTimeNullable")]
        [Required(ErrorMessage = "Please select First Check in.")]
        [NopResourceDisplayName("Admin.EmployeeAttendanceModel.Fields.CheckIn")]
        public DateTime? CheckIn { get; set; }

        [NopResourceDisplayName("Admin.EmployeeAttendanceModel.Fields.CheckOut")]
        [UIHint("DateTimeNullable")]
        [Required(ErrorMessage = "Please select First Check out.")]
        public DateTime? CheckOut { get; set; }

        [NopResourceDisplayName("Admin.EmployeeAttendanceModel.Fields.SpentHours")]
        public int SpentHours { get; set; }

        public int SpentMinutes { get; set; }

        [NopResourceDisplayName("Admin.EmployeeAttendanceModel.Fields.SpentTime")]

        public string Times { get; set; }

        [Required(ErrorMessage = "Please select Status.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select Status.")]
        [NopResourceDisplayName("Admin.EmployeeAttendanceModel.Fields.StatusId")]
        public int StatusId { get; set; }

        [NopResourceDisplayName("Admin.EmployeeAttendanceModel.Fields.StatusName")]
        public string StatusName { get; set; }
        public IList<SelectListItem> Status { get; set; }

        [NopResourceDisplayName("Admin.EmployeeAttendanceModel.Fields.CreateOn")]
        public DateTime CreateOn { get; set; }

        [NopResourceDisplayName("Admin.EmployeeAttendanceModel.Fields.UpdateOn")]
        public DateTime UpdateOn { get; set; }

        public IList<SelectListItem> AvailableEmployees { get; set; }


        [NopResourceDisplayName("Admin.Common.Fields.SelectedEmployeeId")]

        public IList<int> SelectedEmployeeId { get; set; }

   

        #endregion
    }
}

