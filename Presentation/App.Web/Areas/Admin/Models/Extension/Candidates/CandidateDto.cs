using App.Core;
using System;

namespace App.Web.Areas.Admin.Models.Extension.Candidates
{
    public class CandidateDto : BaseEntity
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string PositionApplied { get; set; }
        public string Position { get; set; }
        public string ExperienceYears { get; set; }
        public string Source { get; set; }
        public string Status { get; set; }
        public string CandidateType { get; set; }
        public string RateType { get; set; }
        public string LinkType { get; set; }
        public string Url { get; set; }
        public decimal? Amount { get; set; }
        public string CurrentCompany { get; set; }
        public decimal? CurrentCTC { get; set; }
        public decimal? ExpectedCTC { get; set; }
        public string NoticePeriod { get; set; }
        public string City { get; set; }
        public string AdditionalInformation { get; set; }
        public string HrNotes { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public DateTime UpdatedOnUtc { get; set; }
    }

}
