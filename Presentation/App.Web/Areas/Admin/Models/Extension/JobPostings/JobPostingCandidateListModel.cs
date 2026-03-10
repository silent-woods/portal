using App.Web.Framework.Models;

namespace App.Web.Areas.Admin.Models.JobPostings
{
    /// <summary>
    /// Represents a synchronization record list model
    /// </summary>
    public record JobPostingCandidateListModel : BasePagedListModel<JobPostingCandidateModel>
    {
    }
}