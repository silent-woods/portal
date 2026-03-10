using App.Core;

namespace Satyanam.Nop.Core.Domains
{
    /// <summary>
    /// Represents a  TechnologySkillMapping 
    /// </summary>
    public class TechnologySkillMapping : BaseEntity
    {
        public int TechnologyId { get; set; }
        public int SkillSetId { get; set; }

    }

}
