using App.Web.Framework.Models;

namespace App.Web.Areas.Admin.Models.Blogs
{
    /// <summary>
    /// Represents a blog post list model
    /// </summary>
    public partial record BlogPostListModel : BasePagedListModel<BlogPostModel>
    {
    }
}