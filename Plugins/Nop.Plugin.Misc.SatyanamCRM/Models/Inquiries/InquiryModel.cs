using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;
using System;
using System.ComponentModel.DataAnnotations;

namespace Satyanam.Nop.Plugin.SatyanamCRM.Models.Inquiries
{
    public record InquiryModel : BaseNopEntityModel
    {
        #region Properties

        public int SourceId { get; set; }
        public string Source { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Describe { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ContactNo { get; set; }
        public string Company { get; set; }
        public string WantToHire { get; set; }
        public string ProjectType { get; set; }
        public string EngagementDuration { get; set; }
        public string TimeCommitment { get; set; }
        public string Budget { get; set; }

        #endregion
    }
}