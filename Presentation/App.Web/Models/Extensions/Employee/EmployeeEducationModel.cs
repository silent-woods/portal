using System;
using System.ComponentModel.DataAnnotations;
using App.Web.Framework.Mvc.ModelBinding;
using App.Web.Framework.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using App.Core.Domain.Employees;

namespace App.Web.Models.Employee

{
    /// <summary>
    /// Represents a education model
    /// </summary>
    public partial record EmployeeEducationModel : BaseNopEntityModel
    {

        public EmployeeEducationModel() 
        {
            Educations = new List<EmployeeEducationModel>();
            Employess = new List<SelectListItem>();
        }
        #region Properties

        [NopResourceDisplayName("Account.Educations.Fields.EmployeeID")]
        public int EmployeeID { get; set; }

        [NopResourceDisplayName("Account.Educations.Fields.EmployeeName")]
        public string EmployeeName { get; set; }
        public IList<SelectListItem> Employess { get; set; }
        [NopResourceDisplayName("Account.Educations.Fields.Course")]
        [Required(ErrorMessage = "Please enter a course name.")]
        public string Course { get; set; }

        [NopResourceDisplayName("Account.Educations.Fields.InstitutionName")]
        public string InstitutionName { get; set; }

        [Required(ErrorMessage = "Marks Scored must be greater than zero")]
        [Range(0.1, double.MaxValue, ErrorMessage = "Marks Scored must be greater than zero")]
        [NopResourceDisplayName("Account.Educations.Fields.MarksScored")]
        public decimal MarksScored { get; set; }

        [NopResourceDisplayName("Account.Educations.Fields.YearOfCompletion")]
        public string YearOfCompletion { get; set; }

        public IList<EmployeeEducationModel> Educations { get; set; }

        #endregion
    }
}