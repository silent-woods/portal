using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;
using System;
using System.Collections.Generic;

namespace App.Web.Models.Extensions.UpdateForm
{

    public partial record SubmissionCardModel : BaseNopEntityModel
    {
        public SubmissionCardModel()
        {
            Questions = new List<SubmitUpdateQuestionModel>();

        }

        #region Properties

        [NopResourceDisplayName("Admin.SubmissionCardModel.Fields.SubmissionId")]

        public int SubmissionId { get; set; }
        [NopResourceDisplayName("Admin.SubmissionCardModel.Fields.SubmitterName")]
        public string SubmitterName { get; set; }
        [NopResourceDisplayName("Admin.SubmissionCardModel.Fields.SubmittedOn")]
        public DateTime SubmittedOn { get; set; }
        public List<SubmitUpdateQuestionModel> Questions { get; set; }
        public string TemplateName { get; set; }
        //public List<CommentModel> Comments { get; set; } = new();
        public List<UpdateSubmissionCommentModel> Comments { get; set; }
        #endregion
    }

    public class CommentModel
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public int? ParentCommentId { get; set; }
    }
}
