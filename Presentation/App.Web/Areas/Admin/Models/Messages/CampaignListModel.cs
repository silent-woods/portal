using App.Web.Framework.Models;

namespace App.Web.Areas.Admin.Models.Messages
{
    /// <summary>
    /// Represents a campaign list model
    /// </summary>
    public partial record CampaignListModel : BasePagedListModel<CampaignModel>
    {
    }
}