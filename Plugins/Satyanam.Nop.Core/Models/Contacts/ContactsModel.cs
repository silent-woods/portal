using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Satyanam.Nop.Core.Models.Contacts
{
    public record ContactsModel : BaseNopEntityModel
    {
        public ContactsModel()
        {
            Titles = new List<SelectListItem>();
            ContactsSources = new List<SelectListItem>();
            Industrys = new List<SelectListItem>();
            Countrys = new List<SelectListItem>();
            States = new List<SelectListItem>();
			Customers = new List<SelectListItem>();
			Tags = new List<SelectListItem>();
            SelectedTagIds = new List<int>();
            contactsDealsSearch = new ContactsDealsSearchModel();
            contactsDealsModels = new List<ContactsDealsModel>();
            EmailStatus = new List<SelectListItem>();
        }

        #region Properties
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.Contacts.Name")]
        public string Name { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.Contacts.FirstName")]
        [Required(ErrorMessage = "Please enter a firstname.")]
        public string FirstName { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.Contacts.LastName")]
        [Required(ErrorMessage = "Please enter a lastname.")]
        public string LastName { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.Contacts.CompanyName")]
        public string CompanyName { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.Contacts.Phone")]
        public string Phone { get; set; }
		[DataType(DataType.EmailAddress)]
		[Required(ErrorMessage = "Please enter a valid email")]
		[NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.Contacts.Email")]
		public string Email { get; set; }

        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.Contacts.AnnualRevenue")]
        public int AnnualRevenue { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.Contacts.WebsiteUrl")]
        public string WebsiteUrl { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.Contacts.NoofEmployee")]
        public int NoofEmployee { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.Contacts.SkypeId")]
        public string SkypeId { get; set; }
		[NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.Contacts.SecondaryEmail")]
        public string SecondaryEmail { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.Contacts.TitleId")]
        public int TitleId { get; set; }
        public IList<SelectListItem> Titles { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.Contacts.CustomerId")]
        public int CustomerId { get; set; }
		public IList<SelectListItem> Customers { get; set; }
		[NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.Contacts.ContactsSourceId")]
        public int ContactsSourceId { get; set; }
        public IList<SelectListItem> ContactsSources { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.Contacts.IndustryId")]
        public int IndustryId { get; set; }
        public IList<SelectListItem> Industrys { get; set; }
        
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.Contacts.EmailOptOut")]
        public bool EmailOptOut { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.Contacts.Twitter")]
        public string Twitter { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.Contacts.LinkedinUrl")]
        public string LinkedinUrl { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.Contacts.Facebookurl")]
        public string Facebookurl { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.Contacts.ZipCode")]
        public string ZipCode { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.Contacts.StateId")]
        public int? StateId { get; set; }
        public IList<SelectListItem> States { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.Contacts.City")]
        public string City { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.Contacts.CountryId")]
        public int? CountryId { get; set; }
        public IList<SelectListItem> Countrys { get; set; }
        public int AddressId { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.Contacts.Address1")]
        public string Address1 { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.Contacts.Address2")]
        public string Address2 { get; set; }

        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.Contacts.Description")]
        public string Description { get; set; }
        
		[NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.Contacts.TagsId")]
		public int TagsId { get; set; }
        
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.Contacts.SelectedTagIds")]
		public IList<int> SelectedTagIds { get; set; }
        public IList<SelectListItem> Tags { get; set; }
        public string TitleName { get; set; }
       
        public DateTime CreatedOnUtc { get; set; }
        public DateTime UpdatedOnUtc { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.Contacts.EmailStatusId")]
        public int EmailStatusId { get; set; }
        public string EmailStatusName { get; set; }
        public IList<SelectListItem> EmailStatus { get; set; }
        public ContactsDealsSearchModel contactsDealsSearch { get; set; }
        public List<ContactsDealsModel> contactsDealsModels { get; set; }
        #endregion
    }
}