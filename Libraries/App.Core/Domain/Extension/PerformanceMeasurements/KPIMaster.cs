using System;
namespace App.Core.Domain.PerformanceMeasurements
{
    /// <summary>
    /// Represents a KPIMaster
    /// </summary>
    public partial class KPIMaster : BaseEntity
    {
        public string Name { get; set; }
        public int DisplayOrder { get; set; }
        public DateTime CreateOnUtc { get; set; }
        public DateTime UpdateOnUtc { get; set; }
    }
}