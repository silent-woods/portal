using Newtonsoft.Json;
using System.Collections.Generic;

namespace Satyanam.Plugin.Misc.TrackerAPI.DTO;

public partial class ProjectsWithTasksRootObject
{
    #region Ctor

    public ProjectsWithTasksRootObject()
    {
        AvailableProjects = new List<ProjectRootObject>();
    }

    #endregion

    #region Properties

    [JsonProperty("projects")]
    public IList<ProjectRootObject> AvailableProjects { get; set; }

    #endregion
}
