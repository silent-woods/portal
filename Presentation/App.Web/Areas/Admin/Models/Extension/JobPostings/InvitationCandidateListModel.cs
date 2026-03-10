using App.Web.Framework.Models;

namespace App.Web.Areas.Admin.Models.JobPostings
{
    /// <summary>
    /// Represents a InvitationCandidate list model
    /// </summary>
    public partial record InvitationCandidateListModel : BasePagedListModel<InvitationCandidateModel>
    {
    }
}