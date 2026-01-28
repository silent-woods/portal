using App.Web.Framework.Models;

namespace App.Web.Areas.Admin.Models.ProjectEmployeeMappings
{
    /// <summary>
    /// Represents a Projectemployeemapping list model
    /// </summary>
    public partial record ProjectEmployeeMappingListModel : BasePagedListModel<ProjectEmployeeMappingModel>
    {
    }
}