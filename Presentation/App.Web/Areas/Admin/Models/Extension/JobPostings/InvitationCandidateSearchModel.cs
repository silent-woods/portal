using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace App.Web.Areas.Admin.Models.JobPostings
{
    /// <summary>
    /// Represents a InvitaionCandidate search model
    /// </summary>
    public partial record InvitationCandidateSearchModel : BaseSearchModel
    {
        public InvitationCandidateSearchModel()
        {
            AvailableStatus = new List<SelectListItem>();
            AvailableCandidateTypeName = new List<SelectListItem>();
        }

        public int JobPostingId { get; set; }
        [NopResourceDisplayName("Admin.InvitationCandidateSearchModel.Fields.SearchName")]
        public string SearchName { get; set; }
        [NopResourceDisplayName("Admin.InvitationCandidateSearchModel.Fields.SearchEmail")]
        public string SearchEmail { get; set; }
        [NopResourceDisplayName("Admin.InvitationCandidateSearchModel.Fields.SearchStatus")]
        public int? SearchStatusId { get; set; }
        [NopResourceDisplayName("Admin.InvitationCandidateSearchModel.Fields.SearchCandidateType")]
        public int? SearchCandidateTypeId { get; set; }

        // Dropdown
        public IList<SelectListItem> AvailableStatus { get; set; }
        public IList<SelectListItem> AvailableCandidateTypeName { get; set; }
    }
}