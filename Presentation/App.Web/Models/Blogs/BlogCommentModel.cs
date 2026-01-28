using System;
using App.Web.Framework.Models;

namespace App.Web.Models.Blogs
{
    public partial record BlogCommentModel : BaseNopEntityModel
    {
        public int CustomerId { get; set; }

        public string CustomerName { get; set; }

        public string CustomerAvatarUrl { get; set; }

        public string CommentText { get; set; }

        public DateTime CreatedOn { get; set; }

        public bool AllowViewingProfiles { get; set; }
    }
}