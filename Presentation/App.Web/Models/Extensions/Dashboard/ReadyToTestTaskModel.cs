using App.Web.Framework.Models;
using System;

public record ReadyToTestTaskModel : BaseNopModel
{
    public int TaskId { get; set; }
    public string TaskTitle { get; set; }
    public string ProjectName { get; set; }
    public string DeveloperName { get; set; }
    public string AssignedTo { get; set; }
    public string EstimatedTime { get; set; }
    public string SpentTime { get; set; }
    public int StatusId { get; set; }   
    public int ProcessWorkflowId { get; set; }
    public string DueDate { get; set; }
    public string StatusName { get; set; }
    public string StatusColorCode { get; set; }
    public int PendingSinceDays { get; set; }
    public DateTime? ReadyToTestStartDate { get; set; }
}
