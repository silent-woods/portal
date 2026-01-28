using System;

namespace App.Core.Domain.Designations
{
    /// <summary>
    /// Represents a Designation
    /// </summary>
    public partial class Designation : BaseEntity
    {
       
        public string Name { get; set; }

        public bool CanGiveRatings { get; set; }
        public DateTime CreateOnUtc { get; set; }
        public DateTime UpdateOnUtc { get; set; }
    }
}