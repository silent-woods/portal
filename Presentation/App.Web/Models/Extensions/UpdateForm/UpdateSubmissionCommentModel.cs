using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;
using System;
using System.Collections.Generic;

namespace App.Web.Models.Extensions.UpdateForm
{

    public partial record UpdateSubmissionCommentModel : BaseNopEntityModel
    {
        public UpdateSubmissionCommentModel()
        {
            Replies = new List<UpdateSubmissionCommentModel>();
        }

        #region Properties
        [NopResourceDisplayName("Admin.UpdateSubmissionCommentModel.Fields.Id")]
        public int Id { get; set; }
        [NopResourceDisplayName("Admin.UpdateSubmissionCommentModel.Fields.UpdateSubmissionId")]
        public int UpdateSubmissionId { get; set; }
        [NopResourceDisplayName("Admin.UpdateSubmissionCommentModel.Fields.ParentCommentId")]
        public int? ParentCommentId { get; set; }
        [NopResourceDisplayName("Admin.UpdateSubmissionCommentModel.Fields.CommentedByCustomerId")]
        public int CommentedByCustomerId { get; set; }
        [NopResourceDisplayName("Admin.UpdateSubmissionCommentModel.Fields.CommentedByName")]
        public string CommentedByName { get; set; }
        [NopResourceDisplayName("Admin.UpdateSubmissionCommentModel.Fields.CommentText")]
        public string CommentText { get; set; }
        [NopResourceDisplayName("Admin.UpdateSubmissionCommentModel.Fields.CreatedOnUtc")]
        public DateTime CreatedOnUtc { get; set; }

        public List<UpdateSubmissionCommentModel> Replies { get; set; }

        #endregion
    }




}
