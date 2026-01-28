using App.Core;
using App.Web.Framework.Mvc.ModelBinding;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Satyanam.Nop.Core.Domains
{
    /// <summary>
    /// Represents a  Inquiry
    /// </summary>
    public class Inquiry : BaseEntity
    {
        public int SourceId { get; set; }
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
    }
}
