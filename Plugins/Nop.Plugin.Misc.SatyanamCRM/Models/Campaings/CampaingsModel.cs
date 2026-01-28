using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System;
using System.ComponentModel.DataAnnotations;

namespace Satyanam.Nop.Plugin.SatyanamCRM.Models.Campaings
{
    public record CampaingsModel : BaseNopEntityModel
    {
        public CampaingsModel()
        {
            Emails = new List<SelectListItem>();
            Status = new List<SelectListItem>();
            Tags = new List<SelectListItem>();
			TagsId = new List<int>();
            AllowedTokens = new List<string>();
            campaingsSearchModel = new CampaingsSearchModel();
            CampaingsEmailLogSearchModel = new CampaingsEmailLogSearchModel();
        }
        public static class FilterFieldNames
        {
            public const string Email = "CampaingsEmailLogSearchModel.Email";
            public const string IsOpened = "CampaingsEmailLogSearchModel.IsOpened";
            public const string IsClicked = "CampaingsEmailLogSearchModel.IsClicked";
        }
        #region Properties
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.CampaingsModel.Name")]
        [Required(ErrorMessage = "Please enter a name.")]
        public string Name { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.CampaingsModel.EmailAccountId")]
        public int EmailAccountId { get; set; }
        public IList<SelectListItem> Emails { get; set; }

        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.CampaingsModel.Subject")]
        public string Subject { get; set; }
        
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.CampaingsModel.Body")]
        public string Body { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.CampaingsModel.StatusId")]
        public int StatusId { get; set; }
        public string StatusName { get; set; }
        public IList<SelectListItem> Status { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.CampaingsModel.TagsId")]
        public IList<int> TagsId { get; set; }
        public IList<SelectListItem> Tags { get; set; }
        [UIHint("DateTimeNullable")]
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.CampaingsModel.ScheduledDate")]
        public DateTime? ScheduledDate { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public DateTime UpdateOnUtc { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.CampaingsModel.SendTestEmailTo")]
        public string SendTestEmailTo { get; set; }
        public int TotalLeads { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.CampaingsModel.AllowedTokens")]
        public List<string> AllowedTokens { get; set; }
        public int TotalLead { get; set; }
        public int TotalSent { get; set; }
        public int TotalEmailsSent { get; set; }
        public int TotalOpens { get; set; }
        public int TotalClicks { get; set; }
        public int UnsubscribedCount { get; set; }
        public string UnsubscribeRate { get; set; }
        public string OpenRate => TotalEmailsSent == 0 ? "0" : $"{(TotalOpens * 100 / TotalEmailsSent)}";
        public int ClickRate => TotalEmailsSent > 0 ? (int)Math.Round((double)TotalClicks * 100 / TotalEmailsSent) : 0;
        public CampaingsSearchModel campaingsSearchModel { get; set; }
        public CampaingsEmailLogSearchModel CampaingsEmailLogSearchModel { get; set; }
       
        #endregion
    }
}