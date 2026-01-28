using App.Web.Framework.Models;

namespace App.Web.Areas.Admin.Models.JobPostings
{
    /// <summary>
    /// Represents a timesheet list model
    /// </summary>
    public partial record JobPostingListModel : BasePagedListModel<JobPostingModel>
    {
    }
}