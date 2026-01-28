using App.Web.Framework.Models;

namespace App.Web.Areas.Admin.Models.Topics
{
    /// <summary>
    /// Represents a topic list model
    /// </summary>
    public partial record TopicListModel : BasePagedListModel<TopicModel>
    {
    }
}