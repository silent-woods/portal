using App.Web.Framework.Models;

namespace App.Web.Areas.Admin.Models.Plugins
{
    /// <summary>
    /// Represents a plugin list model
    /// </summary>
    public partial record PluginListModel : BasePagedListModel<PluginModel>
    {
    }
}