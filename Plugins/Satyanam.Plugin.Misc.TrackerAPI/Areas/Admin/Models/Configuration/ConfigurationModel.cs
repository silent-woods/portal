using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace Satyanam.Plugin.Misc.TrackerAPI.Areas.Admin.Models.Configuration;

public partial record ConfigurationModel : BaseNopEntityModel
{
    #region Properties

    [NopResourceDisplayName("Satyanam.Plugin.Misc.TrackerAPI.Admin.Configuration.Fields.APIKey")]
    public string APIKey { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.TrackerAPI.Admin.Configuration.Fields.APISecretKey")]
    public string APISecretKey { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.TrackerAPI.Admin.Configuration.Fields.EnableKeyboardClick")]
    public bool EnableKeyboardClick { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.TrackerAPI.Admin.Configuration.Fields.EnableMouseClick")]
    public bool EnableMouseClick { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.TrackerAPI.Admin.Configuration.Fields.MinimumKeyboardMouseClick")]
    public int MinimumKeyboardMouseClick { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.TrackerAPI.Admin.Configuration.Fields.EnableScreenShot")]
    public bool EnableScreenShot { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.TrackerAPI.Admin.Configuration.Fields.LastAwayCount")]
    public int LastAwayCount { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.TrackerAPI.Admin.Configuration.Fields.TrackingDuration")]
    public int TrackingDuration { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.TrackerAPI.Admin.Configuration.Fields.AlertDuration")]
    public int AlertDuration { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.TrackerAPI.Admin.Configuration.Fields.AlertMinimumDuration")]
    public int AlertMinimumDuration { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.TrackerAPI.Admin.Configuration.Fields.SwitchTaskDuration")]
    public int SwitchTaskDuration { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.TrackerAPI.Admin.Configuration.Fields.EnableLogging")]
    public bool EnableLogging { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.TrackerAPI.Admin.Configuration.Fields.ClientId")]
    public string ClientId { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.TrackerAPI.Admin.Configuration.Fields.ClientSecret")]
    public string ClientSecret { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.TrackerAPI.Admin.Configuration.Fields.TenantId")]
    public string TenantId { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.TrackerAPI.Admin.Configuration.Fields.UserId")]
    public string UserId { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.TrackerAPI.Admin.Configuration.Fields.UploadTimeTrackerId")]
    [UIHint("Download")]
    public int UploadTimeTrackerId { get; set; }

    #endregion
}
