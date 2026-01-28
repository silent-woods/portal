using App.Web.Areas.Admin.Models.Projects;
using App.Web.Framework.Models;
using System.Collections.Generic;

namespace App.Web.Models.Extensions.ProjectTasks

{
    /// <summary>
    /// Represents a project list model
    /// </summary>
    public partial record ProjectTaskListModel : BasePagedListModel<ProjectTaskModel>
    {
    
    }
}

