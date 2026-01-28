using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace App.Web.Areas.Admin.Models.CheckListMappings
{
    /// <summary>
    /// Represents a checklist mapping model (for create/edit views)
    /// </summary>
    public partial record CheckListMappingModel : BaseNopEntityModel
    {
        public CheckListMappingModel()
        {
            AvailableTaskCategories = new List<SelectListItem>();
            AvailableStatuses = new List<SelectListItem>();
            AvailableCheckLists = new List<SelectListItem>();
            AvailableProcessWorkflows = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Admin.CheckListMapping.TaskCategory")]
        public int TaskCategoryId { get; set; }
        public IList<SelectListItem> AvailableTaskCategories { get; set; }

        [NopResourceDisplayName("Admin.CheckListMapping.Status")]
        public int StatusId { get; set; }
        public IList<SelectListItem> AvailableStatuses { get; set; }

        [NopResourceDisplayName("Admin.CheckListMapping.CheckList")]
        public int CheckListId { get; set; }
        public IList<SelectListItem> AvailableCheckLists { get; set; }

        [NopResourceDisplayName("Admin.CheckListMapping.IsMandatory")]
        public bool IsMandatory { get; set; }

        [NopResourceDisplayName("Admin.CheckListMapping.OrderBy")]
        public int OrderBy { get; set; }

        public string  TaskCategoryName { get; set; }

        public string  StatusName { get; set; }

        [NopResourceDisplayName("Admin.CheckListMapping.CheckListName")]
        [Required(ErrorMessage = "Please Enter Checklist Name")]
        public string  CheckListName { get; set; }

        [NopResourceDisplayName("Admin.CheckListMapping.ProcessWorkflowId")]
        public int ProcessWorkflowId { get; set; }

        public IList<SelectListItem> AvailableProcessWorkflows { get; set; }

    }
}
