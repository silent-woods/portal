using App.Web.Areas.Admin.Models.Extension.ProcessRules;
using App.Web.Areas.Admin.Models.Extension.WorkflowStatus;
using App.Web.Areas.Admin.Models.ProjectEmployeeMappings;
using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;



namespace App.Web.Areas.Admin.Models.Extension.ProcessWorkflows
{
    public partial record ProcessWorkflowModel : BaseNopEntityModel
    {

    
       public ProcessWorkflowModel()
        {
            WorkflowStatusModel = new List<WorkflowStatusModel>();
            workflowStatusSearchModel = new WorkflowStatusSearchModel();

            ProcessRulesModel = new List<ProcessRulesModel>();
            processRulesSearchModel = new ProcessRulesSearchModel();


        }
        [NopResourceDisplayName("Admin.ProcessWorkflows.Fields.Name")]
        [Required(ErrorMessage = "Name Is Required")]

        public string Name { get; set; }
        [NopResourceDisplayName("Admin.ProcessWorkflows.Fields.IsActive")]

        public bool IsActive { get; set; }

        [NopResourceDisplayName("Admin.ProcessWorkflows.Fields.Description")]

        public string Description { get; set; }

        [NopResourceDisplayName("Admin.ProcessWorkflows.Fields.DisplayOrder")]

        public int DisplayOrder { get; set; }

        public DateTime CreatedOn { get; set; }

        public IList<WorkflowStatusModel> WorkflowStatusModel { get; set; }

        public WorkflowStatusSearchModel workflowStatusSearchModel { get; set; }

        public IList<ProcessRulesModel> ProcessRulesModel { get; set; }

        public ProcessRulesSearchModel processRulesSearchModel { get; set; }

    }
}
