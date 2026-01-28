using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Satyanam.Plugin.Misc.TrackerAPI.DTO;

public partial class TaskDetailsRootObject
{
    #region Ctor

    public TaskDetailsRootObject()
    {
        AvailableStatuses = new List<TaskStatusesRootObject>();
        AvailableTaskTypes = new List<TaskTypesRootObject>();
        AvailableProcessWorkflows = new List<ProcessWorkflowRootObject>();
        AvailableParentTasks = new List<ParentTasksRootObject>();
    }

    #endregion

    #region Properties

    [JsonProperty("task_title")]
	public string TaskTitle { get; set; }

    [JsonProperty("parent_task_id")]
    public int ParentTaskId { get; set; }

    [JsonProperty("project_name")]
    public string ProjectName { get; set; }

    [JsonProperty("assigned_to")]
    public int AssignedTo { get; set; }

    [JsonProperty("estimation_time")]
    public string EstimationTime { get; set; }

    [JsonProperty("status_id")]
    public int StatusId { get; set; }

    [JsonProperty("type_id")]
    public int TaskTypeId { get; set; }

    [JsonProperty("due_date")]
    public DateTime? DueDate { get; set; }

    [JsonProperty("task_description")]
    public string TaskDescription { get; set; }

    [JsonProperty("statuses")]
    public IList<TaskStatusesRootObject> AvailableStatuses { get; set; }

    [JsonProperty("task_types")]
    public IList<TaskTypesRootObject> AvailableTaskTypes { get; set; }

    [JsonProperty("process_workflows")]
    public IList<ProcessWorkflowRootObject> AvailableProcessWorkflows { get; set; }

    [JsonProperty("parent_tasks")]
    public IList<ParentTasksRootObject> AvailableParentTasks { get; set; }

    #endregion
}
