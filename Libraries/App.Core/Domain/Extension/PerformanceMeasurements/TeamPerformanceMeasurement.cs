using System;

namespace App.Core.Domain.PerformanceMeasurements
{
    /// <summary>
    /// Represents a TeamPerformanceMeasurement
    /// </summary>
    public partial class TeamPerformanceMeasurement : BaseEntity
    {
        public TeamPerformanceMeasurement() { 
        }
        public int EmployeeId { get; set; }
        public int EmployeeManagerId { get; set; }
        public int MonthId { get; set; }

        public int Year { get; set; }
        public string Feedback { get; set; }
        public string KPIMasterData { get; set; }
        public DateTime CreateOnUtc { get; set; }
        public DateTime UpdateOnUtc { get; set; }
    }
  
}