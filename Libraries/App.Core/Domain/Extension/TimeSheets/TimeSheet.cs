using System;
namespace App.Core.Domain.TimeSheets
{
    /// <summary>
    /// Represents a TimeSheet
    /// </summary>
    public partial class TimeSheet : BaseEntity
    {
      
        public int EmployeeId { get; set; }
        public int ProjectId { get; set; }
        
        public int TaskId { get; set; }

        public int ActivityId { get; set; }

        public DateTime? StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        public bool IsManualEntry { get; set; }


        public DateTime SpentDate { get; set; }
        //public decimal EstimatedHours { get; set; }
        public int SpentHours { get; set; }
        public int SpentMinutes { get; set; }

        public bool Billable { get; set; }

       
        public DateTime CreateOnUtc { get; set; }
        public DateTime UpdateOnUtc { get; set; }

       
    }

    public class MismatchEntryDto
    {
        public int TaskId { get; set; }
        public string ActualTime { get; set; }
        public string TaskTime { get; set; }


        public string TaskName { get; set; }

        public string ProjectName { get; set; }

        public decimal EstimatedTime { get; set; }

    }
}