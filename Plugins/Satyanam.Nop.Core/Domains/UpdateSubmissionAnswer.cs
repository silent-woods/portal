using App.Core;
using System;
using System.Collections.Generic;

namespace Satyanam.Nop.Core.Domains
{
    /// <summary>
    /// Represents a  UpdateSubmissionAnswer 
    /// </summary>
    public class UpdateSubmissionAnswer : BaseEntity
    {

        public int UpdateSubmissionId { get; set; }
        public int UpdateTemplateQuestionId { get; set; }
        public string AnswerText { get; set; }
        public string FilePath { get; set; }
        public string FileName { get; set; }

        public virtual UpdateSubmission Submission { get; set; }

    }
}
