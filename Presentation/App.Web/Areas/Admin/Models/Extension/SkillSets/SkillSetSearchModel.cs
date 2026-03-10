using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace App.Web.Areas.Admin.Models.Extension.SkillSet
{
    public partial record SkillSetSearchModel : BaseSearchModel
    {
        public SkillSetSearchModel()
        {
            AvailableTechnologies = new List<SelectListItem>();
        }
        #region Properties

        [NopResourceDisplayName("Admin.SkillSetSearchModel.Fields.SearchName")]
        public string SearchName { get; set; }
        [NopResourceDisplayName("Admin.SkillSetSearchModel.Fields.SearchTechnologyId")]
        public int SearchTechnologyId { get; set; }
        public List<SelectListItem> AvailableTechnologies { get; set; }
        #endregion 
    }
}
