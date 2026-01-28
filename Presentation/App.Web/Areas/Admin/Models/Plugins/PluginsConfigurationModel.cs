using App.Web.Areas.Admin.Models.Plugins.Marketplace;
using App.Web.Framework.Models;

namespace App.Web.Areas.Admin.Models.Plugins
{
    /// <summary>
    /// Represents a plugins configuration model
    /// </summary>
    public partial record PluginsConfigurationModel : BaseNopModel
    {
        #region Ctor

        public PluginsConfigurationModel()
        {
            PluginsLocal = new PluginSearchModel();
            AllPluginsAndThemes = new OfficialFeedPluginSearchModel();
        }

        #endregion

        #region Properties

        public PluginSearchModel PluginsLocal { get; set; }

        public OfficialFeedPluginSearchModel AllPluginsAndThemes { get; set; }

        #endregion
    }
}
