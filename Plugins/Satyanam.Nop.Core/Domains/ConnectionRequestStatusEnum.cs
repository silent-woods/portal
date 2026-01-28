using System.ComponentModel.DataAnnotations;

namespace Satyanam.Nop.Core.Domains
{
    /// <summary>
    /// Represents a  ConnectionRequestStatusEnum
    /// </summary>
    public enum ConnectionRequestStatusEnum
    {
        [Display(Name = "None")]
        None = 0,

        [Display(Name = "Request Sent")]
        RequestSent = 1,

        [Display(Name = "Accepted")]
        Accepted = 2,

        [Display(Name = "Pending")]
        Pending = 3,

        [Display(Name = "Ignored / Not Accepted")]
        Ignored = 4,

        [Display(Name = "Withdrawn")]
        Withdrawn = 5,

        [Display(Name = "Not Interested")]
        NotInterested = 6,

        [Display(Name = "Blocked / Removed")]
        BlockedOrRemoved = 7

    }

}
