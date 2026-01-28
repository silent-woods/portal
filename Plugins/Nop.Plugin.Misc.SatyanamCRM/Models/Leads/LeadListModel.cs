using App.Web.Framework.Models;

namespace Satyanam.Nop.Plugin.SatyanamCRM.Models.Leads
{
    /// <summary>
    /// Represents a synchronization record list model
    /// </summary>
    public record LeadListModel : BasePagedListModel<LeadModel>
    {
    }
}