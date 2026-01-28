using System;
using System.ComponentModel.DataAnnotations;
using App.Web.Framework.Mvc.ModelBinding;
using App.Web.Framework.Models;

namespace App.Web.Areas.Admin.Models.Departments

{
    /// <summary>
    /// Represents a department model
    /// </summary>
    public partial record DepartmentModel : BaseNopEntityModel
    {
        #region Properties

        [NopResourceDisplayName("Admin.Department.Fields.Name")]
        [Required(ErrorMessage = "Please enter a name.")]
        public string Name { get; set; }

        [NopResourceDisplayName("Admin.Department.Fields.CreatedOnUtc")]
        public DateTime CreatedOnUtc { get; set; }

        [NopResourceDisplayName("Admin.Department.Fields.UpdatedOnUtc")]
        public DateTime UpdatedOnUtc { get; set; }

        #endregion
    }
}