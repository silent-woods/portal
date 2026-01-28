using App.Web.Framework.Models;

namespace App.Web.Areas.Admin.Models.Plugins
{
    /// <summary>
    /// Represents a plugin model that is used for admin navigation
    /// </summary>
    public partial record AdminNavigationPluginModel : BaseNopModel
    {
        #region Properties

        public string FriendlyName { get; set; }

        public string ConfigurationUrl { get; set; }

        #endregion
    }
}