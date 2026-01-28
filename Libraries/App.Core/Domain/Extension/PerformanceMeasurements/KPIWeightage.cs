using System;
namespace App.Core.Domain.PerformanceMeasurements
{
    /// <summary>
    /// Represents a KPIWeightage
    /// </summary>
    public partial class KPIWeightage : BaseEntity
    {
        public int KPIMasterId { get; set; }
        public int DesignationId { get; set; }
        public decimal Percentage { get; set; }
        public DateTime CreateOnUtc { get; set; }
        public DateTime UpdateOnUtc { get; set; }
    }
}