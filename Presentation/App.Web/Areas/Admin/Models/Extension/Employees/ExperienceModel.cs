using System;
using App.Web.Framework.Mvc.ModelBinding;
using App.Web.Framework.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace App.Web.Areas.Admin.Models.Employees

{
    /// <summary>
    /// Represents a Experience model
    /// </summary>
    public partial record ExperienceModel : BaseNopEntityModel
    {

        public ExperienceModel()
        {
            Employess = new List<SelectListItem>();
            SelectedEmployeeId = new List<int>();
            AvailableEmployees = new List<SelectListItem>();
        }
        #region Properties

        [NopResourceDisplayName("Admin.Employee.Experience.Fields.EmployeeID")]
        public int EmployeeID { get; set; }


        [NopResourceDisplayName("Admin.Employee.Experience.Fields.EmployeeName")]
        public string EmployeeName { get; set; }
        public IList<SelectListItem> Employess { get; set; }


        [Required(ErrorMessage = "Please select Previous Company Name.")]
        [NopResourceDisplayName("Admin.Employee.Experience.Fields.PreviousCompanyName")]
        public string PreviousCompanyName { get; set; }

        [Required(ErrorMessage = "Please select Designation.")]
        [NopResourceDisplayName("Admin.Employee.Experience.Fields.Designation")]
        public string Designation { get; set; }

        public IList<SelectListItem> AvailableEmployees { get; set; }


        [NopResourceDisplayName("Admin.Common.Fields.SelectedEmployeeId")]
        public IList<int> SelectedEmployeeId { get; set; }

        [NopResourceDisplayName("Admin.Employee.Experience.Fields.From")]
        [UIHint("DateNullable")]
        [Required(ErrorMessage = "Please select From Date.")]
        public DateTime? From { get; set; }
        public string Froms { get; set; }


      
        [NopResourceDisplayName("Admin.Employee.Experience.Fields.To")]
        [UIHint("DateNullable")]
        [Required(ErrorMessage = "Please select To Date.")]
        public DateTime? To { get; set; }
        public string Tos { get; set; }

        #endregion




    }
}