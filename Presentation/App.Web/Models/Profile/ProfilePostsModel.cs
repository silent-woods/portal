using System.Collections.Generic;
using App.Web.Framework.Models;
using App.Web.Models.Common;

namespace App.Web.Models.Profile
{
    public partial record ProfilePostsModel : BaseNopModel
    {
        public IList<PostsModel> Posts { get; set; }
        public PagerModel PagerModel { get; set; }
    }
}