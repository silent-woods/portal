using App.Web.Framework.Models;

namespace App.Web.Models.Profile
{
    public partial record PostsModel : BaseNopModel
    {
        public int ForumTopicId { get; set; }
        public string ForumTopicTitle { get; set; }
        public string ForumTopicSlug { get; set; }
        public string ForumPostText { get; set; }
        public string Posted { get; set; }
    }
}