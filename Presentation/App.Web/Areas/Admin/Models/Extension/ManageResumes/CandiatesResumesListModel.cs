using App.Web.Framework.Models;

namespace App.Web.Areas.Admin.Models.ManageResumes
{
    /// <summary>
    /// Represents a timesheet list model
    /// </summary>candiatesResumesModel
    public partial record CandiatesResumesListModel : BasePagedListModel<CandiatesResumesModel>
    {
    }
}