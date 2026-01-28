using App.Web.Framework.Models;
using System;

namespace App.Web.Models.Dashboard
{
    public partial record FollowUpTaskModel : BaseNopEntityModel
    {
        public int TaskId { get; set; }
        public string TaskName { get; set; }
        public int ReviewerId { get; set; }
        public string ReviewerName { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public string ProjectName { get; set; }
        public string EstimationTime { get; set; }
        public string DevelopementTime { get; set; }
        public int StatusId { get; set; }
        public string StatusName { get; set; }
        public string StatusColor { get; set; }
        public string LastComment { get; set; }
        public bool IsCompleted { get; set; }
        public bool CanTakeFollowUp { get; set; }
        public int AlertId { get; set; }
        public int ReasonId { get; set; }
        public bool OnTrack { get; set; }
        public string ETAHours { get; set; }
        public string AlertType { get; set; }
        public string TrackReason { get; set; }
        public bool IsManual { get; set; }      
        public DateTime? LastFollowupDateTime { get; set; }
        public DateTime? NextFollowupDateTime { get; set; }
        public DateTime UpdatedOn { get; set; }
    }

    public class SaveFollowUpRequestModel
    {
        public int Id { get; set; }
        public string NextDate { get; set; }
        public string Comment { get; set; }
    }
    public class FollowUpSubGridModel
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public int TaskId { get; set; }
        public int AlertId { get; set; }
        public decimal Variation { get; set; }
        public bool MailSent { get; set; }
        public int ReasonId { get; set; }
        public string ReasonName { get; set; }
        public string Comment { get; set; }
        public bool OnTrack { get; set; }
        public string ETAHours { get; set; }
        public int FollowUpTaskId { get; set; }
        public int ReviewerId { get; set; }
        public string ReviewerName { get; set; }
        public bool IsAutomatic { get; set; }
        public string AlertPercentage { get; set; }
        public string AlertType { get; set; }
        public DateTime? NextFollowupDateTime { get; set; }
        public DateTime CreatedOnUtc { get; set; }
    }
}