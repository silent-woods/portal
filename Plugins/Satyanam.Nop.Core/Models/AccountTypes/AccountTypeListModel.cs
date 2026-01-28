using App.Web.Framework.Models;

namespace Satyanam.Nop.Core.Models.AccountTypes
{
    /// <summary>
    /// Represents a synchronization record list model
    /// </summary>
    public record AccountTypeListModel : BasePagedListModel<AccountTypeModel>
    {
    }
}