using App.Web.Framework.Mvc.ModelBinding;
using App.Web.Framework.Models;

namespace App.Web.Models.Blogs
{
    public partial record AddBlogCommentModel : BaseNopEntityModel
    {
        [NopResourceDisplayName("Blog.Comments.CommentText")]
        public string CommentText { get; set; }

        public bool DisplayCaptcha { get; set; }
    }
}