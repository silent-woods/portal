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
using Satyanam.Plugin.Misc.TrackerAPI.Services;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Satyanam.Plugin.Misc.TrackerAPI;

public partial class TrackerAPIPlugin : BasePlugin, IMiscPlugin, IAdminMenuPlugin
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

    public TrackerAPIPlugin(IPermissionService permissionService,
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
        return _webHelper.GetStoreLocation() + "Admin/TrackerAPI/Configure";
    }

    #endregion

    #region Insert/Delete Local Resources

    protected virtual async Task InsertLocalResourcesAsync()
    {
        var languages = _languageRepository.Table.Where(l => l.Published).ToList();
        foreach (var language in languages)
        {
            foreach (var filePath in Directory.EnumerateFiles(_nopFileProvider.MapPath($"~/Plugins/Misc.TrackerAPI/Localization"), "ResourceStrings.xml", SearchOption.TopDirectoryOnly))
                using (var streamReader = new StreamReader(filePath))
                    await _localizationService.ImportResourcesFromXmlAsync(language, streamReader);
        }
    }

    protected virtual async Task DeleteLocalResourcesAsync()
    {
        var file = Path.Combine(_nopFileProvider.MapPath($"~/Plugins/Misc.TrackerAPI/Localization"), "ResourceStrings.xml");
        var languageResourceNames = from name in XDocument.Load(file).Document.Descendants("LocaleResource")
                                    select name.Attribute("Name").Value;

        foreach (var languageResourceName in languageResourceNames)
            await _localizationService.DeleteLocaleResourcesAsync(languageResourceName);
    }

    #endregion

    #region Plugin Install/Uninstall Methods

    public override async Task InstallAsync()
    {
        await InsertLocalResourcesAsync();

        var settings = new TrackerAPISettings
        {
            APIKey = string.Empty,
            APISecretKey = string.Empty,
            EnableKeyboardClick = true,
            EnableMouseClick = true,
            EnableScreenShot = true,
            TrackingDuration = 5,
            EnableLogging = true
        };

        await _settingService.SaveSettingAsync(settings);

        await base.InstallAsync();
    }

    public override async Task UninstallAsync()
    {
        await DeleteLocalResourcesAsync();

        await _settingService.DeleteSettingAsync<TrackerAPISettings>();

        await base.UninstallAsync();
    }

    #endregion

    #region Manage SiteMap Methods

    public async Task ManageSiteMapAsync(SiteMapNode siteMapNode)
    {
        var pluginMenuItem = new SiteMapNode()
        {
            Title = await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.TrackerAPI.MainMenu.Title"),
            Visible = await Authenticate(),
            IconClass = "fa fa-cube"
        };

        var configuration = new SiteMapNode()
        {
            SystemName = "Satyanam.Plugin.Misc.TrackerAPI.Admin.Configuration",
            Title = await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.TrackerAPI.Admin.Configuration"),
            ControllerName = "TrackerAPI",
            ActionName = "Configure",
            Visible = await _permissionService.AuthorizeAsync(TrackerAPIPermissionProvider.ManageTrackerAPIConfiguration, PermissionAction.View),
            IconClass = "far fa-dot-circle",
            RouteValues = new RouteValueDictionary() { { "Satyanam.Plugin.Misc.TrackerAPI", null } }
        };
        pluginMenuItem.ChildNodes.Add(configuration);

        var trackerAPILog = new SiteMapNode()
        {
            SystemName = "Satyanam.Plugin.Misc.TrackerAPI.Admin.TrackerAPILog",
            Title = await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.TrackerAPI.Admin.TrackerAPILog"),
            ControllerName = "TrackerAPI",
            ActionName = "List",
            Visible = await _permissionService.AuthorizeAsync(TrackerAPIPermissionProvider.ManageTrackerAPILog, PermissionAction.View),
            IconClass = "far fa-dot-circle",
            RouteValues = new RouteValueDictionary() { { "Satyanam.Plugin.Misc.TrackerAPI", null } }
        };
        pluginMenuItem.ChildNodes.Add(trackerAPILog);

        var title = await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.TrackerAPI.MainMenu.Title");
        var targetMenu = siteMapNode.ChildNodes.FirstOrDefault(x => x.Title == title);

        if (targetMenu != null)
            targetMenu.ChildNodes.Add(pluginMenuItem);
        else
            siteMapNode.ChildNodes.Add(pluginMenuItem);
    }

    public async Task<bool> Authenticate()
    {
        bool flag = false;
        if (await _permissionService.AuthorizeAsync(TrackerAPIPermissionProvider.ManageTrackerAPIConfiguration, PermissionAction.View) ||
            await _permissionService.AuthorizeAsync(TrackerAPIPermissionProvider.ManageTrackerAPILog, PermissionAction.View))
            flag = true;
        return flag;
    }

    #endregion
}
