using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;

namespace App.Web.Models.Extensions.UpdateForm
{

    public partial record SubmitUpdateFormModel : BaseNopEntityModel
    {
        public SubmitUpdateFormModel()
        {
            Questions = new List<SubmitUpdateQuestionModel>();
            QuestionFiles = new List<IFormFile>();
            Periods = new List<PeriodStatusModel>();
        }

        #region Properties

        [NopResourceDisplayName("Admin.SubmitUpdateFormModel.Fields.UpdateTemplateId")]

        public int UpdateTemplateId { get; set; }
        [NopResourceDisplayName("Admin.SubmitUpdateFormModel.Fields.Title")]
        public string Title { get; set; }
        [NopResourceDisplayName("Admin.SubmitUpdateFormModel.Fields.Description")]
        public string Description { get; set; }
        [NopResourceDisplayName("Admin.SubmitUpdateFormModel.Fields.IsFileAttachmentRequired")]
        public bool IsFileAttachmentRequired { get; set; }
        [NopResourceDisplayName("Admin.SubmitUpdateFormModel.Fields.UploadedFile")]
        public IFormFile UploadedFile { get; set; }
        public List<SubmitUpdateQuestionModel> Questions { get; set; }
        public List<IFormFile> QuestionFiles { get; set; }
        public int PeriodId { get; set; }
        public bool HasAlreadySubmitted { get; set; }
        public bool AllowEditingAfterSubmit { get; set; }

        public List<PeriodStatusModel> Periods { get; set; }

       // public List<UpdateSubmissionCommentModel> Comments { get; set; } = new();
        public IList<UpdateSubmissionCommentModel> UpdateSubmissionComments { get; set; } = new List<UpdateSubmissionCommentModel>();

        public bool IsCurrentUserReviewer { get; set; }
        public bool IsCurrentUserSubmitter { get; set; }
        public List<UpdateSubmissionCommentModel> Comments { get; set; } = new();

        #endregion
    }

    public class PeriodStatusModel
        {
            public DateTime PeriodStart { get; set; }
            public DateTime PeriodEnd { get; set; }
            public int PeriodId { get; set; }
            public string PeriodLabel { get; set; } 
            public string Status { get; set; }
            public DateTime? SubmittedOnUtc { get; set; }
        }


}
