using Newtonsoft.Json;

namespace Satyanam.Plugin.Misc.TrackerAPI.Models;

public partial class EmployeeActivityEventParametersModel : TrackerAPIResponseModel
{
	#region Properties

	[JsonProperty("keyboard_hit")]
	public int KeyboardHits { get; set; }

	[JsonProperty("mouse_hit")]
	public int MouseHits { get; set; }

    [JsonProperty("capture_screenshot_url")]
    public string ScreenShotUrl { get; set; }

    #endregion
}
