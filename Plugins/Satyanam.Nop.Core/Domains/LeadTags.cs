using App.Core;

namespace Satyanam.Nop.Core.Domains
{
	/// <summary>
	/// Represents a  LeadTags
	/// </summary>
	public class LeadTags : BaseEntity
    {
        public int LeadId { get; set; }
        public int TagsId { get; set; }
    }
}
