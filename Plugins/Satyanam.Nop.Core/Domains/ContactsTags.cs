using App.Core;

namespace Satyanam.Nop.Core.Domains
{
    /// <summary>
    /// Represents a  ContactsTags
    /// </summary>
    public class ContactsTags : BaseEntity
    {
        public int ContactsId { get; set; }
        public int TagsId { get; set; }
    }
}
