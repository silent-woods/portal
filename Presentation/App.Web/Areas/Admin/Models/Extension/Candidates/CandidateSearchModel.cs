using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace App.Web.Areas.Admin.Models.Extension.Candidates
{
    public partial record CandidateSearchModel : BaseSearchModel
    {
        public CandidateSearchModel()
        {
            Status = new List<SelectListItem>();
            CandidateTypes = new List<SelectListItem>();
        }
        #region Properties
        [NopResourceDisplayName("Admin.CandidateSearchModel.Fields.SearchName")]
        public string SearchName { get; set; }
        [NopResourceDisplayName("Admin.CandidateSearchModel.Fields.SearchFirstName")]
        public string SearchFirstName { get; set; }

        [NopResourceDisplayName("Admin.CandidateSearchModel.Fields.SearchLastName")]
        public string SearchLastName { get; set; }

        [NopResourceDisplayName("Admin.CandidateSearchModel.Fields.SearchEmail")]
        public string SearchEmail { get; set; }

        [NopResourceDisplayName("Admin.CandidateSearchModel.Fields.SearchStatusId")]
        public int SearchStatusId { get; set; }
        public IList<SelectListItem> Status { get; set; }
        public int JobPostingId { get; set; }
        [NopResourceDisplayName("Admin.CandidateSearchModel.Fields.SearchCandidateTypeId")]
        public int SearchCandidateTypeId { get; set; }
        public IList<SelectListItem> CandidateTypes { get; set; }

        [NopResourceDisplayName("Admin.CandidateSearchModel.Fields.SearchPositionApplied")]
        public string SearchPositionApplied { get; set; }

        #endregion
    }
}
