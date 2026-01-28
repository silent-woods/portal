using App.Core;
using App.Services.Common;
using App.Services.Plugins;
using App.Services.Security;
using App.Web.Framework.Menu;
using Microsoft.AspNetCore.Routing;
using System.Linq;
using System.Threading.Tasks;

namespace Satyanam.Nop.Core
{
    /// <summary>
    /// Represents the SatyanamNopCore plugin
    /// </summary>
    public class SatyanamNopCore : BasePlugin, IAdminMenuPlugin, IMiscPlugin
    {
        #region Fields

        private readonly IPermissionService _permissionService;
        private readonly IWebHelper _webHelper;

        #endregion

        #region Ctor

        public SatyanamNopCore(IPermissionService permissionService,
            IWebHelper webHelper)
        {
            _permissionService = permissionService;
            _webHelper = webHelper;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets a configuration page URL
        /// </summary>
        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/WebApi/Configure";
        }

        /// <summary>
        /// Install the plugin
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        public override async Task InstallAsync()
        {
            await base.InstallAsync();
        }

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            
        }

        /// <summary>
        /// Uninstall the plugin
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        public override async Task UninstallAsync()
        {
            //await base.UninstallAsync();
        }

        #endregion
    }
}
