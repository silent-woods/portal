using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace App.Web.Areas.Admin.Models.JobPostings
{
    /// <summary>
    /// Represents a TimeSheet model
    /// </summary>
    public partial record JobPostingModel : BaseNopEntityModel
    {
        public JobPostingModel()
        {
            AvailablePosition = new List<SelectListItem>();
        }
        #region Properties


        [NopResourceDisplayName("Admin.JobPosting.Fields.Title")]
        [Required(ErrorMessage = "Please enter a Title.")]
        public string Title { get; set; }
        public IList<SelectListItem> AvailablePosition { get; set; }

        [NopResourceDisplayName("Admin.JobPosting.Fields.Description")]
        [Required(ErrorMessage = "Please enter a Description.")]
        public string Description { get; set; }

        [NopResourceDisplayName("Admin.JobPosting.Fields.PositionId")]
        [Required(ErrorMessage = "Please select a Position.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a Position.")]
        public int PositionId { get; set; }

        [NopResourceDisplayName("Admin.JobPosting.Fields.Position")]
        public string Position { get; set; }

        [NopResourceDisplayName("Admin.JobPosting.Fields.Publish")]
        public bool Publish { get; set; }

        [NopResourceDisplayName("Admin.JobPosting.Fields.CreateOn")]
        public DateTime CreateOn { get; set; }

        [NopResourceDisplayName("Admin.JobPosting.Fields.UpdateOn")]
        public DateTime UpdateOn { get; set; }

        #endregion
    }
}