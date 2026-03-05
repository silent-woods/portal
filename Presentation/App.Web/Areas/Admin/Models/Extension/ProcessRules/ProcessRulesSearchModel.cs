using App.Web.Areas.Admin.Models.Extension.WorkflowStatus;
using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;

namespace App.Web.Areas.Admin.Models.Extension.ProcessRules
{
    public partial record ProcessRulesSearchModel : BaseSearchModel
    {
        public ProcessRulesSearchModel() {
            AvailableStatuses = new List<WorkflowStatusModel>();
        }
        #region Properties

        public int ProcessWorkflowId { get; set; }

        public int FromStateId { get; set; }

        public int ToStateId { get; set; }

        public IList<WorkflowStatusModel> AvailableStatuses { get; set; }

        #endregion
    }
}
