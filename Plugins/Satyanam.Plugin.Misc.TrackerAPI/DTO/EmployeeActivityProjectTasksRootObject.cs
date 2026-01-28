using Newtonsoft.Json;
using System.Collections.Generic;

namespace Satyanam.Plugin.Misc.TrackerAPI.DTO;

public partial class EmployeeActivityProjectTasksRootObject
{
	#region Ctor

	public EmployeeActivityProjectTasksRootObject()
	{
        AvailableLastActivity = new EmployeeActivityProjectTaskRootObject();
        AvailableProjectTasks = new List<ProjectTasksRootObject>();
    }

    #endregion

    #region Properties

    [JsonProperty("last_activity")]
    public EmployeeActivityProjectTaskRootObject AvailableLastActivity { get; set; }

    [JsonProperty("tasks")]
	public IList<ProjectTasksRootObject> AvailableProjectTasks { get; set; }

	#endregion
}
