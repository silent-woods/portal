using App.Web.Areas.Admin.Models.Extension.WorkflowStatus;
using App.Web.Areas.Admin.Models.ProjectEmployeeMappings;
using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;



namespace App.Web.Areas.Admin.Models.Extension.ProcessRules
{
    public partial record ProcessRulesModel : BaseNopEntityModel
    {

    
       public ProcessRulesModel()
        {

            StateList = new List<SelectListItem>();


        }

        public int ProcessWorkflowId { get; set; }

        [Required(ErrorMessage = "Please select From State")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select From State")]
        [NopResourceDisplayName("Admin.ProcessRules.Fields.FromStateId")]

        public int FromStateId { get; set; }


        [Required(ErrorMessage = "Please select To State")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select To State")]
        [NopResourceDisplayName("Admin.ProcessRules.Fields.ToStateId")]

        public int ToStateId { get; set; }

    

        public string ProcessWorkflowName { get; set; }

      


        public string FromStateName { get; set; }

        public string ToStateName { get; set; }




        public IList<SelectListItem> StateList { get; set; }

        [NopResourceDisplayName("Admin.ProcessRules.Fields.IsCommentRequired")]

        public bool IsCommentRequired { get; set; }

        [NopResourceDisplayName("Admin.ProcessRules.Fields.IsActive")]

        public bool IsActive { get; set; }


        [NopResourceDisplayName("Admin.ProcessRules.Fields.CommentTemplate")]

        public string CommentTemplate { get; set; }


        public DateTime CreatedOn { get; set; }

    }
}
