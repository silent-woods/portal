using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace App.Web.Areas.Admin.Models.ProjectTaskCategoryMappings
{
    public partial record ProjectTaskCategoryMappingSearchModel : BaseSearchModel
    {
      

        public int ProjectId { get; set; }

      
    }
}
