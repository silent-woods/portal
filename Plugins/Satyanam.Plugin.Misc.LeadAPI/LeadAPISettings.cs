using App.Core.Configuration;

namespace Satyanam.Plugin.Misc.LeadAPI;

public partial class LeadAPISettings : ISettings
{
    #region Properties

    public string APIKey { get; set; }
    public string APISecretKey { get; set; }
    public bool EnableLogging { get; set; }
    #endregion
}
