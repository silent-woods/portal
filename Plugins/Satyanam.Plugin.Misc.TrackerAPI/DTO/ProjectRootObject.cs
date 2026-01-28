using Newtonsoft.Json;
using System.Collections.Generic;

namespace Satyanam.Plugin.Misc.TrackerAPI.DTO;

public partial class ProjectRootObject
{
    #region Ctor

    public ProjectRootObject()
    {
        AvailableTasks = new List<TaskRootObject>();
    }

    #endregion

    #region Properties

    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("project_name")]
    public string ProjectName { get; set; }

    [JsonProperty("tasks")]
    public IList<TaskRootObject> AvailableTasks { get; set; }

    #endregion
}
