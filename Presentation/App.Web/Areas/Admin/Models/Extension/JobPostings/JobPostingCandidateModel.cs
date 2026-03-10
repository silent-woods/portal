using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace App.Web.Areas.Admin.Models.JobPostings
{
    public record JobPostingCandidateModel : BaseNopEntityModel
    {
        public JobPostingCandidateModel()
        {
            SourceTypeList = new List<SelectListItem>();
            StatusList = new List<SelectListItem>();
            RateTypeList = new List<SelectListItem>();
            LinkTypeList = new List<SelectListItem>();
            AvailableCandidateTypes = new List<SelectListItem>();
            AvailableNoticePeriodDays = new List<SelectListItem>();
        }

        #region Properties
        public string Name { get; set; }
        [NopResourceDisplayName("Admin.Candidate.Fields.FirstName")]
        [Required(ErrorMessage = "Please enter a firstname.")]
        public string FirstName { get; set; }

        [NopResourceDisplayName("Admin.Candidate.Fields.LastName")]
        [Required(ErrorMessage = "Please enter a lastname.")]
        public string LastName { get; set; }

        [DataType(DataType.EmailAddress)]
        [Required(ErrorMessage = "Please enter a valid email.")]
        [NopResourceDisplayName("Admin.Candidate.Fields.Email")]
        public string Email { get; set; }

        [NopResourceDisplayName("Admin.Candidate.Fields.Phone")]
        public string Phone { get; set; }

        [NopResourceDisplayName("Admin.Candidate.Fields.PositionApplied")]
        public string PositionApplied { get; set; }

        [NopResourceDisplayName("Admin.Candidate.Fields.ExperienceYears")]
        public string ExperienceYears { get; set; }

        [NopResourceDisplayName("Admin.Candidate.Fields.AdditionalInformation")]
        public string AdditionalInformation { get; set; }
        [UIHint("Download")]
        [NopResourceDisplayName("Admin.Candidate.Fields.Resume")]
        public int? ResumeDownloadId { get; set; }
        public string DownloadGuid { get; set; }
        public string ResumeFileName { get; set; }

        [NopResourceDisplayName("Admin.Candidate.Fields.SourceType")]
        public int SourceTypeId { get; set; }
        public string SourceTypeName { get; set; }

        [NopResourceDisplayName("Admin.Candidate.Fields.Status")]
        public int StatusId { get; set; }
        public string Status { get; set; }
        [NopResourceDisplayName("Admin.Candidate.Fields.RateType")]
        public int RateTypeId { get; set; }
        public string RateType { get; set; }
        [NopResourceDisplayName("Admin.Candidate.Fields.LinkType")]
        public int LinkTypeId { get; set; }
        public string LinkType { get; set; }

        [NopResourceDisplayName("Admin.Candidate.Fields.Url")]
        public string Url { get; set; }

        [NopResourceDisplayName("Admin.Candidate.Fields.Amount")]
        public decimal? Amount { get; set; }


        [NopResourceDisplayName("Admin.Candidate.Fields.HrNotes")]
        public string HrNotes { get; set; }

        public DateTime CreatedOnUtc { get; set; }
        public DateTime UpdatedOnUtc { get; set; }
        [NopResourceDisplayName("Admin.Candidate.Fields.CandidateTypeId")]
        [Required(ErrorMessage = "Please select a candidate type.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a candidate type.")]
        public int CandidateTypeId { get; set; }
        public string CandidateTypeName { get; set; }
        [NopResourceDisplayName("Admin.Candidate.Fields.CurrentCompany")]
        public string CurrentCompany { get; set; }
        [NopResourceDisplayName("Admin.Candidate.Fields.CurrentCTC")]
        public decimal? CurrentCTC { get; set; }
        [NopResourceDisplayName("Admin.Candidate.Fields.ExpectedCTC")]
        public decimal? ExpectedCTC { get; set; }
        [NopResourceDisplayName("Admin.Candidate.Fields.NoticePeriodId")]
        public int NoticePeriodId { get; set; }
        public string NoticePeriodDays { get; set; }

        public int JobPostingId { get; set; }

        public IList<SelectListItem> AvailableCandidateTypes { get; set; }

        public IList<SelectListItem> SourceTypeList { get; set; }
        public IList<SelectListItem> StatusList { get; set; }
        public IList<SelectListItem> RateTypeList { get; set; }
        public IList<SelectListItem> LinkTypeList { get; set; }
        public IList<SelectListItem> AvailableNoticePeriodDays { get; set; }
        #endregion
    }
}