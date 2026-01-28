using System;

namespace App.Core.Domain.Leaves
{
    /// <summary>
    /// Represents a Leave
    /// </summary>
    public partial class Leave : BaseEntity
    {
        public string Type { get; set; }
        public string Description { get; set; }
        public DateTime CreateOnUtc { get; set; }
        public DateTime UpdateOnUtc { get; set; }
        public int Total_Allowed { get; set; }
    }
}