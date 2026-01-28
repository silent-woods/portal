using App.Web.Framework.Models;

namespace Satyanam.Nop.Plugin.SatyanamCRM.Models.ConnectionRequests
{
    /// <summary>
    /// Represents a synchronization record list model
    /// </summary>
    public record ConnectionRequestListModel : BasePagedListModel<ConnectionRequestModel>
    {
    }
}