using App.Web.Areas.Admin.Models.CheckListMappings;
using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace App.Web.Areas.Admin.Models.Extension.TaskCategories
{
    /// <summary>
    /// Represents a Task Category model in Admin
    /// </summary>
    public partial record TaskCategoryModel : BaseNopEntityModel
    {

        public TaskCategoryModel()
        {
            CheckListMappingModel = new List<CheckListMappingModel>();
            checkListMappingSearchModel = new CheckListMappingSearchModel();
        }
        [NopResourceDisplayName("Admin.TaskCategories.Fields.CategoryName")]
        [Required(ErrorMessage = "Category Name is required")]
        public string CategoryName { get; set; }

        [NopResourceDisplayName("Admin.TaskCategories.Fields.Description")]
        public string Description { get; set; }

        [NopResourceDisplayName("Admin.TaskCategories.Fields.IsActive")]
        public bool IsActive { get; set; }

        [NopResourceDisplayName("Admin.TaskCategories.Fields.CreatedOn")]
        public DateTime CreatedOn { get; set; }

        [NopResourceDisplayName("Admin.TaskCategories.Fields.DisplayName")]
        [Required(ErrorMessage = "Display Name is required")]

        public string DisplayName { get; set; }



        public IList<CheckListMappingModel> CheckListMappingModel { get; set; }
        public CheckListMappingSearchModel checkListMappingSearchModel { get; set; }
    }
}
