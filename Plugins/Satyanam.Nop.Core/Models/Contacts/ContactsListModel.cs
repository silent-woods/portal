using App.Web.Framework.Models;

namespace Satyanam.Nop.Core.Models.Contacts
{
    /// <summary>
    /// Represents a synchronization record list model
    /// </summary>
    public record ContactsListModel : BasePagedListModel<ContactsModel>
    {
    }
}