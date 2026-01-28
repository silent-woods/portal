using App.Web.Framework.Models;

namespace App.Web.Areas.Admin.Models.Affiliates
{
    /// <summary>
    /// Represents an affiliate list model
    /// </summary>
    public partial record AffiliateListModel : BasePagedListModel<AffiliateModel>
    {
    }
}