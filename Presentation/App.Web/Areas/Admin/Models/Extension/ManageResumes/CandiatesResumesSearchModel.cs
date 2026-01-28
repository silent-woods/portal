using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace App.Web.Areas.Admin.Models.ManageResumes
{
    /// <summary>
    /// Represents a timesheet search model
    /// </summary>
    public partial record CandiatesResumesSearchModel : BaseSearchModel
    {
        public CandiatesResumesSearchModel()
        {
            AvailableApplyFor = new List<SelectListItem>();
            AvailableStatus = new List<SelectListItem>();
            AvailableInterviewer = new List<SelectListItem>();
            SelectedInterviewer = new List<int>();
        }
        public IList<int> SelectedInterviewer { get; set; }
        public IList<SelectListItem> AvailableApplyFor { get; set; }
        public IList<SelectListItem> AvailableStatus { get; set; }
        public IList<SelectListItem> AvailableInterviewer { get; set; }
        #region Properties

        [NopResourceDisplayName("Admin.CandiatesResumes.Fields.AppliedFor")]
        public int AppliedForId { get; set; }
        [NopResourceDisplayName("Admin.CandiatesResumes.Fields.StatusId")]
        public int StatusId { get; set; }
        [NopResourceDisplayName("Admin.CandiatesResumes.Fields.Interviewer")]
        public int InterviewerId { get; set; }

        [NopResourceDisplayName("Admin.CandiatesResumes.Fields.TechnicalRoundDate")]
        [UIHint("DateNullable")]
        public DateTime? TechnicalRoundDate { get; set; }

        [NopResourceDisplayName("Admin.CandiatesResumes.Fields.PracticalRoundDate")]
        [UIHint("DateNullable")]
        public DateTime? PracticalRoundDate { get; set; }

        [NopResourceDisplayName("Admin.CandiatesResumes.Fields.FirstName")]
        public string FirstName { get; set; }
        [NopResourceDisplayName("Admin.CandiatesResumes.Fields.LastName")]
        public string LastName { get; set; }

        [NopResourceDisplayName("Admin.CandiatesResumes.Fields.Email")]
        public string Email { get; set; }

        [NopResourceDisplayName("Admin.CandiatesResumes.Fields.MobileNumber")]
        public string MobileNo { get; set; }

        [NopResourceDisplayName("Admin.CandiatesResumes.Fields.Degree")]
        public string Degree { get; set; }
        #endregion
    }
}