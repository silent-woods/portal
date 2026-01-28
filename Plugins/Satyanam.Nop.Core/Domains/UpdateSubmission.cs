using App.Core;
using System;
using System.Collections.Generic;

namespace Satyanam.Nop.Core.Domains
{
    /// <summary>
    /// Represents a  UpdateSubmission 
    /// </summary>
    public class UpdateSubmission : BaseEntity
    {

        public int UpdateTemplateId { get; set; }
        public int? SubmittedByCustomerId { get; set; }
        public DateTime SubmittedOnUtc { get; set; }
        public int PeriodId { get; set; }
        public virtual ICollection<UpdateSubmissionAnswer> Answers { get; set; }

    }
}
