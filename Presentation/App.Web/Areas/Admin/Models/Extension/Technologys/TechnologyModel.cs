using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;
using System;
using System.ComponentModel.DataAnnotations;

namespace App.Web.Areas.Admin.Models.Extension.Technologys
{
    public partial record TechnologyModel : BaseNopEntityModel
    {
        public TechnologyModel()
        {

        }

        [NopResourceDisplayName("Admin.Technology.Fields.Name")]
        [Required(ErrorMessage = "Please enter a technology name.")]
        public string Name { get; set; }
        [NopResourceDisplayName("Admin.Technology.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }
        [NopResourceDisplayName("Admin.Technology.Fields.Published")]
        public bool Published { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public DateTime UpdatedOnUtc { get; set; }
    }
}
