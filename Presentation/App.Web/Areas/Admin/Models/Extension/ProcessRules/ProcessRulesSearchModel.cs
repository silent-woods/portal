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
          
        }
        #region Properties


        public int ProcessWorkflowId { get; set; }



        #endregion
    }
}
