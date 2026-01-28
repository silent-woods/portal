using App.Web.Framework.Models;

namespace App.Web.Areas.Admin.Models.Cms
{
    /// <summary>
    /// Represents a widget list model
    /// </summary>
    public partial record WidgetListModel : BasePagedListModel<WidgetModel>
    {
    }
}