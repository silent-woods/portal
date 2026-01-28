using App.Core;
using App.Core.Domain.Localization;
using App.Core.Infrastructure;
using App.Data;
using App.Services.Common;
using App.Services.Localization;
using App.Services.Plugins;
using App.Services.Security;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Satyanam.Plugin.Misc.AzureIntegration;

public partial class AzureIntegrationPlugin : BasePlugin, IMiscPlugin
{
    #region Fields

    protected readonly IPermissionService _permissionService;
    protected readonly ILocalizationService _localizationService;
    protected readonly INopFileProvider _nopFileProvider;
    protected readonly IRepository<Language> _languageRepository;
    protected readonly IWebHelper _webHelper;

    #endregion

    #region Ctor

    public AzureIntegrationPlugin(IPermissionService permissionService,
        ILocalizationService localizationService,
        INopFileProvider nopFileProvider,
        IRepository<Language> languageRepository,
        IWebHelper webHelper)
    {
        _permissionService = permissionService;
        _localizationService = localizationService;
        _nopFileProvider = nopFileProvider;
        _languageRepository = languageRepository;
        _webHelper = webHelper;
    }

    #endregion

    #region Plugin Configuration Methods

    public override string GetConfigurationPageUrl()
    {
        return _webHelper.GetStoreLocation() + "Admin/AzureIntegration/Configure";
    }

    #endregion

    #region Insert/Delete Local Resources

    protected virtual async Task InsertLocalResourcesAsync()
    {
        var languages = _languageRepository.Table.Where(l => l.Published).ToList();
        foreach (var language in languages)
        {
            foreach (var filePath in Directory.EnumerateFiles(_nopFileProvider.MapPath($"~/Plugins/Misc.AzureIntegration/Localization"), "ResourceStrings.xml", SearchOption.TopDirectoryOnly))
                using (var streamReader = new StreamReader(filePath))
                    await _localizationService.ImportResourcesFromXmlAsync(language, streamReader);
        }
    }

    protected virtual async Task DeleteLocalResourcesAsync()
    {
        var file = Path.Combine(_nopFileProvider.MapPath($"~/Plugins/Misc.AzureIntegration/Localization"), "ResourceStrings.xml");
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

        await base.InstallAsync();
    }

    public override async Task UninstallAsync()
    {
        await DeleteLocalResourcesAsync();

        await base.UninstallAsync();
    }

    #endregion
}
