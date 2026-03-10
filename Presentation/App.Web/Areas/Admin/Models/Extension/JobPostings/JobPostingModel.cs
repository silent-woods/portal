using App.Web.Areas.Admin.Models.Extension.Candidates;
using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace App.Web.Areas.Admin.Models.JobPostings
{
    /// <summary>
    /// Represents a TimeSheet model
    /// </summary>
    public partial record JobPostingModel : BaseNopEntityModel
    {
        public JobPostingModel()
        {
            AvailablePosition = new List<SelectListItem>();
            AvailableCandidateTypes = new List<SelectListItem>();
            CandidateSearchModel = new CandidateSearchModel();
            jobPostingCandidateSearchModel = new JobPostingCandidateSearchModel();
            CandidateModel = new CandidateModel();
            InvitationCandidateSearchModel = new InvitationCandidateSearchModel();
            SelectedTechnologyIds = new List<int>();
            SelectedSkillIds = new List<int>();
            AvailableTechnologies = new List<SelectListItem>();
            AvailableSkills = new List<SelectListItem>();

        }
        #region Properties


        [NopResourceDisplayName("Admin.JobPosting.Fields.Title")]
        [Required(ErrorMessage = "Please enter a Title.")]
        public string Title { get; set; }
        public IList<SelectListItem> AvailablePosition { get; set; }

        [NopResourceDisplayName("Admin.JobPosting.Fields.Description")]
        [Required(ErrorMessage = "Please enter a Description.")]
        public string Description { get; set; }

        [NopResourceDisplayName("Admin.JobPosting.Fields.PositionId")]
        [Required(ErrorMessage = "Please select a Position.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a Position.")]
        public int PositionId { get; set; }

        [NopResourceDisplayName("Admin.JobPosting.Fields.Position")]
        public string Position { get; set; }

        [NopResourceDisplayName("Admin.JobPosting.Fields.Publish")]
        public bool Publish { get; set; }

        [NopResourceDisplayName("Admin.JobPosting.Fields.CreateOn")]
        public DateTime CreateOn { get; set; }

        [NopResourceDisplayName("Admin.JobPosting.Fields.UpdateOn")]
        public DateTime UpdateOn { get; set; }
        [NopResourceDisplayName("Admin.JobPosting.Fields.CandiadateTypeId")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a CandidateType.")]

        public int CandidateTypeId { get; set; }
        public string CandidateType { get; set; }
        public IList<SelectListItem> AvailableCandidateTypes { get; set; }
        public CandidateSearchModel CandidateSearchModel { get; set; }
        public CandidateModel CandidateModel { get; set; }
        public JobPostingCandidateSearchModel jobPostingCandidateSearchModel { get; set; }
        public InvitationCandidateSearchModel InvitationCandidateSearchModel { get; set; }
        [NopResourceDisplayName("Admin.JobPosting.Fields.SelectedTechnologyIds")]
        public List<int> SelectedTechnologyIds { get; set; }
        public List<string> NewTechnologyNames { get; set; } = new();
        [NopResourceDisplayName("Admin.JobPosting.Fields.SelectedSkillIds")]
        public List<int> SelectedSkillIds { get; set; }
        public List<SelectListItem> AvailableTechnologies { get; set; }
        public List<SelectListItem> AvailableSkills { get; set; }
        #endregion
    }
}