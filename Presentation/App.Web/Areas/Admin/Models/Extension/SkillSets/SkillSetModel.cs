using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace App.Web.Areas.Admin.Models.Extension.SkillSet
{
    public partial record SkillSetModel : BaseNopEntityModel
    {
        public SkillSetModel()
        {
            SelectedTechnologyIds = new List<int>();
            AvailableTechnologies = new List<SelectListItem>();
        }
        [NopResourceDisplayName("Admin.SkillSet.Fields.SkillTags")]
        public string SkillTags { get; set; }

        [NopResourceDisplayName("Admin.SkillSet.Fields.Name")]
        public string Name { get; set; }
        [NopResourceDisplayName("Admin.SkillSet.Fields.DisplayOrder")]
        [Range(0, int.MaxValue, ErrorMessage = "Display order must be 0 or greater")]
        public int DisplayOrder { get; set; }
        [NopResourceDisplayName("Admin.SkillSet.Fields.Published")]
        public bool Published { get; set; }
        [NopResourceDisplayName("Admin.SkillSet.Fields.TechnologyId")]
        [MinLength(1, ErrorMessage = "Please select at least one technology")]
        public List<int> SelectedTechnologyIds { get; set; }
        public string TechnologyName { get; set; }
        public List<SelectListItem> AvailableTechnologies { get; set; }
        public DateTime CreatedOnUtc { get; set; }

        public DateTime UpdatedOnUtc { get; set; }
    }
}
