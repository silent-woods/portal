using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace App.Web.Areas.Admin.Models.Extension.TaskCategories
{
    /// <summary>
    /// Represents a Task Category search model
    /// </summary>
    public partial record TaskCategorySearchModel : BaseSearchModel
    {
           [NopResourceDisplayName("Admin.TaskCategories.Fields.SearchCategoryName")]
        
        public string SearchCategoryName { get; set; }

    }
}
