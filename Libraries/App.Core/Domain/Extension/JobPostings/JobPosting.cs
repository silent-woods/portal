using System;
namespace App.Core.Domain.JobPostings
{
    /// <summary>
    /// Represents a TimeSheet
    /// </summary>
    public partial class JobPosting : BaseEntity
    {
       
        public string Title { get; set; }
        public string Description { get; set; }
        public int PositionId { get; set; }
        public bool Publish { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
       
    }
}