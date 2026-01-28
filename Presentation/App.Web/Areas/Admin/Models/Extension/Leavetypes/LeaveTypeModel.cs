using App.Web.Framework.Mvc.ModelBinding;
using App.Web.Framework.Models;
using System;
using System.ComponentModel.DataAnnotations;

namespace App.Web.Areas.Admin.Models.Leavetypes
{
    /// <summary>
    /// Represents a Leavetype model
    /// </summary>
    public partial record LeaveTypeModel : BaseNopEntityModel
    {
        #region Properties

        [NopResourceDisplayName("Admin.Extension.Leavetypes.Fields.Type")]
        [Required(ErrorMessage = "Please enter a type.")]
        public string Type { get; set; }

        [NopResourceDisplayName("Admin.Extension.Leavetypes.Fields.Description")]
        public string Description { get; set; }

        [NopResourceDisplayName("Admin.Extension.Leavetypes.Fields.CreateOn")]
        public DateTime CreateOn { get; set; }

        [NopResourceDisplayName("Admin.Extension.Leavetypes.Fields.UpdateOn")]
        public DateTime UpdateOn { get; set; }

        [NopResourceDisplayName("Admin.Extension.Leavetypes.Fields.Total_Allowed")]
        public int Total_Allowed {  get; set; }

        public int TakenLeave { get; set; }
        public int RemainingLeave { get; set; }

        #endregion
    }
}