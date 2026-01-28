using App.Web.Framework.Models;

namespace App.Web.Areas.Admin.Models.News
{
    /// <summary>
    /// Represents a news item list model
    /// </summary>
    public partial record NewsItemListModel : BasePagedListModel<NewsItemModel>
    {
    }
}