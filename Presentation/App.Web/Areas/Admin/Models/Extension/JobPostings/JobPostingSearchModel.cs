using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace App.Web.Areas.Admin.Models.JobPostings
{
    /// <summary>
    /// Represents a timesheet search model
    /// </summary>
    public partial record JobPostingSearchModel : BaseSearchModel
    {
        public JobPostingSearchModel()
        {
            AvailablePosition = new List<SelectListItem>();
        }
        public IList<SelectListItem> AvailablePosition { get; set; }
        #region Properties

        [NopResourceDisplayName("Admin.JobPosting.Fields.Title")]
        public string Title { get; set; }
        [NopResourceDisplayName("Admin.JobPosting.Fields.PositionId")]
        public int PositionId { get; set; }
        #endregion
    }
}