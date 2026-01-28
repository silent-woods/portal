using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Satyanam.Nop.Plugin.SatyanamCRM.Models.LinkedInFollowups
{
    public record LinkedInFollowupsSearchModel : BaseSearchModel
    {
        public LinkedInFollowupsSearchModel()
        {
            Status = new List<SelectListItem>();
        }

        #region Properties

        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Models.Titles.TitleSearchModel.SearchFirstName")]
        public string SearchFirstName { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Models.Titles.TitleSearchModel.SearchLastName")]
        public string SearchLastName { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Models.Titles.TitleSearchModel.SearchLinkedinUrl")]
        public string SearchLinkedinUrl { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Models.Titles.TitleSearchModel.SearchEmail")]
        public string SearchEmail { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Models.Titles.TitleSearchModel.SearchWebsiteUrl")]
        public string SearchWebsiteUrl { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Models.Titles.TitleSearchModel.SearchLastMessDate")]
        [UIHint("DateNullable")]
        public DateTime? SearchLastMessDate { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Models.Titles.TitleSearchModel.NextFollowUpDate")]
        [UIHint("DateNullable")]
        public DateTime? NextFollowUpDate { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Models.Titles.TitleSearchModel.SearchStatus")]
        public int? SearchStatus { get; set; }
        public IList<SelectListItem> Status { get; set; }

        #endregion
    }
}