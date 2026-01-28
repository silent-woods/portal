using App.Web.Framework.Models;

namespace Satyanam.Nop.Core.Models.Deals
{
    /// <summary>
    /// Represents a synchronization record list model
    /// </summary>
    public record DealsListModel : BasePagedListModel<DealsModel>
    {
    }
}