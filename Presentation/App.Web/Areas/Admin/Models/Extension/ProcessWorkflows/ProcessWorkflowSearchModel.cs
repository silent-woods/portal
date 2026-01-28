using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;

namespace App.Web.Areas.Admin.Models.Extension.ProcessWorkflows
{
    public partial record ProcessWorkflowSearchModel : BaseSearchModel
    {
        public ProcessWorkflowSearchModel() {
          
        }
        #region Properties

        [NopResourceDisplayName("Admin.ProcessWorkflows.Fields.SearchProcessWorkflowName")]

        public string SearchProcessWorkflowName { get; set; }

      
        #endregion
    }
}
