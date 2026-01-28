using App.Core;
using App.Core.Domain.Localization;
using App.Core.Domain.Security;
using App.Core.Infrastructure;
using App.Data;
using App.Services.Common;
using App.Services.Configuration;
using App.Services.Localization;
using App.Services.Plugins;
using App.Services.Security;
using App.Web.Framework.Menu;
using Microsoft.AspNetCore.Routing;
using Satyanam.Plugin.Misc.LeadAPI.Services;
using System.Linq;
using System.Threading.Tasks;

namespace Satyanam.Plugin.Misc.LeadAPI;

public partial class LeadAPIPlugin : BasePlugin, IMiscPlugin, IAdminMenuPlugin
{
    #region Fields

    protected readonly IPermissionService _permissionService;
    protected readonly ILocalizationService _localizationService;
    protected readonly INopFileProvider _nopFileProvider;
    protected readonly IRepository<Language> _languageRepository;
    protected readonly ISettingService _settingService;
    protected readonly IWebHelper _webHelper;

    #endregion

    #region Ctor

    public LeadAPIPlugin(IPermissionService permissionService,
        ILocalizationService localizationService,
        INopFileProvider nopFileProvider,
        IRepository<Language> languageRepository,
        ISettingService settingService,
        IWebHelper webHelper)
    {
        _permissionService = permissionService;
        _localizationService = localizationService;
        _nopFileProvider = nopFileProvider;
        _languageRepository = languageRepository;
        _settingService = settingService;
        _webHelper = webHelper;
    }

    #endregion

    #region Plugin Configuration Methods

    public override string GetConfigurationPageUrl()
    {
        return _webHelper.GetStoreLocation() + "Admin/LeadAPI/Configure";
    }

    #endregion

    #region Plugin Install/Uninstall Methods

    public override async Task InstallAsync()
    {
        //await InsertLocalResourcesAsync();

        var settings = new LeadAPISettings
        {
            APIKey = string.Empty,
            APISecretKey = string.Empty,
        };

        await _settingService.SaveSettingAsync(settings);

        await base.InstallAsync();
    }

    public override async Task UninstallAsync()
    {

        await _settingService.DeleteSettingAsync<LeadAPISettings>();

        await base.UninstallAsync();
    }

    #endregion

    #region Manage SiteMap Methods

    public async Task ManageSiteMapAsync(SiteMapNode siteMapNode)
    {
        var pluginMenuItem = new SiteMapNode()
        {
            Title = await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.LeadAPI.MainMenu.Title"),
            Visible = await Authenticate(),
            IconClass = "fa fa-id-card"
        };

        var configuration = new SiteMapNode()
        {
            SystemName = "Satyanam.Plugin.Misc.LeadAPI.Admin.Configuration",
            Title = await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.LeadAPI.Admin.Configuration"),
            ControllerName = "LeadAPI",
            ActionName = "Configure",
            Visible = await _permissionService.AuthorizeAsync(LeadAPIPermissionProvider.ManageLeadAPIConfiguration, PermissionAction.View),
            IconClass = "far fa-dot-circle",
            RouteValues = new RouteValueDictionary() { { "Satyanam.Plugin.Misc.LeadAPI", null } }
        };
        pluginMenuItem.ChildNodes.Add(configuration);

        var trackerAPILog = new SiteMapNode()
        {
            SystemName = "Satyanam.Plugin.Misc.LeadAPI.Admin.LeadAPILog",
            Title = await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.LeadAPI.Admin.LeadAPILog"),
            ControllerName = "LeadAPI",
            ActionName = "List",
            Visible = await _permissionService.AuthorizeAsync(LeadAPIPermissionProvider.ManageLeadAPILog, PermissionAction.View),
            IconClass = "far fa-dot-circle",
            RouteValues = new RouteValueDictionary() { { "Satyanam.Plugin.Misc.LeadAPI", null } }
        };
        pluginMenuItem.ChildNodes.Add(trackerAPILog);

        var title = await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.LeadAPI.MainMenu.Title");
        var targetMenu = siteMapNode.ChildNodes.FirstOrDefault(x => x.Title == title);

        if (targetMenu != null)
            targetMenu.ChildNodes.Add(pluginMenuItem);
        else
            siteMapNode.ChildNodes.Add(pluginMenuItem);
    }

    public async Task<bool> Authenticate()
    {
        bool flag = false;
        if (await _permissionService.AuthorizeAsync(LeadAPIPermissionProvider.ManageLeadAPIConfiguration, PermissionAction.View) ||
            await _permissionService.AuthorizeAsync(LeadAPIPermissionProvider.ManageLeadAPILog, PermissionAction.View))
            flag = true;
        return flag;
    }

    #endregion
}
