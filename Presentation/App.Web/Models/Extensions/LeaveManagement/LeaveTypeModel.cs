using System;
using App.Web.Framework.Mvc.ModelBinding;
using App.Web.Framework.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using App.Core.Domain.Leaves;

namespace App.Web.Models.Extensions.LeaveManagement
{
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
        public int Total_Allowed { get; set; }

        public string LeaveTypeName { get; set; }
        public decimal TotalLeaves { get; set; }
        public decimal TakenLeaves { get; set; }
        public decimal RemainingLeaves { get; set; }

        #endregion
    }
}

