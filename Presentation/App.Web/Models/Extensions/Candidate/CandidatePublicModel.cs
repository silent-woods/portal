using App.Web.Framework.Mvc.ModelBinding;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace App.Web.Models.Extensions.Candidate
{

    public partial record CandidatePublicModel
    {
        public CandidatePublicModel()
        {

        }
        #region Properties

        [NopResourceDisplayName("Admin.CandidatePublicModel.Fields.FirstName")]
        [Required(ErrorMessage = "Please enter a firstname.")]
        public string FirstName { get; set; }
        [NopResourceDisplayName("Admin.CandidatePublicModel.Fields.LastName")]
        [Required(ErrorMessage = "Please enter a lastname.")]
        public string LastName { get; set; }
        [NopResourceDisplayName("Admin.CandidatePublicModel.Fields.Email")]
        [DataType(DataType.EmailAddress)]
        [Required(ErrorMessage = "Please enter a valid email")]
        public string Email { get; set; }
        [NopResourceDisplayName("Admin.CandidatePublicModel.Fields.Phone")]
        public string Phone { get; set; }
        [NopResourceDisplayName("Admin.CandidatePublicModel.Fields.PositionApplied")]
        public string PositionApplied { get; set; }
        [NopResourceDisplayName("Admin.CandidatePublicModel.Fields.AdditionalInformation")]
        public string AdditionalInformation { get; set; }
        [NopResourceDisplayName("Admin.CandidatePublicModel.Fields.CoverLetter ")]
        public string CoverLetter { get; set; }
        [NopResourceDisplayName("Admin.CandidatePublicModel.Fields.ResumeFile")]
        [Required(ErrorMessage = "Please upload your file.")]
        public IFormFile ResumeFile { get; set; }
        [NopResourceDisplayName("Admin.CandidatePublicModel.Fields.ExperienceYears")]
        public string ExperienceYears { get; set; }
        [NopResourceDisplayName("Admin.CandidatePublicModel.Fields.CandidateTypeId")]
        public int CandidateTypeId { get; set; }
        [NopResourceDisplayName("Admin.CandidatePublicModel.Fields.CurrentCompany")]
        public string CurrentCompany { get; set; }
        [NopResourceDisplayName("Admin.CandidatePublicModel.Fields.CurrentCTC")]
        public decimal? CurrentCTC { get; set; }
        [NopResourceDisplayName("Admin.CandidatePublicModel.Fields.ExpectedCTC")]
        public decimal? ExpectedCTC { get; set; }
        [NopResourceDisplayName("Admin.CandidatePublicModel.Fields.NoticePeriodId")]
        public int NoticePeriodId { get; set; }
        public int JobPostingId { get; set; }

        public SelectList AvailableCandidateTypes { get; set; }
        public SelectList AvailableNoticePeriods { get; set; }

        public bool IsFreelancer { get; set; }
        [NopResourceDisplayName("Admin.CandidatePublicModel.Fields.RateType")]
        public int? RateTypeId { get; set; }

        [NopResourceDisplayName("Admin.CandidatePublicModel.Fields.Amount")]
        public decimal? Amount { get; set; }
        [NopResourceDisplayName("Admin.CandidatePublicModel.Fields.City")]
        public string City { get; set; }
        [NopResourceDisplayName("Admin.CandidatePublicModel.Fields.Skills")]
        public string Skills { get; set; }
        public SelectList AvailableRateTypes { get; set; }
        public int? CandidateId { get; set; }
        #endregion
    }
}
