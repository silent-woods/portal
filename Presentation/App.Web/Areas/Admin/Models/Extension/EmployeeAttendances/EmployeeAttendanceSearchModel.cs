using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace App.Web.Areas.Admin.Models.EmployeeAttendances
{
    /// <summary>
    /// Represents a EmployeeAttendance search model
    /// </summary>
    public partial record EmployeeAttendanceSearchModel : BaseSearchModel
    {
        public EmployeeAttendanceSearchModel()
        {
            AvailableStatus = new List<SelectListItem>();
        }

        #region Properties
        [NopResourceDisplayName("Admin.Extension.EmployeeAttendance.Fields.EmployeeName")]
        public string EmployeeName { get; set; }

        [NopResourceDisplayName("Admin.Extension.EmployeeAttendance.Fields.From")]
        [UIHint("DateNullable")]

        public DateTime? From { get; set; }


        [NopResourceDisplayName("Admin.Extension.EmployeeAttendance.Fields.To")]
        [UIHint("DateNullable")]

        public DateTime? To { get; set; }


        [NopResourceDisplayName("Admin.Extension.EmployeeAttendance.Fields.StatusId")]
        public int StatusId { get; set; }

        public IList<SelectListItem> AvailableStatus { get; set; }
        #endregion
    }
}