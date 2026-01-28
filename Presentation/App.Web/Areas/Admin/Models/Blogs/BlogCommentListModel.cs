using App.Web.Framework.Models;

namespace App.Web.Areas.Admin.Models.Blogs
{
    /// <summary>
    /// Represents a blog comment list model
    /// </summary>
    public partial record BlogCommentListModel : BasePagedListModel<BlogCommentModel>
    {
    }
}