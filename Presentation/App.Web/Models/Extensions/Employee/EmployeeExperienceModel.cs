using System;
using App.Web.Framework.Mvc.ModelBinding;
using App.Web.Framework.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace App.Web.Models.Employee

{
    /// <summary>
    /// Represents a Experience model
    /// </summary>
    public partial record EmployeeExperienceModel : BaseNopEntityModel
    {

        public EmployeeExperienceModel()
        {
            Experiences = new List<EmployeeExperienceModel>();
            Employess = new List<SelectListItem>();
        }
        #region Properties

        [NopResourceDisplayName("Account.Employee.Experience.Fields.EmployeeID")]
        public int EmployeeID { get; set; }

        [NopResourceDisplayName("Account.Employee.Experience.Fields.EmployeeName")]
        public string EmployeeName { get; set; }
        public IList<SelectListItem> Employess { get; set; }

        [NopResourceDisplayName("Account.Employee.Experience.Fields.PreviousCompanyName")]
        public string PreviousCompanyName { get; set; }

        [NopResourceDisplayName("Account.Employee.Experience.Fields.Designation")]
        public string Designation { get; set; }

        [NopResourceDisplayName("Account.Employee.Experience.Fields.From")]
        [UIHint("DateNullable")]
        public DateTime From { get; set; }
        public string Froms { get; set; }

        [NopResourceDisplayName("Account.Employee.Experience.Fields.To")]
        [UIHint("DateNullable")]
        public DateTime To { get; set; }
        public string Tos { get; set; }

        public IList<EmployeeExperienceModel> Experiences { get; set; }

        #endregion
    }
}