using App.Web.Framework.Models;

namespace App.Web.Areas.Admin.Models.Polls
{
    /// <summary>
    /// Represents a poll list model
    /// </summary>
    public partial record PollListModel : BasePagedListModel<PollModel>
    {
    }
}