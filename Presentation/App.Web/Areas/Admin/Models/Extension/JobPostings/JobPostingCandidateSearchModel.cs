using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace App.Web.Areas.Admin.Models.JobPostings
{
    public record JobPostingCandidateSearchModel : BaseSearchModel
    {
        public JobPostingCandidateSearchModel()
        {
            AvailableCandidateTypes = new List<SelectListItem>();
            AvailableStatuses = new List<SelectListItem>();
        }

        #region Properties

        public int JobPostingId { get; set; }
        [NopResourceDisplayName("Admin.JobPostingCandidateSearchModel.Fields.Name")]
        public string Name { get; set; }
        [NopResourceDisplayName("Admin.JobPostingCandidateSearchModel.Fields.Email")]
        public string Email { get; set; }
        [NopResourceDisplayName("Admin.JobPostingCandidateSearchModel.Fields.CandidateTypeId")]

        public int? CandidateTypeId { get; set; }
        [NopResourceDisplayName("Admin.JobPostingCandidateSearchModel.Fields.Status")]
        public int? StatusId { get; set; }
        public IList<SelectListItem> AvailableCandidateTypes { get; set; }
        public IList<SelectListItem> AvailableStatuses { get; set; }


        #endregion
    }
}