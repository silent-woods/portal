namespace App.Web.Models.Dashboard
{
    public record OverdueTaskModel
    {
        public int TaskId { get; set; }
        public string TaskTitle { get; set; }
        public string ProjectName { get; set; }
        public string AssignedTo { get; set; }
        public string DeveloperName { get; set; }
        public int ProcessWorkflowId { get; set; }
        public string StatusName { get; set; }
        public string StatusColorCode { get; set; }
        public string EstimationTime { get; set; }
        public string DeveloperTime { get; set; }
        public int StatusId { get; set; }
        public string DueDate { get; set; }
    }
}