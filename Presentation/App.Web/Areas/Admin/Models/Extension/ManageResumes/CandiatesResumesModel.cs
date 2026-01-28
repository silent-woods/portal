using App.Web.Areas.Admin.InterviewQeations.Models;
using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace App.Web.Areas.Admin.Models.ManageResumes
{
    /// <summary>
    /// Represents a TimeSheet model
    /// </summary>
    public partial record CandiatesResumesModel : BaseNopEntityModel
    {
        public CandiatesResumesModel()
        {
            AvailableApplyFor = new List<SelectListItem>();
            AvailableStatus = new List<SelectListItem>();
            AvailableResultStatus = new List<SelectListItem>();
            Employee = new List<SelectListItem>();
            SelectedInterviewer = new List<int>();
            Addresses = new List<RecruitementModel>();
            resultModel = new List<CandiatesResultModel>();
        }
        #region Properties

        [NopResourceDisplayName("Admin.CandiatesResumes.Fields.Gender")]
        public string Gender { get; set; }

        [NopResourceDisplayName("Admin.CandiatesResumes.Fields.FirstName")]
        public string FirstName { get; set; }

        [NopResourceDisplayName("Admin.CandiatesResumes.Fields.LastName")]
        public string LastName { get; set; }

        [UIHint("DateNullable")]
        [NopResourceDisplayName("Admin.CandiatesResumes.Fields.DateOfBirth")]
        public DateTime? DateOfBirth { get; set; }

        [NopResourceDisplayName("Admin.CandiatesResumes.Fields.Email")]
        public string Email { get; set; }

        [NopResourceDisplayName("Admin.CandiatesResumes.Fields.CollegeName")]
        public string CollegeName { get; set; }

        [NopResourceDisplayName("Admin.CandiatesResumes.Fields.Degree")]
        public string Degree { get; set; }

        [NopResourceDisplayName("Trainee.Fields.CompeletdYear")]
        public string CompletedYear { get; set; }

        [NopResourceDisplayName("Admin.CandiatesResumes.Fields.Percentage")]
        public decimal Percentage { get; set; }
        public String Percentages { get; set; }

        [NopResourceDisplayName("Admin.CandiatesResumes.Fields.ResumeUrl")]
        [UIHint("Download")]
        public int ResumeUrl { get; set; }

        [NopResourceDisplayName("Admin.CandiatesResumes.Fields.Skill")]
        public string Skill { get; set; }

        [NopResourceDisplayName("Admin.CandiatesResumes.Fields.MobileNumber")]
        public string MobileNumber { get; set; }

        [NopResourceDisplayName("Admin.CandiatesResumes.Fields.StreetAddress")]
        public string StreetAddress { get; set; }

        [NopResourceDisplayName("Admin.CandiatesResumes.Fields.StreetAddress2")]
        public string StreetAddress2 { get; set; }

        [NopResourceDisplayName("Admin.CandiatesResumes.Fields.ZipPostalCode")]
        public string ZipPostalCode { get; set; }

        [NopResourceDisplayName("Admin.CandiatesResumes.Fields.City")]
        public string City { get; set; }

        [NopResourceDisplayName("Admin.CandiatesResumes.Fields.AboutYourSelf")]
        public string AboutYourSelf { get; set; }

        [NopResourceDisplayName("Admin.Trainee.DateForInterview")]
        public DateTime DateForInterview { get; set; }

        [NopResourceDisplayName("Admin.CandiatesResumes.PersonInCharge")]
        public string PersonInCharge { get; set; }

        [NopResourceDisplayName("Admin.CandiatesResumes.Venue")]
        public string Venue { get; set; }

        public IList<RecruitementModel> Addresses { get; set; }
        public IList<CandiatesResultModel> resultModel { get; set; }
        public int? DateOfBirthDay { get; set; }
        [NopResourceDisplayName("Admin.CandiatesResumes.Fields.DateOfBirth")]
        public int? DateOfBirthMonth { get; set; }
        [NopResourceDisplayName("Admin.CandiatesResumes.Fields.DateOfBirth")]
        public int? DateOfBirthYear { get; set; }
        public DateTime? ParseDateOfBirth()
        {
            if (!DateOfBirthYear.HasValue || !DateOfBirthMonth.HasValue || !DateOfBirthDay.HasValue)
                return null;

            DateTime? dateOfBirth = null;
            try
            {
                dateOfBirth = new DateTime(DateOfBirthYear.Value, DateOfBirthMonth.Value, DateOfBirthDay.Value);
            }
            catch { }
            return dateOfBirth;
        }

        [NopResourceDisplayName("Admin.CandiatesResumes.Fields.AppliedForId")]
        public int AppliedForId { get; set; }
        [NopResourceDisplayName("Admin.CandiatesResumes.Fields.AppliedFor")]
        public string AppliedFor { get; set; }

        [NopResourceDisplayName("Admin.CandiatesResumes.Fields.StatusId")]
        public int StatusId { get; set; }

        [NopResourceDisplayName("Admin.CandiatesResumes.Fields.Status")]
        public string Status { get; set; }
        [NopResourceDisplayName("Admin.CandiatesResumes.Fields.InterviewerId")]
        public int EmployeeId { get; set; }
        [NopResourceDisplayName("Admin.CandiatesResumes.Fields.Interviewer")]
        public string Interviewer { get; set; }

        [NopResourceDisplayName("Admin.CandiatesResumes.Fields.TechnicalRoundDate")]
        public DateTime TechnicalRoundDate { get; set; }
        [NopResourceDisplayName("Admin.CandiatesResumes.Fields.CreatedOnUTC")]
        public DateTime? CreatedOnUTC { get; set; }
        public int TraineeId { get; set; }

        public int CategoryId { get; set; }
        public string Question { get; set; }

        [NopResourceDisplayName("Admin.CandiatesResumes.Fields.PracticalRoundDate")]
        public DateTime PracticalRoundDate { get; set; }
        public IList<SelectListItem> AvailableApplyFor { get; set; }
        public IList<SelectListItem> AvailableStatus { get; set; }
        public IList<SelectListItem> AvailableResultStatus { get; set; }
        public IList<SelectListItem> Employee { get; set; }
        public IList<int> SelectedInterviewer { get; set; }

        //Manage Interviewer result//

        [NopResourceDisplayName("Admin.CandiatesResumes.Fields.ResulstatusId")]
        public int ResultStatusId { get; set; }
        [NopResourceDisplayName("Admin.CandiatesResumes.Fields.Feedback")]
        public string Feedback { get; set; }

        [NopResourceDisplayName("Admin.CandiatesResumes.Fields.Communication")]
        public string Communication { get; set; }
        [NopResourceDisplayName("Admin.CandiatesResumes.Fields.ConfidentLevel")]
        public string ConfidentLevel { get; set; }
        public string Incorrect { get; set; }
        public string partially { get; set; }
        public string correct { get; set; }
        public string marks { get; set; }

        [NopResourceDisplayName("Admin.CandiatesResumes.Fields.CandidateName")]
        public string CandidateName { get; set; }
        public int CandidateId { get; set; }

        #endregion
    }

}