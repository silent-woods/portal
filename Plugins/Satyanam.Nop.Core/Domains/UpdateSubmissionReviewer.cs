using App.Core;
using System;

namespace Satyanam.Nop.Core.Domains
{
    /// <summary>
    /// Represents a  UpdateSubmissionReviewer  
    /// </summary>
    public class UpdateSubmissionReviewer : BaseEntity
    {

        public int UpdateSubmissionId { get; set; }

        public int ReviewerCustomerId { get; set; }


    }
}
