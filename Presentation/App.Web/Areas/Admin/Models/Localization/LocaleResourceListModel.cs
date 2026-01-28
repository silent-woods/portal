using App.Web.Framework.Models;

namespace App.Web.Areas.Admin.Models.Localization
{
    /// <summary>
    /// Represents a locale resource list model
    /// </summary>
    public partial record LocaleResourceListModel : BasePagedListModel<LocaleResourceModel>
    {
    }
}