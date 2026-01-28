using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;

namespace App.Web.Areas.Admin.Models.Settings
{
    /// <summary>
    /// Represents a plugin configuration model
    /// </summary>
    public partial record PluginConfigModel : BaseNopModel, IConfigModel
    {
        #region Properties
        
        [NopResourceDisplayName("Admin.Configuration.AppSettings.Plugin.UseUnsafeLoadAssembly")]
        public bool UseUnsafeLoadAssembly { get; set; }

        #endregion
    }
}