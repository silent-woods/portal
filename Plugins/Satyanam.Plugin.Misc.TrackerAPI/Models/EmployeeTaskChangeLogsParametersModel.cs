using Newtonsoft.Json;
using System;

namespace Satyanam.Plugin.Misc.TrackerAPI.Models;

public partial class EmployeeTaskChangeLogsParametersModel : TrackerAPIResponseModel
{
	#region Properties

	[JsonProperty("project_id")]
	public int ProjectId { get; set; }

    [JsonProperty("task_id")]
    public int TaskId { get; set; }

    [JsonProperty("task_name")]
    public string TaskName { get; set; }

    [JsonProperty("task_status_id")]
    public int TaskStatusId { get; set; }

    [JsonProperty("task_type_id")]
    public int TaskTypeId { get; set; }

    [JsonProperty("process_workflow_id")]
    public int ProcessWorkflowId { get; set; }

    [JsonProperty("due_date")]
    public DateTime? DueDate { get; set; }

    #endregion
}
