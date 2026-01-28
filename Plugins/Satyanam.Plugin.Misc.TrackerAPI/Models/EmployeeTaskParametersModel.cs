using Newtonsoft.Json;
using System;

namespace Satyanam.Plugin.Misc.TrackerAPI.Models;

public partial class EmployeeTaskParametersModel : TrackerAPIResponseModel
{
    #region Properties

    [JsonProperty("assigned_to")]
    public int AssignedTo { get; set; }

    [JsonProperty("developer_id")]
    public int DeveloperId { get; set; }

    [JsonProperty("task_title")]
    public string TaskTitle { get; set; }

    [JsonProperty("parent_task_id")]
    public int ParentTaskId { get; set; }

    [JsonProperty("task_category_id")]
    public int TaskCategoryId { get; set; }

    [JsonProperty("task_description")]
    public string TaskDescription { get; set; }

    [JsonProperty("estimation_time")]
    public string EstimationTime { get; set; }

    [JsonProperty("type_id")]
    public int TaskTypeId { get; set; }

    [JsonProperty("process_workflow_id")]
    public int ProcessWorkflowId { get; set; }

    [JsonProperty("due_date")]
    public DateTime? DueDate { get; set; }

    [JsonProperty("is_sync")]
    public bool IsSync { get; set; }

    #endregion
}
