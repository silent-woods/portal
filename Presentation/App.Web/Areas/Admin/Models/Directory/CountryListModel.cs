using App.Web.Framework.Models;

namespace App.Web.Areas.Admin.Models.Directory
{
    /// <summary>
    /// Represents a country list model
    /// </summary>
    public partial record CountryListModel : BasePagedListModel<CountryModel>
    {
    }
}