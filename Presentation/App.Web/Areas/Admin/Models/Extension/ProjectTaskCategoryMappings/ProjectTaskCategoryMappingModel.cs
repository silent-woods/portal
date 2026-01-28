using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace App.Web.Areas.Admin.Models.ProjectTaskCategoryMappings
{
    public partial record ProjectTaskCategoryMappingModel : BaseNopEntityModel
    {
        public ProjectTaskCategoryMappingModel()
        {
            AvailableProjects = new List<SelectListItem>();
            AvailableTaskCategories = new List<SelectListItem>();        
        }

        [NopResourceDisplayName("Admin.Catalog.ProjectTaskCategoryMapping.Fields.Project")]
        public int ProjectId { get; set; }
        public string ProjectName { get; set; }
        public IList<SelectListItem> AvailableProjects { get; set; }

        [NopResourceDisplayName("Admin.Catalog.ProjectTaskCategoryMapping.Fields.TaskCategory")]
        [Required(ErrorMessage = "Please Select Task Category")]
        [Range(1, int.MaxValue, ErrorMessage = "Please Select Task Category")]
        public int TaskCategoryId { get; set; }
        public string TaskCategoryName { get; set; }
        public IList<SelectListItem> AvailableTaskCategories { get; set; }

        [NopResourceDisplayName("Admin.Catalog.ProjectTaskCategoryMapping.Fields.IsActive")]
        public bool IsActive { get; set; }

        [NopResourceDisplayName("Admin.Catalog.ProjectTaskCategoryMapping.Fields.OrderBy")]
        public int OrderBy { get; set; }

        [NopResourceDisplayName("Admin.ProjectTaskCategoryMapping.SelectSourceProject")]
        public int SourceProjectId { get; set; }

        public int TargetProjectId { get; set; }

    }
}
