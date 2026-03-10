using App.Core;

namespace Satyanam.Nop.Core.Domains
{
    /// <summary>
    /// Represents a  JobPostingTechnologyMapping  
    /// </summary>
    public class JobPostingTechnologyMapping : BaseEntity
    {
        public int JobPostingId { get; set; }
        public int TechnologyId { get; set; }

    }

}
