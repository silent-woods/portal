using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Satyanam.Nop.Plugin.SatyanamCRM.Models.Contacts
{
    public record ContactsDealsModel : BaseNopEntityModel
    {
        public ContactsDealsModel()
        {
            DealOwner = new List<SelectListItem>();
            Companys = new List<SelectListItem>();
            Stages = new List<SelectListItem>();
            Types = new List<SelectListItem>();
            LeadSources = new List<SelectListItem>();
            Contacts = new List<SelectListItem>();

        }

        #region Properties
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.ContactsDealsModel.DealName")]
        [Required(ErrorMessage = "Please enter a dealname.")]
        public string DealName { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.ContactsDealsModel.Amount")]
        public int Amount { get; set; }
        public string Stage { get; set; }
        public string StageName { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.ContactsDealsModel.Probability")]
        public decimal Probability { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.ContactsDealsModel.ClosingDate")]
        [UIHint("DateNullable")]
        public DateTime? ClosingDate { get; set; }
        public string Date { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.ContactsDealsModel.CustomerId")]
        public int CustomerId { get; set; }
        public IList<SelectListItem> DealOwner { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.ContactsDealsModel.CompanyId")]
        public int? CompanyId { get; set; }
        public IList<SelectListItem> Companys { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.ContactsDealsModel.StageId")]
        public int ?StageId { get; set; }
        public IList<SelectListItem> Stages { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.ContactsDealsModel.TypeId")]
        public int ?TypeId { get; set; }
        public IList<SelectListItem> Types { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.ContactsDealsModel.NextStep")]
        public string NextStep { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.ContactsDealsModel.ExpectedRevenue")]
        public decimal ExpectedRevenue { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.ContactsDealsModel.LeadSourceId")]
        public int LeadSourceId { get; set; }
        public IList<SelectListItem> LeadSources { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.ContactsDealsModel.Description")]
        public string Description { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.ContactsDealsModel.ContactId")]
        public int? ContactId { get; set; }
        public IList<SelectListItem> Contacts { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public DateTime UpdatedOnUtc { get; set; }
        #endregion
    }
}