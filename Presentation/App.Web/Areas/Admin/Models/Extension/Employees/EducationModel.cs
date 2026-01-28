using System;
using System.ComponentModel.DataAnnotations;
using App.Web.Framework.Mvc.ModelBinding;
using App.Web.Framework.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace App.Web.Areas.Admin.Models.Employees

{
    /// <summary>
    /// Represents a education model
    /// </summary>
    public partial record EducationModel : BaseNopEntityModel
    {

        public EducationModel() 
        {
            Employess = new List<SelectListItem>();
            SelectedEmployeeId = new List<int>();
            AvailableEmployees = new List<SelectListItem>();
        }
        #region Properties
      
        [NopResourceDisplayName("Admin.Educations.Fields.EmployeeID")]
        public int EmployeeID { get; set; }

        [NopResourceDisplayName("Admin.Educations.Fields.EmployeeName")]
        public string EmployeeName { get; set; }
        public IList<SelectListItem> Employess { get; set; }

        public IList<SelectListItem> AvailableEmployees { get; set; }


        [NopResourceDisplayName("Admin.Common.Fields.SelectedEmployeeId")]
        public IList<int> SelectedEmployeeId { get; set; }

        [NopResourceDisplayName("Admin.Educations.Fields.Course")]
        [Required(ErrorMessage = "Please enter a course name.")]
        public string Course { get; set; }

        [Required(ErrorMessage = "Please enter a institution name.")]
        [NopResourceDisplayName("Admin.Educations.Fields.InstitutionName")]
        public string InstitutionName { get; set; }

        [Required(ErrorMessage = "Marks Scored must be greater than zero")]
        [Range(0.1, double.MaxValue, ErrorMessage = "Marks Scored must be greater than zero")]
        [NopResourceDisplayName("Admin.Educations.Fields.MarksScored")]
        public decimal MarksScored { get; set; }

        [Required(ErrorMessage = "Please enter a year of completion.")]
        [NopResourceDisplayName("Admin.Educations.Fields.YearOfCompletion")]
        public string YearOfCompletion { get; set; }

        #endregion
    }
}