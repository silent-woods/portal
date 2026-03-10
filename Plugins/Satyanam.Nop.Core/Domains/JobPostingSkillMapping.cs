using App.Core;

namespace Satyanam.Nop.Core.Domains
{
    /// <summary>
    /// Represents a  JobPostingSkillMapping   
    /// </summary>
    public class JobPostingSkillMapping : BaseEntity
    {
        public int JobPostingId { get; set; }
        public int SkillSetId { get; set; }

    }

}
