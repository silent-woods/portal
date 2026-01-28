using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Satyanam.Nop.Plugin.SatyanamCRM.Models.Deals
{
    public record DealsModel : BaseNopEntityModel
    {
        public DealsModel()
        {
            DealOwner = new List<SelectListItem>();
            Companys = new List<SelectListItem>();
            Stages = new List<SelectListItem>();
            Types = new List<SelectListItem>();
            LeadSources = new List<SelectListItem>();
            Contacts = new List<SelectListItem>();
            AvailableCompanies = new List<SelectListItem>();
            AvailableContacts = new List<SelectListItem>();
            DealsContactSearchModel = new DealsContactSearchModel();
            DealsContactModels = new List<DealsContactModel>();
            DealsCompanySearchModel = new DealsCompanySearchModel();
            DealsCompanyModels = new List<DealsCompanyModel>();
        }

        #region Properties

        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.Deals.CustomerId")]
        public int CustomerId { get; set; }
        public IList<SelectListItem> DealOwner { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.Deals.DealName")]
        [Required(ErrorMessage = "Please enter a dealname.")]
        public string DealName { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.Deals.Amount")]
        public int Amount { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.Deals.CompanyId")]
        public int ?CompanyId { get; set; }
        public IList<SelectListItem> Companys { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.Deals.StageId")]
        public int ?StageId { get; set; }
        public string StageName { get; set; }
        public IList<SelectListItem> Stages { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.Deals.TypeId")]
        public int? TypeId { get; set; }
        public string TypeName { get; set; }
        public IList<SelectListItem> Types { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.Deals.Probability")]
        public decimal Probability { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.Deals.NextStep")]
        public string NextStep { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.Deals.ExpectedRevenue")]
        public decimal ExpectedRevenue { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.Deals.LeadSourceId")]
        public int LeadSourceId { get; set; }
        public IList<SelectListItem> LeadSources { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.Deals.ClosingDate")]
        [UIHint("DateNullable")]
        public DateTime? ClosingDate { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.Deals.Date")]
        public string Date { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.Deals.Description")]
        public string Description { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.Deals.ContactName")]
        public int? ContactId { get; set; }
        public IList<SelectListItem> Contacts { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public DateTime UpdatedOnUtc { get; set; }

        public string ContactName { get; set; }
        public string CompanyName { get; set; }
        public IList<SelectListItem> AvailableCompanies { get; set; }
        public IList<SelectListItem> AvailableContacts { get; set; }

        public DealsContactSearchModel DealsContactSearchModel { get; set; }
        public List<DealsContactModel> DealsContactModels { get; set; }
        public DealsCompanySearchModel DealsCompanySearchModel { get; set; }
        public List<DealsCompanyModel> DealsCompanyModels { get; set; }

        #endregion
    }
}