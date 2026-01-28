using App.Web.Framework.Models;

namespace App.Web.Areas.Admin.Models.Localization
{
    /// <summary>
    /// Represents a language list model
    /// </summary>
    public partial record LanguageListModel : BasePagedListModel<LanguageModel>
    {
    }
}