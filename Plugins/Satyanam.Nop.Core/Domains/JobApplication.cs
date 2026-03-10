using App.Core;
using System;

namespace Satyanam.Nop.Core.Domains
{
    /// <summary>
    /// Represents a  JobApplication  
    /// </summary>
    public class JobApplication : BaseEntity
    {
        public int CandidateId { get; set; }
        public int JobPostingId { get; set; }
        public int StatusId { get; set; }
        public string ExperienceYears { get; set; }
        public string CurrentCompany { get; set; }
        public decimal? CurrentCTC { get; set; }
        public decimal? ExpectedCTC { get; set; }
        public int? NoticePeriodId { get; set; }
        public int PositionId { get; set; }
        public int? ResumeDownloadId { get; set; }
        public int? RateTypeId { get; set; }
        public int LinkTypeId { get; set; }
        public decimal? Amount { get; set; }
        public string Url { get; set; }
        public string CoverLetter { get; set; }
        public string Skills { get; set; }
        public string PositionApplied { get; set; }
        public string City { get; set; }
        public string AdditionalInformation { get; set; }
        public string HrNotes { get; set; }
        public DateTime AppliedOnUtc { get; set; }
        public DateTime UpdatedOnUtc { get; set; }

    }

}
