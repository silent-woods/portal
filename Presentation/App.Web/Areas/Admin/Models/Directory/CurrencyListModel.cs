using App.Web.Framework.Models;

namespace App.Web.Areas.Admin.Models.Directory
{
    /// <summary>
    /// Represents a currency list model
    /// </summary>
    public partial record CurrencyListModel : BasePagedListModel<CurrencyModel>
    {
    }
}