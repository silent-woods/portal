using Newtonsoft.Json;
using System;

namespace Satyanam.Plugin.Misc.TrackerAPI.Models;

public partial class UpdateTaskDetailsParametersModel : TrackerAPIResponseModel
{
    #region Properties

    [JsonProperty("task_id")]
    public int TaskId { get; set; }

    [JsonProperty("task_title")]
    public string TaskTitle { get; set; }

    [JsonProperty("parent_task_id")]
    public int ParentTaskId { get; set; }

    [JsonProperty("assigned_to")]
    public int AssignedTo { get; set; }

    [JsonProperty("status_id")]
    public int StatusId { get; set; }

    [JsonProperty("type_id")]
    public int TaskTypeId { get; set; }

    [JsonProperty("due_date")]
    public DateTime? DueDate { get; set; }

    [JsonProperty("task_description")]
    public string TaskDescription { get; set; }

    [JsonProperty("comments")]
    public string Comments { get; set; }

    #endregion
}
