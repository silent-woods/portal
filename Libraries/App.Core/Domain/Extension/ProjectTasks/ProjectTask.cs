using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Core.Domain.ProjectTasks
{
    public partial class ProjectTask : BaseEntity
    {
        public int ProjectId { get; set; }
        public string TaskTitle { get; set; }
        public string Description { get; set; }
        public int StatusId { get; set; }
        public decimal EstimatedTime { get; set; }
        public int SpentHours { get; set; }
        public int SpentMinutes { get; set; }
        public int BugCount { get; set; }
        public string QualityComments { get; set; }
        public bool DeliveryOnTime { get; set; }
        public decimal? WorkQuality { get; set; }
        public bool IsManualDOT { get; set; }
        public int AssignedTo { get; set; }
        public int DeveloperId { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public int Tasktypeid { get; set; }
        public DateTime? DueDate { get; set; }
        public int ProcessWorkflowId { get; set; }
        public int WorkItemId { get; set; }
        public int ParentTaskId { get; set; }
        public bool IsSync { get; set; }
        public decimal? DOTPercentage { get; set; }
        public int TaskCategoryId { get; set; }
        public bool IsDeleted { get; set; }


    }



    public class SpentTimeDto
    {
        public string BillableDevelopmentTime { get; set; }
        public string NotBillableDevelopmentTime { get; set; }
        public string BillableQATime { get; set; }
        public string NotBillableQATime { get; set; }
        public string TotalDevelopmentTime { get; set; }
        public string TotalQATime { get; set; }
        public string TotalBillableTime { get; set; }
        public string TotalNotBillableTime { get; set; }
        public string TotalSpentTime { get; set; }

    }
}
