using App.Core.Configuration;

namespace Satyanam.Plugin.Misc.TrackerAPI;

public partial class TrackerAPISettings : ISettings
{
	#region Properties

	public string APIKey { get; set; }

	public string APISecretKey { get; set; }

	public bool EnableKeyboardClick { get; set; }

	public bool EnableMouseClick { get; set; }

	public int MinimumKeyboardMouseClick { get; set; }

	public bool EnableScreenShot { get; set; }

    public int LastAwayCount { get; set; }

    public int TrackingDuration { get; set; }

	public int AlertDuration { get; set; }

	public int AlertMinimumDuration { get; set; }

	public int SwitchTaskDuration { get; set; }

	public bool EnableLogging { get; set; }

    public string ClientId { get; set; }

    public string ClientSecret { get; set; }

    public string TenantId { get; set; }

    public string UserId { get; set; }

    public int UploadTimeTrackerId { get; set; }

    #endregion
}
