using App.Web.Framework.Models;

namespace App.Web.Models.Profile
{
    public partial record ProfileIndexModel : BaseNopModel
    {
        public int CustomerProfileId { get; set; }
        public string ProfileTitle { get; set; }
        public int PostsPage { get; set; }
        public bool PagingPosts { get; set; }
        public bool ForumsEnabled { get; set; }
    }
}