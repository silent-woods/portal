using App.Web.Framework.Models;

namespace Satyanam.Nop.Plugin.SatyanamCRM.Models.AccountTypes
{
    /// <summary>
    /// Represents a synchronization record list model
    /// </summary>
    public record AccountTypeListModel : BasePagedListModel<AccountTypeModel>
    {
    }
}