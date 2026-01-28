using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace Satyanam.Nop.Core.Models.Leads
{
    public record LeadSearchModel : BaseSearchModel
    {
        public LeadSearchModel()
        {
            AvailableStatus = new List<SelectListItem>();
            AvailableTitles = new List<SelectListItem>();
            SearchTitlesId = new List<int>();
            Tags = new List<SelectListItem>();
            SelectedTags = new List<int>();
            AvailableEmailStatus = new List<SelectListItem>();
        }

        #region Properties

        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Models.Lead.LeadSearchModel.SearchName")]
        public string SearchName { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Models.Lead.LeadSearchModel.SearchCompanyName")]
        public string SearchCompanytName { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Models.Lead.LeadSearchModel.SearchPhone")]
        public string SearchPhone { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Models.Lead.LeadSearchModel.SearchEmail")]
        public string SearchEmail { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Models.Lead.LeadSearchModel.SearchWebsiteUrl")]
        public string SearchWebsiteUrl { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Models.Lead.LeadSearchModel.SearchNoofEmployee")]
        public int SearchNoofEmployee { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Models.Lead.LeadSearchModel.SearchLeadStatusId")]
        public int SearchLeadStatusId { get; set; }
        public IList<SelectListItem> AvailableStatus { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Models.Lead.LeadSearchModel.SelectedTags")]
        public IList<int> SelectedTags { get; set; }
        public IList<SelectListItem> Tags { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Models.Lead.LeadSearchModel.SearchTitlesId")]
        public IList<int> SearchTitlesId { get; set; }
        public IList<SelectListItem> AvailableTitles { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Models.Lead.LeadSearchModel.SearchEmailStatusId")]
        public int SearchEmailStatusId { get; set; }
        public IList<SelectListItem> AvailableEmailStatus { get; set; }
        #endregion
    }
}