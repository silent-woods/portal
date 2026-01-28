using App.Web.Areas.Admin.Models.Extension.ProcessWorkflows;
using App.Web.Areas.Admin.Models.Projects;
using App.Web.Framework.Models;
using System.Collections.Generic;

namespace App.Web.Areas.Admin.Models.Extension.ProcessWorkflows

{
    /// <summary>
    /// Represents a project list model
    /// </summary>
    public partial record ProcessWorkflowListModel : BasePagedListModel<ProcessWorkflowModel>
    {
    
    }
}

