using App.Web.Framework.Models;

namespace Satyanam.Nop.Plugin.SatyanamCRM.Models.Contacts
{
    /// <summary>
    /// Represents a synchronization record list model
    /// </summary>
    public record ContactsListModel : BasePagedListModel<ContactsModel>
    {
    }
}