using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Satyanam.Plugin.Misc.TrackerAPI.DTO;

public partial class TaskAlertRootObject
{
    #region Ctor

    public TaskAlertRootObject()
    {
        AvailableProjects = new List<ProjectRootObject>();
        AvailableTaskAlertConfigurations = new List<TaskAlertConfigurationRootObject>();
    }

    #endregion

    #region Properties

    [JsonProperty("employee_id")]
    public int EmployeeId { get; set; }

    [JsonProperty("project_id")]
    public int ProjectId { get; set; }

    [JsonProperty("task_id")]
    public int TaskId { get; set; }

    [JsonProperty("activity_id")]
    public int ActivityId { get; set; }

    [JsonProperty("task_name")]
    public string TaskName { get; set; }

    [JsonProperty("activity_name")]
    public string ActivityName { get; set; }

    [JsonProperty("estimation_time")]
    public decimal EstimationTime { get; set; }

    [JsonProperty("billable")]
    public bool Billable { get; set; }

    [JsonProperty("status_id")]
    public int StatusId { get; set; }

    [JsonProperty("projects")]
    public IList<ProjectRootObject> AvailableProjects { get; set; }

    [JsonProperty("task_alert_configurations")]
    public IList<TaskAlertConfigurationRootObject> AvailableTaskAlertConfigurations { get; set; }

    #endregion
}
