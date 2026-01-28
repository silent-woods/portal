using Newtonsoft.Json;

namespace Satyanam.Plugin.Misc.TrackerAPI.Models;

public partial class EmployeeLoginParametersModel : TrackerAPIResponseModel
{
	#region Properties

	[JsonProperty("username")]
	public string Username { get; set; }

    [JsonProperty("password")]
    public string Password { get; set; }

	#endregion
}
