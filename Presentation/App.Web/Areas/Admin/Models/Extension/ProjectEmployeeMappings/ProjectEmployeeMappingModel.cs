using System;
using App.Web.Framework.Mvc.ModelBinding;
using App.Web.Framework.Models;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using FluentMigrator.Infrastructure;
using System.ComponentModel.DataAnnotations;
using App.Core.Domain.Projects;

namespace App.Web.Areas.Admin.Models.ProjectEmployeeMappings
{
    /// <summary>
    /// Represents a Project model
    /// </summary>
    public partial record ProjectEmployeeMappingModel : BaseNopEntityModel
    {
        public ProjectEmployeeMappingModel()
        {
            Projectsemp = new List<SelectListItem>();
            Projects = new List<SelectListItem>();
            Roles = new List<SelectListItem>();
            SelectedEmployeeId = new List<int>();
            AvailableEmployees = new List<SelectListItem>();
        }
        #region Properties

        public bool ProjectEmp { get; set; }
        [NopResourceDisplayName("Admin.ProjectEmployeeMapping.Fields.ProjectId")]
        [Required(ErrorMessage = "Please select Project Name.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select Project Name.")]
        public int ProjectId { get; set; }
        [NopResourceDisplayName("Admin.ProjectEmployeeMapping.Fields.ProjectName")]
        public string ProjectName { get; set; }
        public IList<SelectListItem> Projects { get; set; }

        [NopResourceDisplayName("Admin.ProjectEmployeeMapping.Fields.IsActive")]
        public bool IsActive { get; set; }



        [NopResourceDisplayName("Admin.ProjectEmployeeMapping.Fields.EmployeeId")]
        public int EmployeeId { get; set; }
        [NopResourceDisplayName("Admin.ProjectEmployeeMapping.Fields.EmployeeName")]
        public string EmployeeName { get; set; }
        public IList<SelectListItem> Projectsemp { get; set; }

        [Required(ErrorMessage = "Please select Role.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select Role.")]
        [NopResourceDisplayName("Admin.ProjectEmployeeMapping.Fields.RoleId")]
        public int RoleId { get; set; }
        [NopResourceDisplayName("Admin.ProjectEmployeeMapping.Fields.Role")]
        public string Role { get; set; }
        public IList<SelectListItem> Roles { get; set; }

        [NopResourceDisplayName("Admin.ProjectEmployeeMapping.Fields.CreateOn")]
        public DateTime CreateOn { get; set; }

        public ProjectEmployeeMappingSearchModel projectEmployeeMappingSearchModel { get; set; }
        public IList<SelectListItem> AvailableEmployees { get; set; }


        [NopResourceDisplayName("Admin.Common.Fields.SelectedEmployeeId")]
        //[AtLeastOneSelectedAttribute(ErrorMessage = "Please select Employee.")]
        public IList<int> SelectedEmployeeId { get; set; }


        #endregion
    }
}