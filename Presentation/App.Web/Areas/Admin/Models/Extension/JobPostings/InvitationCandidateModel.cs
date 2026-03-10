using App.Web.Framework.Models;
using System;

namespace App.Web.Areas.Admin.Models.JobPostings
{
    /// <summary>
    /// Represents a InvitationCandidate model
    /// </summary>
    public partial record InvitationCandidateModel : BaseNopEntityModel
    {
        public InvitationCandidateModel()
        {

        }
        #region Properties

        public string Name { get; set; }
        public string Email { get; set; }
        public string CandidateTypeName { get; set; }
        public string Status { get; set; }
        public DateTime CreatedOnUtc { get; set; }


        #endregion
    }
}