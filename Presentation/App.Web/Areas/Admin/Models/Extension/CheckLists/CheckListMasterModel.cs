using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;
using System;
using System.ComponentModel.DataAnnotations;

namespace App.Web.Areas.Admin.Models.Extension.CheckLists
{
    /// <summary>
    /// Represents a CheckList Master model in Admin
    /// </summary>
    public partial record CheckListMasterModel : BaseNopEntityModel
    {
        [NopResourceDisplayName("Admin.CheckLists.Fields.Title")]
        [Required(ErrorMessage = "Title is required")]
        public string Title { get; set; }

        [NopResourceDisplayName("Admin.CheckLists.Fields.Description")]
        public string Description { get; set; }

        [NopResourceDisplayName("Admin.CheckLists.Fields.IsActive")]
        public bool IsActive { get; set; }

        [NopResourceDisplayName("Admin.CheckLists.Fields.CreatedOn")]
        public DateTime CreatedOn { get; set; }
    }
}
