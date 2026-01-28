using App.Web.Framework.Mvc.ModelBinding;
using App.Web.Framework.Models;

namespace App.Web.Models.News
{
    public partial record AddNewsCommentModel : BaseNopModel
    {
        [NopResourceDisplayName("News.Comments.CommentTitle")]
        public string CommentTitle { get; set; }

        [NopResourceDisplayName("News.Comments.CommentText")]
        public string CommentText { get; set; }

        public bool DisplayCaptcha { get; set; }
    }
}