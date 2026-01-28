using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;



namespace App.Web.Areas.Admin.Models.Extension.WorkflowStatus
{
    public partial record WorkflowStatusModel : BaseNopEntityModel
    {

    
       public WorkflowStatusModel()
        {
        
        }
        public int ProcessWorkflowId { get; set; }

        [Required(ErrorMessage = "Status Name Is Required")]
        [NopResourceDisplayName("Admin.WorkflowStatus.Fields.StatusName")]

        public string StatusName { get; set; }

        [NopResourceDisplayName("Admin.WorkflowStatus.Fields.IsDefaultDeveloperStatus")]
        public bool IsDefaultDeveloperStatus { get; set; }

        [NopResourceDisplayName("Admin.WorkflowStatus.Fields.IsDefaultQAStatus")]
        public bool IsDefaultQAStatus { get; set; }


        [NopResourceDisplayName("Admin.WorkflowStatus.Fields.ColorCode")]

        public string ColorCode { get; set; }

        [NopResourceDisplayName("Admin.WorkflowStatus.Fields.DisplayOrder")]

        public int DisplayOrder { get; set; }

        public string ProcessWorkflowName { get; set; }


        public DateTime CreatedOn { get; set; }
    }
}
