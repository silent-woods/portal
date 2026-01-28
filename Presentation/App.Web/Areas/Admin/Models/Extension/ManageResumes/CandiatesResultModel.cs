using System;
using App.Web.Framework.Mvc.ModelBinding;
using App.Web.Framework.Models;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using App.Core.Domain.Common;
using App.Web.Areas.Admin.InterviewQeations.Models;
using Nop.Core.Domain.Catalog;

namespace App.Web.Areas.Admin.Models.ManageResumes
{
    /// <summary>
    /// Represents a TimeSheet model
    /// </summary>
    public partial record CandiatesResultModel : BaseNopEntityModel
    {
        public CandiatesResultModel()
        {
            
            AvailableResultStatus = new List<SelectListItem>();
            Addresses = new List<RecruitementModel>();
        }
        #region Properties
        public IList<RecruitementModel> Addresses { get; set; }
        public IList<SelectListItem> AvailableResultStatus { get; set; }

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

        [NopResourceDisplayName("Admin.CandiatesResumes.Fields.CandidateName")]
        public string CandidateName { get; set; }
        public int CandidateId { get; set; }

        [NopResourceDisplayName("Admin.CandiatesResumes.Fields.Resulstatus")]
        public string Resulstatus { get; set; }

        public int CategoryId { get; set; }
        public string Qeations { get; set; }
        public string Marks { get; set; }
        public string category { get; set; }

        #endregion
    }
}