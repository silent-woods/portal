using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Satyanam.Nop.Plugin.SatyanamCRM.Models.Deals
{
    public record DealsCompanyModel : BaseNopEntityModel
    {
        public DealsCompanyModel()
        {
            AccountTypes = new List<SelectListItem>();
            Industrys = new List<SelectListItem>();
            Customers = new List<SelectListItem>();
            OwnerShips = new List<SelectListItem>();
            BillingStates = new List<SelectListItem>();
            BillingCountrys = new List<SelectListItem>();
            ShipingStates = new List<SelectListItem>();
            ShipingCountrys = new List<SelectListItem>();
            ParentAccounts = new List<SelectListItem>();
            CompanyOwner = new List<SelectListItem>();
        }

        #region Properties
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.Company.ContactId")]
        public int ContactId { get; set; }
        public IList<SelectListItem> CompanyOwner { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.Company.CompanyName")]
        [Required(ErrorMessage = "Please enter a companyname.")]
        public string CompanyName { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.Company.WebsiteUrl")]
        public string WebsiteUrl { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.Company.ParentAccountID")]
        public int ParentAccountID { get; set; }
        public IList<SelectListItem> ParentAccounts { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.Company.AccountNumber")]
        public int AccountNumber { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.Company.AccountTypeId")]
        public int AccountTypeId { get; set; }
        public IList<SelectListItem> AccountTypes { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.Company.IndustryId")]
        public int IndustryId { get; set; }
        public IList<SelectListItem> Industrys { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.Company.CustomerId")]
        public int CustomerId { get; set; }
        public IList<SelectListItem> Customers { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.Company.Phone")]
        public string Phone { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.Company.OwnerShipId")]
        public int OwnerShipId { get; set; }
        public string OwnerShipName { get; set; }
        public IList<SelectListItem> OwnerShips { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.Company.NoofEmployee")]
        public int NoofEmployee { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.Company.AnnualRevenue")]
        public int AnnualRevenue { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.Company.BillingAddressId")]
        public int BillingAddressId { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.Company.BillingZipCode")]
        public string BillingZipCode { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.Company.BillingStateId")]
        public int? BillingStateId { get; set; }
        public IList<SelectListItem> BillingStates { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.Company.BillingCity")]
        public string BillingCity { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.Company.BillingCountryId")]
        public int? BillingCountryId { get; set; }
        public IList<SelectListItem> BillingCountrys { get; set; }

        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.Company.BillingAddress1")]
        public string BillingAddress1 { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.Company.BillingAddress2")]
        public string BillingAddress2 { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.Company.ShipingAddressId")]
        public int ShipingAddressId { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.Company.ShipingZipCode")]
        public string ShipingZipCode { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.Company.ShipingStateId")]
        public int? ShipingStateId { get; set; }
        public IList<SelectListItem> ShipingStates { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.Company.ShipingCity")]
        public string ShipingCity { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.Company.ShipingCountryId")]
        public int? ShipingCountryId { get; set; }
        public IList<SelectListItem> ShipingCountrys { get; set; }

        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.Company.ShipingAddress1")]
        public string ShipingAddress1 { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.Company.ShipingAddress2")]
        public string ShipingAddress2 { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.Company.Description")]
        public string Description { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public DateTime UpdatedOnUtc { get; set; }

        #endregion
    }
}