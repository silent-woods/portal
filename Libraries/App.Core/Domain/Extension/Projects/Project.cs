using System;
namespace App.Core.Domain.Projects
{
    /// <summary>
    /// Represents a Projects
    /// </summary>
    public partial class Project : BaseEntity
    {
        public string ProjectTitle { get; set; }
      
        public string Description { get; set; }
        public DateTime CreateOnUtc { get; set; }
        public DateTime UpdateOnUtc { get; set; }

        public int StatusId { get; set; }

        public int CompanyId { get; set; }


        public string ProcessWorkflowIds { get; set; }


        public bool IsDeleted { get; set; }



    }
}