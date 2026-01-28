using App.Web.Framework.Models;

namespace Satyanam.Nop.Plugin.SatyanamCRM.Models.Deals
{
    /// <summary>
    /// Represents a synchronization record list model
    /// </summary>
    public record DealsListModel : BasePagedListModel<DealsModel>
    {
    }
}