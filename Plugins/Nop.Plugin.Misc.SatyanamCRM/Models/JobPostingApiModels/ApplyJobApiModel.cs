using App.Web.Framework.Models;
using Microsoft.AspNetCore.Http;

namespace Satyanam.Nop.Plugin.SatyanamCRM.Models.JobPostingApiModels
{
    public record ApplyJobApiModel : BaseNopEntityModel
    {
        #region Properties

        public int JobPostingId { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }

        public string Phone { get; set; }

        public string ExperienceYears { get; set; }

        public string CurrentCompany { get; set; }

        public decimal? CurrentCTC { get; set; }

        public decimal? ExpectedCTC { get; set; }

        public string City { get; set; }

        public int NoticePeriodId { get; set; }

        public string Skills { get; set; }

        public string AdditionalInformation { get; set; }
        public IFormFile ResumeFile { get; set; }
        public int? RateTypeId { get; set; }

        public decimal? Amount { get; set; }

        public string CoverLetter { get; set; }

        #endregion
    }
}