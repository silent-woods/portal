using Newtonsoft.Json;

namespace Satyanam.Plugin.Misc.TrackerAPI.DTO;

public partial class EmployeeDetailsRootObject
{
	#region Properties

	[JsonProperty("id")]
	public int Id { get; set; }

    [JsonProperty("full_name")]
    public string FullName { get; set; }

	#endregion
}
