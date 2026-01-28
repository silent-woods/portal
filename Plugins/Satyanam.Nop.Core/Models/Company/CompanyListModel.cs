using App.Web.Framework.Models;

namespace Satyanam.Nop.Core.Models.Company
{
    /// <summary>
    /// Represents a synchronization record list model
    /// </summary>
    public record CompanyListModel : BasePagedListModel<CompanyModel>
    {
    }
}