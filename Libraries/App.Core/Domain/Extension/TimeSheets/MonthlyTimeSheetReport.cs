using System;
namespace App.Core.Domain.TimeSheets
{
    /// <summary>
    /// Represents a TimeSheet
    /// </summary>
    public partial class MonthlyTimeSheetReport : BaseEntity
    {
        public int EmployeeId { get; set; }
        public int ProjectId { get; set; }

        public int TaskId { get; set; }
        public DateTime SpentDate { get; set; }
        public decimal EstimatedHours { get; set; }
        public decimal SpentHours { get; set; }
        public bool Billable { get; set; }

        public decimal ExtraTime {  get; set; }

        public decimal AllowedVariations {  get; set; }

        public int BugCount {  get; set; }

        public string QualityComments {  get; set; }

        public bool DeliveredOnTime { get; set; }
        public DateTime CreateOnUtc { get; set; }
        public DateTime UpdateOnUtc { get; set; }

        public int TotalTask { get; set; }

        public int TotalDeliveredOnTime { get; set; }

        public double  ResultPercentage { get; set; }

        public decimal  TotalEstimatedHours {  get; set; }

        public decimal TotalSpentHours { get; set; }

        public int OverDueCount { get; set; }



            public int FirstOverDueThresholdCount { get; set; }
        public int SecondOverDueThresholdCount { get; set; }

        public int ThirdOverDueThresholdCount { get; set; }

        public decimal FirstOverDueThreshold { get; set; }
        public decimal SecondOverDueThreshold { get; set; }

        public decimal ThirdOverDueThreshold { get; set; }

        public decimal AvgWorkQuality { get; set; }

        public decimal WorkQuality { get; set; }

        public decimal DotPercentage { get; set; }








    }
}