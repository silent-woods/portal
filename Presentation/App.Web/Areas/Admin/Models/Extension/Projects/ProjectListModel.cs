using App.Web.Framework.Models;

namespace App.Web.Areas.Admin.Models.Projects
{
    /// <summary>
    /// Represents a project list model
    /// </summary>
    public partial record ProjectListModel : BasePagedListModel<ProjectModel>
    {
    }
}