using Newtonsoft.Json;
using System.Collections.Generic;

namespace Satyanam.Plugin.Misc.TrackerAPI.DTO;

public partial class EmployeeTaskDetailsRootObject
{
	#region Ctor

	public EmployeeTaskDetailsRootObject()
	{
        AvailableProjects = new List<ProjectsRootObject>();
        AvailableStatuses = new List<TaskStatusesRootObject>();
        AvailableTaskTypes = new List<TaskTypesRootObject>();
        AvailableProcessWorkflows = new List<ProcessWorkflowRootObject>();

    }

    #endregion

    #region Properties

    [JsonProperty("projects")]
	public IList<ProjectsRootObject> AvailableProjects { get; set; }

    [JsonProperty("statuses")]
    public IList<TaskStatusesRootObject> AvailableStatuses { get; set; }

    [JsonProperty("task_types")]
    public IList<TaskTypesRootObject> AvailableTaskTypes { get; set; }

    [JsonProperty("process_workflows")]
    public IList<ProcessWorkflowRootObject> AvailableProcessWorkflows { get; set; }

    #endregion
}
