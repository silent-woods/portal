using System;
using App.Web.Framework.Mvc.ModelBinding;
using App.Web.Framework.Models;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using App.Web.Areas.Admin.Models.ProjectEmployeeMappings;
using App.Web.Areas.Admin.Models.ProjectTaskCategoryMappings;

namespace App.Web.Areas.Admin.Models.Projects
{
    /// <summary>
    /// Represents a Project model
    /// </summary>
    public partial record ProjectModel : BaseNopEntityModel
    {
        public ProjectModel()
        {
            Projects = new List<SelectListItem>();
            ProjectEmployeeMappingModel = new List<ProjectEmployeeMappingModel>();
            projectEmployeeMappingSearchModel = new ProjectEmployeeMappingSearchModel();
            ProjectTaskCategoryMappingModel = new List<ProjectTaskCategoryMappingModel>();
            projectTaskCategoryMappingSearchModel = new ProjectTaskCategoryMappingSearchModel();

            ProjectStatus = new List<SelectListItem>();
            SelectedProjectLeaderId = new List<int>();
            AvailableEmployees = new List<SelectListItem>();
            SelectedProjectManagerId = new List<int>();
            SelectedProcessWorkflowIds= new List<int>();
            AvailableProcessWorkflows = new List<SelectListItem>();


        }
        #region Properties

        [NopResourceDisplayName("Admin.Projects.Fields.ProjectTitle")]
        [Required(ErrorMessage = "Please enter a project title.")]
        public string ProjectTitle { get; set; }


        

        [NopResourceDisplayName("Admin.Projects.Fields.ProjectManagerName")]
        

        public string ProjectManagerName { get; set; }
        public IList<SelectListItem> Projects { get; set; }

        [NopResourceDisplayName("Admin.Projects.Fields.Description")]
        [Required(ErrorMessage = "Please enter a description.")]
        public string Description { get; set; }

        [NopResourceDisplayName("Admin.Projects.Fields.CreateOn")]
        public DateTime CreateOn { get; set; }

        [NopResourceDisplayName("Admin.Projects.Fields.UpdateOn")]
        public DateTime UpdateOn { get; set; }

        [Required(ErrorMessage = "Please select status")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select status")]
        [NopResourceDisplayName("Admin.Common.Fields.StatusId")]

        public int StatusId { get; set; }
        public string Status { get; set; }

        public IList<SelectListItem> ProjectStatus { get; set; }

        public IList<SelectListItem> AvailableEmployees { get; set; }
        [NopResourceDisplayName("Admin.Common.Fields.SelectedProjectManagerId")]

        public IList<int> SelectedProjectManagerId { get; set; }

        [NopResourceDisplayName("Admin.Common.Fields.SelectedProjectLeaderId")]
        
        public IList<int> SelectedProjectLeaderId { get; set; }
      

     public string ProjectLeaderName {  get; set; }
   
        public bool IsDeleted { get; set; }


        public string ProcessWorkflowIds { get; set; }

        [NopResourceDisplayName("Admin.Projects.Fields.SelectedProcessWorkflowIds")]

        public IList<int> SelectedProcessWorkflowIds { get; set; }

        public IList<SelectListItem> AvailableProcessWorkflows { get; set; }

        public IList<ProjectEmployeeMappingModel> ProjectEmployeeMappingModel { get; set; }
        public ProjectEmployeeMappingSearchModel projectEmployeeMappingSearchModel { get; set; }


        public IList<ProjectTaskCategoryMappingModel> ProjectTaskCategoryMappingModel { get; set; }
        public ProjectTaskCategoryMappingSearchModel projectTaskCategoryMappingSearchModel { get; set; }
        #endregion
    }
}