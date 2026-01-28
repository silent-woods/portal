using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;

namespace App.Web.Areas.Admin.Models.Extension.WorkflowStatus
{
    public partial record WorkflowStatusSearchModel : BaseSearchModel
    {
        public WorkflowStatusSearchModel() {
          
        }
        #region Properties

      

        public int ProcessWorkflowId { get; set; }

        public string SearchStatusName { get; set; }



        #endregion
    }
}
