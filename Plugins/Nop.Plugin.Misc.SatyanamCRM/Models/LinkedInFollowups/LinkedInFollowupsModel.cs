using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Satyanam.Nop.Plugin.SatyanamCRM.Models.LinkedInFollowups
{
    public record LinkedInFollowupsModel : BaseNopEntityModel
    {
        public LinkedInFollowupsModel()
        {
            Status = new List<SelectListItem>();
        }

        #region Properties

        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.LinkedInFollowups.FirstName")]
        public string FirstName { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.LinkedInFollowups.LastName")]
        public string LastName { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.LinkedInFollowups.LinkedinUrl")]
        public string LinkedinUrl { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.LinkedInFollowups.Email")]
        public string Email { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.LinkedInFollowups.WebsiteUrl")]
        public string WebsiteUrl { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.LinkedInFollowups.LastMessageDate")]
        [UIHint("DateNullable")]
        public DateTime? LastMessageDate { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.LinkedInFollowups.LastMessDate")]
        public string LastMessDate { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.LinkedInFollowups.FollowUp")]
        public int FollowUp { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.LinkedInFollowups.NextFollowUpDate")]
        [UIHint("DateNullable")]
        public DateTime? NextFollowUpDate { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.LinkedInFollowups.NextFollowupsDate")]
        public string NextFollowupsDate { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.LinkedInFollowups.DaysUntilNext")]
        public int DaysUntilNext { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.LinkedInFollowups.RemainingFollowUps")]
        public int RemainingFollowUps { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.LinkedInFollowups.AutoStatus")]
        public string AutoStatus { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.LinkedInFollowups.Status")]
        public int StatusId { get; set; }
        public IList<SelectListItem> Status { get; set; }
        public string StatusText { get; set; }

        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.LinkedInFollowups.Notes")]
        public string Notes { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.LinkedInFollowups.CreatedOnUtc")]
        public DateTime CreatedOnUtc { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.LinkedInFollowups.UpdatedOnUtc")]
        public DateTime UpdatedOnUtc { get; set; }
        #endregion
    }
}