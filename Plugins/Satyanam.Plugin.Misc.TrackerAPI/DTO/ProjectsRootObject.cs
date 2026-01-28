using Newtonsoft.Json;

namespace Satyanam.Plugin.Misc.TrackerAPI.DTO;

public partial class ProjectsRootObject
{
    #region Properties

    [JsonProperty("id")]
    public int Id { get; set; }

	[JsonProperty("project_name")]
	public string ProjectName { get; set; }

	#endregion
}
