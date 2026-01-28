using App.Web.Framework.Models;

namespace App.Web.Areas.Admin.Models.Forums
{
    /// <summary>
    /// Represents a forum list model
    /// </summary>
    public partial record ForumListModel : BasePagedListModel<ForumModel>
    {
    }
}