using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using Satyanam.Nop.Plugin.SatyanamCRM.Models.Deals;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Satyanam.Nop.Plugin.SatyanamCRM.Models.Leads
{
    public record LeadModel : BaseNopEntityModel
    {
        public LeadModel()
        {
            Titles = new List<SelectListItem>();
            LeadSources = new List<SelectListItem>();
            Industrys = new List<SelectListItem>();
            LeadStatus = new List<SelectListItem>();
            Categorys = new List<SelectListItem>();
            Countrys = new List<SelectListItem>();
            States = new List<SelectListItem>();
            Customers = new List<SelectListItem>();
            Tags = new List<SelectListItem>();
            EmailStatus = new List<SelectListItem>();
            SelectedTagIds = new List<int>();
            DealsModels = new DealsModel();

        }

        #region Properties
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.Lead.Name")]
        public string Name { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.Lead.FirstName")]
        [Required(ErrorMessage = "Please enter a firstname.")]
        public string FirstName { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.Lead.LastName")]
        [Required(ErrorMessage = "Please enter a lastname.")]
        public string LastName { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.Lead.CompanyName")]
        public string CompanyName { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.Lead.Phone")]
        public string Phone { get; set; }
        [DataType(DataType.EmailAddress)]
        [Required(ErrorMessage = "Please enter a valid email")]
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.Lead.Email")]
        public string Email { get; set; }

        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.Lead.AnnualRevenue")]
        public int AnnualRevenue { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.Lead.WebsiteUrl")]
        public string WebsiteUrl { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.Lead.NoofEmployee")]
        public int NoofEmployee { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.Lead.SkypeId")]
        public string SkypeId { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.Lead.SecondaryEmail")]
        [EmailAddress(ErrorMessage = "Please enter a valid email format")]
        public string SecondaryEmail { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.Lead.TitleId")]
        public int TitleId { get; set; }
        public IList<SelectListItem> Titles { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.Lead.CustomerId")]
        public int CustomerId { get; set; }
        public IList<SelectListItem> Customers { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.Lead.LeadSourceId")]
        public int LeadSourceId { get; set; }
        public IList<SelectListItem> LeadSources { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.Lead.IndustryId")]
        public int IndustryId { get; set; }
        public IList<SelectListItem> Industrys { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.Lead.LeadStatusId")]
        public int LeadStatusId { get; set; }
        public string LeadStatusName { get; set; }
        public IList<SelectListItem> LeadStatus { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.Lead.EmailOptOut")]
        public bool EmailOptOut { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.Lead.Twitter")]
        public string Twitter { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.Lead.LinkedinUrl")]
        public string LinkedinUrl { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.Lead.Facebookurl")]
        public string Facebookurl { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.Lead.ZipCode")]
        public string ZipCode { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.Lead.StateId")]
        public int? StateId { get; set; }
        public IList<SelectListItem> States { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.Lead.City")]
        public string City { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.Lead.CountryId")]
        public int? CountryId { get; set; }
        public IList<SelectListItem> Countrys { get; set; }
        public int AddressId { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.Lead.Address1")]
        public string Address1 { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.Lead.Address2")]
        public string Address2 { get; set; }

        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.Lead.Description")]
        public string Description { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.Lead.CategoryId")]
        public int CategoryId { get; set; }
        public IList<SelectListItem> Categorys { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.Lead.TagsId")]
        public int TagsId { get; set; }
        public string TagsName { get; set; }
        public string TitleName { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.Lead.SelectedTagIds")]
        public IList<int> SelectedTagIds { get; set; }
        public IList<SelectListItem> Tags { get; set; }
        [ValidateNever]
        public DealsModel DealsModels { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.Lead.EmailStatusId")]
        public int EmailStatusId { get; set; }
        public string EmailStatusName { get; set; }
        public IList<SelectListItem> EmailStatus { get; set; }
        public bool IsSyncedToReply { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public DateTime UpdatedOnUtc { get; set; }
        #endregion
    }
}