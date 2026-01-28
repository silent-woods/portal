using System;
using System.Collections.Generic;

namespace App.Web.Models.Extensions.UpdateForm
{

    public class AddCommentDto
    {
        public int UpdateSubmissionId { get; set; }
        public int? ParentCommentId { get; set; }
        public string CommentText { get; set; }
    }

    public class CommentTreeDto
    {
        public int Id { get; set; }
        public int? ParentCommentId { get; set; }
        public string CommentText { get; set; }
        public int CommentedByCustomerId { get; set; }
        public string CommentedByName { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public List<CommentTreeDto> Replies { get; set; } = new List<CommentTreeDto>();
    }



}
