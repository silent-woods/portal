using System.ComponentModel.DataAnnotations;

namespace Satyanam.Nop.Core.Domains
{
    /// <summary>
    /// Represents a  FollowUpStatusEnum
    /// </summary>
    public enum FollowUpStatusEnum
    {
        [Display(Name = "None")]
        None = 0,

        [Display(Name = "Pending")]
        Pending = 1,

        [Display(Name = "Message Sent")]
        MessageSent = 2,

        [Display(Name = "Replied")]
        Replied = 3,

        [Display(Name = "In Conversation")]
        InConversation = 4,

        [Display(Name = "Not Interested")]
        NotInterested = 5,

        [Display(Name = "Converted")]
        Converted = 6,

        [Display(Name = "Closed")]
        Closed = 7,

        [Display(Name = "Blocked / Removed")]
        BlockedOrRemoved = 8
    }

}
