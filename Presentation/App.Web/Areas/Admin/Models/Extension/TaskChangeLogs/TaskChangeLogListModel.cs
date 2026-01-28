using App.Web.Areas.Admin.Models.Extension.TaskChangeLogs;
using App.Web.Areas.Admin.Models.Projects;
using App.Web.Framework.Models;
using System.Collections.Generic;

namespace App.Web.Areas.Admin.Models.Extension.TaskChangeLogs

{
    /// <summary>
    /// Represents a project list model
    /// </summary>
    public partial record TaskChangeLogListModel : BasePagedListModel<TaskChangeLogModel>
    {
    
    }
}

