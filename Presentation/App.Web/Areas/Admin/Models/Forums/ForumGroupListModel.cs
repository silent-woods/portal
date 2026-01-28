using App.Web.Framework.Models;

namespace App.Web.Areas.Admin.Models.Forums
{
    /// <summary>
    /// Represents a forum group list model
    /// </summary>
    public partial record ForumGroupListModel : BasePagedListModel<ForumGroupModel>
    {
    }
}