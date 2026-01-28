using System;
using App.Web.Framework.Mvc.ModelBinding;
using App.Web.Framework.Models;
using System.ComponentModel.DataAnnotations;

namespace App.Web.Areas.Admin.Models.Designation
{
    /// <summary>
    /// Represents a Designation model
    /// </summary>
    public partial record DesignationModel : BaseNopEntityModel
    {
        #region Properties

        [NopResourceDisplayName("Admin.Extension.Designation.Fields.Name")]
        [Required(ErrorMessage = "Please enter a name.")]
        public string Name { get; set; }

        [NopResourceDisplayName("Admin.Extension.Designation.Fields.CreateOn")]
        public DateTime CreateOn { get; set; }

        [NopResourceDisplayName("Admin.Extension.Designation.Fields.UpdateOn")]
        public DateTime UpdateOn { get; set; }

        [NopResourceDisplayName("Admin.Extension.Designation.Fields.CanGiveRatings")]
        public bool CanGiveRatings { get; set; }

        #endregion
    }
}